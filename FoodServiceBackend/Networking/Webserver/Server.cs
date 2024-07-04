using WebSocketSharp.Server;
using FoodServiceBackend.Networking.Webserver;
using FoodServiceBackend;

namespace FoodServiceBackend.Networking
{
    /// <summary>
    /// Webserver to host UI API
    /// </summary>
    public class Server
    {
        private readonly HttpServer? webserver = null;

        public Server()
        {
            if (Program.settings == null)
            {
                return;
            }

            webserver = new HttpServer(System.Net.IPAddress.Parse(Program.settings.NetworkInterface), Program.settings.Port, false);

            webserver.OnConnect += Webserver_OnConnect;
            webserver.OnGet += Webserver_OnGet;
            webserver.OnPost += Webserver_OnPost;
            webserver.KeepClean = true;
            webserver.WaitTime = TimeSpan.FromSeconds(4);

            webserver.Start();
            Helper.WriteLineCyan("Webserver         started. --> " + " Networkinterface: " + Program.settings.NetworkInterface.PadRight(20) + " Port: " + Program.settings.Port.ToString());
            Helper.WriteLineCyan("Webserver URL: http://" + Program.settings.NetworkInterface + ":" + Program.settings.Port.ToString());

        }

        ~Server()
        {
            if (webserver != null)
            {
                webserver.SslConfiguration.ServerCertificate?.Reset();
            }
        }

        private void Webserver_OnConnect(object? sender, HttpRequestEventArgs e)
        {
            Helper.WriteIntoConsoleAndLog(e.ToString());
        }

        private void Webserver_OnGet(object? sender, HttpRequestEventArgs e)
        {
            Get.GET(sender, e);
        }

        private void Webserver_OnPost(object? sender, HttpRequestEventArgs e)
        {
            Post.POST(sender, e);
        }

        /// <summary>
        /// Get byte[] from IO.Stream
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
