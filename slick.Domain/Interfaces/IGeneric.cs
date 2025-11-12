using slick.Domain.Entities;
using System.Linq.Expressions;

namespace slick.Domain.Interfaces
{
    public interface IGeneric<TEntity> where TEntity : class
    {
        Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task ClearChildCollectionAsync<TChild>(object parentId, Expression<Func<TEntity, ICollection<TChild>>> collectionSelector, string idPropertyName = "Id") where TChild : class;
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
        Task<int> CountByAttributeAsync<TValue>(string propertyName, TValue value, CancellationToken cancellationToken = default);
        Task<int> CountDeletedAsync(CancellationToken cancellationToken = default);
        Task<int> CountWithNavigationAsync<TNavigation>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TNavigation?>> navigationProperty, Expression<Func<TNavigation, bool>>? navigationPredicate = null, CancellationToken cancellationToken = default) where TNavigation : class;
        Task<int> DeleteAsync(Guid id, string userid, CancellationToken cancellationToken = default);
        Task<int> DeleteByStringIdAsync(string id, string userId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string propertyName, object value, CancellationToken cancellationToken = default);
        Task<bool> ExistsByMultipleColumnsAsync(Dictionary<string, object> columnValues, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByIdWithIncludeAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetByRelationalIdAsync<TKey>(string foreignKeyPropertyName, TKey foreignKeyValue, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetByRelationalIdWithIncludeAsync<TKey, TProperty>(string foreignKeyPropertyName, TKey foreignKeyValue, Expression<Func<TEntity, TProperty>> singleEntitySelector, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByStringIdAsync(string id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetByTwoColumnsAsync<TFilter1, TFilter2>(Expression<Func<TEntity, TFilter1>> column1Property, TFilter1 column1Value, Expression<Func<TEntity, TFilter2>> column2Property, TFilter2 column2Value, CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetExpiredTrashAsync(int daysThreshold = 45, CancellationToken cancellationToken = default);
        Task<TEntity?> GetLastInsertedByRelationalAsync<TKey>(string? foreignKeyPropertyName = null, TKey? foreignKeyValue = default, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetPagedAsync(string? search, Expression<Func<TEntity, bool>>? baseFilter = null, List<Expression<Func<TEntity, string>>>? searchProperties = null, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes);
        Task<List<TEntity>> GetPagedWithThenIncludeAsync<TProperty>(string? search, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionSelector, Expression<Func<TProperty, object>> thenIncludeSelector, Expression<Func<TEntity, bool>>? baseFilter = null, List<Expression<Func<TEntity, string>>>? searchProperties = null, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes) where TProperty : class;
        Task<List<TEntity>> GetPagedWithThenIncludeNewAsync<TProperty>(string? search, Expression<Func<TEntity, IEnumerable<TProperty>>> collectionSelector, Expression<Func<TProperty, object>> thenIncludeSelector, Expression<Func<TEntity, bool>>? baseFilter = null, List<Expression<Func<TEntity, string>>>? searchProperties = null, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes) where TProperty : class;
        Task<IEnumerable<TResult>> GetPropertyValuesAsync<TResult>(Expression<Func<TEntity, TResult>> selectExpression, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetRoleAllAsync(CancellationToken cancellationToken = default);
        Task<List<TEntity>> GetTrashedAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
        Task<TResult?> GetXByYAsync<TFilter, TResult>(Expression<Func<TEntity, TFilter>> yProperty, TFilter yValue, Expression<Func<TEntity, TResult>> xProperty, CancellationToken cancellationToken = default);
        Task<List<TResult>> GetXListByTwoColumnsAsync<TFilter1, TFilter2, TResult>(Expression<Func<TEntity, TFilter1>> column1Property, TFilter1 column1Value, Expression<Func<TEntity, TFilter2>> column2Property, TFilter2 column2Value, Expression<Func<TEntity, TResult>> selectProperty, CancellationToken cancellationToken = default);
        Task<List<TResult>> GetXListByYAsync<TFilter, TResult>(Expression<Func<TEntity, TFilter>> yProperty, TFilter yValue, Expression<Func<TEntity, TResult>> xProperty, CancellationToken cancellationToken = default);
        Task<int> HardDeleteAsync(Guid id);
        Task<int> HardDeleteByStringIdAsync(string id);
        Task<int> HardDeleteonDeleteCascadeAsync(Guid id);
        Task<int> HardDeleteonDeleteCascadeofstringAsync(string id);
        Task<int> HardDeletewithcheckAsync(Guid id);
        Task<int> MultiHardDeleteAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiHardDeleteByStringIdAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverAsync(List<Guid> ids, CancellationToken cancellationToken = default);
        Task<int> MultiRecoverByStringIdAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteAsync(List<Guid> ids, Guid userid, CancellationToken cancellationToken = default);
        Task<int> MultiSoftDeleteByStringIdAsync(List<string> ids, Guid userId, CancellationToken cancellationToken = default);
        IQueryable<TEntity> Query();
        Task<int> RecoverAsync(Guid id, CancellationToken cancellationToken = default);
        Task<int> RecoverByStringIdAsync(string id, CancellationToken cancellationToken = default);
        Task<decimal?> SumAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, decimal?>> selector, CancellationToken cancellationToken = default);
        Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        void UpdateCollection<TNavEntity>(ICollection<TNavEntity> existingCollection, IEnumerable<Guid> newIds, Func<Guid, TNavEntity> createEntityFunc) where TNavEntity : class;
        Task<int> UpdateEntityWithCollectionsAsync(TEntity entity, Dictionary<string, IEnumerable<Guid>> collectionUpdates, CancellationToken cancellationToken = default);
        Task UpdateNavigationPropertiesAsync<TDto>(TEntity existing, TDto dto, CancellationToken cancellationToken = default) where TDto : class;
     }
}
