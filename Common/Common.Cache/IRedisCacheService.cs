using System;
using System.Threading.Tasks;

namespace Common.Cache
{
    /// <summary>
    /// Redis缓存服务接口（封装常用操作）
    /// </summary>
    public interface IRedisCacheService
    {
        #region 同步操作
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiresIn">过期时间（默认30分钟）</param>
        void Set<T>(string key, T value, TimeSpan? expiresIn = null);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值（不存在返回default(T)）</returns>
        T Get<T>(string key);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否删除成功</returns>
        bool Remove(string key);

        /// <summary>
        /// 判断缓存键是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        bool Exists(string key);

        /// <summary>
        /// 刷新缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiresIn">新的过期时间</param>
        void RefreshExpire(string key, TimeSpan expiresIn);
        #endregion

        #region 异步操作（推荐，适合Web项目）
        /// <summary>
        /// 异步设置缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expiresIn">过期时间（默认30分钟）</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiresIn = null);

        /// <summary>
        /// 异步获取缓存
        /// </summary>
        /// <typeparam name="T">缓存值类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>缓存值（不存在返回default(T)）</returns>
        Task<T> GetAsync<T>(string key);

        /// <summary>
        /// 异步删除缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否删除成功</returns>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// 异步判断缓存键是否存在
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// 异步刷新缓存过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="expiresIn">新的过期时间</param>
        Task RefreshExpireAsync(string key, TimeSpan expiresIn);
        /// <summary>
        /// 异步校验值相等
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        Task<bool> ValidateAsync(string key, string value);
        Task AddOrderDelayJobAsync(int orderId, TimeSpan delay);
        Task<List<int>> GetExpiredOrderIdsAsync();
        #endregion
    }
}