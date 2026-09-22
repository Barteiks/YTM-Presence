
using H.NotifyIcon;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using WinRT.Interop;
using YTM_Presence.Handler;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace YTM_Presence
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private TaskerManager _taskerManager;
        private TrackInfo? _lastTrackInfo = null;
        public MainWindow(TaskerManager taskerManager)
        {
            InitializeComponent();
            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            _appWindow = AppWindow.GetFromWindowId(windowId);
            _appWindow.Closing += AppWindow_Closing;
           
            _taskerManager = taskerManager;
            AppWindow.SetIcon("Assets\\Icon.ico");
            AppWindow.TitleBar.PreferredTheme = TitleBarTheme.UseDefaultAppMode;
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            if (AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
            }
            AppWindow.TitleBar.ButtonBackgroundColor = ColorHelper.FromArgb(0, 0, 0, 0);
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(0, 0, 0, 0);

            CompactOverlayPresenter presenter = CompactOverlayPresenter.Create();
            AppWindow.SetPresenter(presenter);

            AppWindow.Resize(new Windows.Graphics.SizeInt32(512, 256));
            SetUpUpdates();
            var visual = ElementCompositionPreview.GetElementVisual(coverImage);
            var compositor = visual.Compositor;

            var shadow = compositor.CreateDropShadow();
            shadow.Color = Colors.DeepSkyBlue;   // kolor poświaty
            shadow.BlurRadius = 40;
            shadow.Opacity = 1f;
            shadow.Offset = new System.Numerics.Vector3(0, 0, 0);

            var sprite = compositor.CreateSpriteVisual();
            sprite.Size = new System.Numerics.Vector2(
                (float)coverImage.ActualWidth,
                (float)coverImage.ActualHeight);

            sprite.Shadow = shadow;

            ElementCompositionPreview.SetElementChildVisual(coverImage, sprite);

        }
        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            args.Cancel = true;
            // Schowaj okno
            this.Hide();
        }
        private void SetUpUpdates()
        {
            _taskerManager.OnDataReceived += (trackData) =>
            {
                bool trackChanged = _lastTrackInfo == null || _lastTrackInfo.title != trackData.title;
                _lastTrackInfo = trackData;
                if (trackChanged)
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        titleText.Text = trackData.title;
                        artistText.Text = trackData.artist;
                        if (trackData.coverUrl != null)
                        {
                            coverImage.Source = new BitmapImage(new Uri(trackData.coverUrl));
                        }
                    });
                }
                this.DispatcherQueue.TryEnqueue(() =>
                {
                    trackSlider.Maximum = trackData.totalSec;
                    trackSlider.Value = trackData.currentSec;

                    currentTimeText.Text = TimeSpan.FromSeconds(trackData.currentSec).ToString(@"m\:ss");
                    totalTimeText.Text = TimeSpan.FromSeconds(trackData.totalSec).ToString(@"m\:ss");
                });
                
            };
        }
    }
}
