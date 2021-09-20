namespace BandoriBot.Models
{
    /// <summary>
    /// 群成员信息
    /// </summary>
    public class GroupMemberInfo
    {
        /// <summary>
        /// 获取或设置一个值, 指示成员所在群
        /// </summary>
        public long GroupId { get; set; }
        /// <summary>
        /// 获取或设置一个值, 指示成员QQ
        /// </summary>
        public long QQId { get; set; }
        /// <summary>
        /// 获取或设置一个值, 指示成员最后发言时间
        /// </summary>
        //public DateTime LastDateTime { get; set; }
        /// <summary>
        /// 获取或设置一个值, 指示成员在此群的权限
        /// </summary>
        public PermitType PermitType { get; set; }
    }
}
