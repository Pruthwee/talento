using StackExchange.Redis;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Provides distributed caching and shared state using Amazon ElastiCache for Redis.
    /// Replaces static collections and in-process session state for cloud scalability.
    /// </summary>
    public static class RedisCacheHelper
    {
        private static readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        private static readonly string _redisConnectionString;

        static RedisCacheHelper()
        {
            _redisConnectionString = ConfigurationManager.AppSettings["AWS:Redis:ConnectionString"] ?? "localhost:6379";
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
                ConnectionMultiplexer.Connect(_redisConnectionString));
        }

        public static ConnectionMultiplexer Connection => _lazyConnection.Value;

        public static IDatabase GetDatabase()
        {
            return Connection.GetDatabase();
        }

        public static IServer GetServer()
        {
            var endpoints = Connection.GetEndPoints();
            return Connection.GetServer(endpoints[0]);
        }

        // String operations (for key-value storage replacing static dictionaries)
        public static bool StringSet(string key, string value, TimeSpan? expiry = null)
        {
            return GetDatabase().StringSet(key, value, expiry);
        }

        public static string StringGet(string key)
        {
            return GetDatabase().StringGet(key);
        }

        // List operations (for replacing static List<T>)
        public static void ListAdd(string key, string value)
        {
            GetDatabase().ListRightPush(key, value);
        }

        public static string[] ListGetAll(string key)
        {
            return GetDatabase().ListRange(key);
        }

        public static long ListRemove(string key, string value)
        {
            return GetDatabase().ListRemove(key, value);
        }

        // Hash operations (for replacing static Dictionary<TKey, TValue>)
        public static bool HashSet(string key, string field, string value)
        {
            return GetDatabase().HashSet(key, field, value);
        }

        public static string HashGet(string key, string field)
        {
            return GetDatabase().HashGet(key, field);
        }

        public static HashEntry[] HashGetAll(string key)
        {
            return GetDatabase().HashGetAll(key);
        }

        // Set operations (for replacing static HashSet<T>)
        public static bool SetAdd(string key, string value)
        {
            return GetDatabase().SetAdd(key, value);
        }

        public static bool SetContains(string key, string value)
        {
            return GetDatabase().SetContains(key, value);
        }

        public static long SetLength(string key)
        {
            return GetDatabase().SetLength(key);
        }

        // Counter operations (for replacing static counters)
        public static long Increment(string key, long value = 1)
        {
            return GetDatabase().StringIncrement(key, value);
        }

        public static long Decrement(string key, long value = 1)
        {
            return GetDatabase().StringDecrement(key, value);
        }
    }
}
