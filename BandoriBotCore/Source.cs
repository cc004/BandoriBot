using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BandoriBot.Config;
using BandoriBot.Models;
using Sora.Entities.Base;
using Sora.Enumeration.EventParamsType;

namespace BandoriBot
{
    public struct Source
    {
        public DateTime time;
        public long FromGroup, FromQQ;
        public SoraApi Session;
        public bool IsGuild;

        internal static readonly HashSet<long> AdminQQs = new(File.ReadAllText("adminqq.txt").Split('\n').Select(long.Parse));

        public bool IsSuperadmin => AdminQQs.Contains(FromQQ);
        
        private static PermissionConfig cfg = Configuration.GetConfig<PermissionConfig>();

        private async Task<bool> CheckPermission(long target = 0,
            MemberRoleType required = MemberRoleType.Admin)
        {
            if (IsGuild)
            {
                var res = (await Session.GetGuildMembers(MessageHandler.GetGroupCache(target).guild)).memberInfo;
                var qq = FromQQ;

                var info = res.bots.Concat(res.admins).Concat(res.members).FirstOrDefault(m => m.UserId == qq); 
                this.Log(LoggerLevel.Info, $"guild perm {target}::{FromQQ} = {info?.Role}");
                return (info?.Role ?? MemberRoleType.Unknown) >= required;
            }
            return (await Session.GetGroupMemberInfo(target, FromQQ)).memberInfo.Role >= required;
        }

        public async Task<bool> HasPermission(string perm) => await HasPermission(perm, -1);
        public async Task<bool> HasPermission(string perm, long group) =>
            IsSuperadmin || perm == null ||
            cfg.t.ContainsKey(FromQQ) && (
                cfg.t[FromQQ].Contains($"*.{perm}") ||
                cfg.t[FromQQ].Contains($"{group}.{perm}")) ||
            perm.Contains('.') && await HasPermission(perm.Substring(0, perm.LastIndexOf('.')), group) ||
            perm != "*" && await HasPermission("*", group) || group != 0 && group != -1 && await CheckPermission(group);
    }
}