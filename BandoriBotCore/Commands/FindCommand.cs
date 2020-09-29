using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class FindCommand : ICommand
    {
        private List<GroupMemberInfo> infos = new List<GroupMemberInfo>();
        private List<GroupInfo> groups = new List<GroupInfo>();
        public List<string> Alias => new List<string>
        {
            "/find"
        };

        public void Run(CommandArgs args)
        {
            string[] splits = args.Arg.Trim().Split(' ');
            if (splits.Length < 1)
            {
                args.Callback("/find <refresh/count/id> ...");
                return;
            }
            switch (splits[0])
            {
                case "refresh":
                    if (!args.IsAdmin)
                    {
                        args.Callback("Access denied!");
                        return;
                    }
                    infos.Clear();
                    args.Callback($"refreshing...please wait.");
                    foreach (var group in (groups = Common.CqApi.GetGroupList()))
                        foreach (var member in Common.CqApi.GetMemberList(group.Id) ?? new List<GroupMemberInfo>())
                            infos.Add(member);
                    var idhash = new HashSet<long>(groups.Select((group) => group.Id));
                    var groupfile = Path.Combine("groups.json");
                    if (File.Exists(groupfile))
                    {
                        try
                        {
                            args.Callback("reading from groups.json...");
                            var json = JArray.Parse(File.ReadAllText(groupfile));
                            foreach (JObject group in json)
                            {
                                GroupInfo info = new GroupInfo
                                {
                                    Id = (long)group["id"],
                                    Name = (string)group["name"]
                                };
                                if (!idhash.Contains(info.Id))
                                {
                                    groups.Add(info);
                                    idhash.Add(info.Id);
                                }
                                foreach (JObject member in group["members"])
                                    infos.Add(new GroupMemberInfo
                                    {
                                        Nick = (string)member["nick"],
                                        QQId = (long)member["qq"],
                                        Age = (int)member["age"],
                                        Sex = (Sex)Enum.Parse(typeof(Sex), (string)member["sex"]),
                                        Card = (string)member["card"],
                                        PermitType = (PermitType)Enum.Parse(typeof(PermitType), (string)member["position"]),
                                        JoiningTime = DateTime.Parse((string)member["join"]),
                                        LastDateTime = DateTime.Parse((string)member["last"]),
                                        SpecialTitle = (string)member["title"],
                                        GroupId = info.Id
                                    });

                            }
                        }
                        catch (Exception e)
                        {
                            args.Callback(e.ToString());
                        }
                    }
                    args.Callback($"Member info has refreshed succussfully. ({infos.Count} records in {groups.Count} groups)");
                    break;
                case "count":
                    var users = new HashSet<long>();
                    DateTime active;
                    try
                    {
                        active = DateTime.Parse(splits[1]);
                    }
                    catch
                    {
                        active = new DateTime(0);
                    }
                    foreach (var member in infos)
                        users.Add(member.QQId);
                    args.Callback(@$"{
                        new HashSet<long>(infos
                            .Where((info) => info.LastDateTime > active)
                            .Select((info) => info.QQId)).Count
                        } members in total {(active.Ticks == 0 ? "" : "is active after " + active.ToString())} (counting in {groups.Count} groups).");
                    break;
                case "id":
                    if (splits.Length < 2)
                    {
                        args.Callback("Invalid argument count.");
                        return;
                    }
                    if (long.TryParse(splits[1], out long qq))
                    {
                        if (!args.IsAdmin)
                        {
                            args.Callback("Access denied.");
                            return;
                        }
                        var total = 0;
                        var list = string.Concat(infos.Where((member) => member.QQId == qq)
                            .Select((member) => @$"{++total}. {
                                groups.Where((group) => group.Id == member.GroupId)
                                .FirstOrDefault()?.Name ?? "<未找到群信息>"}({member.GroupId}) {
                                member.PermitType switch
                                {
                                    PermitType.Holder => "(群主)",
                                    PermitType.Manage => "(管理)",
                                    _ => "",
                                }}
"));
                        args.Callback($"{qq}所在的群(共{total}个):\n{list}");
                    }
                    break;
                    
            }
        }
    }
}
