using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EFCoreDemo.Entities {
  /// <summary>
  ///   实体基类
  /// </summary>
  public abstract class Entity {
    /// <summary>
    ///   Id 主键
    /// </summary>
    [Description("Id")]
    public Guid Id { get; set; }

    /// <summary>
    ///   状态信息 <see cref="EntityStatus" />
    /// </summary>
    [Description("状态")]
    [DefaultValue(0)]
    public virtual int Status { get; set; }

    /// <summary>
    ///   SqlServer:最后修改时间
    ///   Mysql:行标签
    /// </summary>
    [Description("最后记录")]
    public virtual DateTime LastChange { get; set; }

    /// <summary>
    ///   备注信息
    /// </summary>
    [Description("备注信息")]
    [MaxLength(100)]
    public virtual string Memo { get; set; }

    /// <summary>
    ///   创建一个浅表克隆对象
    /// </summary>
    /// <returns></returns>
    public object Clone() {
      return MemberwiseClone();
    }

    /// <summary>
    ///   生成有序的Guid
    /// </summary>
    /// <param name="isSqlServer">是否是 sql server</param>
    public static Guid CreateGuid(bool isSqlServer = false) {
      return SequentialGuid.Create(isSqlServer
        ? SequentialGuidType.SequentialAtEnd
        : SequentialGuidType.SequentialAsString);
    }
  }

  /// <summary>
  ///   实体基本状态
  /// </summary>
  public enum EntityStatus {
    /// <summary>
    ///   正常
    /// </summary>
    [Description("正常")]
    Normal = 0,

    /// <summary>
    ///   已删除
    /// </summary>
    [Description("已删除")]
    Deleted = 1000
  }
}