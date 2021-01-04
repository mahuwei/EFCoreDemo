using System.Collections.Generic;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace System.Linq {
  public static class QueryableExtensions {
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, string propertyName) {
      return QueryableHelper<T>.OrderBy(queryable, propertyName, false);
    }

    public static IQueryable<T> OrderBy<T>(this IQueryable<T> queryable, string propertyName, bool desc) {
      return QueryableHelper<T>.OrderBy(queryable, propertyName, desc);
    }

    private static class QueryableHelper<T> {
      private static readonly Dictionary<string, LambdaExpression> cache = new Dictionary<string, LambdaExpression>();

      public static IQueryable<T> OrderBy(IQueryable<T> queryable, string propertyName, bool desc) {
        var name = $"{nameof(T)}-{propertyName}-{(desc ? 1 : 0)}";
        dynamic keySelector = GetLambdaExpression(name, propertyName);
        return desc ? Queryable.OrderByDescending(queryable, keySelector) : Queryable.OrderBy(queryable, keySelector);
      }

      private static LambdaExpression GetLambdaExpression(string catchName, string propertyName) {
        if (cache.ContainsKey(propertyName)) {
          return cache[propertyName];
        }

        var param = Expression.Parameter(typeof(T));
        var body = Expression.Property(param, propertyName);
        var keySelector = Expression.Lambda(body, param);
        cache[propertyName] = keySelector;
        return keySelector;
      }
    }
  }
}