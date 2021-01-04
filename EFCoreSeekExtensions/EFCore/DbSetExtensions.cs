using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LinqSeekExtensions.EFCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore {
  /// <summary>
  ///   DbSet 分页查询扩展
  /// </summary>
  public static class DbSetExtensions {
    /// <summary>
    ///   分页查询
    /// </summary>
    /// <typeparam name="TItem">实体类型</typeparam>
    /// <param name="dbSet">DbSet</param>
    /// <param name="request">查询条件</param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    public static async Task<QueryPageResponse<TItem>> Where<TItem>(this DbSet<TItem> dbSet,
      QueryPageRequest request,
      CancellationToken cancellationToken) where TItem : class, new() {
      var response = new QueryPageResponse<TItem>();
      // 判断是否直接执行Sql脚本还是组合Lambda
      var queryable = request.IsSql
        ? dbSet.FromSqlRaw(request.SqlText)
        : dbSet.Where(request.KeyValueActions.GetFilterLambda<TItem>());

      //首次执行先获取记录数
      response.TotalCount = queryable.Count();
      if (response.TotalCount == 0) {
        return response;
      }

      // order by
      if (request.OrderByItems != null && request.OrderByItems.Any()) {
        foreach (var orderByItem in request.OrderByItems) {
          var sortOrder = orderByItem.OrderByType == OrderByType.Desc ? OrderByType.Desc : OrderByType.Asc;
          queryable = queryable.OrderBy(orderByItem.OrderByFiledName, sortOrder == OrderByType.Desc);
        }
      }

      var items = queryable.Skip((request.PageIndex - 1) * request.PageItems)
        .Take(request.PageItems);
      response.Items = await items.ToListAsync(cancellationToken);
      return response;
    }

    public static IQueryable GetDbSetForClrType(this DbContext dbContext, Type clrType) {
      var dbSetType = typeof(DbSet<>);

      var propertyInfo = dbContext.GetType()
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => new { PropertyInfo = p, p.PropertyType })
        .First(t =>
          t.PropertyType.IsGenericType
          && t.PropertyType.GetGenericTypeDefinition() == dbSetType
          && t.PropertyType.GetGenericArguments()[0] == clrType)
        .PropertyInfo;
      return propertyInfo.GetValue(dbContext) as IQueryable;
    }
  }
}