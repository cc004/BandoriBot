using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace BandoriBot
{
	/// <summary>
	/// 酷Q Api封装类
	/// </summary>
	public class CqApi
	{
		public GroupMemberInfo GetMemberInfo (long groupId, long qqId, bool notCache = false)
		{
			throw new NotImplementedException();
		}

		public List<GroupMemberInfo> GetMemberList (long groupId)
		{
			throw new NotImplementedException();
		}

		public List<GroupInfo> GetGroupList ()
		{
			throw new NotImplementedException();
		}

		public GroupInfo GetGroupInfo (long groupId, bool notCache = false)
		{
			throw new NotImplementedException();
		}

		#region --管理--

		/// <summary>
		/// 置群成员专属头衔
		/// </summary>
		/// <param name="groupId">目标群</param>
		/// <param name="qqId">目标QQ</param>
		/// <param name="specialTitle">如果要删除，这里填空</param>
		/// <param name="time">专属头衔有效期，单位为秒。如果永久有效，time填写负数</param>
		/// <returns></returns>
		public int SetGroupSpecialTitle (long groupId, long qqId, string specialTitle, TimeSpan time)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
