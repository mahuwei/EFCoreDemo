using System.Threading.Tasks;

namespace QuartzDemo.Jobs {
  public interface ISchedulerCenter {
    /// <summary>
    ///   开启任务调度
    /// </summary>
    /// <returns></returns>
    Task<MessageModel<string>> StartScheduleAsync();

    /// <summary>
    ///   停止任务调度
    /// </summary>
    /// <returns></returns>
    Task<MessageModel<string>> StopScheduleAsync();

    /// <summary>
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    Task<MessageModel<string>> AddScheduleJobAsync(Demo demo);

    /// <summary>
    ///   停止一个任务
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    Task<MessageModel<string>> StopScheduleJobAsync(Demo demo);

    /// <summary>
    ///   恢复一个任务
    /// </summary>
    /// <param name="demo"></param>
    /// <returns></returns>
    Task<MessageModel<string>> ResumeJob(Demo demo);
  }
}