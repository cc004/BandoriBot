using System;
using System.IO;

namespace BandoriBot
{
    /// <summary>
    /// 用于存放 App 数据的公共类
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// 获取或设置当前 App 使用的 酷Q Api 接口实例
        /// </summary>
        public static CqApi CqApi { get; set; } = new CqApi();
    }
}
