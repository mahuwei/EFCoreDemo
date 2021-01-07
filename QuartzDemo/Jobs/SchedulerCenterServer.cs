using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;

namespace QuartzDemo.Jobs {
  /// <summary>
  ///   任务调度管理中心
  /// </summary>
  public class SchedulerCenterServer : ISchedulerCenter {
    private readonly IJobFactory _iocJobFactory;
    private readonly ISchedulerFactory _schedulerFactory;
    private IScheduler _scheduler;

    public SchedulerCenterServer(IJobFactory jobFactory, ISchedulerFactory schedulerFactory) {
      _iocJobFactory = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));
      _schedulerFactory = schedulerFactory;
      _scheduler = GetSchedulerAsync().Result;
    }


    /// <summary>
    ///   添加一个计划任务（映射程序集指定IJob实现类）
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    public async Task<MessageModel<string>> AddScheduleJobAsync(Demo demo) {
      var result = new MessageModel<string>();

      if (demo != null) {
        try {
          var jobKey = new JobKey(demo.Id.ToString(), demo.AppId);
          if (await _scheduler.CheckExists(jobKey)) {
            result.success = false;
            result.msg = $"该任务计划已经在执行:【{demo.Name}】,请勿重复启动！";
            return result;
          }

          #region 通过反射获取程序集类型和类

          //var assembly = Assembly.Load(new AssemblyName(demo.AssemblyName));
          //var jobType = assembly.GetType(demo.AssemblyName + "." + demo.ClassName);

          #endregion

          //判断任务调度是否开启
          if (!_scheduler.IsStarted) {
            await StartScheduleAsync();
          }

          #region 泛型传递

          //传入反射出来的执行程序集
          //IJobDetail job = new JobDetailImpl(demo.Id.ToString(), demo.AppId, jobType);
          //job.JobDataMap.Add("JobParam", demo.JobParams);

          var jobDetail = JobBuilder.Create<DemoJob>()
            .WithIdentity(demo.Id.ToString(), demo.AppId)
            .SetJobData(new JobDataMap {
              new KeyValuePair<string, object>("Name", demo.Name),
              new KeyValuePair<string, object>("Id", demo.Id),
              new KeyValuePair<string, object>("Cron", demo.Cron),
              new KeyValuePair<string, object>("AppId", demo.AppId)
            })
            .Build();

          //IJobDetail job = JobBuilder.Create<T>()
          //    .WithIdentity(sysSchedule.Name, sysSchedule.JobGroup)
          //    .Build();

          #endregion

          ITrigger trigger;
          if (demo.Cron != null && CronExpression.IsValidExpression(demo.Cron) && demo.IsCron) {
            trigger = CreateCronTrigger(demo);
          }
          else {
            //demo.IntervalSecond = 5;
            trigger = CreateSimpleTrigger(demo);
          }

          // 告诉Quartz使用我们的触发器来安排作业
          await _scheduler.ScheduleJob(jobDetail, trigger);
          result.success = true;
          result.msg = $"启动任务:【{demo.Name}】成功";
          return result;
        }
        catch (Exception ex) {
          result.success = false;
          result.msg = $"任务计划异常:【{ex.Message}】";
          return result;
        }
      }

      result.success = false;
      result.msg = $"任务计划不存在:【{demo?.Name}】";
      return result;
    }

    /// <summary>
    ///   暂停一个指定的计划任务
    /// </summary>
    /// <returns></returns>
    public async Task<MessageModel<string>> StopScheduleJobAsync(Demo demo) {
      var result = new MessageModel<string>();
      var jobKey = new JobKey(demo.Id.ToString(), demo.AppId);
      if (!await _scheduler.CheckExists(jobKey)) {
        result.success = false;
        result.msg = $"未找到要停止的任务:【{demo.Name}】";
        return result;
      }

      //暂停还在_scheduler中，这边直接移除
      await _scheduler.PauseJob(jobKey);
      //var aaaa = await this._scheduler.GetTriggersOfJob(jobKey);
      //var sss = await this._scheduler.IsJobGroupPaused(demo.AppId);
      //var sss2 = await this._scheduler.GetJobGroupNames();
      //var bbbb = this._scheduler.TriggerJob(aaaa.First().JobKey);
      var res = await _scheduler.DeleteJob(jobKey);
      if (!res) {
        await _scheduler.ResumeJob(jobKey);
        result.msg = $"停止任务:【{demo.Name}】失败";
        return result;
      }

      result.success = true;
      result.msg = $"停止任务:【{demo.Name}】成功";
      return result;
    }

    /// <summary>
    ///   恢复指定的计划任务
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    public async Task<MessageModel<string>> ResumeJob(Demo demo) {
      var result = new MessageModel<string>();
      var jobKey = new JobKey(demo.Id.ToString(), demo.AppId);
      if (!await _scheduler.CheckExists(jobKey)) {
        return await AddScheduleJobAsync(demo);
      }

      //await this._scheduler.ResumeJob(jobKey);
      //await this._scheduler.TriggerJob(jobKey);//立即执行
      //_scheduler.RescheduleJob(triggerKey, trigger);//更新时间表达式、
      //_scheduler.GetCurrentlyExecutingJobs
      ITrigger trigger;
      if (demo.Cron != null && CronExpression.IsValidExpression(demo.Cron) && demo.IsCron) {
        trigger = CreateCronTrigger(demo);
      }
      else {
        trigger = CreateSimpleTrigger(demo);
      }

      var triggerKey = new TriggerKey(demo.Id.ToString(), demo.AppId);
      await _scheduler.RescheduleJob(triggerKey, trigger);


      result.success = true;
      result.msg = $"恢复计划任务:【{demo.Name}】成功";
      return result;
    }

    private async Task<IScheduler> GetSchedulerAsync() {
      if (_scheduler != null) {
        return _scheduler;
      }

      // 从Factory中获取Scheduler实例
      return _scheduler = await _schedulerFactory.GetScheduler();
    }

    #region _scheduler层级

    /// <summary>
    ///   开启任务调度
    /// </summary>
    /// <returns></returns>
    public async Task<MessageModel<string>> StartScheduleAsync() {
      var result = new MessageModel<string>();
      _scheduler.JobFactory = _iocJobFactory;
      if (!_scheduler.IsStarted) {
        //等待任务运行完成
        await _scheduler.Start();
        await Console.Out.WriteLineAsync("任务调度开启！");
        result.success = true;
        result.msg = "任务调度开启成功";
        return result;
      }

      result.success = false;
      result.msg = "任务调度已经开启";
      return result;
    }

    /// <summary>
    ///   停止任务调度
    /// </summary>
    /// <returns></returns>
    public async Task<MessageModel<string>> StopScheduleAsync() {
      var result = new MessageModel<string>();
      if (!_scheduler.IsShutdown) {
        await Task.Delay(30);
        //等待任务运行完成
        await _scheduler.Shutdown();
        await Console.Out.WriteLineAsync("任务调度停止！");
        result.success = true;
        result.msg = "任务调度停止成功";
        return result;
      }

      result.success = false;
      result.msg = "任务调度已经停止";
      return result;
    }

    #endregion

    #region 创建触发器帮助方法

    /// <summary>
    ///   创建SimpleTrigger触发器（简单触发器）
    /// </summary>
    /// <returns></returns>
    private ITrigger CreateSimpleTrigger(Demo demo) {
      /*
      if (demo.RunTimes > 0)
      {
          ITrigger trigger = TriggerBuilder.Create()
          //.StartNow()
          .WithIdentity(demo.Id.ToString(), demo.AppId)
          .WithSimpleSchedule(x =>
          x.WithIntervalInSeconds(demo.IntervalSecond)
          .WithRepeatCount(demo.RunTimes))//指定了执行次数
          .ForJob(demo.Id.ToString(), demo.AppId).Build();
          return trigger;
      }
      else
      {
          ITrigger trigger = TriggerBuilder.Create()
          //.StartNow()
          .WithIdentity(demo.Id.ToString(), demo.AppId)
          .WithSimpleSchedule(x =>
          x.WithIntervalInSeconds(demo.IntervalSecond)
          .RepeatForever()).ForJob(demo.Id.ToString(), demo.AppId).Build();
          return trigger;
      }
      */

      var trigger = TriggerBuilder.Create()
        //.StartNow() // 触发作业立即运行，然后每10秒重复一次，无限循环
        .WithIdentity(demo.Id.ToString(), demo.AppId)
        .WithSimpleSchedule(x =>
          x.WithIntervalInSeconds(demo.IntervalSecond)
            .RepeatForever())
        .ForJob(demo.Id.ToString(), demo.AppId)
        .Build();
      return trigger;
    }


    /// <summary>
    ///   创建类型Cron的触发器
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    private ITrigger CreateCronTrigger(Demo demo) {
      // 作业触发器
      return TriggerBuilder.Create()
        .WithIdentity(demo.Id.ToString(), demo.AppId)
        //.StartAt(demo.BeginTime.Value)//开始时间
        //.EndAt(demo.EndTime.Value)//结束数据
        .WithCronSchedule(demo.Cron) //指定cron表达式
        //.StartNow()
        .ForJob(demo.Id.ToString(), demo.AppId) //作业名称
        .Build();
    }

    #endregion
  }

  public class SingletonJobFactory : IJobFactory {
    private readonly IServiceProvider _serviceProvider;

    public SingletonJobFactory(IServiceProvider serviceProvider) {
      _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    //public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    //{
    //    return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
    //}

    //public void ReturnJob(IJob job)
    //{

    //}

    ///// <summary>
    ///// 实现接口Job
    ///// </summary>
    ///// <param name="bundle"></param>
    ///// <param name="scheduler"></param>
    ///// <returns></returns>
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler) {
      var serviceScope = _serviceProvider.CreateScope();
      var job = serviceScope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
      return job;
    }

    public void ReturnJob(IJob job) {
      if (job is IDisposable disposable) {
        disposable.Dispose();
      }
    }
  }
}