using System;
using System.IO;
using System.Threading.Tasks;
using MetaDefender.Cloud.NET;

namespace MetaDefender.Cloud.Example
{
    class Program
    {
        static async Task Main()
        {
            var client = new CloudClient("");
            using (var filestream = File.OpenRead("Image.jpg"))
            {
                filestream.Seek(0, SeekOrigin.Begin);
                var scanResult = await client.ScanFileAndWaitAsync("Image.jpg", filestream);
                Console.Write("Time " + scanResult.ScanResults.TotalTime);
            }
            await Task.Delay(-1);
        }
        
    }
}
