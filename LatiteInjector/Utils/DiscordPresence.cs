#region

using System.Collections.Generic;
using System.IO;
using System.Timers;
using DiscordRPC;

#endregion

namespace LatiteInjector.Utils;

public static class DiscordPresence
{
    private static readonly DiscordRpcClient DiscordClient = new("1066896173799047199");

    private record PresenceDetails(
        string Name,
        string LogoKey,
        string LogoTooltip
    );

    private static readonly Dictionary<string, PresenceDetails> SupportedPresenceDict = new()
    {
        {
            "geo.hivebedrock.network",
            new PresenceDetails("The Hive", "thehive", "The Hive Logo")
        },
        {
            "mco.cubecraft.net",
            new PresenceDetails("Cubecraft Games", "cubecraft", "Cubecraft Games Logo")
        },
        {
            "play.galaxite.net",
            new PresenceDetails("Galaxite Network", "galaxite", "Galaxite Network Logo")
        },
        {
            "zeqa.net",
            new PresenceDetails("Zeqa Practice", "zeqa", "Zeqa Logo")
        },
        {
            "play.nethergames.org",
            new PresenceDetails("NetherGames Network", "nethergames", "NetherGames Network Logo")
        }
    };

    public static void InitializePresence() => DiscordClient.Initialize();
    public static Timestamps CurrentTimestamp = Timestamps.Now;

    public static void DefaultPresence()
    {
        DiscordClient.SetPresence(new RichPresence
        {
            State = "Idling in the injector",
            Timestamps = CurrentTimestamp,
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
        SettingsWindow.IsDiscordPresenceEnabled = true;
    }

    public static void PlayingPresence()
    {
        DiscordClient.UpdateLargeAsset("minecraft", "Minecraft Bedrock Logo");
        DiscordClient.UpdateSmallAsset("latite", "Latite Client Icon");
        if (!Injector.IsCustomDll)
        {
            DiscordClient.UpdateDetails(
                $"Playing Minecraft {Injector.MinecraftVersion}");
            DiscordClient.UpdateState("with Latite Client");
        }
        else if (Injector.IsCustomDll)
        {
            DiscordClient.UpdateDetails(
                $"Playing Minecraft {Injector.MinecraftVersion}");
            DiscordClient.UpdateState($"with {Injector.CustomDllName}");
        }
    }

    public static void DetailedPlayingPresence(object? sender, ElapsedEventArgs e)
    {
        string serverIP = File.ReadAllText($@"{Logging.LatiteFolder}\Logs\serverip.txt");

        if (!SettingsWindow.IsDiscordPresenceEnabled || !Injector.IsMinecraftRunning()) return;
        if (SupportedPresenceDict.TryGetValue(serverIP, out PresenceDetails presenceDetails))
        {
            DiscordClient.UpdateDetails($"Playing on {presenceDetails.Name}");
            if (!Injector.IsCustomDll)
                DiscordClient.UpdateState("with Latite Client");
            else if (Injector.IsCustomDll)
                DiscordClient.UpdateState($"with {Injector.CustomDllName}");
            DiscordClient.UpdateLargeAsset(presenceDetails.LogoKey, presenceDetails.LogoTooltip);
            DiscordClient.UpdateSmallAsset("latite", "Latite Client Icon");
        }
        else if (serverIP == "none")
            PlayingPresence();
    }

    public static void IdlePresence()
    {
        DiscordClient.SetPresence(new RichPresence
        {
            State = "Idling in the injector",
            Timestamps = CurrentTimestamp,
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

    public static void SettingsPresence() => DiscordClient.UpdateState("Changing settings");
    public static void ChangelogPresence() => DiscordClient.UpdateState("Reading the changelog");
    public static void CreditsPresence() => DiscordClient.UpdateState("Reading the credits");

    public static void StopPresence()
    {
        DiscordClient.ClearPresence();
        SettingsWindow.IsDiscordPresenceEnabled = false;
    }

    public static void ShutdownPresence()
    {
        if (DiscordClient.IsDisposed) return;
        DiscordClient.ClearPresence();
        DiscordClient.Deinitialize();
        DiscordClient.Dispose();
        SettingsWindow.IsDiscordPresenceEnabled = false;
    }
}
