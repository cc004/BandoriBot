using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class MessageStatistic : Configuration, IMessageHandler
    {
        public Dictionary<long, Dictionary<long, int>> Data;

        public override string Name => "Statistic";

        public override void LoadDefault()
        {
            Data = new Dictionary<long, Dictionary<long, int>>();
        }

        public override void LoadFrom(BinaryReader br)
        {
            Data = new Dictionary<long, Dictionary<long, int>>();

            int size = br.ReadInt32();
            for (int i = 0; i < size; ++i)
            {
                long group = br.ReadInt64();
                int size2 = br.ReadInt32();
                Data[group] = new Dictionary<long, int>();
                for (int j = 0; j < size2; ++j)
                    Data[group][br.ReadInt64()] = br.ReadInt32();
            }
        }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            if (Sender.FromGroup == 0) return false;
            lock (Data)
            {
                if (!Data.ContainsKey(Sender.FromGroup))
                    Data[Sender.FromGroup] = new Dictionary<long, int>();
                Dictionary<long, int> dic = Data[Sender.FromGroup];
                if (!dic.ContainsKey(Sender.FromQQ))
                    dic[Sender.FromQQ] = 1;
                else
                    ++dic[Sender.FromQQ];
                Save();
            }
            return false;
        }

        public override void SaveTo(BinaryWriter bw)
        {
            bw.Write(Data.Count);

            foreach (var pair in Data)
            {
                bw.Write(pair.Key);
                bw.Write(pair.Value.Count);
                foreach (var pair2 in pair.Value)
                {
                    bw.Write(pair2.Key);
                    bw.Write(pair2.Value);
                }
            }
        }
    }
}
