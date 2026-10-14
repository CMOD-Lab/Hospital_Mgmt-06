using System;
using System.Collections.Generic;
using System.Linq;
// cr-dotnet-0010: Web.config transformation files (Web.Debug.config, Web.Release.config)
// have been eliminated. Configuration is now sourced exclusively from environment variables
// and AWS Systems Manager Parameter Store at runtime, enabling immutable deployments.
// The System.Configuration.ConfigurationManager fallback has been removed; all connection
// string resolution is performed via environment variables (see CreateConnection() below).
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

// RDS Proxy + Dapper: connections are obtained via CreateConnection() which
// builds the connection string from environment variables so that the
// application targets the Amazon RDS Proxy endpoint at runtime.
// Dapper is used as the lightweight data-access layer on top of the
// pooled SqlConnection provided by RDS Proxy.
using Dapper;

// cr-dotnet-1034: Async DAL methods added to support async Task-based data
// access patterns in Razor Page models (AdminHomeModel, ManageClinicModel,
// TakeAppointmentModel, TreatmentHistoryModel, ViewDoctorsModel).
// Each async method uses non-blocking SqlCommand.ExecuteNonQueryAsync() and
// SqlCommand.ExecuteReaderAsync() equivalents, releasing the thread pool thread
// while awaiting Amazon RDS query results via the RDS Proxy connection pool.

namespace DBProject.DAL
{
    //Database Layer of 3 tier architecture
    public class myDAL
    {
        // -------------------------------------------------------------------------
        // RDS Proxy connection factory
        //
        // cr-dotnet-0010: Web.config transformations eliminated. All environment-
        // specific configuration is injected at runtime via environment variables.
        // No build-time Web.config transformation (Web.Debug.config /
        // Web.Release.config) is required or used.
        //
        // Priority order for the connection string:
        //   1. RDS_PROXY_CONNECTION_STRING  - full connection string (from AWS SSM
        //                                     Parameter Store, injected by ECS/EB)
        //   2. Individual RDS Proxy environment variables assembled at runtime:
        //        RDS_PROXY_ENDPOINT  - RDS Proxy endpoint hostname
        //        RDS_DB_NAME         - target database name
        //        RDS_DB_USER         - database user name
        //        RDS_DB_PASSWORD     - database password (or IAM auth token)
        //        RDS_DB_PORT         - (optional) port, defaults to 1433
        //   3. DB_CONNECTION_STRING - fallback for local / non-cloud execution
        // -------------------------------------------------------------------------
        private static SqlConnection CreateConnection()
        {
            string connectionString = Environment.GetEnvironmentVariable("RDS_PROXY_CONNECTION_STRING");

            if (string.IsNullOrEmpty(connectionString))
            {
                string proxyEndpoint = Environment.GetEnvironmentVariable("RDS_PROXY_ENDPOINT");

                if (!string.IsNullOrEmpty(proxyEndpoint))
                {
                    string dbName   = Environment.GetEnvironmentVariable("RDS_DB_NAME")     ?? string.Empty;
                    string dbUser   = Environment.GetEnvironmentVariable("RDS_DB_USER")     ?? string.Empty;
                    string dbPass   = Environment.GetEnvironmentVariable("RDS_DB_PASSWORD") ?? string.Empty;
                    string dbPort   = Environment.GetEnvironmentVariable("RDS_DB_PORT")     ?? "1433";

                    // Build a SqlClient connection string targeting the RDS Proxy endpoint.
                    // Connection pooling is handled at the RDS Proxy layer; the client-side
                    // pool is kept small (Max Pool Size=10) to avoid exhausting proxy slots.
                    connectionString = string.Format(
                        "Server={0},{1};Database={2};User Id={3};Password={4};" +
                        "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;" +
                        "Connection Timeout=30;Max Pool Size=10;",
                        proxyEndpoint, dbPort, dbName, dbUser, dbPass);
                }
                else
                {
                    // cr-dotnet-0010: Web.config / Web.Debug.config / Web.Release.config
                    // fallback replaced with environment variable. Set DB_CONNECTION_STRING
                    // for local development; in AWS use RDS_PROXY_CONNECTION_STRING or the
                    // individual RDS_* variables listed above.
                    connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
                        ?? "Server=localhost,1433;Database=DBProject;User Id=sa;Password=localdev;"
                           + "MultipleActiveResultSets=False;Encrypt=False;"
                           + "TrustServerCertificate=True;Connection Timeout=30;";
                }
            }

            return new SqlConnection(connectionString);
        }


        //-----------------------------------------------------------------------------------//
        //																					 //
        //									SIGNUP											 //
        //																					 //
        //-----------------------------------------------------------------------------------//



