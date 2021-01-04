#nullable enable
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqSeekExtensions.EFCore;
using LinqSeekExtensions.Share;

// ReSharper disable once CheckNamespace
namespace System.Linq {
  public static class LambdaExtensions {
    /// <summary>
    ///   指定 FilterKeyValueAction 集合获取 Lambda 表达式
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="filters"></param>
    /// <returns></returns>
    public static Expression<Func<TItem, bool>> GetFilterLambda<TItem>(this IEnumerable<KeyValueAction> filters) {
      Expression<Func<TItem, bool>>? ret = null;
      var expP = Expression.Parameter(typeof(TItem));
      var visitor = new ComboExpressionVisitor(expP);

      foreach (var filter in filters) {
        var exp = filter.GetFilterLambda<TItem>();
        if (ret == null) {
          ret = exp;
          continue;
        }

        var left = visitor.Visit(ret.Body);
        var right = visitor.Visit(exp.Body);

        ret = filter.FilterLogicType switch {
          FilterLogicType.And => Expression.Lambda<Func<TItem, bool>>(Expression.AndAlso(left, right), expP),
          _ => Expression.Lambda<Func<TItem, bool>>(Expression.OrElse(left, right), expP)
        };
      }

      return ret ?? (r => true);
    }

    /// <summary>
    ///   指定 FilterKeyValueAction 获取 Lambda 表达式
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static Expression<Func<TItem, bool>> GetFilterLambda<TItem>(this KeyValueAction filter) {
      Expression<Func<TItem, bool>> ret = t => true;
      if (string.IsNullOrEmpty(filter.FieldKey) || filter.FieldValue == null) {
        return ret;
      }

      var prop = typeof(TItem).GetProperty(filter.FieldKey);
      if (prop == null) {
        return ret;
      }

      var p = Expression.Parameter(typeof(TItem));
      var fieldExpression = Expression.Property(p, prop);

      Expression eq = fieldExpression;

      // 可为空类型转化为具体类型
      if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
        eq = Expression.Convert(fieldExpression, prop.PropertyType.GenericTypeArguments[0]);
      }

      eq = filter.GetExpression(eq);
      ret = Expression.Lambda<Func<TItem, bool>>(eq, p);

      return ret;
    }

    private static Expression GetExpression(this KeyValueAction filter, Expression left) {
      var right = Expression.Constant(filter.FieldValue);
      var method = right.Type.GetMethod("CompareTo", new[] { typeof(string) });
      if (method == null) {
        return Expression.Empty();
      }

      var zero = Expression.Constant(0);
      var isString = left.Type == typeof(string);
      if (!isString
          || (filter.FilterActionType != FilterActionType.GreaterThan
              && filter.FilterActionType != FilterActionType.GreaterThanOrEqual
              && filter.FilterActionType != FilterActionType.LessThan
              && filter.FilterActionType != FilterActionType.LessThanOrEqual)) {
        return filter.FilterActionType switch {
          FilterActionType.Equal => Expression.Equal(left, right),
          FilterActionType.NotEqual => Expression.NotEqual(left, right),
          FilterActionType.GreaterThan => Expression.GreaterThan(left, right),
          FilterActionType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(left, right),
          FilterActionType.LessThan => Expression.LessThan(left, right),
          FilterActionType.LessThanOrEqual => Expression.LessThanOrEqual(left, right),
          FilterActionType.Contains => left.Contains(right),
          FilterActionType.NotContains => Expression.Not(left.Contains(right)),
          FilterActionType.CustomPredicate => filter.FieldValue switch {
            LambdaExpression t => Expression.Invoke(t, left),
            Delegate => Expression.Invoke(right, left),
            _ => throw new ArgumentException(nameof(KeyValueAction.FieldValue))
          },
          _ => Expression.Empty()
        };
      }

      var converted = right.Type != left.Type
        ? Expression.Convert(right, left.Type)
        : (Expression)right;

      var result = Expression.Call(left, method, converted);

      var operation = ExpressionType.Equal;
      switch (filter.FilterActionType) {
        case FilterActionType.GreaterThan:
          operation = ExpressionType.GreaterThan;
          break;
        case FilterActionType.GreaterThanOrEqual:
          operation = ExpressionType.GreaterThanOrEqual;
          break;
        case FilterActionType.LessThan:
          operation = ExpressionType.LessThan;
          break;
        case FilterActionType.LessThanOrEqual:
          operation = ExpressionType.LessThanOrEqual;
          break;
      }


      return Expression.MakeBinary(operation, result, zero);
    }

    private static Expression Contains(this Expression left, Expression right) {
      Expression<Func<string, string, bool>> expression = (l, r) => l.Contains(r);
      return Expression.Invoke(expression, left, right);
    }

    /// <summary>
    ///   通过base.Visit(node)返回的Expression统一node变量
    /// </summary>
    private class ComboExpressionVisitor : ExpressionVisitor {
      /// <summary>
      ///   构造
      /// </summary>
      /// <param name="parameter"></param>
      public ComboExpressionVisitor(ParameterExpression parameter) {
        exp_p = parameter;
      }

      private ParameterExpression exp_p { get; }

      /// <summary>
      /// </summary>
      /// <param name="p"></param>
      /// <returns></returns>
      protected override Expression VisitParameter(ParameterExpression p) {
        return exp_p;
      }
    }

    #region order by

    public static IQueryable<TItem> OrderBy<TItem>(this IQueryable<TItem> queryable, string propertyName) {
      return QueryableHelper<TItem>.OrderBy(queryable, propertyName, false);
    }

    public static IQueryable<TItem> OrderBy<TItem>(this IQueryable<TItem> queryable, string propertyName, bool desc) {
      return QueryableHelper<TItem>.OrderBy(queryable, propertyName, desc);
    }

    private static class QueryableHelper<TItem> {
      private static readonly Dictionary<string, LambdaExpression> OrderByCache =
        new Dictionary<string, LambdaExpression>();

      public static IQueryable<TItem> OrderBy(IQueryable<TItem> queryable, string propertyName, bool desc) {
        var name = $"{nameof(TItem)}-{propertyName}-{(desc ? 1 : 0)}";
        dynamic keySelector = GetLambdaExpression(name, propertyName);
        return desc ? Queryable.OrderByDescending(queryable, keySelector) : Queryable.OrderBy(queryable, keySelector);
      }

      private static LambdaExpression GetLambdaExpression(string catchName, string propertyName) {
        if (OrderByCache.ContainsKey(catchName)) {
          return OrderByCache[propertyName];
        }

        var param = Expression.Parameter(typeof(TItem));
        var body = Expression.Property(param, propertyName);
        var keySelector = Expression.Lambda(body, param);
        OrderByCache[propertyName] = keySelector;
        return keySelector;
      }
    }

    #endregion
  }
}