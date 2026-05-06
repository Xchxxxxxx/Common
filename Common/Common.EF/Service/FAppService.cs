using AutoMapper;
using Common.EF.Api;
using Common.EF.EF;
using Common.EF.EF.entity;
using Common.uniwork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Common.EF.Service
{
    public abstract class FAppService<TEntity, TKey, TCreateDto, TUpdateDto, TResponseDto> : AppService,
    IBaseAppService<TEntity, TKey, TCreateDto, TUpdateDto, TResponseDto>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
    where TCreateDto : class
    where TUpdateDto : class
    where TResponseDto : class
    {
        protected readonly IRepository<TEntity, TKey> _repository;

        protected FAppService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger logger,
            ICurrentUser currentUser)
            : base(unitOfWork, mapper, logger, currentUser)
        {
            _repository = unitOfWork.Repository<TEntity, TKey>();
        }

        #region Query Operations

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public virtual async Task<ApiResponse<TResponseDto>> GetByIdAsync(TKey id)
        {
            return await ExecuteWithLoggingAsync(async () =>
            {
                var entity = await _repository.GetByIdAsync(id, CancellationToken);
                if (entity == null)
                {
                    return ApiResponse<TResponseDto>.NotFound($"{typeof(TEntity).Name} with id {id} not found");
                }

                var dto = _mapper.Map<TResponseDto>(entity);
                return ApiResponse<TResponseDto>.Success(dto);
            }, nameof(GetByIdAsync), new { Id = id });
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public virtual async Task<ApiResponse<IReadOnlyList<TResponseDto>>> GetAllAsync()
        {
            return await ExecuteWithLoggingAsync(async () =>
            {
                var entities = await _repository.GetAllAsync(CancellationToken);
                var dtos = _mapper.Map<IReadOnlyList<TResponseDto>>(entities);
                return ApiResponse<IReadOnlyList<TResponseDto>>.Success(dtos);
            }, nameof(GetAllAsync));
        }

        /// <summary>
        /// 分页获取实体
        /// </summary>
        public virtual async Task<ApiResponse<PagedResult<TResponseDto>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null)
        {
            return await ExecuteWithLoggingAsync(async () =>
            {
                var predicate = BuildSearchPredicate(searchTerm);
                var pagedResult = await _repository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    predicate,
                    ApplyDefaultOrder,
                    CancellationToken);

                var pagedResponse = new PagedResult<TResponseDto>
                {
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount,
                    Items = _mapper.Map<IReadOnlyList<TResponseDto>>(pagedResult.Items)
                };

                return ApiResponse<PagedResult<TResponseDto>>.Success(pagedResponse);
            }, nameof(GetPagedAsync), new { pageNumber, pageSize, searchTerm });
        }

        /// <summary>
        /// 根据条件查找多个实体
        /// </summary>
        public virtual async Task<ApiResponse<IReadOnlyList<TResponseDto>>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await ExecuteWithLoggingAsync(async () =>
            {
                var entities = await _repository.FindAsync(predicate, CancellationToken);
                var dtos = _mapper.Map<IReadOnlyList<TResponseDto>>(entities);
                return ApiResponse<IReadOnlyList<TResponseDto>>.Success(dtos);
            }, nameof(FindAsync));
        }

        /// <summary>
        /// 根据条件查找单个实体
        /// </summary>
        public virtual async Task<ApiResponse<TResponseDto>> FindOneAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await ExecuteWithLoggingAsync(async () =>
            {
                var entity = await _repository.FindOneAsync(predicate, CancellationToken);
                if (entity == null)
                {
                    return ApiResponse<TResponseDto>.NotFound("Entity not found");
                }

                var dto = _mapper.Map<TResponseDto>(entity);
                return ApiResponse<TResponseDto>.Success(dto);
            }, nameof(FindOneAsync));
        }

        #endregion

        #region Write Operations

        /// <summary>
        /// 创建实体
        /// </summary>
        public virtual async Task<ApiResponse<TResponseDto>> CreateAsync(TCreateDto createDto)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                // 1. 验证
                var validationResult = await ValidateCreateAsync(createDto);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<TResponseDto>.ValidationError(validationResult.Errors);
                }

                // 2. 创建实体（优先调用自定义创建方法）
                TEntity entity;

                // 尝试调用子类重写的创建方法
                var customEntity = await CreateEntityAsync(createDto);
                if (customEntity != null)
                {
                    entity = customEntity;
                }
                else
                {
                    // 默认使用 AutoMapper 映射
                    entity = _mapper.Map<TEntity>(createDto);
                }

                // 3. 设置审计信息
                SetAuditInfoForCreate(entity);

                // 4. 保存
                await _repository.AddAsync(entity, CancellationToken);
                await _unitOfWork.SaveChangesAsync(CancellationToken);

                // 5. 后处理
                await AfterCreateAsync(entity, createDto);

                // 6. 返回
                var dto = _mapper.Map<TResponseDto>(entity);
                return ApiResponse<TResponseDto>.Success(dto, "创建成功", 201);
            }, nameof(CreateAsync));
        }


        /// <summary>
        /// 更新实体
        /// </summary>
        public virtual async Task<ApiResponse<TResponseDto>> UpdateAsync(TKey id, TUpdateDto updateDto)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                // 获取实体
                var entity = await _repository.GetByIdAsync(id, CancellationToken);
                if (entity == null)
                {
                    return ApiResponse<TResponseDto>.NotFound($"{typeof(TEntity).Name} with id {id} not found");
                }

                // 验证
                var validationResult = await ValidateUpdateAsync(entity, updateDto);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<TResponseDto>.ValidationError(validationResult.Errors);
                }

                // 映射更新
                _mapper.Map(updateDto, entity);

                // 设置审计信息
                SetAuditInfoForUpdate(entity);

                // 保存
                await _repository.UpdateAsync(entity, CancellationToken);
                await _unitOfWork.SaveChangesAsync(CancellationToken);

                // 后处理
                await AfterUpdateAsync(entity, updateDto);

                // 返回
                var dto = _mapper.Map<TResponseDto>(entity);
                return ApiResponse<TResponseDto>.Success(dto, "更新成功");
            }, nameof(UpdateAsync));
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public virtual async Task<ApiResponse<bool>> DeleteAsync(TKey id)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var entity = await _repository.GetByIdAsync(id, CancellationToken);
                if (entity == null)
                {
                    return ApiResponse<bool>.NotFound($"{typeof(TEntity).Name} with id {id} not found");
                }

                var validationResult = await ValidateDeleteAsync(entity);
                if (!validationResult.IsValid)
                {
                    return ApiResponse<bool>.ValidationError(validationResult.Errors);
                }

                var result = await _repository.DeleteByIdAsync(id, CancellationToken);
                await _unitOfWork.SaveChangesAsync(CancellationToken);

                await AfterDeleteAsync(entity);

                return result
                    ? ApiResponse<bool>.Success(true, "删除成功")
                    : ApiResponse<bool>.Error("删除失败");
            }, nameof(DeleteAsync));
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        public virtual async Task<ApiResponse<int>> BatchDeleteAsync(IEnumerable<TKey> ids)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var idList = ids.ToList();
                if (!idList.Any())
                {
                    return ApiResponse<int>.Success(0, "没有要删除的记录");
                }

                var entities = await _repository.FindAsync(e => idList.Contains(e.Id), CancellationToken);

                foreach (var entity in entities)
                {
                    var validationResult = await ValidateDeleteAsync(entity);
                    if (!validationResult.IsValid)
                    {
                        return ApiResponse<int>.ValidationError(validationResult.Errors);
                    }
                }

                var result = await _repository.DeleteBatchAsync(e => idList.Contains(e.Id), CancellationToken);
                await _unitOfWork.SaveChangesAsync(CancellationToken);

                return ApiResponse<int>.Success(result, $"成功删除 {result} 条记录");
            }, nameof(BatchDeleteAsync));
        }

        /// <summary>
        /// 软删除
        /// </summary>
        public virtual async Task<ApiResponse<bool>> SoftDeleteAsync(TKey id)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var result = await _repository.SoftDeleteAsync(id, CurrentUser.UserId, CancellationToken);
                await _unitOfWork.SaveChangesAsync(CancellationToken);

                return result
                    ? ApiResponse<bool>.Success(true, "软删除成功")
                    : ApiResponse<bool>.NotFound("实体不存在或不支持软删除");
            }, nameof(SoftDeleteAsync));
        }

        /// <summary>
        /// 检查是否存在
        /// </summary>
        public virtual async Task<ApiResponse<bool>> ExistsAsync(TKey id)
        {
            var result = await _repository.AnyAsync(e => e.Id.Equals(id), CancellationToken);
            return ApiResponse<bool>.Success(result);
        }

        #endregion

        #region Virtual Methods for Customization

        /// <summary>构建搜索条件</summary>
        protected virtual Expression<Func<TEntity, bool>>? BuildSearchPredicate(string? searchTerm)
        {
            return null;
        }

        /// <summary>应用默认排序</summary>
        protected virtual IOrderedQueryable<TEntity> ApplyDefaultOrder(IQueryable<TEntity> query)
        {
            return query.OrderBy(e => Microsoft.EntityFrameworkCore.EF.Property<object>(e, "Id"));
        }

        /// <summary>创建前验证（返回验证结果）</summary>
        protected virtual Task<ValidationResult> ValidateCreateAsync(TCreateDto createDto)
        {
            return Task.FromResult(ValidationResult.Success);
        }

        /// <summary>更新前验证（返回验证结果）</summary>
        protected virtual Task<ValidationResult> ValidateUpdateAsync(TEntity entity, TUpdateDto updateDto)
        {
            return Task.FromResult(ValidationResult.Success);
        }

        /// <summary>删除前验证（返回验证结果）</summary>
        protected virtual Task<ValidationResult> ValidateDeleteAsync(TEntity entity)
        {
            return Task.FromResult(ValidationResult.Success);
        }

        /// <summary>设置创建时的审计信息</summary>
        protected virtual void SetAuditInfoForCreate(TEntity entity)
        {
            if (entity is IAuditableEntity auditable)
            {
                auditable.CreatedAt = DateTime.UtcNow;
                auditable.CreatedBy = CurrentUser.UserId;
                auditable.UpdatedAt = DateTime.UtcNow;
                auditable.UpdatedBy = CurrentUser.UserId;
            }
        }

        /// <summary>设置更新时的审计信息</summary>
        protected virtual void SetAuditInfoForUpdate(TEntity entity)
        {
            if (entity is IAuditableEntity auditable)
            {
                auditable.UpdatedAt = DateTime.UtcNow;
                auditable.UpdatedBy = CurrentUser.UserId;
            }
        }
        /// <summary>
        /// 创建实体的方法（子类可重写，用于调用实体的工厂方法）
        /// </summary>
        /// <param name="createDto">创建DTO</param>
        /// <returns>创建好的实体，返回null则使用AutoMapper</returns>
        protected virtual Task<TEntity?> CreateEntityAsync(TCreateDto createDto)
        {
            return Task.FromResult<TEntity?>(null);
        }
        /// <summary>创建后处理</summary>
        protected virtual Task AfterCreateAsync(TEntity entity, TCreateDto createDto)
        {
            return Task.CompletedTask;
        }

        /// <summary>更新后处理</summary>
        protected virtual Task AfterUpdateAsync(TEntity entity, TUpdateDto updateDto)
        {
            return Task.CompletedTask;
        }

        /// <summary>删除后处理</summary>
        protected virtual Task AfterDeleteAsync(TEntity entity)
        {
            return Task.CompletedTask;
        }

        #endregion
       
    }
}