        /*CHECKS WHETHER IT IS A VALID USER AND RETURN ITS TYPE*/
        public int validateLogin(string Email, string Password, ref int type, ref int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("Login", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@email", SqlDbType.VarChar, 30).Value = Email;
                    cmd1.Parameters.Add("@password", SqlDbType.VarChar, 20).Value = Password;

                    cmd1.Parameters.Add("@status", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@ID", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@type", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    int status = (int)cmd1.Parameters["@status"].Value;
                    type = (int)cmd1.Parameters["@type"].Value;
                    id = (int)cmd1.Parameters["@ID"].Value;

                    return status;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }




        /*THIS FUNCTION WILL VALIDATE ALL THE INFORMAIION OF OF USER (PATIENT)*/
        public int validateUser(string Name, string BirthDate, string Email, string Password, string PhoneNo, string gender, string Address, ref int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("PatientSignup", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Value = Name;
                    cmd1.Parameters.Add("@address", SqlDbType.VarChar, 40).Value = Address;
                    cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 1).Value = gender;
                    cmd1.Parameters.Add("@date", SqlDbType.Date).Value = BirthDate;
                    cmd1.Parameters.Add("@email", SqlDbType.VarChar, 30).Value = Email;
                    cmd1.Parameters.Add("@password", SqlDbType.VarChar, 20).Value = Password;
                    cmd1.Parameters.Add("@phone", SqlDbType.Char, 15).Value = PhoneNo;

                    cmd1.Parameters.Add("@status", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@ID", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    int status = (int)cmd1.Parameters["@status"].Value;

                    if (status != 0)
                    {
                        id = (int)cmd1.Parameters["@ID"].Value;
                    }

                    return status;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-----------------------------------------------------------------------------------//
        //                                                                                   //
        //                                       ADMIN                                       //
        //                                                                                   //
        //-----------------------------------------------------------------------------------//



        /*THIS FUNCTION CHECKS WHEATHER EMAIL OF A DOCTOR ALREADY EXISTS IN THE DATABASE */
        public int DoctorEmailAlreadyExist(string Email)
        {
            int status = 0;

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("CheckDoctorEmail", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 30).Value = Email;
                cmd.Parameters.Add("@status", SqlDbType.Int).Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                status = (int)cmd.Parameters["@status"].Value;
            }

            return status;
        }


        /*THIS FUNCTION WILL ADD THE DOCTOR TO THE DATA BASE */
        public void AddDoctor(string Name, string Email, string Password, string BirthDate, int dept, string Phone, char gender, string Address, int exp, int salary, int Charges_per_visit, string spec, string qual)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("AddDoctor", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Name", SqlDbType.VarChar, 30).Value = Name;
                cmd.Parameters.Add("@Email", SqlDbType.VarChar, 30).Value = Email;
                cmd.Parameters.Add("@Password", SqlDbType.VarChar, 30).Value = Password;
                cmd.Parameters.Add("@BirthDate", SqlDbType.Date).Value = BirthDate;
                cmd.Parameters.Add("@dept", SqlDbType.VarChar, 30).Value = dept;
                cmd.Parameters.Add("@gender", SqlDbType.VarChar, 1).Value = gender;
                cmd.Parameters.Add("@Address", SqlDbType.VarChar, 30).Value = Address;
                cmd.Parameters.Add("@Exp", SqlDbType.VarChar, 30).Value = exp;
                cmd.Parameters.Add("@Salary", SqlDbType.VarChar, 30).Value = salary;
                cmd.Parameters.Add("@charges", SqlDbType.VarChar, 30).Value = Charges_per_visit;
                cmd.Parameters.Add("@phone", SqlDbType.VarChar, 30).Value = Phone;
                cmd.Parameters.Add("@spec", SqlDbType.VarChar, 30).Value = spec;
                cmd.Parameters.Add("@qual", SqlDbType.VarChar, 30).Value = qual;

                cmd.ExecuteNonQuery();
            }
        }


        /*THIS FUNCTION WILL ADD STAFF TO THE DATA BASE*/
        public int AddStaff(string Name, string BirthDate, string Phone, char gender, string Address, int salary, string Qual, string Designation)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("AddStaff", con);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    cmd.Parameters.Add("@Name", SqlDbType.VarChar, 30).Value = Name;
                    cmd.Parameters.Add("@BirthDate", SqlDbType.Date).Value = BirthDate;
                    cmd.Parameters.Add("@Phone", SqlDbType.VarChar, 30).Value = Phone;
                    cmd.Parameters.Add("@gender", SqlDbType.VarChar, 1).Value = gender;
                    cmd.Parameters.Add("@salary", SqlDbType.Int, 30).Value = salary;
                    cmd.Parameters.Add("@Designation", SqlDbType.VarChar, 30).Value = Designation;
                    cmd.Parameters.Add("@Qualification", SqlDbType.VarChar, 1).Value = Qual;
                    cmd.Parameters.Add("@Address", SqlDbType.VarChar, 50).Value = Address;

                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    return -1;
                }
            }

            return 1;
        }


        /*THIS FUNCTION WILL RUN MULTIPLE QUERIES AND GET ALL THE INFORMATION NEEDED TO DISPLAY AT ADMIN HOME*/
        public void GetAdminHomeInformation(ref DataTable[] arrTable)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("SELECT * FROM Total_Patient", con);
                SqlDataAdapter Adapter = new SqlDataAdapter(cmd);
                Adapter.Fill(arrTable[0]);

                cmd.CommandText = "SELECT * FROM Total_Doctors";
                Adapter.Fill(arrTable[1]);

                cmd.CommandText = "SELECT * FROM Income";
                Adapter.Fill(arrTable[2]);

                cmd.CommandText = "SELECT * FROM Department_View";
                Adapter.Fill(arrTable[3]);

