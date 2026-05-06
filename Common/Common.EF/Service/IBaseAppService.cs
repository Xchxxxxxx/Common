using Common.EF.Api;
using Common.EF.EF;
using Common.EF.EF.entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Common.EF.Service
{
    public interface IBaseAppService<TEntity, TKey, TCreateDto, TUpdateDto, TResponseDto> : IAppService
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
    where TCreateDto : class
    where TUpdateDto : class
    where TResponseDto : class
    {
        /// <summary>根据ID获取</summary>
        Task<ApiResponse<TResponseDto>> GetByIdAsync(TKey id);

        /// <summary>获取所有</summary>
        Task<ApiResponse<IReadOnlyList<TResponseDto>>> GetAllAsync();

        /// <summary>分页获取</summary>
        Task<ApiResponse<PagedResult<TResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);

        /// <summary>创建</summary>
        Task<ApiResponse<TResponseDto>> CreateAsync(TCreateDto createDto);

        /// <summary>更新</summary>
        Task<ApiResponse<TResponseDto>> UpdateAsync(TKey id, TUpdateDto updateDto);

        /// <summary>删除</summary>
        Task<ApiResponse<bool>> DeleteAsync(TKey id);

        /// <summary>批量删除</summary>
        Task<ApiResponse<int>> BatchDeleteAsync(IEnumerable<TKey> ids);
        Task<ApiResponse<bool>> SoftDeleteAsync(TKey id);

        /// <summary>检查是否存在</summary>
        Task<ApiResponse<bool>> ExistsAsync(TKey id);
        Task<ApiResponse<IReadOnlyList<TResponseDto>>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<ApiResponse<TResponseDto>> FindOneAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
