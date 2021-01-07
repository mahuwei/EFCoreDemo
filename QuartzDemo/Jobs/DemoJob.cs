using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace QuartzDemo.Jobs {
  [DisallowConcurrentExecution]
  public class DemoJob : Demo, IJob {
    private readonly ILogger<DemoJob> _logger;

    public DemoJob(ILogger<DemoJob> logger) {
      _logger = logger;
    }

    public Task Execute(IJobExecutionContext context) {
      Name = context.MergedJobDataMap.GetString("Name");
      Cron = context.MergedJobDataMap.GetString("Cron");
      AppId = context.MergedJobDataMap.GetString("AppId");
      Id = context.MergedJobDataMap.GetGuidValue("Id");

      return Task.Factory.StartNew(() => {
        _logger.LogInformation(
          $"{DateTime.Now.ToLongTimeString()} UserName:{(string.IsNullOrEmpty(Name) ? "空值" : Name)} Cron:'{Cron}' DemoJob Execute");
      });
    }
  }

  public class Demo {
    public bool IsCron { get; set; } = true;

    public int IntervalSecond { get; set; } = 8;


    public string Cron { get; set; }

    public Guid Id { get; set; }

    public string AppId { get; set; }

    public string Name { get; set; }
  }
}