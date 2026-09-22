using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Diagnostics;
namespace YTM_Presence.Handler
{
    public class DiscordRPHandler : IDisposable
    {
        private DiscordRpcClient? _client;
        private TaskerManager _taskerManager;

        // Zmienne do śledzenia stanu, żeby nie spamować serwerów Discorda (Rate Limit)
        private TrackInfo? _lastTrackInfo = null;
        private DateTime _lastExpectedEnd;

        public DiscordRPHandler(TaskerManager taskerManager)
        {
            _taskerManager = taskerManager;
        }

        public void Setup()
        {
            try
            {
                _client = new DiscordRpcClient("SMTH")
                {
                    Logger = new ConsoleLogger() { Level = LogLevel.Info }
                };

                _client.OnReady += (sender, msg) =>
                {
                    Debug.WriteLine("Connected to discord with user {0}", msg.User.Username);
                };

                _client.Initialize();
                SetUpUpdates();
            } catch (TimeoutException) {
            }
        }

        private void SetUpUpdates()
        {
            _taskerManager.OnDataReceived += (trackData) =>
            {
                try
                {
                    // 1. ZARZĄDZANIE PAUZĄ
                    if (trackData.isPaused)
                    {
                        // Jeśli wcześniej nie było pauzy, to czyścimy status
                        if (_lastTrackInfo == null || !_lastTrackInfo.isPaused)
                        {
                            _client?.ClearPresence();
                            _lastTrackInfo = trackData;
                        }
                        return; // Nic więcej nie robimy póki jest zapauzowane
                    }

                    // 2. OBLICZAMY NOWY CZAS ZAKOŃCZENIA
                    DateTime expectedEnd = DateTime.UtcNow.AddSeconds(trackData.totalSec - trackData.currentSec);

                    // Sprawdzamy co się zmieniło
                    bool trackChanged = _lastTrackInfo == null || _lastTrackInfo.title != trackData.title;
                    bool wasPaused = _lastTrackInfo != null && _lastTrackInfo.isPaused;

                    // 3. WYKRYWANIE PRZEWIJANIA (Jeśli obliczony czas zakończenia różni się np. o ponad 3 sekundy)
                    bool wasSeeked = false;
                    if (!trackChanged && !wasPaused)
                    {
                        double timeDrift = Math.Abs((expectedEnd - _lastExpectedEnd).TotalSeconds);
                        if (timeDrift > 3)
                        {
                            wasSeeked = true;
                        }
                    }

                    // 4. AKTUALIZACJA DISCORDA TYLKO GDY TRZEBA
                    if (trackChanged || wasPaused || wasSeeked)
                    {
                        _lastTrackInfo = trackData;
                        _lastExpectedEnd = expectedEnd;
                        if (_client == null) return;
                        _client.SetPresence(new RichPresence()
                        {
                            Type = ActivityType.Listening,
                            StatusDisplay = StatusDisplayType.Details,
                            Details = trackData.title,
                            State = trackData.artist,
                            Assets = new Assets()
                            {
                                LargeImageKey = trackData.coverUrl,
                                LargeImageUrl = trackData.trackUrl,
                                SmallImageKey = "https://www.google.com/s2/favicons?domain=music.youtube.com&sz=128",
                            },
                            Timestamps = new Timestamps()
                            {
                                Start = DateTime.UtcNow.AddSeconds(-trackData.currentSec),
                                End = expectedEnd
                            }
                        });
                    }
                }
                catch (TimeoutException)
                {
                } 
            };
        }
        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
