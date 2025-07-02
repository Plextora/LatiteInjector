using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LatiteInjector.Utils
{
    public static class FileHelper
    {
        private static readonly HttpClient Client = new();

        public static async Task DownloadFile(Uri uri, string fileName)
        {
            await using Stream asyncStream = await Client.GetStreamAsync(uri);
            await using FileStream fs = new(fileName, FileMode.CreateNew);
            await asyncStream.CopyToAsync(fs);
        }

        public static async Task<string> DownloadString(Uri uri) => await Client.GetStringAsync(uri);

        public static async Task<bool> DeleteFile(string filePath, int maxAttempts = 5, int waitMs = 100)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    if (!File.Exists(filePath)) return true;
                    File.Delete(filePath);
                    return true;
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    if (attempt < maxAttempts) await Task.Delay(waitMs);
                }
                catch (UnauthorizedAccessException)
                {
                    ClearReadOnlyFlag(filePath);
                }
            }

            return false;
        }

        private static bool IsFileLocked(IOException ex)
        {
            int errorCode = ex.HResult & 0xFFFF;
            return errorCode == 32 || errorCode == 33; // Sharing/Lock violation
        }

        private static void ClearReadOnlyFlag(string filePath)
        {
            try
            {
                if (File.Exists(filePath) &&
                    (File.GetAttributes(filePath) & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }
            }
            catch
            {
                /* Ignore cleanup failures */
            }
        }
    }
}
