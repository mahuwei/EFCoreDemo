using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuartzDemo.Jobs;

namespace QuartzDemo.Controllers {
  [ApiController]
  [Route("[controller]/[action]")]
  public class JobsController : ControllerBase {
    private static readonly List<Demo> DemoJobs = new List<Demo>();
    private static readonly Random RandomCurrent = new Random();
    private readonly ILogger<JobsController> _logger;
    private readonly ISchedulerCenter _schedulerCenter;


    public JobsController(ILogger<JobsController> logger, ISchedulerCenter schedulerCenter) {
      _logger = logger;
      _schedulerCenter = schedulerCenter;
    }

    /// <summary>
    ///   新增任务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<MessageModel<string>>> Add(string name) {
      var next = RandomCurrent.Next(1, 20);
      var demo = new Demo { Id = Guid.NewGuid(), Name = name, AppId = "Test", Cron = $"0/{next} * * * * ?" };
      _logger.LogDebug("new demo：{@demo}", demo);
      var addScheduleJobAsync = await _schedulerCenter.AddScheduleJobAsync(demo);
      DemoJobs.Add(demo);
      return addScheduleJobAsync;
    }

    /// <summary>
    ///   停止任务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<MessageModel<string>>> Stop(string name) {
      var demo = GetDemo(name);
      if (demo == null) {
        return BadRequest($"没有找到名称:{name}的Job");
      }

      var ret = await _schedulerCenter.StopScheduleJobAsync(demo);
      return ret;
    }

    private Demo GetDemo(string name) {
      var demo = DemoJobs.FirstOrDefault(d => d.Name == name);
      return demo;
    }

    /// <summary>
    ///   重启任务
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<MessageModel<string>>> Resume(string name) {
      var demo = GetDemo(name);
      if (demo == null) {
        return BadRequest($"没有找到名称:{name}的Job");
      }

      var ret = await _schedulerCenter.ResumeJob(demo);
      return ret;
    }
  }
}