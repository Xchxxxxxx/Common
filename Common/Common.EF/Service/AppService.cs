using AutoMapper;
using Common.uniwork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Service
{
    public abstract class AppService : IAppService
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;
        protected readonly ICurrentUser _currentUser;

        protected AppService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger logger,
            ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUser = currentUser;
        }

        public ICurrentUser CurrentUser => _currentUser;
        public CancellationToken CancellationToken => CancellationToken.None;

        /// <summary>执行带事务的操作</summary>
        protected virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            string operationName = "unknown")
        {
            try
            {
                _logger.LogDebug("开始执行事务操作: {OperationName}", operationName);
                var result = await _unitOfWork.ExecuteInTransactionAsync(action, CancellationToken);
                _logger.LogDebug("事务操作完成: {OperationName}", operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "事务操作失败: {OperationName}", operationName);
                throw;
            }
        }

        /// <summary>执行带日志的操作</summary>
        protected virtual async Task<TResult> ExecuteWithLoggingAsync<TResult>(
            Func<Task<TResult>> action,
            string operationName,
            object? parameters = null)
        {
            try
            {
                _logger.LogInformation("开始执行: {OperationName}, 参数: {@Parameters}", operationName, parameters);
                var result = await action();
                _logger.LogInformation("执行成功: {OperationName}", operationName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行失败: {OperationName}, 参数: {@Parameters}", operationName, parameters);
                throw;
            }
        }
    }
}
