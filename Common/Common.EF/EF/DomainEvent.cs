using Common.DamainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public Guid EventId { get; } = Guid.NewGuid();
    }
}
