using DiscordRPC;
using static LatiteInjector.MainWindow;

namespace LatiteInjector.Utils;

public static class DiscordPresence
{
    private static readonly DiscordRpcClient DiscordClient = new("1066896173799047199");

    public static void InitalizePresence()
    {
        DiscordClient.Initialize();
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

    public static void PlayingPresence() => DiscordClient.UpdateState(
        IsCustomDll
            ? $"Playing Minecraft {Updater.GetSelectedVersion()} with {CustomDllName}"
            : $"Playing Minecraft {Updater.GetSelectedVersion()} with Latite");
    public static void IdlePresence() => DiscordClient.UpdateState("Idling in the injector");
    public static void ChangelogPresence() => DiscordClient.UpdateState("Reading the changelog");
    public static void CreditsPresence() => DiscordClient.UpdateState("Reading the credits");
    public static void MinimizeToTrayPresence()
    {
        if (!IsMinecraftRunning)
        {
            DiscordClient.UpdateState("Minimized to tray");
            return;
        }
        PlayingPresence();
    }

    public static void StopPresence()
    {
        if (DiscordClient.IsDisposed) return;
        DiscordClient.ClearPresence();
        DiscordClient.Deinitialize();
        DiscordClient.Dispose();
    }
}