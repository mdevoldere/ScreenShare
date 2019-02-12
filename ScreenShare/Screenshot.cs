using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public class Screenshot
    {
        public string Name { get; set; }

        public bool HasPreview { get; set; }

        public Bitmap Preview { get; protected set; }

        public MemoryStream IPreview { get; protected set; }

        private ReaderWriterLock rwl = new ReaderWriterLock();

        public Screenshot() : this("ScreenShare") {}

        public Screenshot(string _name)
        {
            Name = _name;
        }

        protected void SetPreview(Bitmap bmp)
        {
            if (HasPreview)
            {
                IPreview = new MemoryStream();
                bmp.Save(IPreview, ImageFormat.Jpeg);
                Preview = new Bitmap(IPreview);
            }
        }

        public void CaptureScreen(bool captureMouse)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                    if(captureMouse)
                    {
                        g.DrawIcon(Properties.Resources.cursor, Cursor.Position.X-5, Cursor.Position.Y);
                    }
                }

                //rwl.AcquireWriterLock(Timeout.Infinite);
                //bitmap.Save(ServerInfo.Path + "/" + Name + ".jpg", ImageFormat.Jpeg);
                //rwl.ReleaseWriterLock();

                SetPreview(bitmap);
            }

        }

        public void SetDefault()
        {
            if(File.Exists(ServerInfo.Path + "/" + Name + ".jpg"))
            {
                rwl.AcquireWriterLock(Timeout.Infinite);
                File.Delete(ServerInfo.Path + "/" + Name + ".jpg");
                rwl.ReleaseWriterLock();
            }

            rwl.AcquireWriterLock(Timeout.Infinite);
            File.Copy(ServerInfo.Path + "/nosession.jpg", ServerInfo.Path + "/" + Name + ".jpg");
            rwl.ReleaseWriterLock();
        }

    }
}
