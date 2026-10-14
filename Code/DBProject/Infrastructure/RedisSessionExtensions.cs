using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

// cr-dotnet-0045 remediation: Amazon ElastiCache for Redis distributed session helper.
// Replaces in-process HttpContext.Session (InProc) integer and string helpers with
// IDistributedCache operations backed by Amazon ElastiCache for Redis.  This enables
// stateless horizontal scaling across multiple ECS tasks or Kubernetes pods without
// server affinity.
//
// cr-dotnet-1034 remediation: Async overloads (SetInt32Async, SetStringAsync, GetInt32Async,
// GetStringAsync) added to support async Task-based page handlers in Razor Pages PageModels.
// These prevent thread pool exhaustion by using non-blocking IDistributedCache.SetAsync /
// GetAsync operations against Amazon ElastiCache for Redis.

namespace DBProject.Infrastructure
{
    /// <summary>
    /// Extension methods for <see cref="IDistributedCache"/> that provide typed integer
    /// and string session-value access backed by Amazon ElastiCache for Redis.
    /// These replace the ASP.NET Core in-process <c>HttpContext.Session.GetInt32</c> /
    /// <c>HttpContext.Session.SetInt32</c> / <c>HttpContext.Session.GetString</c> /
    /// <c>HttpContext.Session.SetString</c> calls that were flagged by rule cr-dotnet-0045.
    /// </summary>
    public static class RedisSessionExtensions
    {
        private static readonly DistributedCacheEntryOptions DefaultOptions =
            new DistributedCacheEntryOptions
            {
                // Session idle timeout – matches the default ASP.NET Core session timeout.
                // Override via REDIS_SESSION_IDLE_MINUTES environment variable if needed.
                SlidingExpiration = TimeSpan.FromMinutes(
                    int.TryParse(
                        Environment.GetEnvironmentVariable("REDIS_SESSION_IDLE_MINUTES"),
                        out int minutes) ? minutes : 20)
            };

        /// <summary>
        /// Reads an integer value from the Redis-backed distributed cache.
        /// Returns <c>null</c> when the key is absent (mirrors
        /// <c>ISession.GetInt32</c> semantics).
        /// </summary>
        public static int? GetInt32(this IDistributedCache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = cache.Get(key);
            if (data == null || data.Length == 0)
                return null;

            string raw = Encoding.UTF8.GetString(data);
            return int.TryParse(raw, out int value) ? value : (int?)null;
        }

        /// <summary>
        /// Writes an integer value to the Redis-backed distributed cache with a
        /// sliding expiration window.
        /// </summary>
        public static void SetInt32(this IDistributedCache cache, string key, int value)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = Encoding.UTF8.GetBytes(value.ToString());
            cache.Set(key, data, DefaultOptions);
        }

        /// <summary>
        /// cr-dotnet-1034: Async version of SetInt32.
        /// Writes an integer value to the Redis-backed distributed cache asynchronously,
        /// releasing the thread pool thread while awaiting the Redis I/O operation.
        /// Used by async Razor Page handlers (e.g. OnGetSelectAsync in PatientHistory.cshtml.cs).
        /// </summary>
        public static async Task SetInt32Async(this IDistributedCache cache, string key, int value)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = Encoding.UTF8.GetBytes(value.ToString());
            await cache.SetAsync(key, data, DefaultOptions);
        }

        /// <summary>
        /// cr-dotnet-1034: Async version of GetInt32.
        /// Reads an integer value from the Redis-backed distributed cache asynchronously,
        /// releasing the thread pool thread while awaiting the Redis I/O operation.
        /// Returns <c>null</c> when the key is absent.
        /// </summary>
        public static async Task<int?> GetInt32Async(this IDistributedCache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = await cache.GetAsync(key);
            if (data == null || data.Length == 0)
                return null;

            string raw = Encoding.UTF8.GetString(data);
            return int.TryParse(raw, out int value) ? value : (int?)null;
        }

        /// <summary>
        /// Reads a string value from the Redis-backed distributed cache.
        /// Returns <c>null</c> when the key is absent (mirrors
        /// <c>ISession.GetString</c> semantics).
        /// </summary>
        public static string GetString(this IDistributedCache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = cache.Get(key);
            if (data == null || data.Length == 0)
                return null;

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Writes a string value to the Redis-backed distributed cache with a
        /// sliding expiration window.
        /// </summary>
        public static void SetString(this IDistributedCache cache, string key, string value)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = Encoding.UTF8.GetBytes(value ?? string.Empty);
            cache.Set(key, data, DefaultOptions);
        }

        /// <summary>
        /// cr-dotnet-1034: Async version of SetString.
        /// Writes a string value to the Redis-backed distributed cache asynchronously,
        /// releasing the thread pool thread while awaiting the Redis I/O operation.
        /// </summary>
        public static async Task SetStringAsync(this IDistributedCache cache, string key, string value)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = Encoding.UTF8.GetBytes(value ?? string.Empty);
            await cache.SetAsync(key, data, DefaultOptions);
        }

        /// <summary>
        /// cr-dotnet-1034: Async version of GetString.
        /// Reads a string value from the Redis-backed distributed cache asynchronously,
        /// releasing the thread pool thread while awaiting the Redis I/O operation.
        /// Returns <c>null</c> when the key is absent.
        /// </summary>
        public static async Task<string> GetStringAsync(this IDistributedCache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            byte[] data = await cache.GetAsync(key);
            if (data == null || data.Length == 0)
                return null;

            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Removes a key from the Redis-backed distributed cache.
        /// </summary>
        public static void RemoveKey(this IDistributedCache cache, string key)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));
            cache.Remove(key);
        }
    }
}
