using System.Collections.Generic;
using LinqSeekExtensions.Share;

namespace LinqSeekExtensions.EFCore {
  public class QueryPageRequest {
    /// <summary>
    ///   每页数据数量 默认 20 行
    /// </summary>
    internal const int DefaultPageItems = 20;

    /// <summary>
    ///   是否通过Sql查询
    /// </summary>
    public bool IsSql { get; set; }

    /// <summary>
    ///   Sql脚本
    ///   包括
    /// </summary>
    public string SqlText { get; set; }

    /// <summary>
    /// </summary>
    public IEnumerable<OrderByItem> OrderByItems { get; set; }

    /// <summary>
    ///   查询条件
    /// </summary>
    public IEnumerable<KeyValueAction> KeyValueActions { get; set; }

    /// <summary>
    ///   查询实体类型定义
    /// </summary>
    public object SearchModelType { get; set; }


    /// <summary>
    ///   获得/设置 当前页码 首页为 第一页
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    ///   获得/设置 每页条目数量
    /// </summary>
    public int PageItems { get; set; } = DefaultPageItems;
  }
}