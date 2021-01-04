using LinqSeekExtensions.EFCore;

namespace LinqSeekExtensions.Sql {
  public class QueryPageSqlRequest : QueryPageRequest {
  }

  public class Field {
    public string Name { get; set; }
    public string NameCn { get; set; }
  }
}