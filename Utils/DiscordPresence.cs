using DiscordRPC;

namespace LatiteInjector.Utils;

public static class DiscordPresence
{
    public static readonly DiscordRpcClient DiscordClient = new("1066896173799047199");
    
    public static void IdlePresence()
    {
        DiscordClient.SetPresence(new RichPresence
        {
            Details = "Launcher made by Plextora#0033",
            State = "Idling in the client",
            Timestamps = Timestamps.Now,
            Assets = new Assets
            {
                LargeImageKey = "latite",
                LargeImageText = "Latite Client Icon"
            }
        });
    }
    
    public static void StopPresence()
    {
        if (DiscordClient.IsDisposed) return;
        DiscordClient.ClearPresence();
        DiscordClient.Deinitialize();
        DiscordClient.Dispose();
    }
}