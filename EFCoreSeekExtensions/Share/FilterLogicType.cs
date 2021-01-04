using System.ComponentModel;

namespace LinqSeekExtensions.Share {
  /// <summary>
  ///   逻辑运算符
  /// </summary>
  public enum FilterLogicType {
    /// <summary>
    ///   并且
    /// </summary>
    [Description("并且")]
    And,

    /// <summary>
    ///   或者
    /// </summary>
    [Description("或者")]
    Or
  }
}