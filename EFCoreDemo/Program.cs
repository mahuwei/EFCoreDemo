using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EFCoreDemo.Commands.AddEmployeeNoTracking;
using EFCoreDemo.Commands.AddEmployeeTracking;
using EFCoreDemo.Commands.MasterSlaveUpdate;
using EFCoreDemo.Commands.NewRecord;
using EFCoreDemo.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace EFCoreDemo {
  internal class Program {
    private static async Task Main() {
      Console.Title = "Ef core 测试...";
      Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();
      var assemblyMc = typeof(ContextDemo).GetTypeInfo().Assembly;


      var serviceProvider = new ServiceCollection()
        .AddPooledDbContextFactory<ContextDemo>(builder => {
          builder.UseMySql(ContextDemo.MysqlConnection, ServerVersion.AutoDetect(ContextDemo.MysqlConnection));
        })
        .AddLogging(builder => {
          builder.AddSerilog(Log.Logger, true);
        })
        .AddMediatR(assemblyMc)
        .BuildServiceProvider();

      var cts = new CancellationTokenSource();
      var log = serviceProvider.GetService<ILogger<Program>>();

      var factory = serviceProvider.GetService<IDbContextFactory<ContextDemo>>();
      if (factory == null) {
        log.LogError("无法获取到ILogger对象。");
        ReadyQuit(cts, log);
        return;
      }

      try {
        await using var cd = factory.CreateDbContext();
        await cd.Database.EnsureDeletedAsync(cts.Token);
        await cd.Database.MigrateAsync(cts.Token);
        await cd.DisposeAsync();
      }
      catch (Exception e) {
        log.LogError(e, "删除已存在的数据库，并重新进行数据库脚本迁移出错。");
        ReadyQuit(cts, log);
        return;
      }


      var mediator = serviceProvider.GetService<IMediator>();
      if (mediator == null) {
        log.LogError("无法获取到IMediator对象。");
        cts.Cancel();
        log.LogInformation("按任意键退出...");
        Console.ReadKey();
        return;
      }

      List<Business> businesses = null;
      log.LogInformation("开始新增或获取已有数据...");
      // 新增数据
      var newRecordRequest = new NewRecordRequest();
      try {
        businesses = await mediator.Send(newRecordRequest, cts.Token);
        if (businesses == null || businesses.Count != 2) {
          log.LogError($"结果应该是2条记录，实际为:{businesses?.Count ?? 0}");
          ReadyQuit(cts, log);
          return;
        }

        log.LogInformation("新增或获取已有数据成功。结果信息:{@businesses}", businesses);
      }
      catch (Exception ex) {
        log.LogError(ex, "新增或获取已有数据。");
        ReadyQuit(cts, log);
        return;
      }

      log.LogInformation("跟踪添加子项-...");
      var addEmployeeTrackingRequest = new AddEmployeeTrackingRequest(businesses.First());
      try {
        var businessResult = await mediator.Send(addEmployeeTrackingRequest, cts.Token);
        if (businessResult.Employees == null || businessResult.Employees.Count != 4) {
          log.LogError($"应该为4条员工记录，实际为{businessResult.Employees?.Count ?? 0}");
        }
        else {
          log.LogInformation("跟踪添加子项后的员工记录：{@employees}", businessResult.Employees);
        }
      }
      catch (Exception ex) {
        log.LogError(ex, "跟踪添加子项出错");
      }

      log.LogInformation("不跟踪添加子项-...");
      var addEmployeeNoTrackingRequest = new AddEmployeeNoTrackingRequest(businesses.Last());
      try {
        var businessResult = await mediator.Send(addEmployeeNoTrackingRequest, cts.Token);
        if (businessResult.Employees == null || businessResult.Employees.Count != 4) {
          log.LogError($"应该为4条员工记录，实际为{businessResult.Employees?.Count ?? 0}");
        }
        else {
          log.LogInformation("不跟踪添加子项后的员工记录：{@employees}", businessResult.Employees);
        }
      }
      catch (Exception ex) {
        log.LogError(ex, "不跟踪添加子项出错");
      }

      log.LogInformation("主从同时修改-...");
      var masterSlaveUpdateRequest = new MasterSlaveUpdateRequest(businesses.First());
      try {
        var businessResult = await mediator.Send(masterSlaveUpdateRequest, cts.Token);
        log.LogInformation("主从同时修改员工记录：{@business}", businessResult);
      }
      catch (Exception ex) {
        log.LogError(ex, "主从同时修改出错");
      }

      log.LogInformation("主从同时修改(跟踪：先读取但没有添加 AsNoTracking())-...");
      var masterSlaveUpdateTrackingRequest = new MasterSlaveUpdateTrackingRequest(businesses.First());
      try {
        var businessResult = await mediator.Send(masterSlaveUpdateTrackingRequest, cts.Token);
        log.LogInformation("主从同时修改(跟踪)员工记录：{@business}", businessResult);
      }
      catch (Exception ex) {
        log.LogError(ex, "主从同时修改(跟踪)出错");
      }


      // 运行结束，准备退出。
      log.LogInformation("按任意键退出...");
      Console.ReadKey();
    }

    private static void ReadyQuit(CancellationTokenSource cts, ILogger<Program> log) {
      cts.Cancel();
      log.LogInformation("按任意键退出...");
      Console.ReadKey();
    }
  }
}