using Common.DamainEvent;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.EF
{
    public class MediatRDomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MediatRDomainEventDispatcher> _logger;

        public MediatRDomainEventDispatcher(
            IMediator mediator,
            ILogger<MediatRDomainEventDispatcher> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
        {
            foreach (var @event in events)
            {
                try
                {
                    _logger.LogInformation("分发领域事件: {EventType}", @event.GetType().Name);
                    await _mediator.Publish(@event, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "领域事件分发失败: {EventType}", @event.GetType().Name);
                    // 可以选择继续或抛出异常
                    throw;
                }
            }
        }
    }
}
