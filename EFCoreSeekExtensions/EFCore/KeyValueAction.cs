namespace LinqSeekExtensions.EFCore {
  /// <summary>
  ///   过滤条件项目
  /// </summary>
  public class KeyValueAction {
    /// <summary>
    ///   获得/设置 Filter 项字段名称
    /// </summary>
    public string FieldKey { get; set; }

    /// <summary>
    ///   获得/设置 Filter 项字段值
    /// </summary>
    public object FieldValue { get; set; }

    /// <summary>
    ///   获得/设置 Filter 项与其他 Filter 逻辑关系
    /// </summary>
    public FilterLogicType FilterLogicType { get; set; }

    /// <summary>
    ///   获得/设置 Filter 条件行为
    /// </summary>
    public FilterActionType FilterActionType { get; set; }
  }
}