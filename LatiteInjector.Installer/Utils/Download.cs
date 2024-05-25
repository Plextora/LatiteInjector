using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LatiteInjector.Installer.Utils
{
    public class Download
    {
        public static async Task DownloadFile(Uri uri, string fileName)
        {
            using HttpClient client = new();
            using Stream asyncStream = await client.GetStreamAsync(uri);
            using FileStream fs = new(fileName, FileMode.CreateNew);
            await asyncStream.CopyToAsync(fs);
        }
    }
}
