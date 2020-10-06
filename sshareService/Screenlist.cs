using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sshareService
{
    class Screenlist
    {
        readonly Screenshot[] scs;

        public Screenlist()
        {
            scs = new Screenshot[Screen.AllScreens.Count()];

            for(int i = 0; i < Screen.AllScreens.Count(); i++)
            {
                scs[i] = new Screenshot(Screen.AllScreens[i]);
            }
        }

        public void Capture()
        {
            try
            {
                foreach(Screenshot s in scs)
                {
                    s.Capture();
                }
            }
            catch
            {

            }
        }
    }
}
