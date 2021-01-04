using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Entities;
using LinqSeekExtensions.EFCore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Commands.QueryPageGet {
  public class QueryPageGetRequest : IRequest<QueryPageResponse<Business>> {
    public QueryPageGetRequest(QueryPageRequest queryPageOptions) {
      QueryPageOptions = queryPageOptions;
    }

    public QueryPageRequest QueryPageOptions { get; }
  }

  public class QueryPageGetHandler : IRequestHandler<QueryPageGetRequest, QueryPageResponse<Business>> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private ILogger<QueryPageGetHandler> _logger;

    public QueryPageGetHandler(IDbContextFactory<ContextDemo> df,
      ILogger<QueryPageGetHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<QueryPageResponse<Business>> Handle(QueryPageGetRequest request,
      CancellationToken cancellationToken) {
      await using var dc = _df.CreateDbContext();
      //var query = dc.Businesses.AsQueryable();
      //var filterLambda = request.QueryPageOptions.FilterKeyValueActions.GetFilterLambda<Business>();
      //query = query.Where(filterLambda);


      //if (request.QueryPageOptions.IsFirst) {
      //  request.QueryPageOptions.TotalCount = query.Count();
      //}

      //if (string.IsNullOrEmpty(request.QueryPageOptions.SortName) == false) {
      //  var sortOrder = request.QueryPageOptions.SortOrder == SortOrder.Desc ? SortOrder.Desc : SortOrder.Asc;
      //  query = query.OrderBy(request.QueryPageOptions.SortName, sortOrder == SortOrder.Desc);
      //}

      //var items = query.Skip((request.QueryPageOptions.PageIndex - 1) * request.QueryPageOptions.PageItems)
      //  .Take(request.QueryPageOptions.PageItems);
      //request.QueryPageOptions.Data = await items.ToListAsync(cancellationToken);
      //return request.QueryPageOptions;

      var query =await dc.Businesses.Where(request.QueryPageOptions, cancellationToken);
      return query;
    }
  }
}