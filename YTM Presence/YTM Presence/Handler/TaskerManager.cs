using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTM_Presence.Handler
{
    public class TaskerManager : IDisposable
    {
        public event Action<TrackInfo>? OnDataReceived;

        private BrowserExtensionHandler _browserExtensionHandler;
        private DiscordRPHandler _discordRPHandler;

        public TaskerManager()
        {
            _browserExtensionHandler = new BrowserExtensionHandler();
            _browserExtensionHandler.StartLocalServer();

            _browserExtensionHandler.OnDataReceived += (trackInfo) =>
            {
                OnDataReceived?.Invoke(trackInfo);
            };

            _discordRPHandler = new DiscordRPHandler(this);
            _discordRPHandler.Setup();
        }
        public void Dispose()
        {
            _discordRPHandler.Dispose();
        }
    }
}
