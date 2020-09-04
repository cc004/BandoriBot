using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.DataStructures;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

namespace BandoriBot.Handler
{
    using DataType = Dictionary<string, List<Reply>>;
    using Function = Func<Match, Source, string, bool, ResponseCallback, string>;

    public class ReplyHandler : SerializableConfiguration<List<DataType>>, IMessageHandler
    {
        private const int version = 2;
        public override string Name => "reply.json";
        public DataType data, data2, data3, data4;

        public DataType this[int index] =>
            index switch
            {
                1 => data,
                2 => data2,
                3 => data3,
                4 => data4,
                _ => null
            };

        public override void LoadDefault()
        {
            data = new DataType();
            data2 = new DataType();
            data3 = new DataType();
            data4 = new DataType();
        }

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            data = t[0];
            data2 = t[1];
            data3 = t[2];
            data4 = t[3];
        }

        public override void SaveTo(BinaryWriter bw)
        {
            t = new List<DataType>
            {
                data,
                data2,
                data3,
                data4
            };
            base.SaveTo(bw);
        }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            var raw = Utils.FindAtMe(message, out var isme, Sender.Session.QQNumber ?? 0).Trim();
            if (isme)
            {
                if (GetConfig<Blacklist>().hash.Contains(Sender.FromQQ))
                {
                    callback("调戏机器人吃枣药丸");
                    return true;
                }

                var pending = data.TryGetValue(raw, out var list) ?
                    list.Select((r) => r.reply).ToList() :
                    new List<string>();

                foreach (var pair in data2)
                {
                    var match = new Regex(@$"^{Utils.FixRegex(pair.Key)}$").Match(raw);
                    if (match.Success)
                    {
                        var reply = pair.Value[new Random().Next(pair.Value.Count)].reply;
                        pending.Add(new Regex(@"\$((?!&).|&...;)").Replace(reply, (m) =>
                        {
                            var c = m.Value[1];
                            if (c == '&')
                            {
                                return m.Groups[1].Value.Decode();
                            }
                            if (c >= '0' && c <= '9')
                            {
                                var n = (int)(c - '0');
                                if (n < match.Groups.Count)
                                    return match.Groups[n].Value;
                            }
                            else if (c == '$') return "$";
                            else if (c == 'g') return Sender.FromGroup.ToString();
                            else if (c == 'q') return Sender.FromQQ.ToString();
                            return m.Value;
                        }));
                    }
                }
                if (pending.Count > 0)
                    callback(pending[new Random().Next(pending.Count)]);

                return true;
            }
            else
            {
                var pending = new List<Func<string>>();
                foreach (var pair in data3)
                {
                    var match = new Regex(@$"^{Utils.FixRegex(pair.Key)}$").Match(raw);
                    if (match.Success)
                    {
                        var reply = pair.Value[new Random().Next(pair.Value.Count)].reply;
                        var temp = new Regex(@"\$((?!&).|&...;)").Replace(reply, (m) =>
                        {
                            var c = m.Value[1];
                            if (c == '&')
                            {
                                return m.Groups[1].Value.Decode();
                            }
                            if (c >= '0' && c <= '9')
                            {
                                var n = (int)(c - '0');
                                if (n < match.Groups.Count)
                                    return match.Groups[n].Value;
                            }
                            else if (c == '$') return "$";
                            else if (c == 'g') return Sender.FromGroup.ToString();
                            else if (c == 'q') return Sender.FromQQ.ToString();
                            return m.Value;
                        });
                        pending.Add(() => temp);
                    }
                }

                foreach (var pair in data4)
                {
                    var match = new Regex(@$"^{Utils.FixRegex(pair.Key)}$").Match(raw);

                    if (match.Success)
                    {
                        var reply = pair.Value[new Random().Next(pair.Value.Count)].reply;
                        pending.Add(() => GetFunction(reply)(match, Sender, message, isAdmin, callback));
                    }
                }
                
                if (pending.Count > 0)
                {
                    callback(pending[new Random().Next(pending.Count)]());
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

        public void ReloadAssembly()
        {
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
                "System.Text.RegularExpressions"
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
                foreach (var reply in pair.Value)
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
        }
    }
}
