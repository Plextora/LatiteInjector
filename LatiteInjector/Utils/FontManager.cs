﻿using System;
using System.Drawing;
using System.IO;

namespace LatiteInjector.Utils;

public static class FontManager
{
    public static bool IsFontInstalled(string fontName)
    {
        using var testFont = new Font(fontName, 8);
        return 0 == string.Compare(fontName, testFont.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public static void InstallFont(string fontSourcePath)
    {
        var shellAppType = Type.GetTypeFromProgID("Shell.Application");
        var shell = Activator.CreateInstance(shellAppType);
        var fontFolder = (Shell32.Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { Environment.GetFolderPath(Environment.SpecialFolder.Fonts) });

        if (File.Exists(fontSourcePath))
        {
            fontFolder.CopyHere(fontSourcePath);
        }
    }
}