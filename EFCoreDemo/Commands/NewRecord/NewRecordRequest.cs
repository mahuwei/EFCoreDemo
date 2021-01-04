using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Commands.NewRecord {
  public class NewRecordRequest : IRequest<List<Business>> {
  }

  public class NewRecordHandler : IRequestHandler<NewRecordRequest, List<Business>> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private readonly ILogger<NewRecordHandler> _logger;

    public NewRecordHandler(IDbContextFactory<ContextDemo> df, ILogger<NewRecordHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<List<Business>> Handle(NewRecordRequest request, CancellationToken cancellationToken) {
      await using var cd = _df.CreateDbContext();
      if (await cd.Businesses.AnyAsync(cancellationToken)) {
        return await cd.Businesses.Include(d => d.Employees).ToListAsync(cancellationToken);
      }

      var businesses = new List<Business>();
      for (var i = 0; i < 100; i++) {
        var num = (i + 1).ToString().PadLeft(3, '0');
        businesses.Add(CreateBusiness(num, $"公司{num}"));
      }

      await cd.Businesses.AddRangeAsync(businesses, cancellationToken);
      await cd.SaveChangesAsync(cancellationToken);
      return await cd.Businesses.Include(d => d.Employees).ToListAsync(cancellationToken);
    }

    private static Business CreateBusiness(string no, string name) {
      var business = new Business {
        Id = Entity.CreateGuid(),
        No = no,
        Name = name,
        Employees = new List<Employee>(),
        ChangeNumber = 1,
        StreetAddress = new StreetAddress { ChangeNumber = 1, City = "太原市", Street = "平阳路18号" }
      };

      for (var i = 0; i < 3; i++) {
        var employee = new Employee {
          Id = Entity.CreateGuid(),
          BusinessId = business.Id,
          WorkNo = $"{no}-{(i + 1).ToString().PadLeft(3, '0')}",
          Name = $"员工{i + 1}",
          MobileNo = $"1391234567{i}",
          ChangeNumber = 1,
          StreetAddress = new StreetAddress { ChangeNumber = 1, City = "太原市", Street = $"平阳路{i + 1}号" }
        };
        business.Employees.Add(employee);
      }

      return business;
    }
  }
}