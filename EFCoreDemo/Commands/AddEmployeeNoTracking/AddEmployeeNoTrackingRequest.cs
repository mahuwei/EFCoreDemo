using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Commands.AddEmployeeNoTracking {
  public class AddEmployeeNoTrackingRequest : IRequest<Business> {
    public AddEmployeeNoTrackingRequest(Business business) {
      Business = business;
    }

    public Business Business { get; }
  }

  public class AddEmployeeNoTrackingHandler : IRequestHandler<AddEmployeeNoTrackingRequest, Business> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private readonly ILogger<AddEmployeeNoTrackingHandler> _logger;

    public AddEmployeeNoTrackingHandler(IDbContextFactory<ContextDemo> df,
      ILogger<AddEmployeeNoTrackingHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<Business> Handle(AddEmployeeNoTrackingRequest request, CancellationToken cancellationToken) {
      await using var cd = _df.CreateDbContext();
      var clone = (Business)request.Business.Clone();
      clone.Employees = new List<Employee>();
      var employee = new Employee {
        Id = Entity.CreateGuid(),
        BusinessId = request.Business.Id,
        WorkNo = DateTime.Now.ToString("yyyyMMddHHmmss"),
        MobileNo = "13911111111",
        ChangeNumber = 1,
        StreetAddress = new StreetAddress { ChangeNumber = 1, City = "太原市", Street = "平阳路" }
      };
      clone.Employees.Add(employee);
      cd.Businesses.Update(clone);
      await cd.SaveChangesAsync(cancellationToken);
      return await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);
    }
  }
}