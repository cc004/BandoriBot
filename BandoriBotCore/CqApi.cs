using BandoriBot.Models;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot
{
	/// <summary>
	/// 酷Q Api封装类
	/// </summary>
	public static class CqApi
	{
		public static async Task<List<Models.GroupMemberInfo>> GetMemberList(this MiraiHttpSession session, long groupId)
		{
			return (await session.GetGroupMemberListAsync(groupId)).Select(info => new Models.GroupMemberInfo
            {
				GroupId = groupId,
				QQId = info.Id,
				PermitType = info.Permission switch
				{
					GroupPermission.Owner => PermitType.Holder,
					GroupPermission.Administrator => PermitType.Manage,
					_ => PermitType.None
				}
			}).ToList();
		}

		public static async Task<List<Models.GroupInfo>> GetGroupList(this MiraiHttpSession session)
		{
			return (await session.GetGroupListAsync()).Select(info => new Models.GroupInfo
            {
				Id = info.Id,
				Name = info.Name
			}).ToList();
		}

		public static int SetGroupSpecialTitle(this MiraiHttpSession session, long groupId, long qqId, string specialTitle, TimeSpan time)
		{
			throw new NotImplementedException();
		}
	}
}
