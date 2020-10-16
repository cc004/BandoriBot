using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.DataStructures;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Handler
{
    using DataType = List<Tuple<Regex, List<Reply>>>;
    using DataTypeS = Dictionary<string, List<Reply>>;
    using Function = Func<Match, Source, string, bool, ResponseCallback, string>;

    public class ReplyHandler : SerializableConfiguration<List<DataTypeS>>, IMessageHandler
    {
        private const int version = 2;
        public override string Name => "reply.json";
        public DataType data2, data3, data4;

        public DataType this[int index] =>
            index switch
            {
                2 => data2,
                3 => data3,
                4 => data4,
                _ => null
            };

        public override void LoadDefault()
        {
            data2 = new DataType();
            data3 = new DataType();
            data4 = new DataType();
        }

        public static Tuple<Regex, List<Reply>> D2T(KeyValuePair<string, List<Reply>> pair)
            => new Tuple<Regex, List<Reply>>(new Regex(@$"^{Utils.FixRegex(pair.Key)}$", RegexOptions.Compiled), pair.Value);

        private static KeyValuePair<string, List<Reply>> T2D(Tuple<Regex, List<Reply>> tuple)
            => new KeyValuePair<string, List<Reply>>(tuple.Item1.ToString()[1..^1], tuple.Item2);

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            data2 = t[1].Select(D2T).ToList();
            data3 = t[2].Select(D2T).ToList();
            data4 = t[3].Select(D2T).ToList();

            ReloadAssembly();
        }

        public override void SaveTo(BinaryWriter bw)
        {
            t = new List<DataTypeS>
            {
                null,
                new DataTypeS(data2.Select(T2D)),
                new DataTypeS(data3.Select(T2D)),
                new DataTypeS(data4.Select(T2D))
            };
            base.SaveTo(bw);
        }

        private static Regex replace = new Regex(@"\$((?!&).|&...;)", RegexOptions.Compiled);

        public static IEnumerable<(Match, Reply)> FitRegex(DataType data, string content)
        {
            IEnumerable<(Match, Reply)> result = new List<(Match, Reply)>();

            foreach (var tuple in data)
            {
                var match = tuple.Item1.Match(content);
                if (match.Success)
                    result = result.Concat(tuple.Item2.Select(reply => (match, reply)));
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

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            var raw = Utils.FindAtMe(message, out var isme, Sender.Session.QQNumber ?? 0).Trim();

            if (!GetConfig<Whitelist>().hash.Contains(Sender.FromGroup) && !isAdmin)
            {
                var pending = FitRegex(data4, raw).Select(tuple => new Func<string>(() =>
                    GetFunction(tuple.Item2.reply)(tuple.Item1, Sender, message, isAdmin, callback)));
                var pa = pending.ToArray();

                if (pa.Length > 0)
                {
                    callback(pa[new Random().Next(pa.Length)]());
                    return true;
                }
                return false;
            }

            if (isme)
            {
                if (GetConfig<Blacklist>().hash.Contains(Sender.FromQQ))
                {
                    callback("调戏机器人吃枣药丸");
                    return true;
                }

                var pending = FitRegex(data2, raw).ToArray();

                if (pending.Length > 0)
                    callback(FitReply(pending[new Random().Next(pending.Length)], Sender));

                return true;
            }
            else
            {
                IEnumerable<Func<string>> pending = new List<Func<string>>();

                pending = pending.Concat(FitRegex(data3, raw).Select(tuple => new Func<string>(() => FitReply(tuple, Sender))));

                pending = pending.Concat(FitRegex(data4, raw).Select(tuple => new Func<string>(() =>
                    GetFunction(tuple.Item2.reply)(tuple.Item1, Sender, message, isAdmin, callback))));

                var pa = pending.ToArray();

                if (pa.Length > 0)
                {
                    callback(pa[new Random().Next(pa.Length)]());
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
            typeof(HttpUtility).GetType();
        }

        public void ReloadAssembly()
        {
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

            var references = new List<PortableExecutableReference>();

            foreach (var asm2 in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(asm2.Location));
                }
                catch { }
            }

            foreach (var @using in usings)
                sb.Append($"using {@using};\n");

            sb.Append("\npublic static class FunctionHolder\n{\n");

            foreach (var pair in data4)
            {
                foreach (var reply in pair.Item2)
                {
                    if (dict.ContainsKey(reply.reply)) continue;

                    sb.Append($"public static string Function{++func}(Match match, Source source, string message, bool isAdmin, ResponseCallback callback)\n{{\n");
                    sb.Append(reply.reply.Decode());
                    sb.Append("\n}\n");
                    dict.Add(reply.reply, func);
                }
            }

            sb.Append("\n}\n");

            var codeText = sb.ToString();
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
