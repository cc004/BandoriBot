using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BandoriBot;

namespace BasePlugin
{
    internal class Testing
    {
        private static string Test(Match match, Source source)
        {
            var file = match.Groups[1].Value.Trim();
            var content = PCRApi.Controllers.AssetController.manager.ResolveFile(file).Result;
            var filename = file.Split('/').Last();
            var localfile = System.IO.Path.GetFullPath(System.IO.Path.Combine("imagecache", filename));
            System.IO.File.WriteAllBytes(localfile, content);
            var res = source.Session.UploadGroupFile(source.FromGroup, localfile, filename).AsTask().Result;
            return $"{res.ApiMessage}\n{res.ApiStatusStr}\n{res.RetCode}";
        }
    }
}