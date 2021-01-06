using System.Collections.Generic;

namespace LinqSeekExtensions.Sql {
  public class QueryPageSqlResponse {
    /// <summary>
    ///   返回的结果集
    /// </summary>
    public IEnumerable<object> Items { get; set; } = new List<object>();

    /// <summary>
    ///   总记录数
    /// </summary>
    public int TotalCount { get; set; }
  }
}