using System.Collections.Generic;

namespace EFCoreDemo.QueryExtensions {
  /// <summary>
  ///   查询条件实体类
  /// </summary>
  public class QueryPageOptions<TEntity> {
    /// <summary>
    ///   每页数据数量 默认 20 行
    /// </summary>
    internal const int DefaultPageItems = 20;

    /// <summary>
    ///   获得/设置 查询关键字
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    ///   获得/设置 排序字段名称
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    ///   获得/设置 排序方式
    /// </summary>
    public SortOrder SortOrder { get; set; }

    /// <summary>
    ///   查询条件
    /// </summary>
    public IEnumerable<FilterKeyValueAction> FilterKeyValueActions { get; set; }

    /// <summary>
    ///   获得/设置 搜索条件绑定模型
    /// </summary>
    public object? SearchModel { get; set; }

    /// <summary>
    ///   获得/设置 当前页码 首页为 第一页
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    ///   返回的结果数据
    /// </summary>
    public List<TEntity> Data { get; set; }

    /// <summary>
    ///   是否首次执行
    ///   首次执行返回总记录数
    /// </summary>
    public bool IsFirst { get; set; }

    /// <summary>
    ///   总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    ///   获得/设置 每页条目数量
    /// </summary>
    public int PageItems { get; set; } = DefaultPageItems;
  }
}