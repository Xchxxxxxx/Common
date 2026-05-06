using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Common.Cache
{
    /// <summary>
    /// Redis缓存服务实现类
    /// </summary>
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _redisDb;
        // 默认过期时间：30分钟
        private readonly TimeSpan _defaultExpireTime = TimeSpan.FromMinutes(30);
        private const string DelayQueueKey = "delay:orders"; // 延迟队列Key

        /// <summary>
        /// 构造函数注入IDatabase
        /// </summary>
        /// <param name="redisDb">Redis数据库对象</param>
        public RedisCacheService(IDatabase redisDb)
        {
            _redisDb = redisDb ?? throw new ArgumentNullException(nameof(redisDb), "Redis数据库对象不能为空");
        }

        #region 同步操作
        public void Set<T>(string key, T value, TimeSpan? expiresIn = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            // 处理过期时间
            var expireTime = expiresIn ?? _defaultExpireTime;
            // 序列化对象
            string cacheValue = SerializeObject(value);
            // 设置缓存
            _redisDb.StringSet(key, cacheValue, expireTime);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            // 获取缓存值
            var redisValue = _redisDb.StringGet(key);
            if (redisValue.IsNullOrEmpty)
                return default(T);

            // 反序列化对象
            return DeserializeObject<T>(redisValue.ToString());
        }
        /// <summary>
        /// 添加延迟订单任务
        /// </summary>
        public async Task AddOrderDelayJobAsync(int orderId, TimeSpan delay)
        {
            // 计算UTC过期时间戳（逻辑不变）
            long expireTimestamp = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
            // 添加到有序集合：Score=UTC过期时间戳，Value=订单ID
            await _redisDb.SortedSetAddAsync(DelayQueueKey, orderId.ToString(), expireTimestamp);
        }

        /// <summary>
        /// 获取所有已到期的订单ID并移除
        /// </summary>
        public async Task<List<int>> GetExpiredOrderIdsAsync()
        {
            var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // 1. 获取所有Score <= 当前时间的订单ID（已到期）
            var expiredIds = await _redisDb.SortedSetRangeByScoreAsync(
                DelayQueueKey,
                0,
                nowTimestamp);

            if (!expiredIds.Any())
                return new List<int>();

            // 2. 原子移除已到期的订单ID（避免重复处理）
            await _redisDb.SortedSetRemoveRangeByScoreAsync(
                DelayQueueKey,
                0,
                nowTimestamp);

            // 3. 转换为int列表
            return expiredIds
                .Select(id => int.TryParse(id.ToString(), out var orderId) ? orderId : 0)
                .Where(id => id > 0)
                .ToList();
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return _redisDb.KeyDelete(key);
        }

        public bool Exists(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return _redisDb.KeyExists(key);
        }

        public void RefreshExpire(string key, TimeSpan expiresIn)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            if (!Exists(key))
                return;

            _redisDb.KeyExpire(key, expiresIn);
        }
        public async Task<bool> ValidateAsync(string phone, string code)
        {
            string cachedCode =await GetAsync<string>(phone);
            return cachedCode == code;
        }
        #endregion

        #region 异步操作
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiresIn = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            var expireTime = expiresIn ?? _defaultExpireTime;
            string cacheValue = SerializeObject(value);
            await _redisDb.StringSetAsync(key, cacheValue, expireTime);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            var redisValue = await _redisDb.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
                return default(T);

            return DeserializeObject<T>(redisValue.ToString());
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return await _redisDb.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            return await _redisDb.KeyExistsAsync(key);
        }

        public async Task RefreshExpireAsync(string key, TimeSpan expiresIn)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "缓存键不能为空");

            if (!await ExistsAsync(key))
                return;

            await _redisDb.KeyExpireAsync(key, expiresIn);
        }
        #endregion

        #region 私有辅助方法：序列化/反序列化
        /// <summary>
        /// 对象序列化为JSON字符串
        /// </summary>
        private string SerializeObject<T>(T obj)
        {
            if (obj == null)
                return string.Empty;

            // 处理值类型和字符串类型，避免多余的JSON格式
            if (obj is string || obj.GetType().IsValueType)
                return obj.ToString();

            // 序列化复杂对象
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, // 忽略循环引用
                DateFormatString = "yyyy-MM-dd HH:mm:ss" // 统一日期格式
            });
        }

        /// <summary>
        /// JSON字符串反序列化为对象
        /// </summary>
        private T DeserializeObject<T>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return default(T);

            // 处理值类型和字符串类型
            var targetType = typeof(T);
            if (targetType == typeof(string))
                return (T)(object)jsonStr;

            if (targetType.IsValueType)
                return (T)Convert.ChangeType(jsonStr, targetType);

            // 反序列化复杂对象
            return JsonConvert.DeserializeObject<T>(jsonStr);
        }
        #endregion
    }
}