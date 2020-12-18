using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDemo.Entities {
  public class ContextDemo : DbContext {
    public const string MysqlConnection = "Server=192.168.1.51;Database=ContextDemo;User=root;Password=123456;";

    protected ContextDemo() {
    }

    /// <summary>
    ///   导入配置构造函数
    /// </summary>
    /// <param name="options"></param>
    public ContextDemo(DbContextOptions<ContextDemo> options) : base(options) {
    }

    public DbSet<Business> Businesses { get; set; }
    public DbSet<Employee> Employees { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
      if (optionsBuilder.IsConfigured == false) {
        optionsBuilder.UseMySql(MysqlConnection, ServerVersion.AutoDetect(MysqlConnection));
      }

      base.OnConfiguring(optionsBuilder);
    }


    /// <summary>
    ///   迁移代码生成
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);
      //将实现了IEntityTypeConfiguration<Entity>接口的模型配置类加入到modelBuilder中，进行注册
      var assembly = typeof(ContextDemo).GetTypeInfo().Assembly;
      var typesToRegister = assembly.GetTypes()
        .Where(q => {
          var fullName = typeof(IEntityTypeConfiguration<>).FullName;
          return fullName != null && q.GetInterface(fullName) != null;
        });
      foreach (var type in typesToRegister) {
        if (type == typeof(EntityTypeConfiguration<>)) {
          continue;
        }

        dynamic configurationInstance = Activator.CreateInstance(type);
        if (configurationInstance == null) {
          continue;
        }

        modelBuilder.ApplyConfiguration(configurationInstance);
      }
    }
  }
}