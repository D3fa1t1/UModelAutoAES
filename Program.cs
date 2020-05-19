using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoAES
{
    class Program
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);
        private static readonly HttpClient client = new HttpClient();

        static void Main(string[] args) =>
            Main().GetAwaiter().GetResult();

        private static async Task Main()
        {
            if(!File.Exists("Newtonsoft.Json.dll"))
            {
                MessageBox((IntPtr)0, "Newtonsoft.Json.dll not found", "Error", 0);
                return;
            }
            if(!File.Exists("umodel.exe"))
            {
                MessageBox((IntPtr)0, "Couldn't find UModel, make sure it's in the same folder and that it's called umodel.exe", "Error", 0);
                return;
            }
            var response = await GetAESAsync();
            if(response.MainKey == null && response.MainKey.Equals(""))
            {
                MessageBox((IntPtr)0, "Couldn't get AES key: Probably not found yet", "Error", 0);
                return;
            }
            var process = new Process();
            process.StartInfo.FileName = "umodel.exe";
            process.StartInfo.Arguments = $"-aes={response.MainKey} -sounds -3rdparty -game=ue4.24";
            if(!process.Start())
            {
                MessageBox((IntPtr)0, "Couldn't start UModel", "Error", 0);
                return;
            }
        }

        private static async Task<BenBotAES> GetAESAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://benbotfn.tk/api/v1/aes");
            var response = await client.SendAsync(request);
            if(!response.IsSuccessStatusCode)
            {
                MessageBox((IntPtr)0, "Couldn't get AES key", "Error", 0);
                Environment.Exit(-1);
            }
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<BenBotAES>(responseBody);
        }

        public class BenBotAES
        {
            [JsonProperty("mainKey")] public string MainKey;
            [JsonProperty("dynamicKeys")] public Dictionary<string, string> DynamicKeys;
        }

    }
}
