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
    /// CRUD控制器基类
    /// </summary>
    /// <typeparam name="TService">应用服务类型</typeparam>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">主键类型</typeparam>
    /// <typeparam name="TCreateDto">创建DTO</typeparam>
    /// <typeparam name="TUpdateDto">更新DTO</typeparam>
    /// <typeparam name="TResponseDto">响应DTO</typeparam>
    public abstract class BaseCrudController<TService, TEntity, TKey, TCreateDto, TUpdateDto, TResponseDto> : BaseApiController
     where TService :FAppService<TEntity, TKey, TCreateDto, TUpdateDto, TResponseDto>
     where TEntity : class, IEntity<TKey>
     where TKey : IEquatable<TKey>
     where TCreateDto : class
     where TUpdateDto : class
     where TResponseDto : class
    {
        protected readonly TService _service;

        protected BaseCrudController(TService service)
        {
            _service = service;
        }

        /// <summary>
        /// 根据ID获取
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public virtual async Task<ActionResult<ApiResponse<TResponseDto>>> GetById(TKey id)
        {
            var result = await _service.GetByIdAsync(id);
            return ToResponse(result);
        }

        /// <summary>
        /// 获取所有
        /// </summary>
        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<IReadOnlyList<TResponseDto>>>> GetAll()
        {
            var result = await _service.GetAllAsync();
            return ToResponse(result);
        }

        /// <summary>
        /// 分页获取
        /// </summary>
        [HttpGet("paged")]
        public virtual async Task<ActionResult<ApiResponse<PagedResult<TResponseDto>>>> GetPaged(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var result = await _service.GetPagedAsync(pageNumber, pageSize, searchTerm);
            return ToResponse(result);
        }

        /// <summary>
        /// 创建
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public virtual async Task<ActionResult<ApiResponse<TResponseDto>>> Create([FromBody] TCreateDto createDto)
        {
            var result = await _service.CreateAsync(createDto);
            return ToResponse(result);
        }

        /// <summary>
        /// 更新
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        [ProducesResponseType(typeof(ApiResponse<object>), 400)]
        public virtual async Task<ActionResult<ApiResponse<TResponseDto>>> Update(TKey id, [FromBody] TUpdateDto updateDto)
        {
            var result = await _service.UpdateAsync(id, updateDto);
            return ToResponse(result);
        }

        /// <summary>
        /// 删除
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Delete(TKey id)
        {
            var result = await _service.DeleteAsync(id);
            return ToResponse(result);
        }

        /// <summary>
        /// 软删除
        /// </summary>
        [HttpDelete("{id}/soft")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<object>), 404)]
        public virtual async Task<ActionResult<ApiResponse<bool>>> SoftDelete(TKey id)
        {
            var result = await _service.SoftDeleteAsync(id);
            return ToResponse(result);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost("batch-delete")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public virtual async Task<ActionResult<ApiResponse<int>>> BatchDelete([FromBody] IEnumerable<TKey> ids)
        {
            var result = await _service.BatchDeleteAsync(ids);
            return ToResponse(result);
        }

        /// <summary>
        /// 检查是否存在
        /// </summary>
        [HttpGet("{id}/exists")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Exists(TKey id)
        {
            var result = await _service.ExistsAsync(id);
            return ToResponse(result);
        }
    }
}
