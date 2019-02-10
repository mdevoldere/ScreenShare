using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public class WebPage
    {
        public string LocalPath { get; protected set; }

        public string FullPath { get; protected set; }

        public bool HasData { get; protected set; }

        private object locker = new object();

        public HttpListenerContext Ctx { get; protected set; }

        public WebPage(HttpListenerContext _ctx)
        {
            Ctx = _ctx;
            LocalPath = (Ctx.Request.Url.LocalPath == "/") ? "/index.html" : Ctx.Request.Url.LocalPath;
            FullPath = (ServerInfo.Path + LocalPath);
        }

        public bool Load()
        {
            try
            {
                lock (locker)
                    HasData = File.Exists(FullPath);

                if (!HasData)
                {
                    SetError(404);
                    return false;
                }

                Ctx.Response.StatusCode = 200;

                FileInfo fileinfo = new FileInfo(FullPath);

                switch (fileinfo.Extension)
                {
                    case ".jpg":
                        Ctx.Response.ContentType = "image/jpeg";
                        break;
                    case ".png":
                        Ctx.Response.ContentType = "image/png";
                        break;
                    case ".svg":
                        Ctx.Response.ContentType = "image/svg+xml";
                        break;
                    case ".css":
                        Ctx.Response.ContentType = "text/css";
                        break;
                    default:
                        Ctx.Response.ContentType = "text/html";
                        break;
                }

                return true;
            }
            catch
            {
                SetError(500);
                return false;
            }
        }

        public void SetError(int _httpCode)
        {
            try
            {
                Ctx.Response.StatusCode = _httpCode;
                Ctx.Response.ContentType = "text/html";
                FullPath = (ServerInfo.Path + "/error.html");
                HasData = true;
            }
            catch
            {

            }
        }
    }
}
