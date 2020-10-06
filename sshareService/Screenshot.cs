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

namespace sshareService
{
    public class Screenshot
    {
        readonly Screen sc;

        readonly Rectangle rc;

        readonly Point p1;

        public Bitmap Preview { get; protected set; }

        public MemoryStream IPreview { get; protected set; }


        public Screenshot(Screen _screen) 
        {
            sc = _screen;

            rc = new Rectangle(sc.WorkingArea.X, sc.WorkingArea.Y, sc.WorkingArea.Width, sc.WorkingArea.Height);

            p1 = new Point(sc.WorkingArea.Left, sc.WorkingArea.Top);
        }

        public void Capture()
        {
            try
            {
                using (Bitmap bmp = new Bitmap(rc.Width, rc.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(p1, Point.Empty, rc.Size);
                    }

                    IPreview = new MemoryStream();
                    bmp.Save(IPreview, ImageFormat.Jpeg);
                    Preview = new Bitmap(IPreview);
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(e.Message);
            }
        }
    }
}
