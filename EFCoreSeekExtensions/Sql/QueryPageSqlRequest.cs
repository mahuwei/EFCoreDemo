using System.Collections.Generic;
using LinqSeekExtensions.EFCore;

namespace LinqSeekExtensions.Sql {
  public class QueryPageSqlRequest : QueryPageRequest {
    public string DbName { get; set; }
    public List<Field> Fields { get; set; }
  }
}