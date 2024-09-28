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

    private static readonly Dictionary<List<string>, PresenceDetails> SupportedPresenceDict = new()
    {
        {
            new List<string>
            {
                "geo.hivebedrock.network",
                "ca.hivebedrock.network",
                "fr.hivebedrock.network",
                "sg.hivebedrock.network",
                "au.hivebedrock.network"
            },
            new PresenceDetails("The Hive", "thehive", "The Hive Logo")
        },
        {
            // apparently cubecraft's region switching doesn't change
            // server ip (at least with how latite detects server ip)
            new List<string> { "mco.cubecraft.net" },
            new PresenceDetails("Cubecraft Games", "cubecraft", "Cubecraft Games Logo")
        },
        {
            new List<string> { "us.play.galaxite.net", "eu.play.galaxite.net", "play.galaxite.net" },
            new PresenceDetails("Galaxite Network", "galaxite", "Galaxite Network Logo")
        },
        {
            new List<string>
            {
                "zeqa.net",
                "na.zeqa.net",
                "au.zeqa.net",
                "me.zeqa.net",
                "za.zeqa.net",
                "as.zeqa.net"
            },
            new PresenceDetails("Zeqa Practice", "zeqa", "Zeqa Logo")
        },
        {
            new List<string>
            {
                "play.nethergames.org",
                "ind.nethergames.org",
                "us.nethergames.org",
                "ap.nethergames.org",
                "eu.nethergames.org"
            },
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
        if (!SettingsWindow.IsDiscordPresenceEnabled || !Injector.IsMinecraftRunning()) return;

        string serverIP = "none";
        if (File.Exists($@"{Logging.LatiteFolder}\serverip.txt"))
            serverIP = File.ReadAllText($@"{Logging.LatiteFolder}\serverip.txt");
        foreach (KeyValuePair<List<string>, PresenceDetails> server in SupportedPresenceDict)
        {
            // if server ip not in list, skip this foreach execution
            if (!server.Key.Contains(serverIP)) continue;

            DiscordClient.UpdateDetails($"Playing on {server.Value.Name}");
            if (!Injector.IsCustomDll)
                DiscordClient.UpdateState("with Latite Client");
            else if (Injector.IsCustomDll)
                DiscordClient.UpdateState($"with {Injector.CustomDllName}");
            DiscordClient.UpdateLargeAsset(server.Value.LogoKey, server.Value.LogoTooltip);
            DiscordClient.UpdateSmallAsset("latite", "Latite Client Icon");
        }
        if (serverIP == "none")
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
    public static void CreditsPresence() => DiscordClient.UpdateState("Reading the credits");
    public static void LanguagesPresence() => DiscordClient.UpdateState("Changing language");

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
