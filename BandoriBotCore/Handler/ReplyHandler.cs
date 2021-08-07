using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualBasic.CompilerServices;
using Mirai_CSharp;
using MsgPack;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Handler
{
    using DataTypeS = Dictionary<string, List<Reply>>;
    using Function = Func<Match, Source, string, bool, Action<string>, string>;

    public class ReplyHandler : SerializableConfiguration<List<DataTypeS>>, IMessageHandler
    {
        public bool IgnoreCommandHandled => false;

        private const int version = 2;
        public override string Name => "reply.json";
        public DataTypeS data2, data3, data4;

        public DataTypeS this[int index] =>
            index switch
            {
                2 => data2,
                3 => data3,
                4 => data4,
                _ => null
            };

        public override void LoadDefault()
        {
            data2 = new DataTypeS();
            data3 = new DataTypeS();
            data4 = new DataTypeS();
        }

        public static Tuple<Regex, List<Reply>> D2T(KeyValuePair<string, List<Reply>> pair)
            => new Tuple<Regex, List<Reply>>(new Regex(@$"^{pair.Key}$", RegexOptions.Compiled), pair.Value);

        private static KeyValuePair<string, List<Reply>> T2D(Tuple<Regex, List<Reply>> tuple)
            => new KeyValuePair<string, List<Reply>>(tuple.Item1.ToString()[1..^1], tuple.Item2);

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            data2 = t[1];
            data3 = t[2];
            data4 = t[3];
            regexCache = new HashSet<string>(t.Where(t => t != null)
                .SelectMany(dict => dict.Select(pair => pair.Key)))
               .ToDictionary(t => t, t => new Regex($"^{Utils.FixRegex(t)}$", RegexOptions.Multiline | RegexOptions.Compiled));
            ReloadAssembly().Wait();
        }

        private static Regex replace = new Regex(@"\$((?!&).|&...;)", RegexOptions.Compiled);
        internal static Dictionary<string, Regex> regexCache = new();

        public static IEnumerable<(Match, Reply)> FitRegex(DataTypeS data, string content)
        {
            IEnumerable<(Match, Reply)> result = new List<(Match, Reply)>();

            foreach (var tuple in data)
            {
                var match = regexCache[tuple.Key].Match(content);
                if (match.Success)
                    result = result.Concat(tuple.Value.Select(reply => (match, reply)));
            }

            return result;
        }

        private static string FitReply((Match, Reply) tuple, Source sender)
            => replace.Replace(tuple.Item2.reply, m =>
            {
                var c = m.Value[1];
                if (c == '&')
                {
                    return m.Groups[1].Value.Decode();
                }
                if (c >= '0' && c <= '9')
                {
                    var n = c - '0';
                    if (n < tuple.Item1.Groups.Count)
                        return tuple.Item1.Groups[c - '0'].Value;
                }
                else if (c == '$') return "$";
                else if (c == 'g') return sender.FromGroup.ToString();
                else if (c == 'q') return sender.FromQQ.ToString();
                return m.Value;
            });

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            var raw = Utils.FindAtMe(args.message, out var isme, args.Sender.Session.QQNumber ?? 0).Trim();
            var isadmin = await args.Sender.HasPermission("*", -1);

            if (!GetConfig<Whitelist>().hash.Contains(args.Sender.FromGroup) && !isadmin)
            {
                var pending = FitRegex(data4, raw).Select(tuple => new Func<string>(() =>
                    GetFunction(tuple.Item2.reply)(tuple.Item1, args.Sender, args.message, isadmin, s => args.Callback(s).Wait())));
                var pa = pending.ToArray();

                if (pa.Length > 0)
                {
                    await args.Callback(pa[new Random().Next(pa.Length)]());
                    return true;
                }
                return false;
            }

            if (isme)
            {
                if (GetConfig<Blacklist>().hash.Contains(args.Sender.FromQQ))
                {
                    await args.Callback("调戏机器人吃枣药丸");
                    return true;
                }

                var pending = FitRegex(data2, raw).ToArray();

                if (pending.Length > 0)
                    await args.Callback(FitReply(pending[new Random().Next(pending.Length)], args.Sender));

                return true;
            }
            else
            {
                IEnumerable<Func<string>> pending = new List<Func<string>>();

                pending = pending.Concat(FitRegex(data3, raw).Select(tuple => new Func<string>(() => FitReply(tuple, args.Sender))));

                pending = pending.Concat(FitRegex(data4, raw).Select(tuple => new Func<string>(() =>
                    GetFunction(tuple.Item2.reply)(tuple.Item1, args.Sender, args.message, isadmin, s => args.Callback(s).Wait()))));

                var pa = pending.ToArray();

                if (pa.Length > 0)
                {
                    try
                    {
                        await args.Callback(pa[new Random().Next(pa.Length)]());
                    }
                    catch
                    {
                        return true;
                    }
                    return true;
                }
                return false;
            }
        }

        public Dictionary<string, Function> functions;

        public Function GetFunction(string code)
        {
            return functions[code];
        }

        private void ResolveAssemblies()
        {
            _ = typeof(HttpUtility);
            _ = typeof(SekaiClient.SekaiClient);
            _ = typeof(Process);
        }

        private static Regex codeReg = new Regex(@"/\* ref:(.*?) \*/", RegexOptions.Compiled | RegexOptions.Singleline);
        public async Task ReloadAssembly()
        {
            await Task.Yield();

            ResolveAssemblies();
            var functions = new Dictionary<string, Function>();
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                false,
                null, null, null, null, OptimizationLevel.Release,
                false, true, null, null, default, null, Platform.AnyCpu,
                ReportDiagnostic.Default, 4, null, true, false, null, null, null, DesktopAssemblyIdentityComparer.Default,
                null, false, MetadataImportOptions.Public
            );

            var options2 = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.Parse, SourceCodeKind.Regular);

            functions = new Dictionary<string, Function>();
            var dict = new Dictionary<string, int>();

            int func = 0;
            var sb = new StringBuilder();

            var usings = new string[]
            {
                "BandoriBot",
                "BandoriBot.Config",
                "BandoriBot.Handler",
                "System",
                "System.Collections.Generic",
                "System.Text.RegularExpressions",
                "System.Linq"
            };

            var refs = new List<string>();

            foreach (var name in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
            {
                try
                {
                    refs.Add(Assembly.Load(name).Location);
                }
                catch { }
            }

            foreach (var asm1 in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    refs.Add(asm1.Location);
                }
                catch { }
            }

            foreach (var @using in usings)
                sb.Append($"using {@using};\n");

            sb.Append("\npublic static class FunctionHolder\n{\n");

            foreach (var pair in data4)
            {
                foreach (var reply in pair.Value)
                {
                    if (dict.ContainsKey(reply.reply)) continue;

                    sb.Append($"public static string Function{++func}(Match match, Source source, string message, bool isAdmin, Action<string> callback)\n{{\n");
                    sb.Append(reply.reply.Decode());
                    sb.Append("\n}\n");
                    dict.Add(reply.reply, func);
                }
            }

            sb.Append("\n}\n");

            var codeText = sb.ToString();

            foreach (Match match in codeReg.Matches(codeText))
            {
                refs.Add(Assembly.Load(new AssemblyName(match.Groups[1].Value)).Location);
            }

            var references = refs.Where(r => !string.IsNullOrEmpty(r)).Select(r => MetadataReference.CreateFromFile(r));

            File.WriteAllText("debug.cs", codeText);

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(codeText, options2, "code.cs", Encoding.UTF8, default);
            var compilation = CSharpCompilation.Create("Reply", new SyntaxTree[] { syntaxTree }, references, options);

            using var ms = new MemoryStream();

            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var msg = string.Join("\n",

                    result.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning).Select(diagnostic =>
                    {
                        FileLinePositionSpan lineSpan = diagnostic.Location.GetLineSpan();
                        return new CompilerError
                        {
                            ErrorNumber = diagnostic.Id,
                            IsWarning = (diagnostic.Severity == DiagnosticSeverity.Warning),
                            ErrorText = diagnostic.GetMessage(null),
                            FileName = (lineSpan.Path ?? ""),
                            Line = lineSpan.StartLinePosition.Line + 1,
                            Column = lineSpan.StartLinePosition.Character
                        }.ToString();
                    }));
                if (msg.Length > 1024) msg = msg.Substring(0, 1024);

                throw new Exception(msg);
            }

            var asm = Assembly.Load(ms.ToArray());

            Type holder = asm.GetType("FunctionHolder");

            foreach (var pair in dict)
                functions.Add(pair.Key, (Function)holder.GetMethod($"Function{pair.Value}").CreateDelegate(typeof(Function), null));

            this.functions = functions;
        }
    }
}
