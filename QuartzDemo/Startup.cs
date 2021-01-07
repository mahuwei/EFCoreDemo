using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Quartz;
using QuartzDemo.Jobs;

namespace QuartzDemo {
  public class Startup {
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
      services.AddControllers();
      services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuartzDemo", Version = "v1" });
      });

      services.AddTransient<DemoJob>();
      services.AddQuartz(q => {
        q.UseMicrosoftDependencyInjectionScopedJobFactory();
        //var jobKey = new JobKey("DemoJobKey");
        //q.AddJob<DemoJob>(opts => {
        //  opts.WithIdentity(jobKey);
        //  opts.SetJobData(new JobDataMap { new KeyValuePair<string, object>("UserName", "Tom") });
        //});

        //q.AddTrigger(opts => {
        //  opts.ForJob(jobKey).WithIdentity("DemoJob-trigger").WithCronSchedule("0/5 * * * * ?");
        //});
      });

      services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
      services.AddSingleton<ISchedulerCenter, SchedulerCenterServer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
      if (env.IsDevelopment()) {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QuartzDemo v1"));
      }

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints => {
        endpoints.MapControllers();
      });
    }
  }
}