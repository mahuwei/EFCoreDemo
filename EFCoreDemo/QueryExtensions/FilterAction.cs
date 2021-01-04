using System.ComponentModel;

namespace EFCoreDemo.QueryExtensions {
  /// <summary>
  ///   关系运算符
  /// </summary>
  public enum FilterAction {
    /// <summary>
    ///   等于
    /// </summary>
    [Description("等于")]
    Equal,

    /// <summary>
    ///   不等于
    /// </summary>
    [Description("不等于")]
    NotEqual,

    /// <summary>
    ///   大于
    /// </summary>
    [Description("大于")]
    GreaterThan,

    /// <summary>
    ///   大于等于
    /// </summary>
    [Description("大于等于")]
    GreaterThanOrEqual,

    /// <summary>
    ///   小于
    /// </summary>
    [Description("小于")]
    LessThan,

    /// <summary>
    ///   小于等于
    /// </summary>
    [Description("小于等于")]
    LessThanOrEqual,

    /// <summary>
    ///   包含
    /// </summary>
    [Description("包含")]
    Contains,

    /// <summary>
    ///   不包含
    /// </summary>
    [Description("不包含")]
    NotContains,

    /// <summary>
    ///   自定义条件
    /// </summary>
    [Description("自定义条件")]
    CustomPredicate
  }
}