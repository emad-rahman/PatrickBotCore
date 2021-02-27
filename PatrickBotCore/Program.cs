using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RedditSharp;

namespace PatrickBotCore
{
    public class Program
    {
        public static Task Main(string[] args)
            =>  Startup.RunAsync(args);
    }
}
