using System;
using System.Collections;
using System.Web;
using System.Web.Caching;

namespace CommonTools.CacheTool
{
    public class CacheHelper
    {
        public static bool Exists(string key)
        {
            return GetCache(key) == null ? false : true;
        }

        public static object GetCache(string key)
        {
            Cache cache = HttpRuntime.Cache;
            return cache[key];
        }

        public static void SetCache(string key, object data)
        {
            if (data != null)
            {
                Cache cache = HttpRuntime.Cache;
                cache.Insert(key, data);
            }
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="cache_time">缓存时间（秒）</param>
        public static void SetCache(string key, object data, int cache_time)
        {
            if (data != null)
            {
                Cache cache = HttpRuntime.Cache;
                cache.Insert(key, data, null, DateTime.MaxValue, TimeSpan.FromSeconds(cache_time), CacheItemPriority.NotRemovable, null);
            }
        }

        public static void SetCache(string key, object data, DateTime cache_time)
        {
            if (data != null)
            {
                var ts = cache_time - DateTime.Now;
                Cache cache = HttpRuntime.Cache;
                cache.Insert(key, data, null, DateTime.MaxValue, ts, CacheItemPriority.NotRemovable, null);
            }
        }

        public static void RemoveByKey(string key)
        {
            if (GetCache(key) != null)
            {
                Cache cache = HttpRuntime.Cache;
                cache.Remove(key);
            }
        }

        public static void RemoveAllCache()
        {
            Cache _cache = HttpRuntime.Cache;
            IDictionaryEnumerator CacheEnum = _cache.GetEnumerator();
            while (CacheEnum.MoveNext())
            {
                _cache.Remove(CacheEnum.Key.ToString());
            }
        }
    }
}