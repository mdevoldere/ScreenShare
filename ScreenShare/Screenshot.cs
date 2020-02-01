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

        Rectangle rect = new Rectangle(
                Screen.PrimaryScreen.WorkingArea.X,
                Screen.PrimaryScreen.WorkingArea.Y,
                Screen.PrimaryScreen.WorkingArea.Width,
                Screen.PrimaryScreen.WorkingArea.Height
            );

        Point p1 = new Point(Screen.PrimaryScreen.WorkingArea.Left, Screen.PrimaryScreen.WorkingArea.Top);

        readonly int decal = 0;

        public Screenshot() : this("ScreenShare") {}

        public Screenshot(string _name)
        {
            Name = _name;

            if (rect.Y > 0)
                decal = (Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height);

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
            //Rectangle bounds = Screen.GetBounds(Point.Empty);

            try
            {
                using (Bitmap bitmap = new Bitmap(rect.Width, rect.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(p1, Point.Empty, rect.Size);

                        if (captureMouse)
                        {
                            g.DrawIcon(Properties.Resources.cursor, Cursor.Position.X - 5, Cursor.Position.Y - decal);
                        }
                    }

                    //rwl.AcquireWriterLock(Timeout.Infinite);
                    //bitmap.Save(ServerInfo.Path + "/" + Name + ".jpg", ImageFormat.Jpeg);
                    //rwl.ReleaseWriterLock();

                    SetPreview(bitmap);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e.Message);
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
