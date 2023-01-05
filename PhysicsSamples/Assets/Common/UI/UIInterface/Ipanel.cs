public interface Ipanel
{
    /// <summary>
    /// 打开此窗口,包括显示和逻辑加载
    /// </summary>
    public void Open();
    /// <summary>
    /// 关闭窗口,包括逻辑卸载
    /// </summary>
    public void Close();
}
