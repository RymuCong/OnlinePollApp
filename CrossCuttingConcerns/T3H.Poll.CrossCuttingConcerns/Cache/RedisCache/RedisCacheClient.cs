﻿using T3H.Poll.CrossCuttingConcerns.ExtensionMethods;
using StackExchange.Redis;
using System.Text;

namespace T3H.Poll.CrossCuttingConcerns.Cache.RedisCache
{
    public class RedisCacheClient : ICacheClient    
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase db;
        private readonly ISerializer serializer;
        private static readonly Encoding encoding = Encoding.UTF8;


        public RedisCacheClient(string connectionString, ISerializer serializer = null, int database = 0)
        {
            if (serializer == null)
            {
                serializer = new JilSerializer();
            }
            this.serializer = serializer;
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            db = connectionMultiplexer.GetDatabase(database);
        }

        /// <summary>
        /// Initializes a new instance of the <see href="RedisCacheClient" /> class.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="database">The database.</param>
        /// <exception href="System.ArgumentNullException">serializer</exception>
        public RedisCacheClient(ISerializer serializer, string connectionString, int database = 0)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            this.serializer = serializer;
            connectionMultiplexer = ConnectionMultiplexer.Connect(connectionString);
            db = connectionMultiplexer.GetDatabase(database);
        }

        /// <summary>
        /// Initializes a new instance of the <see href="RedisCacheClient" /> class.
        /// </summary>
        /// <param name="connectionMultiplexer">The connection multiplexer.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="database">The database.</param>
        /// <exception href="System.ArgumentNullException">
        /// connectionMultiplexer
        /// or
        /// serializer
        /// </exception>
        public RedisCacheClient(ConnectionMultiplexer connectionMultiplexer, ISerializer serializer, int database = 0)
        {
            if (connectionMultiplexer == null)
            {
                throw new ArgumentNullException("connectionMultiplexer");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            this.serializer = serializer;
            this.connectionMultiplexer = connectionMultiplexer;

            db = connectionMultiplexer.GetDatabase(database);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            connectionMultiplexer.Dispose();
            GC.SuppressFinalize(this);

        }

        /// <summary>
        /// Return the instance of <see href="StackExchange.Redis.IDatabase" /> used be ICacheClient implementation
        /// </summary>
        public IDatabase Database
        {
            get { return db; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>
        /// The serializer.
        /// </value>
        public ISerializer Serializer
        {
            get { return serializer; }
        }

        /// <summary>
        /// Verify that the specified cache key exists
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// True if the key is present into Redis. Othwerwise False
        /// </returns>
        public bool Exists(string key)
        {
            return db.KeyExists(key);
        }

        /// <summary>
        /// Verify that the specified cache key exists
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// True if the key is present into Redis. Othwerwise False
        /// </returns>
        public Task<bool> ExistsAsync(string key)
        {
            return db.KeyExistsAsync(key);
        }

        /// <summary>
        /// Removes the specified key from Redis Database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if the key has removed. Othwerwise False
        /// </returns>
        public bool Remove(string key)
        {
            return db.KeyDelete(key);
        }

        /// <summary>
        /// Removes the specified key from Redis Database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if the key has removed. Othwerwise False
        /// </returns>
        public Task<bool> RemoveAsync(string key)
        {
            return db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Removes all specified keys from Redis Database
        /// </summary>
        /// <param name="keys">The key.</param>
        public void RemoveAll(IEnumerable<string> keys)
        {
            keys.ForEach(x => Remove(x));
        }

        /// <summary>
        /// Removes all specified keys from Redis Database
        /// </summary>
        /// <param name="keys">The key.</param>
        /// <returns></returns>
        public Task RemoveAllAsync(IEnumerable<string> keys)
        {
            return keys.ForEachAsync(RemoveAsync);
        }

        /// <summary>
        /// Get the object with the specified key from Redis database
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// Null if not present, otherwise the instance of T.
        /// </returns>
        public T Get<T>(string key) where T : class
        {
            var valueBytes = db.StringGet(key);

            if (!valueBytes.HasValue)
            {
                return default;
            }

            return serializer.Deserialize<T>(valueBytes);
        }

        /// <summary>
        /// Get the object with the specified key from Redis database
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// Null if not present, otherwise the instance of T.
        /// </returns>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var valueBytes = await db.StringGetAsync(key);

            if (!valueBytes.HasValue)
            {
                return default;
            }

            return await serializer.DeserializeAsync<T>(valueBytes);
        }

        public async Task<T> GetValueAsync<T>(string key)
        {
            var valueBytes = await db.StringGetAsync(key);
            if (!valueBytes.HasValue)
            {
                return default;
            }
            return ChangeToType<T>(key, valueBytes);
        }

        public Task<bool> AddValueAsync<T>(string key, T value)
        {
            var entryBytes = serializer.Serialize(value);
            return db.StringSetAsync(key, entryBytes, flags: CommandFlags.FireAndForget);
        }



        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Add<T>(string key, T value) where T : class
        {
            var entryBytes = serializer.Serialize(value);

            return db.StringSet(key, entryBytes, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public Task<bool> AddAsync<T>(string key, T value) where T : class
        {
            var entryBytes = serializer.Serialize(value);
            return db.StringSetAsync(key, entryBytes, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Replace<T>(string key, T value) where T : class
        {
            return Add(key, value);
        }

        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public Task<bool> ReplaceAsync<T>(string key, T value) where T : class
        {
            return AddAsync(key, value);
        }

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Add<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            var entryBytes = serializer.Serialize(value);
            var expiration = expiresAt.Subtract(DateTimeOffset.Now);

            return db.StringSet(key, entryBytes, expiration, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public async Task<bool> AddAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            var entryBytes = await serializer.SerializeAsync(value);
            var expiration = expiresAt.Subtract(DateTimeOffset.Now);

            return await db.StringSetAsync(key, entryBytes, expiration, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Replace<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return Add(key, value, expiresAt);
        }

        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <param name="expiresAt">Expiration time.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public Task<bool> ReplaceAsync<T>(string key, T value, DateTimeOffset expiresAt) where T : class
        {
            return AddAsync(key, value, expiresAt);
        }

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresIn">The duration of the cache using Timespan.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Add<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            var entryBytes = serializer.Serialize(value);

            return db.StringSet(key, entryBytes, expiresIn, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// Adds the specified instance to the Redis database.
        /// </summary>
        /// <typeparam name="T">The type of the class to add to Redis</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The instance of T.</param>
        /// <param name="expiresIn">The duration of the cache using Timespan.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public async Task<bool> AddAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            var entryBytes = await serializer.SerializeAsync(value);

            return await db.StringSetAsync(key, entryBytes, expiresIn, flags: CommandFlags.FireAndForget);
        }


        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <param name="expiresIn">The duration of the cache using Timespan.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public bool Replace<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return Add(key, value, expiresIn);
        }

        /// <summary>
        /// Replaces the object with specified key into Redis database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The instance of T</param>
        /// <param name="expiresIn">The duration of the cache using Timespan.</param>
        /// <returns>
        /// True if the object has been added. Otherwise false
        /// </returns>
        public Task<bool> ReplaceAsync<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return AddAsync(key, value, expiresIn);
        }

        /// <summary>
        /// Get the objects with the specified keys from Redis database with one roundtrip
        /// </summary>
        /// <typeparam name="T">The type of the expected object</typeparam>
        /// <param name="keys">The keys.</param>
        /// <returns>
        /// Empty list if there are no results, otherwise the instance of T.
        /// If a cache key is not present on Redis the specified object into the returned Dictionary will be null
        /// </returns>
        public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys) where T : class
        {
            var keysList = keys.ToList();
            var redisKeys = new RedisKey[keysList.Count];
            var sb = CreateLuaScriptForMget(redisKeys, keysList);

            RedisResult[] redisResults = (RedisResult[])db.ScriptEvaluate(sb, redisKeys);

            var result = new Dictionary<string, T>();

            for (var i = 0; i < redisResults.Count(); i++)
            {
                var obj = default(T);

                if (!redisResults[i].IsNull)
                {
                    //TODO: (byte[])redisResults[i]
                    obj = serializer.Deserialize<T>(encoding.GetBytes(redisResults[i].ToString()));
                }
                result.Add(keysList[i], obj);
            }

            return result;
        }



        public async Task<List<T>> GetAllAsync<T>(List<string> keys) where T : class
        {
            var result = new List<T>();
            try
            {
                var inputKeys = keys.Where(t => !string.IsNullOrEmpty(t)).Select(t => (RedisKey)t).ToList();

                if (inputKeys.Count == 0) return result;

                var data = await db.StringGetAsync(inputKeys.ToArray());
                result.AddRange(
                    data.Where(item => !item.IsNull).Select(redisValue => serializer.Deserialize<T>(encoding.GetBytes(redisValue.ToString()))));
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }


        /// <summary>
        /// Adds all.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        public bool AddAll<T>(IList<Tuple<string, T>> items) where T : class
        {
            var redisInput = items.Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, serializer.Serialize(item.Item2))).ToArray();
            var result = db.StringSet(redisInput, When.Always, CommandFlags.FireAndForget);
            return result;
        }

        /// <summary>
        /// Adds all asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public Task<bool> AddAllAsync<T>(IList<Tuple<string, T>> items) where T : class
        {
            var redisInput = items.Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Item1, serializer.Serialize(item.Item2))).ToArray();
            var result = db.StringSetAsync(redisInput, When.Always, CommandFlags.FireAndForget);
            return result;
        }

        /// <summary>
        /// Run SADD command <see href="http://redis.io/commands/sadd" />
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool SetAdd(string memberName, string key)
        {
            return db.SetAdd(memberName, key);
        }

        /// <summary>
        /// Run SADD command <see href="http://redis.io/commands/sadd" />
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public Task<bool> SetAddAsync(string memberName, string key)
        {
            return db.SetAddAsync(memberName, key);
        }

        /// <summary>
        /// Run SMEMBERS command <see href="http://redis.io/commands/SMEMBERS" />
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        /// <exception href="System.NotImplementedException"></exception>
        public string[] SetMember(string memberName)
        {
            return db.SetMembers(memberName).Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Run SMEMBERS command <see href="http://redis.io/commands/SMEMBERS" />
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        public async Task<string[]> SetMemberAsync(string memberName)
        {
            return (await db.SetMembersAsync(memberName)).Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// Searches the keys from Redis database
        /// </summary>
        /// <remarks>
        /// Consider this as a command that should only be used in production environments with extreme care. It may ruin performance when it is executed against large databases
        /// </remarks>
        /// <param name="pattern">The pattern.</param>
        /// <example>
        ///		if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
        ///		if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
        ///		if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
        /// </example>
        /// <returns>A list of cache keys retrieved from Redis database</returns>
        public IEnumerable<string> SearchKeys(string pattern)
        {
            // var keys = new HashSet<RedisKey>();
            List<string> schemas = new List<string>();
            int nextCursor = 0;
            do
            {
                var arr = (RedisResult[])db.Execute("scan", new object[] { nextCursor.ToString(), "MATCH", $"{pattern}", "COUNT", "1000" });
                nextCursor = (int)arr[0];
                List<string> resultLines = ((string[])arr[1]).ToList();
                schemas.AddRange(resultLines);
            }
            while (nextCursor != 0);

            return schemas;

            //var keys = new HashSet<RedisKey>();

            //var endPoints = db.Multiplexer.GetEndPoints();

            //foreach (var endpoint in endPoints)
            //{
            //    var dbKeys = db.Multiplexer.GetServer(endpoint).Keys(pattern: pattern);

            //    foreach (var dbKey in dbKeys)
            //    {
            //        if (!keys.Contains(dbKey))
            //        {
            //            keys.Add(dbKey);
            //        }
            //    }
            //}

            //return keys.Select(x => (string)x);
        }

        public IEnumerable<string> SearchKeysAll(string pattern)
        {
            //var keys = new HashSet<RedisKey>();

            var keys = new List<string>();

            var endPoints = db.Multiplexer.GetEndPoints();
            foreach (var endpoint in endPoints)
            {
                var server = db.Multiplexer.GetServer(endpoint);
                var dbKeys = server.Keys(pattern: pattern, pageSize: 10000);
                keys.AddRange(dbKeys.Select(f => (string)f).ToList());
            }

            return keys.Distinct();
        }

        /// <summary>
        /// Searches the keys from Redis database
        /// </summary>
        /// <remarks>
        /// Consider this as a command that should only be used in production environments with extreme care. It may ruin performance when it is executed against large databases
        /// </remarks>
        /// <param name="pattern">The pattern.</param>
        /// <example>
        ///		if you want to return all keys that start with "myCacheKey" uses "myCacheKey*"
        ///		if you want to return all keys that contain with "myCacheKey" uses "*myCacheKey*"
        ///		if you want to return all keys that end with "myCacheKey" uses "*myCacheKey"
        /// </example>
        /// <returns>A list of cache keys retrieved from Redis database</returns>
        public Task<IEnumerable<string>> SearchKeysAsync(string pattern)
        {
            return Task.Factory.StartNew(() => SearchKeys(pattern));
        }

        public async Task<long> ListRightPushAsync<T>(string key, T value) where T : class
        {
            var entryBytes = await serializer.SerializeAsync(value);

            return await db.ListRightPushAsync(key, entryBytes, flags: CommandFlags.FireAndForget);
        }

        public void FlushDb()
        {
            var endPoints = db.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {

                var server = db.Multiplexer.GetServer(endpoint);
                if (!server.IsSlave && server.IsConnected)
                {
                    server.FlushAllDatabases();
                }
            }
        }

        public async Task FlushDbAsync()
        {
            var endPoints = db.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                await db.Multiplexer.GetServer(endpoint).FlushDatabaseAsync();
            }
        }

        public void Save(SaveType saveType)
        {
            var endPoints = db.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                db.Multiplexer.GetServer(endpoint).Save(saveType);
            }
        }

        public async void SaveAsync(SaveType saveType)
        {
            var endPoints = db.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                await db.Multiplexer.GetServer(endpoint).SaveAsync(saveType);
            }
        }

        public Dictionary<string, string> GetInfo()
        {
            var info = db.ScriptEvaluate("return redis.call('INFO')").ToString();

            return ParseInfo(info);
        }

        public async Task<Dictionary<string, string>> GetInfoAsync()
        {
            var info = (await db.ScriptEvaluateAsync("return redis.call('INFO')")).ToString();

            return ParseInfo(info);
        }


        public async Task<T> ListLeftPopAsync<T>(string key) where T : class
        {
            var result = default(T);
            var valueBytes = await db.ListLeftPopAsync(key);
            if (valueBytes.HasValue)
            {
                result = await serializer.DeserializeAsync<T>(valueBytes);
            }
            return result;
        }

        public long ListLength(string key)
        {
            return db.ListLength(key);
        }


        private string CreateLuaScriptForMset<T>(RedisKey[] redisKeys, RedisValue[] redisValues, IList<Tuple<string, T>> objects)
        {
            var sb = new StringBuilder("return redis.call('mset',");

            for (var i = 0; i < objects.Count; i++)
            {
                redisKeys[i] = objects[i].Item1;
                redisValues[i] = serializer.Serialize(objects[i].Item2);

                sb.AppendFormat("KEYS[{0}],ARGV[{0}]", i + 1);

                if (i < objects.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        private string CreateLuaScriptForMget(RedisKey[] redisKeys, List<string> keysList)
        {
            var sb = new StringBuilder("return redis.call('mget',");

            for (var i = 0; i < keysList.Count; i++)
            {
                redisKeys[i] = keysList[i];
                sb.AppendFormat("KEYS[{0}]", i + 1);

                if (i < keysList.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(")");

            return sb.ToString();
        }

        private Dictionary<string, string> ParseInfo(string info)
        {
            string[] lines = info.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var data = new Dictionary<string, string>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line) || line[0] == '#')
                {
                    // 2.6+ can have empty lines, and comment lines
                    continue;
                }

                int idx = line.IndexOf(':');
                if (idx > 0) // double check this line looks about right
                {
                    var key = line.Substring(0, idx);
                    var infoValue = line.Substring(idx + 1).Trim();

                    data.Add(key, infoValue);
                }
            }

            return data;
        }

        public string[] GetListKey()
        {
            var keys = connectionMultiplexer.GetServer(db.Multiplexer.GetEndPoints()[0]).Keys();
            return keys.Select(key => (string)key).ToArray();
        }

        private T ChangeToType<T>(string key, RedisValue redisValue)
        {
            if (redisValue.IsNull)
                return default(T);

            try
            {
                var sRedisValue = redisValue;
                if (typeof(T) == typeof(bool))
                {
                    var tempRedisValue = (string)redisValue;
                    if (tempRedisValue.ToLower() == "true")
                        sRedisValue = RedisValue.Unbox(1);
                    else
                        sRedisValue = RedisValue.Unbox(0);
                }

                var obj = Convert.ChangeType(sRedisValue, typeof(T));
                return (T)obj;
            }
            catch
            {
                throw new Exception($"Redis key '{key}' has invalid redis value '{redisValue}'");
            }
        }
    }
}
