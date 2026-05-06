using Common.DamainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    /// <summary>
    /// 聚合根接口
    /// </summary>
    /// <typeparam name="TKey">聚合根标识类型</typeparam>
    public interface IAggregateRoot<TKey> : IEntity<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// 获取领域事件集合
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// 添加领域事件
        /// </summary>
        /// <param name="domainEvent">领域事件</param>
        void AddDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// 移除领域事件
        /// </summary>
        /// <param name="domainEvent">领域事件</param>
        void RemoveDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// 清空所有领域事件
        /// </summary>
        void ClearDomainEvents();
    }
}
