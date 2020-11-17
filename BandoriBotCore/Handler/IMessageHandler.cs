using BandoriBot.Models;
using System;

namespace BandoriBot.Handler
{
    public interface IMessageHandler
    {
        bool OnMessage(string message, Source Sender, bool isAdmin, Action<string> callback);
    }
}
