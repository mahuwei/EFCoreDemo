using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCoreDemo.Entities {
  /// <summary>
  ///   企业信息
  /// </summary>
  public class Business : Entity {
    [MaxLength(20)]
    public string No { get; set; }

    [MaxLength(20)]
    public string Name { get; set; }

    public ICollection<Employee> Employees { get; set; }
    public int ChangeNumber { get; set; }

    public StreetAddress StreetAddress { get; set; }
  }

  public class Employee : Entity {
    public Guid BusinessId { get; set; }

    [MaxLength(20)]
    public string WorkNo { get; set; }

    [MaxLength(20)]
    public string Name { get; set; }

    [MaxLength(20)]
    public string MobileNo { get; set; }

    public int ChangeNumber { get; set; }

    public StreetAddress StreetAddress { get; set; }
  }

  /// <summary>
  ///   从属实体类型
  ///   将转换为表的一部分，不会成为一个单独的表存在。
  /// </summary>
  [Owned]
  public class StreetAddress {
    public int ChangeNumber { get; set; }

    [MaxLength(20)]
    public string City { get; set; }

    [MaxLength(100)]
    public string Street { get; set; }
  }

  public class BusinessTypeConfiguration : EntityTypeConfiguration<Business> {
    public override void Configure(EntityTypeBuilder<Business> builder) {
      base.Configure(builder);
      builder.OwnsOne(d => d.StreetAddress);
    }
  }

  public class EmployeeTypeConfiguration : EntityTypeConfiguration<Employee> {
    public override void Configure(EntityTypeBuilder<Employee> builder) {
      base.Configure(builder);
      builder.OwnsOne(d => d.StreetAddress);
    }
  }

  //public class AddressConfiguration : EntityTypeConfiguration<StreetAddress> {
  //}
}