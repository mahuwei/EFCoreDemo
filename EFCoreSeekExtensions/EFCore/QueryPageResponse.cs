using System.Collections.Generic;

namespace LinqSeekExtensions.EFCore {
  public class QueryPageResponse<TItem> where TItem : class, new() {
    /// <summary>
    ///   获得/设置 要显示页码的数据集合
    /// </summary>
    public IEnumerable<TItem> Items { get; set; } = new List<TItem>();

    /// <summary>
    ///   获得/设置 数据集合总数
    /// </summary>
    public int TotalCount { get; set; }
  }
}