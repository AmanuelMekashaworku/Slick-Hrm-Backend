    using slick.Domain.Entities;
    using slick.Domain.Interfaces;
    using slick.infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;
    using System.Reflection;

    namespace slick.infrastructure.Repositories
    {
        public class GenericRepository<TEntity> : IGeneric<TEntity> where TEntity : class
        {
            private readonly AppDbContext _context;
            private readonly DbSet<TEntity> _dbSet;
            private readonly bool _hasIsDeletedProperty;


            public GenericRepository(AppDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _dbSet = context.Set<TEntity>();
                _hasIsDeletedProperty = typeof(TEntity).GetProperty("IsDeleted") != null;
            }
            public async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                await _dbSet.AddAsync(entity, cancellationToken);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                _dbSet.Update(entity);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            }
            public async Task<TEntity?> GetByIdWithIncludeAsync( Guid id, CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            {
                var query = _dbSet.AsQueryable();

                if (includes != null && includes.Length > 0)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                // Find the correct property name (supports both "ID" and "Id")
                var idPropertyName = GetIdPropertyName();
                return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, idPropertyName) == id, cancellationToken);
            }

            private string GetIdPropertyName()
            {
                var properties = typeof(TEntity).GetProperties();

                // Prefer "ID" over "Id" if both exist (you can reverse this preference)
                var idProperty = properties.FirstOrDefault(p => p.Name == "ID")
                               ?? properties.FirstOrDefault(p => p.Name == "Id");

                if (idProperty == null)
                    throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not have an ID or Id property");

                if (idProperty.PropertyType != typeof(Guid))
                    throw new InvalidOperationException($"Entity {typeof(TEntity).Name} has {idProperty.Name} property but it's not of type Guid");

                return idProperty.Name;
            }
        public async Task<bool> ExistsAsync(string propertyName, object value, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new ArgumentNullException(nameof(propertyName));

                var property = typeof(TEntity).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    throw new ArgumentException($"Property '{propertyName}' does not exist on entity '{typeof(TEntity).Name}'.");

                // Build expression: x => EF.Property<object>(x, propertyName) == value
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var propertyAccess = Expression.Property(parameter, property);
                var constant = Expression.Constant(value);
                var equals = Expression.Equal(propertyAccess, constant);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                return await _dbSet.AnyAsync(lambda, cancellationToken);
            }
            public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
            {
                if (includes == null || !includes.Any())
                    return await GetByIdAsync(id, cancellationToken);

                var query = _dbSet.AsQueryable();

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
            }
            public async Task<TEntity?> GetByStringIdAsync(string id,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("ID cannot be null or empty", nameof(id));

                // If no includes, fetch directly for better performance
                if (includes == null || !includes.Any())
                    return await _dbSet.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);

                // Apply includes if specified
                var query = _dbSet.AsQueryable();

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id, cancellationToken);
            }
            public async Task<IEnumerable<TEntity>> GetRoleAllAsync(CancellationToken cancellationToken = default)
            {
                var query = _dbSet.AsNoTracking();

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                // Filter permissionTitle that contains "Create" or "View"
                query = query.Where(e =>
                    EF.Property<string>(e, "PermissionTitle").Contains("Create") ||
                    EF.Property<string>(e, "PermissionTitle").Contains("View") ||
                     EF.Property<string>(e, "PermissionTitle").Contains("Approve") ||
                     EF.Property<string>(e, "PermissionTitle").Contains("Authorize") ||
                     EF.Property<string>(e, "PermissionTitle").Contains("Check")
                );

                return await query.ToListAsync(cancellationToken);
            }
            public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                var query = _dbSet.AsNoTracking();

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                return await query.ToListAsync(cancellationToken);
            }
            public async Task<List<TEntity>> GetPagedAsync(string? search,Expression<Func<TEntity, bool>>? baseFilter = null, List<Expression<Func<TEntity, string>>>? searchProperties = null,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            {
                IQueryable<TEntity> query = _dbSet.AsQueryable();

                if (includes != null && includes.Length > 0)
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (baseFilter != null)
                {
                    query = query.Where(baseFilter);
                }

                if (!string.IsNullOrWhiteSpace(search) && searchProperties != null && searchProperties.Any())
                {
                    query = ApplySearch(query, search, searchProperties);
                }

                return await query.AsNoTracking().ToListAsync(cancellationToken);
            }
            public async Task<int> DeleteAsync(Guid id,string userid, CancellationToken cancellationToken = default)
            {
                var entity = await GetByIdAsync(id, cancellationToken);
                if (entity == null) return 0;

                // Convert string to Guid first
                if (!Guid.TryParse(userid, out Guid userIdGuid))
                {
                    throw new ArgumentException("Invalid user ID format", nameof(userid));
                }

                dynamic dynamicEntity = entity;
                dynamicEntity.IsDeleted = true;
                dynamicEntity.IsActive = false;
                dynamicEntity.DeletedDate = DateTime.Now;
                dynamicEntity.DeletedBy = userIdGuid;
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> DeleteByStringIdAsync(string id, string userId, CancellationToken cancellationToken = default)
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("ID cannot be empty", nameof(id));

                // Fetch entity directly by string ID (NO Guid conversion)
                var entity = await GetByStringIdAsync(id, cancellationToken);
                if (entity == null)
                    return 0; // No record found

                // Validate userId (if it must be a Guid)
                if (!Guid.TryParse(userId, out Guid userIdGuid))
                    throw new ArgumentException("Invalid user ID format", nameof(userId));

                // Apply soft-delete
                dynamic dynamicEntity = entity;
                dynamicEntity.IsDeleted = true;
                dynamicEntity.IsActive = false;
                dynamicEntity.DeletedDate = DateTime.UtcNow;
                dynamicEntity.DeletedBy = userIdGuid;

                // Save changes
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> MultiSoftDeleteAsync(List<Guid> ids, Guid userId, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any())
                    return 0;

                var idProperty = typeof(TEntity).GetProperty("ID") ??
                    throw new InvalidOperationException("Entity does not have an ID property");

                var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted") ??
                    throw new InvalidOperationException("Entity does not support soft delete (missing IsDeleted property)");
                var isActiveProperty = typeof(TEntity).GetProperty("IsActive") ??
                    throw new InvalidOperationException("Entity does not support soft delete (missing IsActive property)");

            var deletedDateProperty = typeof(TEntity).GetProperty("DeletedDate");
                var deletedByProperty = typeof(TEntity).GetProperty("DeletedBy");

                // More efficient query - filters in database rather than memory
                var entities = await _dbSet
                    .Where(e => ids.Contains(EF.Property<Guid>(e, "ID")))
                    .ToListAsync(cancellationToken);

                if (entities.Count == 0)
                    return 0;

                foreach (var entity in entities)
                {
                    isDeletedProperty.SetValue(entity, true);
                    isActiveProperty.SetValue(entity, false);

                    if (deletedDateProperty != null)
                    {
                        deletedDateProperty.SetValue(entity, DateTime.UtcNow); // Use UtcNow for consistency
                    }

                    if (deletedByProperty != null)
                    {
                        deletedByProperty.SetValue(entity, userId); // Direct Guid assignment
                    }
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> MultiSoftDeleteByStringIdAsync(List<string> ids, Guid userId, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any())
                    return 0;

                var idProperty = typeof(TEntity).GetProperty("Id") ??
                    throw new InvalidOperationException("Entity does not have an ID property");

                var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted") ??
                    throw new InvalidOperationException("Entity does not support soft delete (missing IsDeleted property)");
            var isActiveProperty = typeof(TEntity).GetProperty("IsActive") ??
                   throw new InvalidOperationException("Entity does not support soft delete (missing IsActive property)");

            var deletedDateProperty = typeof(TEntity).GetProperty("DeletedDate");
                var deletedByProperty = typeof(TEntity).GetProperty("DeletedBy");

                // More efficient query - filter in database rather than memory
                var entities = await _dbSet
                    .Where(e => ids.Contains(EF.Property<string>(e, "Id")))
                    .ToListAsync(cancellationToken);

                if (entities.Count == 0)
                    return 0;

                foreach (var entity in entities)
                {
                    isDeletedProperty.SetValue(entity, true);
                    isActiveProperty.SetValue(entity, false);

                if (deletedDateProperty != null)
                        deletedDateProperty.SetValue(entity, DateTime.UtcNow); // Use UtcNow for consistency

                    if (deletedByProperty != null)
                        deletedByProperty.SetValue(entity, userId); // Still Guid
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<List<TEntity>> GetTrashedAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default)
            {
                return await _dbSet
                    .Where(e => EF.Property<bool>(e, "IsDeleted"))
                    .Where(filter)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            public async Task<int> RecoverAsync(Guid id, CancellationToken cancellationToken = default)
            {
                var entity = await _dbSet.FirstOrDefaultAsync(e =>
                    EF.Property<Guid>(e, "ID") == id &&
                    EF.Property<bool>(e, "IsDeleted") &&
                    !EF.Property<bool>(e, "IsActive"),
                    cancellationToken);

                if (entity == null) return 0;

                _context.Entry(entity).Property("IsDeleted").CurrentValue = false;
                _context.Entry(entity).Property("IsActive").CurrentValue = true;
                _context.Entry(entity).Property("DeletedDate").CurrentValue = null;
                _context.Entry(entity).Property("ModifiedBy").CurrentValue = null;

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> RecoverByStringIdAsync(String id, CancellationToken cancellationToken = default)
            {
                var entity = await _dbSet.FirstOrDefaultAsync(e =>
                    EF.Property<String>(e, "Id") == id &&
                    EF.Property<bool>(e, "IsDeleted") &&
                    !EF.Property<bool>(e, "IsActive"),
                    cancellationToken);

                if (entity == null) return 0;

                _context.Entry(entity).Property("IsDeleted").CurrentValue = false;
                _context.Entry(entity).Property("IsActive").CurrentValue = true;
                _context.Entry(entity).Property("DeletedDate").CurrentValue = null;
                _context.Entry(entity).Property("ModifiedBy").CurrentValue = null;

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> MultiRecoverAsync(List<Guid> ids, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any()) return 0;

                var entities = await _dbSet.Where(e =>
                    ids.Contains(EF.Property<Guid>(e, "ID")) &&
                    EF.Property<bool>(e, "IsDeleted") &&
                    !EF.Property<bool>(e, "IsActive"))
                    .ToListAsync(cancellationToken);

                if (!entities.Any()) return 0;

                foreach (var entity in entities)
                {
                    _context.Entry(entity).Property("IsDeleted").CurrentValue = false;
                    _context.Entry(entity).Property("IsActive").CurrentValue = true;
                    _context.Entry(entity).Property("DeletedDate").CurrentValue = null;
                    _context.Entry(entity).Property("ModifiedBy").CurrentValue = null;
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> MultiRecoverByStringIdAsync(List<string> ids, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any())
                    return 0;

                var entities = await _dbSet
                    .Where(e =>
                        ids.Contains(EF.Property<string>(e, "Id")) &&
                        EF.Property<bool>(e, "IsDeleted") &&
                        !EF.Property<bool>(e, "IsActive"))
                    .ToListAsync(cancellationToken);

                if (!entities.Any())
                    return 0;

                foreach (var entity in entities)
                {
                    _context.Entry(entity).Property("IsDeleted").CurrentValue = false;
                    _context.Entry(entity).Property("IsActive").CurrentValue = true;
                    _context.Entry(entity).Property("DeletedDate").CurrentValue = null;
                    _context.Entry(entity).Property("ModifiedBy").CurrentValue = null;
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> HardDeleteAsync(Guid id)
            {
                try
                {
                    // Find the entity
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                    {
                        return 0; // Entity not found
                    }

                    // Get entity type metadata
                    var entityType = _context.Model.FindEntityType(entity.GetType());
                    if (entityType == null)
                    {
                        throw new InvalidOperationException("Entity type not found in DbContext model.");
                    }

                    // Check all collection navigation properties
                    foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection))
                    {
                        // Load the collection to check for related records
                        await _context.Entry(entity).Collection(navigation.Name).LoadAsync();
                        var propertyValue = navigation.PropertyInfo?.GetValue(entity);
                        if (propertyValue is IEnumerable<object> collection && collection.Any())
                        {
                            throw new InvalidOperationException($"Cannot delete entity because it has related records in {navigation.Name}.");
                        }
                    }

                    // No child relationships found, proceed with deletion
                    _dbSet.Remove(entity);
                    return await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException("Cannot delete entity due to database constraints.", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while attempting to delete the entity.", ex);
                }
            }
            public async Task<int> HardDeletewithcheckAsync(Guid id)
        {
            try
            {
                var entity = await _dbSet.FindAsync(id);
                if (entity == null)
                {
                    return 0;
                }

                _dbSet.Remove(entity);
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx) when (IsForeignKeyConstraintViolation(dbEx))
            {
                throw new InvalidOperationException("Cannot delete entity because it is referenced by other records.", dbEx);
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("Cannot delete entity due to database constraints.", dbEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while attempting to delete the entity.", ex);
            }
        }
            private bool IsForeignKeyConstraintViolation(DbUpdateException ex)
            {
                return ex.InnerException is SqlException sqlEx && sqlEx.Number == 547;
            }
            public async Task<int> HardDeleteonDeleteCascadeAsync(Guid id)
                {
                    try
                    {
                        var entity = await _dbSet.FindAsync(id);
                        if (entity == null)
                        {
                            return 0; // not found
                        }

                        _dbSet.Remove(entity);
                        return await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException ex)
                    {
                        throw new InvalidOperationException("Cannot delete entity due to database constraints.", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("An error occurred while attempting to delete the entity.", ex);
                    }
                }
            public async Task<int> HardDeleteonDeleteCascadeofstringAsync(string id)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(id))
                        return 0; // invalid id

                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                    {
                        return 0; // not found
                    }

                    _dbSet.Remove(entity);
                    return await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException("Cannot delete entity due to database constraints.", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while attempting to delete the entity.", ex);
                }
            }
            public async Task<int> HardDeleteByStringIdAsync(string id)
            {
                try
                {
                    // Find the entity (string ID)
                    var entity = await _dbSet.FindAsync(id);
                    if (entity == null)
                    {
                        return 0; // Entity not found
                    }

                    // Get entity type metadata
                    var entityType = _context.Model.FindEntityType(entity.GetType());
                    if (entityType == null)
                    {
                        throw new InvalidOperationException("Entity type not found in DbContext model.");
                    }

                    // Check all collection navigation properties
                    foreach (var navigation in entityType.GetNavigations().Where(n => n.IsCollection))
                    {
                        // Load the collection to check for related records
                        await _context.Entry(entity).Collection(navigation.Name).LoadAsync();
                        var propertyValue = navigation.PropertyInfo?.GetValue(entity);
                        if (propertyValue is IEnumerable<object> collection && collection.Any())
                        {
                            throw new InvalidOperationException(
                                $"Cannot delete entity because it has related records in {navigation.Name}."
                            );
                        }
                    }

                    // No child relationships found, proceed with deletion
                    _dbSet.Remove(entity);
                    return await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException("Cannot delete entity due to database constraints.", ex);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while attempting to delete the entity.", ex);
                }
            }
            public async Task<int> MultiHardDeleteAsync(List<Guid> ids, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any())
                    return 0;

                var entityType = typeof(TEntity);
                var idProperty = entityType.GetProperty("ID") ??
                    throw new InvalidOperationException("Entity does not have an ID property");

                var parameter = Expression.Parameter(entityType, "e");
                var propertyAccess = Expression.Property(parameter, idProperty);
                var containsMethod = typeof(List<Guid>).GetMethod(nameof(List<Guid>.Contains), new[] { typeof(Guid) })!;
                var idsConstant = Expression.Constant(ids);
                var containsCall = Expression.Call(idsConstant, containsMethod, propertyAccess);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);

                var entities = await _dbSet.Where(lambda).ToListAsync(cancellationToken);

                if (entities.Count == 0)
                    return 0;

                _dbSet.RemoveRange(entities);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<int> MultiHardDeleteByStringIdAsync(List<string> ids, CancellationToken cancellationToken = default)
            {
                if (ids == null || !ids.Any())
                    return 0;

            var entityType = typeof(TEntity);
            var idProperty = entityType.GetProperty("Id")
                           ?? entityType.GetProperty("ID")
                           ?? throw new InvalidOperationException("Entity does not have an Id or ID property.");


            var parameter = Expression.Parameter(entityType, "e");
                var propertyAccess = Expression.Property(parameter, idProperty);

                // Change to List<string>
                var containsMethod = typeof(List<string>).GetMethod(nameof(List<string>.Contains), new[] { typeof(string) })!;
                var idsConstant = Expression.Constant(ids);
                var containsCall = Expression.Call(idsConstant, containsMethod, propertyAccess);

                var lambda = Expression.Lambda<Func<TEntity, bool>>(containsCall, parameter);

                var entities = await _dbSet.Where(lambda).ToListAsync(cancellationToken);

                if (entities.Count == 0)
                    return 0;

                _dbSet.RemoveRange(entities);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            public async Task<List<TEntity>> GetExpiredTrashAsync(int daysThreshold = 45,CancellationToken cancellationToken = default)
            {
                var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted") ??
                    throw new InvalidOperationException("Entity does not support soft delete (missing IsDeleted property)");

                var deletedDateProperty = typeof(TEntity).GetProperty("DeletedDate") ??
                    throw new InvalidOperationException("Entity does not have DeletedDate property");

                var cutoffDate = DateTime.Now.AddDays(-daysThreshold);

                return await _dbSet
                    .Where(e => EF.Property<bool>(e, "IsDeleted") && 
                               EF.Property<DateTime>(e, "DeletedDate") <= cutoffDate)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            //public async Task<int> CountDeletedAsync(CancellationToken cancellationToken = default)
            //{
            //    if (!_hasIsDeletedProperty)
            //    {
            //        throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not support soft delete (missing IsDeleted property).");
            //    }

            //    return await _dbSet
            //        .CountAsync(e => EF.Property<bool>(e, "IsDeleted"), cancellationToken);
            //}
            public async Task<int> CountDeletedAsync(CancellationToken cancellationToken = default)
            {
                if (!_hasIsDeletedProperty)
                {
                    throw new InvalidOperationException($"Entity {typeof(TEntity).Name} does not support soft delete (missing IsDeleted property).");
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-45);

                return await _dbSet.CountAsync(e =>
                    EF.Property<bool>(e, "IsDeleted") &&
                    (
                        EF.Property<DateTime?>(e, "DeletedDate") == null ||
                        EF.Property<DateTime?>(e, "DeletedDate") >= cutoffDate
                    ),
                    cancellationToken);
            }

            public async Task<IEnumerable<TResult>> GetPropertyValuesAsync<TResult>(Expression<Func<TEntity, TResult>> selectExpression, CancellationToken cancellationToken = default)
            {
                // Execute query
                return await _dbSet
                    .Select(selectExpression)
                    .ToListAsync(cancellationToken);
            }
            private static readonly MethodInfo _toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
            private static readonly MethodInfo _containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            private static IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string searchTerm,List<Expression<Func<TEntity, string>>> searchProperties)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");
                var searchTermExpression = Expression.Constant(searchTerm.ToLower());

                Expression? combinedExpression = null;

                foreach (var propertyExpression in searchProperties)
                {
                    var memberExpression = (MemberExpression)propertyExpression.Body;
                    var propertyAccess = Expression.Property(parameter, memberExpression.Member.Name);

                    var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
                    var toLowerCall = Expression.Call(propertyAccess, _toLowerMethod);
                    var containsCall = Expression.Call(toLowerCall, _containsMethod, searchTermExpression);

                    var propertyCondition = Expression.AndAlso(nullCheck, containsCall);

                    combinedExpression = combinedExpression == null
                        ? propertyCondition
                        : Expression.OrElse(combinedExpression, propertyCondition);
                }

                if (combinedExpression == null)
                    return query;

                var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
                return query.Where(lambda);
            }
            public IQueryable<TEntity> Query()
            {
                return _dbSet.AsQueryable();
            }
            public async Task<IEnumerable<TEntity>> GetByRelationalIdAsync<TKey>(string foreignKeyPropertyName,TKey foreignKeyValue,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var property = Expression.Property(parameter, foreignKeyPropertyName);

                Expression equals;

                if (typeof(TKey) == typeof(Guid))
                {
                    // Ensure Guid comparison works properly
                    equals = Expression.Equal(
                        property,
                        Expression.Constant(foreignKeyValue, typeof(Guid))
                    );
                }
                else
                {
                    equals = Expression.Equal(
                        property,
                        Expression.Constant(foreignKeyValue, typeof(TKey))
                    );
                }

                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                var query = _dbSet.AsQueryable();

                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                return await query
                    .Where(lambda)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            public async Task<IEnumerable<TEntity>> GetByRelationalIdWithIncludeAsync<TKey, TProperty>(string foreignKeyPropertyName,TKey foreignKeyValue,Expression<Func<TEntity, TProperty>> singleEntitySelector,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "e");
                var property = Expression.Property(parameter, foreignKeyPropertyName);

                // Convert foreignKeyValue to the property type
                var convertedValue = Expression.Convert(Expression.Constant(foreignKeyValue), property.Type);

                var equals = Expression.Equal(property, convertedValue);

                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                IQueryable<TEntity> query = _context.Set<TEntity>();

                // Apply includes
                if (includes != null && includes.Any())
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }

                // Apply soft delete filter if applicable
                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                return await query
                    .Where(lambda)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
            }
            public async Task<TEntity?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
            {
                IQueryable<TEntity> query = _dbSet;

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }

                // assume entity always has property "ID" (Guid)
                return await query.FirstOrDefaultAsync(e =>
                    EF.Property<Guid>(e, "ID") == id);
            }
            // In your CarPerformaRepository
            // Add these methods to your existing GenericRepository class
            public async Task<int> UpdateWithNavigationAsync(TEntity entity, Action<TEntity>? navigationUpdates = null, CancellationToken cancellationToken = default)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));

                // Attach entity if not tracked
                if (_context.Entry(entity).State == EntityState.Detached)
                {
                    _dbSet.Attach(entity);
                }

                // Mark as modified
                _context.Entry(entity).State = EntityState.Modified;

                // Apply navigation updates if provided
                navigationUpdates?.Invoke(entity);

                return await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            public async Task UpdateNavigationPropertiesAsync<TDto>(TEntity existing, TDto dto, CancellationToken cancellationToken = default) where TDto : class
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                if (dto == null) throw new ArgumentNullException(nameof(dto));

                var entityType = typeof(TEntity);
                var dtoType = typeof(TDto);

                // Get all collection properties from both entity and DTO
                var entityCollectionProps = entityType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                    .ToList();

                var dtoCollectionProps = dtoType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType &&
                               p.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    .ToList();

                foreach (var entityProp in entityCollectionProps)
                {
                    var dtoProp = dtoCollectionProps.FirstOrDefault(p =>
                        p.Name.Equals(entityProp.Name, StringComparison.OrdinalIgnoreCase) ||
                        p.Name.Equals(entityProp.Name + "Ids", StringComparison.OrdinalIgnoreCase));

                    if (dtoProp != null)
                    {
                        await UpdateCollectionPropertyAsync(existing, dto, entityProp, dtoProp, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            private async Task UpdateCollectionPropertyAsync<TDto>(TEntity existing, TDto dto,PropertyInfo entityProperty, PropertyInfo dtoProperty, CancellationToken cancellationToken) where TDto : class
            {
                var entityCollection = entityProperty.GetValue(existing) as System.Collections.IEnumerable;
                var dtoCollection = dtoProperty.GetValue(dto) as System.Collections.IEnumerable;

                if (entityCollection == null || dtoCollection == null)
                    return;

                var entityItemType = entityProperty.PropertyType.GetGenericArguments()[0];
                var dtoItemType = dtoProperty.PropertyType.GetGenericArguments()[0];

                // Clear existing collection
                var clearMethod = entityProperty.PropertyType.GetMethod("Clear");
                clearMethod?.Invoke(entityCollection, null);

                // Add new items based on DTO
                var addMethod = entityProperty.PropertyType.GetMethod("Add");

                foreach (var dtoItem in dtoCollection)
                {
                    if (dtoItemType == typeof(Guid) && entityItemType != typeof(Guid))
                    {
                        // Handle GUID ID lists to entity objects conversion
                        var newEntityItem = CreateJunctionEntity(entityItemType, existing, dtoItem);
                        if (newEntityItem != null)
                        {
                            addMethod?.Invoke(entityCollection, new[] { newEntityItem });
                        }
                    }
                    else
                    {
                        // Handle direct object mapping
                        try
                        {
                            var newEntityItem = Convert.ChangeType(dtoItem, entityItemType);
                            addMethod?.Invoke(entityCollection, new[] { newEntityItem });
                        }
                        catch
                        {
                            // If conversion fails, try to create new instance
                            try
                            {
                                var newEntityItem = Activator.CreateInstance(entityItemType);
                                addMethod?.Invoke(entityCollection, new[] { newEntityItem });
                            }
                            catch
                            {
                                // Skip if we can't create the entity
                            }
                        }
                    }
                }

                // Add awaitable operation to satisfy async requirement
                await Task.CompletedTask.ConfigureAwait(false);
            }
            private object? CreateJunctionEntity(Type entityType, object parentEntity, object idValue)
            {
                try
                {
                    var entity = Activator.CreateInstance(entityType);
                    if (entity == null) return null;

                    // Set foreign key property (convention: ParentEntityId + Id)
                    var parentIdProp = entityType.GetProperty($"{parentEntity.GetType().Name}Id");
                    if (parentIdProp != null)
                    {
                        var parentId = parentEntity.GetType().GetProperty("ID")?.GetValue(parentEntity);
                        if (parentId != null)
                        {
                            parentIdProp.SetValue(entity, parentId);
                        }
                    }

                    // Set the related entity ID property
                    var relatedIdProp = entityType.GetProperties()
                        .FirstOrDefault(p => p.Name.EndsWith("Id") && p.Name != $"{parentEntity.GetType().Name}Id");

                    if (relatedIdProp != null && idValue != null)
                    {
                        relatedIdProp.SetValue(entity, idValue);
                    }

                    return entity;
                }
                catch
                {
                    return null;
                }
            }
            public void UpdateCollection<TNavEntity>(ICollection<TNavEntity> existingCollection,IEnumerable<Guid> newIds,Func<Guid, TNavEntity> createEntityFunc)
                where TNavEntity : class
            {
                if (existingCollection == null || newIds == null || createEntityFunc == null)
                    return;

                existingCollection.Clear();

                foreach (var id in newIds)
                {
                    var newEntity = createEntityFunc(id);
                    existingCollection.Add(newEntity);
                }
            }
            public async Task<int> UpdateEntityWithCollectionsAsync(TEntity entity,Dictionary<string, IEnumerable<Guid>> collectionUpdates, CancellationToken cancellationToken = default)
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                if (collectionUpdates == null) throw new ArgumentNullException(nameof(collectionUpdates));

                var entityType = typeof(TEntity);

                foreach (var update in collectionUpdates)
                {
                    var property = entityType.GetProperty(update.Key);
                    if (property != null && update.Value != null)
                    {
                        var collection = property.GetValue(entity) as System.Collections.IEnumerable;
                        if (collection != null)
                        {
                            var clearMethod = property.PropertyType.GetMethod("Clear");
                            clearMethod?.Invoke(collection, null);

                            var addMethod = property.PropertyType.GetMethod("Add");
                            var itemType = property.PropertyType.GetGenericArguments()[0];

                            foreach (var id in update.Value)
                            {
                                var newItem = Activator.CreateInstance(itemType);
                                if (newItem != null)
                                {
                                    var idProperty = itemType.GetProperty("Id") ?? itemType.GetProperty("ID");
                                    idProperty?.SetValue(newItem, id);

                                    addMethod?.Invoke(collection, new[] { newItem });
                                }
                            }
                        }
                    }
                }

                return await UpdateWithNavigationAsync(entity, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            // Clean method for manual navigation updates
            
          
            public async Task<List<TEntity>> GetPagedWithThenIncludeAsync<TProperty>(string? search,Expression<Func<TEntity, IEnumerable<TProperty>>> collectionSelector,Expression<Func<TProperty, object>> thenIncludeSelector,Expression<Func<TEntity, bool>>? baseFilter = null,List<Expression<Func<TEntity, string>>>? searchProperties = null,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes) where TProperty : class
                {
                    IQueryable<TEntity> query = _context.Set<TEntity>();

                    // Apply regular includes
                    if (includes != null && includes.Any())
                    {
                        foreach (var include in includes)
                            query = query.Include(include);
                    }

                    // Apply base filter
                    if (baseFilter != null)
                        query = query.Where(baseFilter);

                    // Apply search filter
                    if (!string.IsNullOrWhiteSpace(search) && searchProperties != null)
                    {
                        foreach (var prop in searchProperties)
                        {
                            query = query.Where(x =>
                                EF.Functions.Like(EF.Property<string>(x, ((MemberExpression)prop.Body).Member.Name), $"%{search}%")
                            );
                        }
                    }

                    // Include collection and ThenInclude
                    query = query.Include(collectionSelector).ThenInclude(thenIncludeSelector);

                    return await query.ToListAsync(cancellationToken);
                }
            public async Task<List<TEntity>> GetPagedWithThenIncludeNewAsync<TProperty>(string? search,Expression<Func<TEntity, IEnumerable<TProperty>>> collectionSelector,Expression<Func<TProperty, object>> thenIncludeSelector,Expression<Func<TEntity, bool>>? baseFilter = null,List<Expression<Func<TEntity, string>>>? searchProperties = null,CancellationToken cancellationToken = default,params Expression<Func<TEntity, object>>[] includes)
            where TProperty : class
                {
                    IQueryable<TEntity> query = _context.Set<TEntity>();

                    // Apply regular includes
                    if (includes != null && includes.Any())
                    {
                        foreach (var include in includes)
                            query = query.Include(include);
                    }

                    // Base filter
                    if (baseFilter != null)
                        query = query.Where(baseFilter);

                    // ✅ Search filter (OR instead of AND)
                    if (!string.IsNullOrWhiteSpace(search) && searchProperties != null && searchProperties.Any())
                    {
                        var parameter = Expression.Parameter(typeof(TEntity), "x");
                        Expression? orExpression = null;

                        foreach (var prop in searchProperties)
                        {
                            var propertyAccess = Expression.Invoke(prop, parameter);

                            var likeCall = Expression.Call(
                                typeof(DbFunctionsExtensions),
                                nameof(DbFunctionsExtensions.Like),
                                Type.EmptyTypes,
                                Expression.Constant(EF.Functions),
                                propertyAccess,
                                Expression.Constant($"%{search}%")
                            );

                            orExpression = orExpression == null ? likeCall : Expression.OrElse(orExpression, likeCall);
                        }

                        var lambda = Expression.Lambda<Func<TEntity, bool>>(orExpression!, parameter);
                        query = query.Where(lambda);
                    }

                    // Includes with ThenInclude
                    query = query.Include(collectionSelector).ThenInclude(thenIncludeSelector);

                    return await query.ToListAsync(cancellationToken);
                }
            public async Task<List<TEntity>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
            {
                IQueryable<TEntity> query = _dbSet.AsNoTracking();

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                return await query.ToListAsync(cancellationToken);
            }
            public async Task<List<TResult>> GetXListByYAsync<TFilter, TResult>(Expression<Func<TEntity, TFilter>> yProperty,TFilter yValue,Expression<Func<TEntity, TResult>> xProperty,CancellationToken cancellationToken = default)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                var filtslickropertyName = ((MemberExpression)yProperty.Body).Member.Name;
                var property = Expression.Property(parameter, filtslickropertyName);

                // Ensure constant matches the property type (important for Guid, Nullable<Guid>, etc.)
                var constant = Expression.Constant(yValue, property.Type);

                var equal = Expression.Equal(property, constant);
                var filterLambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);

                return await _context.Set<TEntity>()
                    .Where(filterLambda)
                    .Select(xProperty)
                    .ToListAsync(cancellationToken);
            }
            public async Task<TResult?> GetXByYAsync<TFilter, TResult>(Expression<Func<TEntity, TFilter>> yProperty,TFilter yValue,Expression<Func<TEntity, TResult>> xProperty,CancellationToken cancellationToken = default)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                var filtslickropertyName = ((MemberExpression)yProperty.Body).Member.Name;
                var property = Expression.Property(parameter, filtslickropertyName);

                // Match type exactly
                var constant = Expression.Constant(yValue, property.Type);

                var equal = Expression.Equal(property, constant);
                var filterLambda = Expression.Lambda<Func<TEntity, bool>>(equal, parameter);

                return await _context.Set<TEntity>()
                    .Where(filterLambda)
                    .Select(xProperty)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            public async Task<int> CountByAttributeAsync<TValue>(string propertyName,TValue value,CancellationToken cancellationToken = default)
            {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            // Get the property info dynamically
            var property = typeof(TEntity).GetProperty(propertyName,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException(
                    $"Property '{propertyName}' does not exist on entity '{typeof(TEntity).Name}'.");

            // Build expression: x => EF.Property<TValue>(x, propertyName) == value
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccess = Expression.Property(parameter, property);

            // Handle nullable and non-nullable property types safely
            Expression constantExpr;
            var propertyType = property.PropertyType;

            if (propertyType.IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Get the underlying type (e.g., bool from bool?)
                var underlyingType = Nullable.GetUnderlyingType(propertyType);

                if (underlyingType == null)
                    throw new InvalidOperationException(
                        $"Could not determine underlying type for nullable property '{propertyName}'.");

                // Convert value safely
                var converted = value is null ? null : Convert.ChangeType(value, underlyingType);

                // Box the value into a nullable expression
                constantExpr = Expression.Convert(
                    Expression.Constant(converted, underlyingType),
                    propertyType);
            }
            else
            {
                constantExpr = Expression.Constant(value, propertyType);
            }

            var equals = Expression.Equal(propertyAccess, constantExpr);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

            var query = _dbSet.AsQueryable();

            // Apply soft delete filter if applicable
            if (_hasIsDeletedProperty)
            {
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
            }

            return await query.CountAsync(lambda, cancellationToken);
        }
        public async Task<int> CountWithNavigationAsync<TNavigation>(
    Expression<Func<TEntity, bool>> predicate,
    Expression<Func<TEntity, TNavigation?>> navigationProperty,
    Expression<Func<TNavigation, bool>>? navigationPredicate = null,
    CancellationToken cancellationToken = default)
    where TNavigation : class
        {
            IQueryable<TEntity> query = _dbSet;

            // Soft delete filter
            if (_hasIsDeletedProperty)
                query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);

            // Apply main predicate
            query = query.Where(predicate);

            if (navigationPredicate != null)
            {
                // Get the navigation property name
                var navName = ((MemberExpression)navigationProperty.Body).Member.Name;

                // Filter by navigation predicate directly using EF.Property
                query = query.Where(e =>
                    EF.Property<TNavigation?>(e, navName) != null &&
                    EF.Property<bool>(EF.Property<TNavigation>(e, navName), "status") == true
                );
            }

            return await query.CountAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
            {
                var query = _dbSet.AsQueryable();

                // Apply soft delete filter if applicable
                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                // Apply predicate if provided
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.CountAsync(cancellationToken);
            }
            public async Task<decimal?> SumAsync(Expression<Func<TEntity, bool>> predicate,Expression<Func<TEntity, decimal?>> selector,CancellationToken cancellationToken = default)
            {
                var query = _dbSet.AsQueryable();

                // Apply soft delete filter if applicable
                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                return await query
                    .Where(predicate)
                    .Select(selector)
                    .SumAsync(cancellationToken);
            }
            public async Task<List<TEntity>> GetByTwoColumnsAsync<TFilter1, TFilter2>(Expression<Func<TEntity, TFilter1>> column1Property,TFilter1 column1Value,Expression<Func<TEntity, TFilter2>> column2Property,TFilter2 column2Value,CancellationToken cancellationToken = default)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                // Build first filter: x.Column1 == column1Value
                var column1Name = ((MemberExpression)column1Property.Body).Member.Name;
                var column1Access = Expression.Property(parameter, column1Name);
                var column1Constant = Expression.Constant(column1Value, column1Access.Type);
                var column1Equals = Expression.Equal(column1Access, column1Constant);

                // Build second filter: x.Column2 == column2Value  
                var column2Name = ((MemberExpression)column2Property.Body).Member.Name;
                var column2Access = Expression.Property(parameter, column2Name);
                var column2Constant = Expression.Constant(column2Value, column2Access.Type);
                var column2Equals = Expression.Equal(column2Access, column2Constant);

                // Combine with AND: x.Column1 == value1 && x.Column2 == value2
                var combinedExpression = Expression.AndAlso(column1Equals, column2Equals);
                var filterLambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);

                return await _context.Set<TEntity>()
                    .Where(filterLambda)
                    .ToListAsync(cancellationToken);
            }
            public async Task<List<TResult>> GetXListByTwoColumnsAsync<TFilter1, TFilter2, TResult>(Expression<Func<TEntity, TFilter1>> column1Property,TFilter1 column1Value,Expression<Func<TEntity, TFilter2>> column2Property,TFilter2 column2Value,Expression<Func<TEntity, TResult>> selectProperty,CancellationToken cancellationToken = default)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "x");

                // Build first filter: x.Column1 == column1Value
                var column1Name = ((MemberExpression)column1Property.Body).Member.Name;
                var column1Access = Expression.Property(parameter, column1Name);
                var column1Constant = Expression.Constant(column1Value, column1Access.Type);
                var column1Equals = Expression.Equal(column1Access, column1Constant);

                // Build second filter: x.Column2 == column2Value  
                var column2Name = ((MemberExpression)column2Property.Body).Member.Name;
                var column2Access = Expression.Property(parameter, column2Name);
                var column2Constant = Expression.Constant(column2Value, column2Access.Type);
                var column2Equals = Expression.Equal(column2Access, column2Constant);

                // Combine with AND: x.Column1 == value1 && x.Column2 == value2
                var combinedExpression = Expression.AndAlso(column1Equals, column2Equals);
                var filterLambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);

                return await _context.Set<TEntity>()
                    .Where(filterLambda)
                    .Select(selectProperty)
                    .ToListAsync(cancellationToken);
            }
            public async Task ClearChildCollectionAsync<TChild>(object parentId,Expression<Func<TEntity, ICollection<TChild>>> collectionSelector,string idPropertyName = "Id") where TChild : class
            {
                var parameter = Expression.Parameter(typeof(TEntity));
                var idProperty = Expression.Property(parameter, idPropertyName);
                var constant = Expression.Constant(parentId);
                var equals = Expression.Equal(idProperty, constant);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);

                var entity = await _dbSet
                    .Include(collectionSelector)
                    .FirstOrDefaultAsync(lambda);

                if (entity != null)
                {
                    var collection = collectionSelector.Compile()(entity);
                    collection.Clear();
                    await _context.SaveChangesAsync();
                }
            }
            public async Task<bool> ExistsByMultipleColumnsAsync(Dictionary<string, object> columnValues, CancellationToken cancellationToken = default)
            {
                if (columnValues == null || !columnValues.Any())
                    throw new ArgumentException("Column values cannot be null or empty");

                var query = _dbSet.AsQueryable();

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                var parameter = Expression.Parameter(typeof(TEntity), "x");
                Expression? combinedExpression = null;

                var regularColumns = columnValues.Where(c => !c.Key.StartsWith("Exclude"));
                var excludeColumns = columnValues.Where(c => c.Key.StartsWith("Exclude"));

                // Regular columns (==)
                foreach (var column in regularColumns)
                {
                    var property = Expression.Property(parameter, column.Key);
                    var propertyType = property.Type;

                    // Convert constant to property type (handles nullable)
                    var constant = Expression.Constant(column.Value, propertyType);

                    var equals = Expression.Equal(property, constant);
                    combinedExpression = combinedExpression == null
                        ? equals
                        : Expression.AndAlso(combinedExpression, equals);
                }

                // Exclusion columns (!=)
                foreach (var column in excludeColumns)
                {
                    var propertyName = column.Key.Replace("Exclude", "");
                    var property = Expression.Property(parameter, propertyName);
                    var propertyType = property.Type;

                    // Convert constant to property type (handles nullable)
                    var constant = Expression.Constant(column.Value, propertyType);

                    var notEquals = Expression.NotEqual(property, constant);
                    combinedExpression = combinedExpression == null
                        ? notEquals
                        : Expression.AndAlso(combinedExpression, notEquals);
                }

                if (combinedExpression == null)
                    return false;

                var lambda = Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
                return await query.AnyAsync(lambda, cancellationToken);
            }
            public async Task<TEntity?> GetLastInsertedAsync(CancellationToken cancellationToken = default)
            {
                var query = _dbSet.AsQueryable();

                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                // Order by CreatedDate descending → newest first
                return await query
                    .OrderByDescending(e => EF.Property<DateTime>(e, "CreatedDate"))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);
            }

           
            public async Task<TEntity?> GetLastInsertedByRelationalAsync<TKey>(string? foreignKeyPropertyName = null,TKey? foreignKeyValue = default, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
            {
                var query = _dbSet.AsQueryable();

                // Apply includes (for navigation properties)
                if (includes != null && includes.Any())
                {
                    query = includes.Aggregate(query, (current, include) => current.Include(include));
                }

                // Apply soft delete filter if applicable
                if (_hasIsDeletedProperty)
                {
                    query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);
                }

                // Apply relational filter if foreignKeyPropertyName is provided
                if (!string.IsNullOrEmpty(foreignKeyPropertyName) && foreignKeyValue is not null)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var property = Expression.Property(parameter, foreignKeyPropertyName);

                    Expression equals;

                    if (typeof(TKey) == typeof(Guid))
                    {
                        equals = Expression.Equal(property, Expression.Constant(foreignKeyValue, typeof(Guid)));
                    }
                    else
                    {
                        equals = Expression.Equal(property, Expression.Constant(foreignKeyValue, typeof(TKey)));
                    }

                    var lambda = Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
                    query = query.Where(lambda);
                }

                // Order by CreatedDate descending and get the latest record
                return await query
                    .OrderByDescending(e => EF.Property<DateTime>(e, "CreatedDate"))
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);
            }
          



    }

}