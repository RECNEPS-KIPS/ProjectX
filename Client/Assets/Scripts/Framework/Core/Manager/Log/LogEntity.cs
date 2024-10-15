// author:KIPKIPS
// describe:日志打印对象
using Framework.Core.Pool;

public struct LogEntity : IPoolAble {
    public void OnRecycled() {
    }
    public bool IsRecycled { get; set; }
    public string Content;
    public bool InnerLine;
    public void Set(string _content, bool _innerLine) {
        Content = _content;
        InnerLine = _innerLine;
    }
}