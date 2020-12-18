using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Entities {
  /// <summary>
  ///   EF实体类型定义，基类定义
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class EntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : Entity {
    /// <summary>
    ///   配置
    /// </summary>
    /// <param name="builder"></param>
    public virtual void Configure(EntityTypeBuilder<T> builder) {
      builder.HasKey(b => b.Id);
      builder.Property(b => b.Status).IsRequired().HasMaxLength(10);
      builder.Property(b => b.LastChange).IsRowVersion().IsConcurrencyToken();
      builder.Property(b => b.Memo).HasMaxLength(200);
    }
  }
}