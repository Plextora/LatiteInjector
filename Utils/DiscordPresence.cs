using DiscordRPC;

namespace LatiteInjector.Utils;

public static class DiscordPresence
{
    public static readonly DiscordRpcClient DiscordClient = new("1066896173799047199");
    
    public static void IdlePresence()
    {
        DiscordClient.SetPresence(new RichPresence
        {
            State = "Idling in the injector",
            Timestamps = Timestamps.Now,
            Buttons = new[]
            {
                new Button { Label = "Download Latite Client", Url = "https://discord.gg/zcJfXxKTA4" }
            },
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