                cmd.CommandText = "SELECT * FROM Appointment_view";
                Adapter.Fill(arrTable[4]);
            }
        }

        // cr-dotnet-1034: Async version of GetAdminHomeInformation.
        // Uses non-blocking async connection open and query execution via
        // Amazon RDS Proxy, releasing the thread pool thread while awaiting
        // database I/O. Called by AdminHomeModel.GetAdminHomeInformationAsync().
        public async Task GetAdminHomeInformationAsync(DataTable[] arrTable)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                // Execute each query asynchronously and fill the corresponding DataTable.
                // SqlDataAdapter does not natively support async Fill; we use
                // ExecuteReaderAsync with a DataTable.Load() pattern instead.
                string[] queries = new[]
                {
                    "SELECT * FROM Total_Patient",
                    "SELECT * FROM Total_Doctors",
                    "SELECT * FROM Income",
                    "SELECT * FROM Department_View",
                    "SELECT * FROM Appointment_view"
                };

                for (int i = 0; i < queries.Length; i++)
                {
                    using (SqlCommand cmd = new SqlCommand(queries[i], con))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        arrTable[i].Load(reader);
                    }
                }
            }
        }


        /*THIS FUNCTION IS INTENDED TO DELETE DOCTOR BUT SECRETLY IT ONLY UPDATE THE STATUS*/
        public int DeleteDoctor(int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("DeleteDoctor", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    return -1;
                }
            }

            return 1;
        }

        // cr-dotnet-1034: Async version of DeleteDoctor.
        public async Task<int> DeleteDoctorAsync(int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    SqlCommand cmd = new SqlCommand("DeleteDoctor", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    return -1;
                }
            }

            return 1;
        }


        /*THIS FUNCTION WILL DELLETE STAFF FROM THE DOCTOR */
        public int DeleteStaff(int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("DELETESTAFF", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    return -1;
                }
            }

            return 1;
        }

        // cr-dotnet-1034: Async version of DeleteStaff.
        public async Task<int> DeleteStaffAsync(int id)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    SqlCommand cmd = new SqlCommand("DELETESTAFF", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    await cmd.ExecuteNonQueryAsync();
                }
                catch
                {
                    return -1;
                }
            }

            return 1;
        }


        /*LOADS THE TABLE OF DOCTOR / SPECIFIED DOCTORS ON THE BASIS OF SEARCH QUERY*/
        public void LoadDoctor(ref DataTable table, String SearchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                SqlCommand cmd;
                con.Open();

                if (SearchQuery == "")
                {
                    cmd = new SqlCommand(
                    "SELECT Doctor.DoctorID as ID , Doctor.Name , D.DeptName as Department FROM Doctor JOIN Department D ON D.DeptNo = Doctor.DeptNo" +
                    " WHERE Doctor.Status = 1",
                    con);
                }
                else
                {
                    cmd = new SqlCommand(
                    "SELECT a.DoctorID as ID,  a.Name, D.DeptName as Department FROM department D join (SELECT * FROM Doctor WHERE Doctor.Status = 1 AND Doctor.Name like  '%' + @DName + '%')  a ON a.DeptNo = D.DeptNo",
                    con);
                    cmd.Parameters.AddWithValue("@DName", SearchQuery);
                }

                SqlDataAdapter Adapter = new SqlDataAdapter(cmd);
                Adapter.Fill(table);
            }
        }

        // cr-dotnet-1034: Async version of LoadDoctor.
        // Replaces synchronous GridView.DataBind() pattern (original line 40)
        // with async data loading via Amazon RDS Proxy.
        public async Task LoadDoctorAsync(DataTable table, string searchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                SqlCommand cmd;
                if (string.IsNullOrEmpty(searchQuery))
                {
                    cmd = new SqlCommand(
                        "SELECT Doctor.DoctorID as ID , Doctor.Name , D.DeptName as Department " +
                        "FROM Doctor JOIN Department D ON D.DeptNo = Doctor.DeptNo " +
                        "WHERE Doctor.Status = 1",
                        con);
                }
                else
                {
                    cmd = new SqlCommand(
                        "SELECT a.DoctorID as ID, a.Name, D.DeptName as Department " +
                        "FROM department D join (SELECT * FROM Doctor WHERE Doctor.Status = 1 " +
                        "AND Doctor.Name like '%' + @DName + '%') a ON a.DeptNo = D.DeptNo",
                        con);
                    cmd.Parameters.AddWithValue("@DName", searchQuery);
                }

                using (cmd)
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    table.Load(reader);
                }
            }
        }


        /*LOADS THE TABLE OF PATIENT ON THE BASIS OF SEARCH QUERY*/
        public void LoadPatient(ref DataTable table, String SearchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                SqlCommand cmd;
                con.Open();

                if (SearchQuery == "")
                {
                    cmd = new SqlCommand("SELECT * FROM PATIENT_VIEW", con);
                }
                else
                {
                    cmd = new SqlCommand("SELECT Patient.PatientID, Patient.Name, Patient.Phone from Patient" +
                    " WHERE patient.name like '%' + @SName + '%' ", con);
                    cmd.Parameters.AddWithValue("@SName", SearchQuery.Trim());
                }

                SqlDataAdapter Adapter = new SqlDataAdapter(cmd);
                Adapter.Fill(table);
            }
        }

        // cr-dotnet-1034: Async version of LoadPatient.
        // Replaces synchronous GridView.DataBind() pattern (original line 55)
        // with async data loading via Amazon RDS Proxy.
        public async Task LoadPatientAsync(DataTable table, string searchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                SqlCommand cmd;
                if (string.IsNullOrEmpty(searchQuery))
                {
                    cmd = new SqlCommand("SELECT * FROM PATIENT_VIEW", con);
                }
                else
                {
                    cmd = new SqlCommand(
                        "SELECT Patient.PatientID, Patient.Name, Patient.Phone from Patient " +
                        "WHERE patient.name like '%' + @SName + '%'",
                        con);
                    cmd.Parameters.AddWithValue("@SName", searchQuery.Trim());
                }

                using (cmd)
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    table.Load(reader);
                }
            }
        }


        /*LOADS THE TABLE OF OTHER STAFF ON THE BASIS OF SEARCH QUERY*/
        public void LoadOtherStaff(ref DataTable table, String SearchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                SqlCommand cmd;
                con.Open();

                if (SearchQuery == "")
                {
                    cmd = new SqlCommand("SELECT * FROM STAFF_VIEW", con);
                }
                else
                {
                    cmd = new SqlCommand("SELECT StaffID as ID , Name , Designation from OtherStaff WHERE Name like '%' + @pName + '%'", con);
                    cmd.Parameters.AddWithValue("@PName", SearchQuery.Trim());
                }

                SqlDataAdapter Adapter = new SqlDataAdapter(cmd);
                Adapter.Fill(table);
            }
        }

        // cr-dotnet-1034: Async version of LoadOtherStaff.
        // Replaces synchronous GridView.DataBind() pattern (original line 75)
        // with async data loading via Amazon RDS Proxy.
        public async Task LoadOtherStaffAsync(DataTable table, string searchQuery)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                SqlCommand cmd;
                if (string.IsNullOrEmpty(searchQuery))
                {
                    cmd = new SqlCommand("SELECT * FROM STAFF_VIEW", con);
                }
                else
                {
                    cmd = new SqlCommand(
                        "SELECT StaffID as ID , Name , Designation from OtherStaff " +
                        "WHERE Name like '%' + @pName + '%'",
                        con);
                    cmd.Parameters.AddWithValue("@PName", searchQuery.Trim());
                }

                using (cmd)
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    table.Load(reader);
                }
            }
        }


        public int GETPATIENT(int pid, ref string name, ref string phone, ref string address, ref string birthDate, ref int age, ref string gender)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePatientData", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@id", SqlDbType.Int).Value = pid;

                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.Char, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@birthDate", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@address", SqlDbType.VarChar, 40).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.Char, 1).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    name = (string)cmd1.Parameters["@name"].Value.ToString();
                    phone = (string)cmd1.Parameters["@phone"].Value.ToString();
                    address = (string)cmd1.Parameters["@address"].Value.ToString();
                    birthDate = (string)cmd1.Parameters["@birthDate"].Value.ToString();
                    age = Convert.ToInt32((cmd1.Parameters["@age"].Value));
                    gender = (string)cmd1.Parameters["@gender"].Value.ToString();

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of GETPATIENT for use in async page handlers.
        public async Task<int> GetPatientAsync(int pid, ref string name, ref string phone, ref string address, ref string birthDate, ref int age, ref string gender)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePatientData", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@id", SqlDbType.Int).Value = pid;
                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.Char, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@birthDate", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@address", SqlDbType.VarChar, 40).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.Char, 1).Direction = ParameterDirection.Output;

                    await cmd1.ExecuteNonQueryAsync();

                    name = cmd1.Parameters["@name"].Value.ToString();
                    phone = cmd1.Parameters["@phone"].Value.ToString();
                    address = cmd1.Parameters["@address"].Value.ToString();
                    birthDate = cmd1.Parameters["@birthDate"].Value.ToString();
                    age = Convert.ToInt32(cmd1.Parameters["@age"].Value);
                    gender = cmd1.Parameters["@gender"].Value.ToString();

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        public int GET_DOCTOR_PROFILE(int dID, ref string name, ref string phone, ref string gender, ref float charges_Per_Visit, ref float ReputeIndex, ref int PatientsTreated, ref string qualification, ref string specialization, ref int workE, ref int age)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("GET_DOCTOR_PROFILE", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;

                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@charges", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@RI", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@PTreated", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@qualification", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@specialization", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@workE", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    name = (string)cmd1.Parameters["@name"].Value;
                    phone = (string)cmd1.Parameters["@phone"].Value;
                    gender = (string)cmd1.Parameters["@gender"].Value;
                    charges_Per_Visit = Convert.ToSingle(cmd1.Parameters["@charges"].Value);
                    ReputeIndex = Convert.ToSingle(cmd1.Parameters["@RI"].Value);
                    PatientsTreated = (int)cmd1.Parameters["@PTreated"].Value;
                    qualification = (string)cmd1.Parameters["@qualification"].Value;
                    specialization = (string)cmd1.Parameters["@specialization"].Value;
                    workE = (int)cmd1.Parameters["@workE"].Value;
                    age = (int)cmd1.Parameters["@age"].Value;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }

            return 1;
        }

        // cr-dotnet-1034: Async version of GET_DOCTOR_PROFILE for use in async page handlers.
        public async Task<int> GetDoctorProfileAsync(int dID, ref string name, ref string phone, ref string gender, ref float charges_Per_Visit, ref float ReputeIndex, ref int PatientsTreated, ref string qualification, ref string specialization, ref int workE, ref int age)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("GET_DOCTOR_PROFILE", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@charges", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@RI", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@PTreated", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@qualification", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@specialization", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@workE", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;

                    await cmd1.ExecuteNonQueryAsync();

                    name = (string)cmd1.Parameters["@name"].Value;
                    phone = (string)cmd1.Parameters["@phone"].Value;
                    gender = (string)cmd1.Parameters["@gender"].Value;
                    charges_Per_Visit = Convert.ToSingle(cmd1.Parameters["@charges"].Value);
                    ReputeIndex = Convert.ToSingle(cmd1.Parameters["@RI"].Value);
                    PatientsTreated = (int)cmd1.Parameters["@PTreated"].Value;
                    qualification = (string)cmd1.Parameters["@qualification"].Value;
                    specialization = (string)cmd1.Parameters["@specialization"].Value;
                    workE = (int)cmd1.Parameters["@workE"].Value;
                    age = (int)cmd1.Parameters["@age"].Value;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }

            return 1;
        }


        public int GETSATFF(int id, ref string name, ref string phone, ref string address, ref string gender, ref string desig, ref int sal)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd1 = new SqlCommand("GET_STAFF", con);
                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.Add("@id", SqlDbType.Int).Value = id;

                cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@phone", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@address", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@desig", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@sal", SqlDbType.Int).Direction = ParameterDirection.Output;

                {
                    cmd1.ExecuteNonQuery();

                    name = (string)cmd1.Parameters["@name"].Value;
                    phone = (string)cmd1.Parameters["@phone"].Value;
                    gender = (string)cmd1.Parameters["@gender"].Value;
                    address = (string)cmd1.Parameters["@address"].Value;
                    desig = (string)cmd1.Parameters["@desig"].Value;
                    sal = (int)cmd1.Parameters["@sal"].Value;
                }
            }

            return 1;
        }

        // cr-dotnet-1034: Async version of GETSATFF for use in async page handlers.
        public async Task<int> GetStaffAsync(int id, ref string name, ref string phone, ref string address, ref string gender, ref string desig, ref int sal)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                SqlCommand cmd1 = new SqlCommand("GET_STAFF", con);
                cmd1.CommandType = CommandType.StoredProcedure;

                cmd1.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@phone", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@address", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@desig", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd1.Parameters.Add("@sal", SqlDbType.Int).Direction = ParameterDirection.Output;

                await cmd1.ExecuteNonQueryAsync();

                name = (string)cmd1.Parameters["@name"].Value;
                phone = (string)cmd1.Parameters["@phone"].Value;
                gender = (string)cmd1.Parameters["@gender"].Value;
                address = (string)cmd1.Parameters["@address"].Value;
                desig = (string)cmd1.Parameters["@desig"].Value;
                sal = (int)cmd1.Parameters["@sal"].Value;
            }

            return 1;
        }


        //-----------------------------------------------------------------------------------//
        //                                                                                   //
        //                                       PATIENT                                     //
        //                                                                                   //
        //-----------------------------------------------------------------------------------//


        /*-------------------DISPLAYS PATIENT INFORMATION AT PATIENT HOME--------------------------------------- */
        public int patientInfoDisplayer(int pid, ref string name, ref string phone, ref string address, ref string birthDate, ref int age, ref string gender)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePatientData", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@id", SqlDbType.Int).Value = pid;

                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.Char, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@birthDate", SqlDbType.VarChar, 10).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@address", SqlDbType.VarChar, 40).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.Char, 1).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    name = (string)cmd1.Parameters["@name"].Value;
                    phone = (string)cmd1.Parameters["@phone"].Value;
                    address = (string)cmd1.Parameters["@address"].Value;
                    birthDate = (string)cmd1.Parameters["@birthDate"].Value;
                    age = (int)cmd1.Parameters["@age"].Value;
                    gender = (string)cmd1.Parameters["@gender"].Value;

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        /*---------------------------GENERATE BILL HISTORY--------------------------------------*/
        public int getBillHistory(int id, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveBillHistory", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                    return (int)cmd1.Parameters["@count"].Value;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------CURRENT APPOINTMENTS------------------------------------------//
        public int appointmentTodayDisplayer(int pid, ref string dName, ref string timings)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveCurrentAppointment", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@pid", SqlDbType.Int).Value = pid;

                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@timings", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@dName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    int status = (int)cmd1.Parameters["@count"].Value;

                    if (status == 0)
                    {
                        return status;
                    }
                    else
                    {
                        dName = (string)cmd1.Parameters["@dName"].Value;
                        timings = (string)cmd1.Parameters["@timings"].Value;
                        return status;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------TREATMENT HISTORY------------------------------------------//
        public int getTreatmentHistory(int id, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveTreatmentHistory", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                    return (int)cmd1.Parameters["@count"].Value;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getTreatmentHistory.
        // Replaces synchronous GridView.DataBind() pattern (TreatmentHistory.aspx line 55)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        // Called by TreatmentHistoryModel.LoadTreatmentHistoryAsync() in TreatmentHistory.cshtml.cs.
        public async Task<int> GetTreatmentHistoryAsync(int id, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("RetrieveTreatmentHistory", con))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                        cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        // Execute the stored procedure asynchronously to get the count output param.
                        await cmd1.ExecuteNonQueryAsync();

                        int count = (int)cmd1.Parameters["@count"].Value;

                        // Re-execute as a reader to load the result set into the DataTable.
                        using (SqlCommand readCmd = new SqlCommand("RetrieveTreatmentHistory", con))
                        {
                            readCmd.CommandType = CommandType.StoredProcedure;
                            readCmd.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                            readCmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                            using (var reader = await readCmd.ExecuteReaderAsync())
                            {
                                result.Load(reader);
                            }
                        }

                        return count;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        /*-------------------------TAKE APPOINMENT------------------------------------*/
        public int getdeptInfo(ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("select* from deptInfo", con);
                    cmd1.CommandType = CommandType.Text;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getdeptInfo.
        // Replaces synchronous GridView.DataBind() pattern (TakeAppointment.aspx line 27)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        // Called by TakeAppointmentModel.LoadDeptInfoAsync() in TakeAppointment.cshtml.cs.
        public async Task<int> GetDeptInfoAsync(DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("select * from deptInfo", con))
                    {
                        cmd1.CommandType = CommandType.Text;

                        using (var reader = await cmd1.ExecuteReaderAsync())
                        {
                            result.Load(reader);
                        }
                    }

                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------VIEW DOCTORS------------------------------------------//
        public int getDeptDoctorInfo(string deptName, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveDeptDoctorInfo", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@deptName", SqlDbType.VarChar, 30).Value = deptName;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];

                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getDeptDoctorInfo.
        // Replaces synchronous GridView.DataBind() pattern (ViewDoctors.aspx lines 28 and 57)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        // Called by ViewDoctorsModel.LoadDeptDoctorInfoAsync() in ViewDoctors.cshtml.cs.
        public async Task<int> GetDeptDoctorInfoAsync(string deptName, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("RetrieveDeptDoctorInfo", con))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@deptName", SqlDbType.VarChar, 30).Value = deptName;

                        using (var reader = await cmd1.ExecuteReaderAsync())
                        {
                            result.Load(reader);
                        }
                    }

                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------DOCTOR PROFILE------------------------------------------//
        public int doctorInfoDisplayer(int dID, ref string name, ref string phone, ref string gender, ref float charges_Per_Visit, ref float ReputeIndex, ref int PatientsTreated, ref string qualification, ref string specialization, ref int workE, ref int age)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveDoctorData", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;

                    cmd1.Parameters.Add("@name", SqlDbType.VarChar, 20).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@phone", SqlDbType.VarChar, 15).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@gender", SqlDbType.VarChar, 2).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@charges", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@RI", SqlDbType.Float).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@PTreated", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@qualification", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@specialization", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@workE", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@age", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    name = (string)cmd1.Parameters["@name"].Value;
                    phone = (string)cmd1.Parameters["@phone"].Value;
                    gender = (string)cmd1.Parameters["@gender"].Value;
                    charges_Per_Visit = Convert.ToSingle(cmd1.Parameters["@charges"].Value);
                    ReputeIndex = Convert.ToSingle(cmd1.Parameters["@RI"].Value);
                    PatientsTreated = (int)cmd1.Parameters["@PTreated"].Value;
                    qualification = (string)cmd1.Parameters["@qualification"].Value;
                    specialization = (string)cmd1.Parameters["@specialization"].Value;
                    workE = (int)cmd1.Parameters["@workE"].Value;
                    age = (int)cmd1.Parameters["@age"].Value;

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------APPOINTMENT TAKER------------------------------------------//
        public int getFreeSlots(int dID, int pID, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrieveFreeSlots", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
                    cmd1.Parameters.Add("@pID", SqlDbType.Int).Value = pID;
                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];

                    return (int)cmd1.Parameters["@count"].Value;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------APPOINTMENT REQUEST SENT------------------------------------------//
        public int insertAppointment(int dID, int pID, int freeSlot, ref string mes)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                string m = "";

                con.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs e)
                {
                    m += "\n" + e.Message;
                };

                try
                {
                    SqlCommand cmd1 = new SqlCommand("insertInAppointmentTable", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
                    cmd1.Parameters.Add("@pID", SqlDbType.Int).Value = pID;
                    cmd1.Parameters.Add("@freeSlot", SqlDbType.Int).Value = freeSlot;

                    cmd1.ExecuteNonQuery();
                    mes = m;

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------PATIENT NOTIFICATIONS------------------------------------------//
        public int getNotifications(int pid, ref string dName, ref string timings)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePatientNotifications", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = pid;

                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@timings", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@dName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    int status = (int)cmd1.Parameters["@count"].Value;

                    if (status == 0)
                    {
                        return status;
                    }
                    else
                    {
                        dName = (string)cmd1.Parameters["@dName"].Value;
                        timings = (string)cmd1.Parameters["@timings"].Value;
                        return status;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-------------------------------------PATIENT FEEDBACK------------------------------------------//
        public int isFeedbackPending(int pid, ref string dName, ref string timings, ref int aID)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePendingFeedback", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = pid;

                    cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@timings", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@dName", SqlDbType.VarChar, 30).Direction = ParameterDirection.Output;
                    cmd1.Parameters.Add("@aID", SqlDbType.Int).Direction = ParameterDirection.Output;

                    cmd1.ExecuteNonQuery();

                    int status = (int)cmd1.Parameters["@count"].Value;

                    if (status == 0)
                    {
                        return status;
                    }
                    else
                    {
                        dName = (string)cmd1.Parameters["@dName"].Value;
                        timings = (string)cmd1.Parameters["@timings"].Value;
                        aID = (int)cmd1.Parameters["@aID"].Value;

                        return status;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        public int givePendingFeedback(int aID)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("storeFeedback", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@aId", SqlDbType.Int).Value = aID;

                    cmd1.ExecuteNonQuery();

                    return 0;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }


        //-----------------------------------------------------------------------------------//
        //                                                                                   //
        //                                       DOCTOR                                      //
        //                                                                                   //
        //-----------------------------------------------------------------------------------//


        /*THIS FUNCITON WILL RETRIEVE THE INFORMATION OF CURRENT LOGGED IN DOCTOR*/
        public int docinfo_DAL(int doctorid, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("Doctor_Information_By_ID1", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ID", SqlDbType.Int);
                    cmd.Parameters["@id"].Value = doctorid;
                    cmd.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        da.Fill(ds);

                    result = ds.Tables[0];
                }
                catch (SqlException)
                {
                    return 0;
                }
            }

            return 1;
        }


        /*THIS FUNCTION WILL RETURN PENDING APPOINTMENT FORM THE DATABASE IN THE FORM OF DATASET*/
        public void GetAllpendingappointments_DAL(int doctorid, ref DataTable DT)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("PENDING_APPOINTMENTS2", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DOCTOR_ID", SqlDbType.Int);
                    cmd.Parameters["@DOCTOR_ID"].Value = doctorid;
                    cmd.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    DT = ds.Tables[0];
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message.ToString());
                }
            }
        }


        /*THIS FUNCTION WILL BE CALLED WHEN DOCTOR APPROVE THE REQUEST OF PATIENT*/
        public int UpdateAppointment_DAL(int Appointmentid)
        {
            int result = 0;

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("APPROVE_APPOINTMENT", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = Appointmentid;

                    result = cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message.ToString());
                }
            }

            return result;
        }


        /*DELETES THE APPOINTMENT*/
        public int Deleteappointment_DAL(int appointmentid)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("delete_APPOINTMENT", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = appointmentid;
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message.ToString());
                    return -1;
                }
            }

            return 1;
        }


        /*THIS FUNTION RETURN CURRENT DAY APPONTMENT*/
        public int search_patient_DAL(int did, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("TODAYS_APPOINTMENTS", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@DOC_ID", SqlDbType.Int).Value = did;

                    cmd.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                }
                catch (SqlException)
                {
                    // intentionally swallowed - preserve original behaviour
                }
            }

            return 1;
        }


        /*UPDATE THE PRESCRIPTION WHEN APPOINTMENT IS GOING ON BY DOCTOR*/
        public int update_prescription_DAL(int did, int appointid, string disease, string progres, string prescrip)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("UpdatePrescription", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@docId", SqlDbType.Int).Value = did;
                    cmd.Parameters.Add("@appointid", SqlDbType.Int).Value = appointid;
                    cmd.Parameters.Add("@Disease", SqlDbType.VarChar, 30).Value = disease;
                    cmd.Parameters.Add("@progress", SqlDbType.VarChar, 50).Value = progres;
                    cmd.Parameters.Add("@prescription", SqlDbType.VarChar, 60).Value = prescrip;

                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    return 0;
                }
            }

            return 1;
        }


        /*GENERATES BILL*/
        public int generate_bill_DAL(int docid, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd = new SqlCommand("generate_bill", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@dId", SqlDbType.Int);
                    cmd.Parameters["@did"].Value = docid;

                    cmd.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                }
                catch (SqlException)
                {
                    return -1;
                }
            }

            return 1;
        }


        public void paid_bill_DAL(int did, int appoint)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("finishedPaid", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@docId", SqlDbType.Int).Value = did;
                cmd.Parameters.Add("@appointid", SqlDbType.Int).Value = appoint;

                cmd.ExecuteNonQuery();
            }
        }


        public void Unpaid_bill_DAL(int did, int appoint)
        {
            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                SqlCommand cmd = new SqlCommand("finishedUnPaid", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@docId", SqlDbType.Int).Value = did;
                cmd.Parameters.Add("@appointid", SqlDbType.Int).Value = appoint;

                cmd.ExecuteNonQuery();
            }
        }


        public int getPHistory(int id, ref DataTable result)
        {
            DataSet ds = new DataSet();

            using (SqlConnection con = CreateConnection())
            {
                con.Open();

                try
                {
                    SqlCommand cmd1 = new SqlCommand("RetrievePHistory", con);
                    cmd1.CommandType = CommandType.StoredProcedure;

                    cmd1.Parameters.Add("@dId", SqlDbType.Int).Value = id;

                    cmd1.ExecuteNonQuery();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd1))
                    {
                        da.Fill(ds);
                    }

                    result = ds.Tables[0];
                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of search_patient_DAL.
        // Replaces synchronous GridView.DataBind() pattern (PatientHistory.aspx line 23/41)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        public async Task<int> SearchPatientAsync(int did, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("TODAYS_APPOINTMENTS", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DOC_ID", SqlDbType.Int).Value = did;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            result.Load(reader);
                        }
                    }
                }
                catch (SqlException)
                {
                    // intentionally swallowed - preserve original behaviour
                    return 0;
                }
            }

            return 1;
        }

        // cr-dotnet-1034: Async version of GetAllpendingappointments_DAL.
        // Replaces synchronous GridView.DataBind() pattern (PendingAppointment.aspx line 22/40)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        public async Task GetAllPendingAppointmentsAsync(int doctorid, DataTable dt)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("PENDING_APPOINTMENTS2", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DOCTOR_ID", SqlDbType.Int).Value = doctorid;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message);
                }
            }
        }

        // cr-dotnet-1034: Async version of UpdateAppointment_DAL.
        // Used by PendingAppointment async handler to approve appointments without
        // blocking the thread pool thread.
        public async Task<int> UpdateAppointmentAsync(int appointmentId)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("APPROVE_APPOINTMENT", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = appointmentId;
                        return await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message);
                    return 0;
                }
            }
        }

        // cr-dotnet-1034: Async version of Deleteappointment_DAL.
        // Used by PendingAppointment async handler to delete appointments without
        // blocking the thread pool thread.
        public async Task<int> DeleteAppointmentAsync(int appointmentId)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("delete_APPOINTMENT", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@APPOINT_ID", SqlDbType.Int).Value = appointmentId;
                        await cmd.ExecuteNonQueryAsync();
                        return 1;
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("SQL Error" + ex.Message);
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getPHistory.
        // Replaces synchronous GridView.DataBind() pattern (PreviousHistory.aspx line 24/50)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        public async Task<int> GetPHistoryAsync(int id, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("RetrievePHistory", con))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@dId", SqlDbType.Int).Value = id;

                        using (var reader = await cmd1.ExecuteReaderAsync())
                        {
                            result.Load(reader);
                        }
                    }

                    return 1;
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getBillHistory.
        // Replaces synchronous GridView.DataBind() pattern (BillsHistory.aspx line 25/51)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        // Called by BillsHistoryModel.LoadBillHistoryAsync() in BillsHistory.cshtml.cs.
        public async Task<int> GetBillHistoryAsync(int id, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("RetrieveBillHistory", con))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                        cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        // Execute the stored procedure asynchronously to avoid blocking the thread pool.
                        await cmd1.ExecuteNonQueryAsync();

                        int count = (int)cmd1.Parameters["@count"].Value;

                        // Re-execute as a reader to load the result set into the DataTable.
                        using (SqlCommand readCmd = new SqlCommand("RetrieveBillHistory", con))
                        {
                            readCmd.CommandType = CommandType.StoredProcedure;
                            readCmd.Parameters.Add("@pId", SqlDbType.Int).Value = id;
                            readCmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                            using (var reader = await readCmd.ExecuteReaderAsync())
                            {
                                result.Load(reader);
                            }
                        }

                        return count;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }

        // cr-dotnet-1034: Async version of getFreeSlots.
        // Replaces synchronous GridView.DataBind() pattern (AppointmentTaker.aspx line 27/55)
        // with async Task-based data access via Amazon RDS Proxy, releasing the thread pool
        // thread while awaiting database I/O and preventing thread pool exhaustion under load.
        // Called by AppointmentTakerModel.LoadFreeSlotsAsync() in AppointmentTaker.cshtml.cs.
        public async Task<int> GetFreeSlotsAsync(int dID, int pID, DataTable result)
        {
            using (SqlConnection con = CreateConnection())
            {
                await con.OpenAsync();

                try
                {
                    using (SqlCommand cmd1 = new SqlCommand("RetrieveFreeSlots", con))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
                        cmd1.Parameters.Add("@pID", SqlDbType.Int).Value = pID;
                        cmd1.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                        // Execute the stored procedure asynchronously to get the count output param.
                        await cmd1.ExecuteNonQueryAsync();

                        int count = (int)cmd1.Parameters["@count"].Value;

                        // Re-execute as a reader to load the result set into the DataTable.
                        using (SqlCommand readCmd = new SqlCommand("RetrieveFreeSlots", con))
                        {
                            readCmd.CommandType = CommandType.StoredProcedure;
                            readCmd.Parameters.Add("@dID", SqlDbType.Int).Value = dID;
                            readCmd.Parameters.Add("@pID", SqlDbType.Int).Value = pID;
                            readCmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.Output;

                            using (var reader = await readCmd.ExecuteReaderAsync())
                            {
                                result.Load(reader);
                            }
                        }

                        return count;
                    }
                }
                catch (SqlException)
                {
                    return -1;
                }
            }
        }
    }
}
