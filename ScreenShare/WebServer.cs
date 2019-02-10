using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenShare
{
    public class WebServer
    {
        public string Url { get; protected set; }

        protected HttpListener server;

        private ReaderWriterLock locker = new ReaderWriterLock();

        public WebServer()
        {
            server = new HttpListener { IgnoreWriteExceptions = true };
        }

        public void Stop()
        {
            ServerInfo.IsWorking = false;
        }

        private async Task Request(HttpListenerContext ctx)
        {
            if (await Auth(ctx))
            {
                WebPage page = new WebPage(ctx);
                page.Load();
                await Response(ctx, page);
            }
        }

        public async Task Start()
        {
            Url = string.Format("http://{0}:{1}/", ServerInfo.Ip, ServerInfo.Port);
            server.Prefixes.Clear();
            server.Prefixes.Add("http://localhost:" + ServerInfo.Port + "/");
            server.Prefixes.Add("http://" + Environment.MachineName + ":" + ServerInfo.Port + "/");
            //serv.Prefixes.Add("http://*:" + _port + "/"); // Uncomment Allow Public IP.
            server.Prefixes.Add(Url);

            server.Start();

            ServerInfo.IsWorking = true;
            
            while (ServerInfo.IsWorking)
            {
                var ctx = await server.GetContextAsync();

                var t = Task.Run(() => Auth(ctx));

                Thread.Sleep(400);
            }
            
            Thread.Sleep(500);

            server.Stop();
        }

        protected async Task<bool> Auth(HttpListenerContext ctx)
        {
            WebPage page = new WebPage(ctx);

            if (ServerInfo.IsPrivate())
            {
                if (!ctx.Request.Headers.AllKeys.Contains("Authorization"))
                {
                    ctx.Response.StatusCode = 401;
                    ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=Screen Share Authentication : ");
                    ctx.Response.Close();
                    return false;
                }
                else
                {
                    var auth1 = ctx.Request.Headers["Authorization"];
                    auth1 = auth1.Remove(0, 6); // Remove "Basic " From Header
                    auth1 = Encoding.UTF8.GetString(Convert.FromBase64String(auth1));
                    string auth2 = string.Format("{0}:{1}", ServerInfo.User, ServerInfo.Password);

                    if (auth1 != auth2)
                    {
                        ctx.Response.StatusCode = 401;
                        ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=Screen Share Authentication : ");
                        ctx.Response.Close();
                        return false;
                    }
                }
            }
              
            await Response(ctx, page);
            return true;
        }

        public async Task Response(HttpListenerContext ctx)
        {
            await Response(ctx, new WebPage(ctx));
        }

        public async Task Response(HttpListenerContext ctx, WebPage page)
        {
            try
            {
                page.Load();

                if (page.HasData)
                {
                    locker.AcquireReaderLock(Timeout.Infinite);
                    byte[] data = File.ReadAllBytes(page.FullPath);
                    locker.ReleaseReaderLock();

                    await ctx.Response.OutputStream.WriteAsync(data, 0, data.Length);
                }
            }
            catch
            {
            }

            ctx.Response.Close();
        }
    }
}
