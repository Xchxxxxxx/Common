using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.EF.Log
{
    public class Logger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
        }
        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(Exception? exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        // 领域事件日志标准化
        public void LogDomainEvent(string eventName, string aggregateId, object eventData)
        {
            var logMessage = $"领域事件触发 - 事件名：{eventName}，聚合根ID：{aggregateId}，数据：{JsonSerializer.Serialize(eventData)}";
            _logger.LogInformation(logMessage);
        }
    }
}
