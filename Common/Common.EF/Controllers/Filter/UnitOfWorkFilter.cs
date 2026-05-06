using Common.uniwork;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers.Filter
{
    /// <summary>
    /// 工作单元过滤器（自动保存更改）
    /// </summary>
    public class UnitOfWorkFilter : IAsyncActionFilter
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnitOfWorkFilter(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            // 如果Action执行成功且没有异常，自动保存更改
            if (resultContext.Exception == null && resultContext.Result is not null)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
