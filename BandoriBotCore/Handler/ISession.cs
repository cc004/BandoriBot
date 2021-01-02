using Mirai_CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Handler
{
    public interface ISession
    {
        public MiraiHttpSession Session { get; set; }
    }
}
