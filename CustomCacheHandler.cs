  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Web.Caching;
  class CustomCacheHandler
        {
            private static Cache cache = null;
            private static object lockObject = new object();
            //expire cache content in 5 minutes
            private const int ABSOLUTE_EXPIRATION = 5;
			//path of folder where dependency files reside
            private const string RESOURCE_FOLDER = "\\Resources";
            readonly string rootPath;
           private Cache CacheHolder { get { return cache; } }

            public CustomCacheHandler()
            {
			    //get application execution path
                rootPath = AppDomain.CurrentDomain.BaseDirectory + RESOURCE_FOLDER;
                if (cache == null)
                {
                    lock (lockObject)
                    {
                        if (cache != null)
                        {
                            cache = new Cache();
                        }
                    }
                }
            }

            /// <summary>
            /// Get cached object
            /// </summary>
            /// <param name="key">Key or file path relative to application</param>
            /// <returns></returns>
            public object GetCacheObject(string cacheKey) => CacheHolder[cacheKey];

            /// <summary>
            /// Safely get cached item
            /// </summary>
            public bool TryGetCacheObject<T>(string key, ref T value)
            {
                if (CacheHolder[key] != null && CacheHolder[key] is T)
                {
                    value = (T)CacheHolder[key];
                    return true;
                }
                return false;
            }            

            /// <summary>
            /// Set object in cach with given key and item.
            /// </summary>
            /// <param name="key">Key of Cache Item</param>
            /// <param name="item">Object to be add in cache</param>
            /// <param name="filePath">file path relative to application. This path is used to create dependency.</param>
            public void SetCacheObject(string key, object item, string filePath)
            {
                if (CacheHolder[key] != null)
                {
                    return;
                }

                lock (lockObject)
                {
                    if (CacheHolder[key] != null)
                    {
                        return;
                    }
                    string serverpath = rootPath + (filePath.StartsWith("\\") ? filePath : "\\" + filePath);
                    if (File.Exists(serverpath))
                        CacheHolder.Insert(key, item, new CacheDependency(serverpath), DateTime.UtcNow.AddMinutes(ABSOLUTE_EXPIRATION), Cache.NoSlidingExpiration);
                    else
                        CacheHolder.Insert(key, item, new CacheDependency(rootPath), DateTime.UtcNow.AddMinutes(ABSOLUTE_EXPIRATION), Cache.NoSlidingExpiration);

                }
            }
        }
