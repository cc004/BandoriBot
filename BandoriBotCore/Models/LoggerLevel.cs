namespace BandoriBot.Models
{
    /// <summary>
    /// 日志等级
    /// </summary>
    public enum LoggerLevel
    {
        /// <summary>
        /// 调试
        /// </summary>
        Debug = 0,
        /// <summary>
        /// 信息
        /// </summary>
        Info = 10,
        /// <summary>
        /// 警告
        /// </summary>
        Warn = 20,
        /// <summary>
        /// 错误
        /// </summary>
        Error = 30,
        /// <summary>
        /// 严重错误
        /// </summary>
        Fatal = 40
    }
}
