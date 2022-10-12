using System;
using BandoriBot.Config;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BandoriBot.Services;
using PCRApi;
using PCRApi.Models.Db;

namespace BandoriBot.Commands
{
    public class WhoisCommand : ICommand
    {
        [DllImport("Texture2DDecoderNative", CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern unsafe bool DecodeETC2A8(void* data, int width, int height, void* image);


        private static unsafe bool DecodeETC2A8(byte[] data, int width, int height, byte[] image)
        {
            fixed (byte* pData = data)
            {
                fixed (byte* pImage = image)
                {
                    return DecodeETC2A8(pData, width, height, pImage);
                }
            }
        }

        public List<string> Alias => new List<string> { "Ë­ÊÇ" };
        public async Task Run(CommandArgs args)
        {
            var unit = PCRApi.Commands.Commands.Parse(args.Arg.Trim());
            var name0 = PCRApi.Utility.Utils.CacheSearchName(unit);
            var skin = 3;
            if (masterContextCache.instance.Rarity6QuestData.Any(r => r.UnitId == unit)) skin = 6;
            unit += skin * 10;
            var name = $"a/unit_icon_unit_{unit}.unity3d";
            var file = await PCRApi.Controllers.AssetController.manager.ResolveFile(name);
            var am = new AssetsManager();
            using var ms = new MemoryStream(file);
            var bun = am.LoadBundleFile(ms, name);
            var inst = am.LoadAssetsFileFromBundle(bun, 0);

            var inf = inst.table.GetAssetsOfType((int) AssetClassID.Texture2D).First();
            var atvf = am.GetTypeInstance(inst.file, inf).GetBaseField();
            var tex = TextureFile.ReadTextureFile(atvf);
            tex.GetTextureData(null, bun.file);
            var data = new byte[tex.m_Width * tex.m_Height * 4];
            DecodeETC2A8(tex.pictureData, tex.m_Width, tex.m_Height, data);

            using var bitmap = new Bitmap(tex.m_Width, tex.m_Height, tex.m_Width * 4, PixelFormat.Format32bppArgb,
                Marshal.UnsafeAddrOfPinnedArrayElement(data, 0));

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            await args.Callback($"{name0}:\n{Utils.GetImageCode(bitmap)}");
        }
    }
}
