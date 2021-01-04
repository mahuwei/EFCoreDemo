using LinqSeekExtensions.EFCore;

namespace LinqSeekExtensions.Share {
  /// <summary>
  ///   Order by item
  /// </summary>
  public class OrderByItem {
    /// <summary>
    ///   字段名
    /// </summary>
    public string OrderByFiledName { get; set; }

    /// <summary>
    ///   获得/设置 排序方式
    /// </summary>
    public OrderByType OrderByType { get; set; }
  }
}