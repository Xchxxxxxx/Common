using Common.DamainEvent;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public interface IDomainEventDispatcher
    {
        Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}
