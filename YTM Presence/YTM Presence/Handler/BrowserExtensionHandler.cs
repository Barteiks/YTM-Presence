using Fleck;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace YTM_Presence.Handler
{
    public class BrowserExtensionHandler
    {
        // Teraz mamy tylko jeden event
        public event Action<TrackInfo>? OnDataReceived;
        private WebSocketServer? _server;

        public void StartLocalServer()
        {
            try
            {
                _server = new WebSocketServer("ws://127.0.0.1:27654");
                _server.Start(socket =>
                {
                    socket.OnOpen = () => Debug.WriteLine("Connected!");
                    socket.OnClose = () => Debug.WriteLine("Disconnected!");
                    socket.OnMessage = message =>
                    {
                        try
                        {
                            var trackData = JsonSerializer.Deserialize<TrackInfo>(message);
                            if (trackData != null)
                            {
                                if (!string.IsNullOrEmpty(trackData.coverUrl))
                                {
                                    trackData.coverUrl = trackData.coverUrl.Replace("w60-h60", "w512-h512");
                                }

                                if (trackData.artist != null && trackData.artist.Contains("•"))
                                {
                                    trackData.artist = trackData.artist.Split('•')[0].Trim();
                                }

                                // Przekazujemy paczkę dalej
                                OnDataReceived?.Invoke(trackData);
                                Debug.WriteLine($"Received: {trackData.totalSec} by {trackData.currentSec}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Exception at parsing: {ex.Message}");
                        }
                    };
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }
    }


    public class TrackInfo
        {
            public string? title { get; set; }
            public string? artist { get; set; }
            public int currentSec { get; set; }
            public int totalSec { get; set; }
            public string? coverUrl { get; set; }

            public string? trackUrl { get; set; }
            public bool isPaused { get; set; }
        }
}

