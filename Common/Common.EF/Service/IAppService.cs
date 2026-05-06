using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Service
{
    public interface IAppService
    {
        /// <summary>获取当前用户信息</summary>
        ICurrentUser CurrentUser { get; }

        /// <summary>取消令牌</summary>
        CancellationToken CancellationToken { get; }
    }
}
