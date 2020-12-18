using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Commands.AddEmployeeTracking {
  public class AddEmployeeTrackingRequest : IRequest<Business> {
    public AddEmployeeTrackingRequest(Business business) {
      Business = business;
    }

    public Business Business { get; }
  }

  public class AddEmployeeTrackingHandler : IRequestHandler<AddEmployeeTrackingRequest, Business> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private readonly ILogger<AddEmployeeTrackingHandler> _logger;

    public AddEmployeeTrackingHandler(IDbContextFactory<ContextDemo> df, ILogger<AddEmployeeTrackingHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<Business> Handle(AddEmployeeTrackingRequest request, CancellationToken cancellationToken) {
      await using var cd = _df.CreateDbContext();
      // 跟踪然后添加子项
      var businessIdDb = await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);
      var employee = new Employee {
        Id = Entity.CreateGuid(),
        BusinessId = request.Business.Id,
        WorkNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
        MobileNo = "13911111111",
        ChangeNumber = 1,
        StreetAddress = new StreetAddress { ChangeNumber = 1, City = "太原市", Street = "平阳路" }
      };
      businessIdDb.Employees ??= new List<Employee>();
      businessIdDb.Employees.Add(employee);
      await cd.SaveChangesAsync(cancellationToken);


      return await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);
    }
  }
}