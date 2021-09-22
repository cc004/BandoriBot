using BandoriBot.Config;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 开启前缀检测
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func1Enabled = true;
                global.Save();
            }
        }
        public class 关闭前缀检测
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func1Enabled = false;
                global.Save();
            }
        }
        public class 关闭自动清人
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func2Enabled = false;
                global.Save();
            }
        }
        public class 开启自动清人
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func2Enabled = true;
                global.Save();
            }
        }
    }
}
