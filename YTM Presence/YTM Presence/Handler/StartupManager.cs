using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace YTM_Presence.Handler;

public static class StartupManager
{
    public static async Task EnableStartup()
    {
        var startupTask = await StartupTask.GetAsync("YTMPresenceStartup");

        if (startupTask.State == StartupTaskState.Disabled)
        {
            await startupTask.RequestEnableAsync();
            Debug.WriteLine("WAF");
        }
    }


    public static async Task DisableStartup()
    {
        var startupTask = await StartupTask.GetAsync("YTMPresenceStartup");

        if (startupTask.State == StartupTaskState.Enabled)
        {
            startupTask.Disable();
            Debug.WriteLine("WAF2");
        }
    }
    public static async Task<StartupTaskState> GetState()
    {
        var startupTask = await StartupTask.GetAsync("YTMPresenceStartup");
        return startupTask.State;
    }
}