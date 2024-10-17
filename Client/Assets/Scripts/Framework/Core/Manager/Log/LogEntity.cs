// author:KIPKIPS
// describe:日志打印对象

using Framework.Core.Pool;

/// <summary>
/// 日志实例
/// </summary>
public struct LogEntity : IPoolAble
{
    /// <summary>
    /// 回收日志实例
    /// </summary>
    public void OnRecycled()
    {
    }

    /// <summary>
    /// 是否回收
    /// </summary>
    public bool IsRecycled { get; set; }

    /// <summary>
    /// 日志内容
    /// </summary>
    public string Content;

    /// <summary>
    /// 是否行内日志
    /// </summary>
    public bool InnerLine;

    /// <summary>
    /// 初始化日志实例
    /// </summary>
    /// <param name="content"></param>
    /// <param name="innerLine"></param>
    public void Set(string content, bool innerLine)
    {
        Content = content;
        InnerLine = innerLine;
    }
}