using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using AnnounceBot.Configuration;
using Discord.Modules;
using Discord.Commands;
using Discord.Audio;
using AnnounceBot.Modules;

namespace AnnounceBot
{
    class AnnouceBot
    {
        private DiscordClient _client;
        private BotConfiguration _config;

        //Bot information
        public string AppName = "AnnounceBot";
        public string AppUrl = "https://github.com/epicmiro/AnnounceBot";
        public string AppVersion = "0.0.1";

        //Start the bot
        public void Start()
        {
            //Load configuration
            _config = BotConfiguration.Load();

            //Initialize Discord.NET Client
            _client = new DiscordClient((x =>
            {
                x.AppName = "AnnounceBot";
                x.AppUrl = "https://github.com/epicmiro/AnnounceBot";
                x.AppVersion = AppVersion;
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = false;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = OnLogMessage;
            }));

            //Initialize audio
            InitializeAudio();

            //Initialize services
            InitializeServices();

            //Initialize modules
            InitializeModules();

            //Set Console title
            Console.Title = $"{AppName} v{AppVersion} (Discord.Net v{DiscordConfig.LibVersion})";

            //Start the Discord client
            _client.ExecuteAndWait(async () =>
            {
                while (true)
                {
                    try
                    {
                        await _client.Connect(BotConfiguration.Discord.Token);
                        _client.SetGame("Black Desert Online");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _client.Log.Error($"Login Failed, is your bot token correct?", ex);
                        await Task.Delay(_client.Config.FailedReconnectDelay);
                    }
                }
            });
        }

        //Initialize audio usage for Discord.NET
        public void InitializeAudio()
        {
            _client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
                x.EnableMultiserver = true;
                x.EnableEncryption = true;
                x.Bitrate = AudioServiceConfig.MaxBitrate;
                x.BufferLength = 10000;
            });
        }

        //Add services to the Discord.NET client
        public void InitializeServices()
        {
        }

        //Enable and add modules to the Discord.NET client
        public void InitializeModules()
        {
            _client.UsingModules();
            _client.AddModule<PatchNotifyModule>();
            _client.AddModule<TextToSpeechModule>();
        }

        // Logging function for Discord.NET
        // Copied from DiscordBot @ https://github.com/RogueException/DiscordBot/blob/master/src/DiscordBot/Program.cs
        private void OnLogMessage(object sender, LogMessageEventArgs e)
        {
            //Color
            ConsoleColor color;
            switch (e.Severity)
            {
                case LogSeverity.Error: color = ConsoleColor.Red; break;
                case LogSeverity.Warning: color = ConsoleColor.Yellow; break;
                case LogSeverity.Info: color = ConsoleColor.White; break;
                case LogSeverity.Verbose: color = ConsoleColor.Gray; break;
                case LogSeverity.Debug: default: color = ConsoleColor.DarkGray; break;
            }

            //Exception
            string exMessage;
            Exception ex = e.Exception;
            if (ex != null)
            {
                while (ex is AggregateException && ex.InnerException != null)
                    ex = ex.InnerException;
                exMessage = ex.Message;
            }
            else
                exMessage = null;

            //Source
            string sourceName = e.Source?.ToString();

            //Text
            string text;
            if (e.Message == null)
            {
                text = exMessage ?? "";
                exMessage = null;
            }
            else
                text = e.Message;

            //Build message
            StringBuilder builder = new StringBuilder(text.Length + (sourceName?.Length ?? 0) + (exMessage?.Length ?? 0) + 5);
            if (sourceName != null)
            {
                builder.Append('[');
                builder.Append(sourceName);
                builder.Append("] ");
            }
            for (int i = 0; i < text.Length; i++)
            {
                //Strip control chars
                char c = text[i];
                if (!char.IsControl(c))
                    builder.Append(c);
            }
            if (exMessage != null)
            {
                builder.Append(": ");
                builder.Append(exMessage);
            }

            text = builder.ToString();
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }


    }
}
