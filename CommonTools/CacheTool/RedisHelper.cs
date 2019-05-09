using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace CommonTools.CacheTool
{
    public class RedisHelper
    {
        private static readonly object _locker = new object();
        private static ConnectionMultiplexer _redis_connect;
        private static IDatabase _redis_db;

        private RedisHelper()
        {
        }

        public static RedisHelper GetInstance(string redis_conn_str)
        {
            if (string.IsNullOrEmpty(redis_conn_str))
                return null;

            if (_redis_connect == null)
            {
                lock (_locker)
                {
                    if (_redis_connect == null || !_redis_connect.IsConnected)
                    {
                        _redis_connect = GetConnection(redis_conn_str);
                        _redis_db = _redis_connect.GetDatabase();
                    }
                }
            }

            return new RedisHelper();
        }

        #region string

        public static void StringSet(string key, object value, TimeSpan? expiry = null)
        {
            if (!string.IsNullOrEmpty(key) && value != null)
            {
                _redis_db.StringSet(key, JsonConvert.SerializeObject(value), expiry);
            }
        }

        public static string StringGet(string key)
        {
            return _redis_db.StringGet(key);
        }

        public static bool Exists(string key)
        {
            if (string.IsNullOrEmpty(StringGet(key)))
                return false;
            return true;
        }

        #endregion string

        #region hash

        public static void HashSet(string hash_table_name, string hash_table_key, object hash_table_data)
        {
            if (hash_table_data != null && !string.IsNullOrEmpty(hash_table_key) && !string.IsNullOrEmpty(hash_table_name))
            {
                List<HashEntry> hash = ObjectToHashEntry(hash_table_key, hash_table_data);
                _redis_db.HashSet(hash_table_name, hash.ToArray());
            }
        }

        public static object GetCache(string hash_table_name, string key)
        {
            return _redis_db.HashGet(hash_table_name, key);
        }

        public static bool Remove(string hash_table_name, string key)
        {
            return _redis_db.HashDelete(hash_table_name, key);
        }

        public static bool Exists(string hash_table_name, string key)
        {
            return _redis_db.HashExists(hash_table_name, key);
        }

        private static List<HashEntry> ObjectToHashEntry(string key, object value)
        {
            List<HashEntry> list = new List<HashEntry>();
            list.Add(new HashEntry(key, JsonConvert.SerializeObject(value)));
            return list;
        }

        #endregion hash

        private static ConnectionMultiplexer GetConnection(string conn)
        {
            var connect = ConnectionMultiplexer.Connect(conn);
            //注册如下事件
            //connect.ConnectionFailed += MuxerConnectionFailed;
            //connect.ConnectionRestored += MuxerConnectionRestored;
            //connect.ErrorMessage += MuxerErrorMessage;
            //connect.ConfigurationChanged += MuxerConfigurationChanged;
            //connect.HashSlotMoved += MuxerHashSlotMoved;
            //connect.InternalError += MuxerInternalError;
            return connect;
        }
    }
}