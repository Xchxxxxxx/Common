using Common.DamainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected AggregateRoot() { }

        protected AggregateRoot(TKey id) : base(id) { }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        protected void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        // 显式接口实现
        void IAggregateRoot<TKey>.AddDomainEvent(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }

        void IAggregateRoot<TKey>.RemoveDomainEvent(IDomainEvent domainEvent)
        {
            RemoveDomainEvent(domainEvent);
        }

        void IAggregateRoot<TKey>.ClearDomainEvents()
        {
            ClearDomainEvents();
        }
    }
}
