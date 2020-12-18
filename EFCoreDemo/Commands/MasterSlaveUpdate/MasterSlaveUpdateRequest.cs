using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemo.Commands.MasterSlaveUpdate {
  public class MasterSlaveUpdateRequest : IRequest<Business> {
    public MasterSlaveUpdateRequest(Business business) {
      Business = business;
    }

    public Business Business { get; }
  }

  public class MasterSlaveUpdateHandler : IRequestHandler<MasterSlaveUpdateRequest, Business> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private readonly ILogger<MasterSlaveUpdateHandler> _logger;

    public MasterSlaveUpdateHandler(IDbContextFactory<ContextDemo> df,
      ILogger<MasterSlaveUpdateHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<Business> Handle(MasterSlaveUpdateRequest request, CancellationToken cancellationToken) {
      await using var cd = _df.CreateDbContext();
      var clone = (Business)request.Business.Clone();
      clone.Employees = new List<Employee> { request.Business.Employees.First() };
      clone.ChangeNumber++;
      clone.StreetAddress.ChangeNumber++;
      clone.Employees.First().ChangeNumber++;
      clone.Employees.First().StreetAddress.ChangeNumber++;
      cd.Update(clone);
      await cd.SaveChangesAsync(cancellationToken);
      var result = await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);
      result.Employees = result.Employees.Where(d => d.Id == clone.Employees.First().Id).ToList();
      return result;
    }
  }

  public class MasterSlaveUpdateTrackingRequest : IRequest<Business> {
    public MasterSlaveUpdateTrackingRequest(Business business) {
      Business = business;
    }

    public Business Business { get; }
  }

  public class MasterSlaveUpdateTrackingHandler : IRequestHandler<MasterSlaveUpdateTrackingRequest, Business> {
    private readonly IDbContextFactory<ContextDemo> _df;
    private readonly ILogger<MasterSlaveUpdateTrackingHandler> _logger;

    public MasterSlaveUpdateTrackingHandler(IDbContextFactory<ContextDemo> df,
      ILogger<MasterSlaveUpdateTrackingHandler> logger) {
      _df = df;
      _logger = logger;
    }

    public async Task<Business> Handle(MasterSlaveUpdateTrackingRequest request, CancellationToken cancellationToken) {
      await using var cd = _df.CreateDbContext();
      var businessInDb = await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);

      var clone = (Business)request.Business.Clone();
      clone.Employees = new List<Employee> { request.Business.Employees.First() };
      clone.ChangeNumber++;
      clone.StreetAddress.ChangeNumber++;
      clone.Employees.First().ChangeNumber++;
      clone.Employees.First().StreetAddress.ChangeNumber++;
      cd.Update(clone);
      await cd.SaveChangesAsync(cancellationToken);
      var result = await cd.Businesses.Include(d => d.Employees)
        .FirstAsync(d => d.Id == request.Business.Id, cancellationToken);
      result.Employees = result.Employees.Where(d => d.Id == clone.Employees.First().Id).ToList();
      return result;
    }
  }
}