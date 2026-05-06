using Common.EF.Api;
using Common.EF.EF;
using Common.EF.EF.entity;
using Common.EF.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.EF.Controllers
{
    /// <summary>
    /// 只读控制器基类
    /// </summary>
    public abstract class BaseReadOnlyController<TService, TEntity, TKey, TResponseDto> : BaseApiController
        where TService : FAppService<TEntity, TKey, object, object, TResponseDto>
        where TEntity : class, IEntity<TKey>, new()
        where TKey : IEquatable<TKey>
        where TResponseDto : class
    {
        protected readonly TService _service;

        protected BaseReadOnlyController(TService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<TResponseDto>>> GetById(TKey id)
        {
            var result = await _service.GetByIdAsync(id);
            return ToResponse(result);
        }

        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<IReadOnlyList<TResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return ToResponse(result);
        }

        [HttpGet("paged")]
        public virtual async Task<ActionResult<ApiResponse<PagedResult<TResponseDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _service.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return ToResponse(result);
        }
    }
}
