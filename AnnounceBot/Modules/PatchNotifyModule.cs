using AnnounceBot.Configuration;
using Discord;
using Discord.API.Status;
using Discord.API.Status.Rest;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Modules;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceBot.Modules
{
    class PatchNotifyModule : IModule
    {
        private DiscordClient _client;
        private bool _isRunning;
        private bool _firstTime;
        private string _latestVersion;

        public void Install(ModuleManager manager)
        {
            _client = manager.Client;

            // Load cache
            LoadCache();

            // We'll need additonal checks the first time the patch version is checked
            _firstTime = true;

            // Start our update loop
            _client.LoggedIn += (s, e) =>
            {
                if (!_isRunning)
                {
                    Task.Run(Run);
                    _isRunning = true;
                }
            };
        }

        // Load the cache file
        public void LoadCache()
        {
            string cacheFile = BotConfiguration.BDO.CacheFile;

            if(File.Exists(cacheFile))
            {
                _latestVersion = File.ReadAllText(cacheFile);
            }
            else
            {
                // Create the new file
                File.Create(cacheFile);

                // Notify the developer that the cache file failed to load.
                _client.Log.Log(LogSeverity.Info, "PatchNotify", "Version cache file has not been found, and therefore has been recreated.");
            }
        }

        // Save the cache file
        public void SaveCache()
        {
            try
            {
                string cacheFile = BotConfiguration.BDO.CacheFile;
                File.WriteAllText(cacheFile, _latestVersion);
            }
            catch(Exception e)
            {
                _client.Log.Log(LogSeverity.Warning, "PatchNotify", "Could not write cache file.", e);
            }
        }

        public async Task Run()
        {
            // Delay our bot for 5 seconds, since the current Discord.NET version doesn't have a .Ready event.
            await Task.Delay(5000);

            while(!_client.CancelToken.IsCancellationRequested)
            {
                // Call our update function
                await Update();

                // Make sure we only check every (n) seconds.
                await Task.Delay(BotConfiguration.BDO.UpdateTime * 1000);
            }

            _isRunning = false;
        }

        public async Task Update()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BotConfiguration.BDO.PatchUrl);
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                // Get the stream associated with the response.
                Stream receiveStream = response.GetResponseStream();

                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                string content = readStream.ReadToEnd();
                
                // If this version does not equal to the previously stored version, notify our users.
                if(_latestVersion != content)
                {

                    _latestVersion = content;

                    //Save cache change
                    SaveCache();

                    if (_firstTime && !BotConfiguration.BDO.Notify)
                    {
                        _firstTime = false;
                        return;
                    }

                    _client.Log.Log(LogSeverity.Info, "PatchNotify", "Found new patch version (" + _latestVersion + "), writing to cache and notifying servers.");

                    foreach (Server server in _client.Servers)
                    {
                        foreach(string channelName in BotConfiguration.BDO.ChannelNames)
                        {
                            IEnumerable<Channel> result = server.FindChannels(channelName, ChannelType.Text);
                            Channel channel = result.FirstOrDefault();

                            if(channel != null)
                            {
                                await channel.SendMessage("A new patch version (" + _latestVersion + ") has just been put online!");
                            }
                           
                        }
                    }

                }

                response.Close();
                readStream.Close();
            }
            catch(Exception e)
            {
                _client.Log.Log(LogSeverity.Error, "PatchNotify", "Could not fetch patch version" + e.ToString(), e);
            }
        }
    }
}
