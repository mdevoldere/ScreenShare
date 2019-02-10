using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public partial class FrmMain : Form
    {        
        WebServer webServer;

        Screenshot screenshot;
        
        public FrmMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false; // For Visual Studio Debug !
            webServer = new WebServer();
            screenshot = new Screenshot();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            foreach (var ip in ServerInfo.GetIPv4Addresses())
            {
                comboIPs.Items.Add(ip.Item2 + " - " + ip.Item1);
            }

            comboIPs.SelectedIndex = (comboIPs.Items.Count > 0) ? 0 : -1;
        }

        private void Log(string text)
        {
            txtLog.Text += DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " : " + text + "\r\n";
            txtLog.SelectionStart = txtLog.Text.Length;
        }

        private void Log_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
            txtURL.Focus();
        }

        #region Web Server

        private void ServerInfo_Changed(object sender, EventArgs e)
        {
            ServerInfo.Ip = ServerInfo.IpList.ElementAt(comboIPs.SelectedIndex).Item2;
            ServerInfo.Port = numPort.Value.ToString();
            ServerInfo.User = null;
            ServerInfo.Password = null;

            gbxPrivate.Enabled = cbPrivate.Checked;
            
            if (cbPrivate.Checked 
                && gbxPrivate.Enabled
                && (txtUser.Text.Length > 2)
                && (txtPassword.Text.Length > 3))
            {
                ServerInfo.User = (!String.IsNullOrEmpty(txtUser.Text) ? txtUser.Text.Trim() : null);
                ServerInfo.Password = (!String.IsNullOrEmpty(txtPassword.Text) ? txtPassword.Text.Trim() : null);
            }

            txtLog.Text = "";
            txtURL.Text = ServerInfo.GetUrl();
            txtUrl2.Text = ServerInfo.GetUrl2();
            Log("ServerInfo: " + ServerInfo.GetInfos());
        }

        private void StartServer_Click(object sender, EventArgs e)
        {
            if (btnStartServer.Tag.ToString() != "start")
            {
                webServer.Stop();
                btnStartServer.Tag = "start";
                btnStartServer.Text = "Start Server";
                
                Log("Server Stopped.");
                gbxServer.Enabled = true;
                return;
            }

            try
            {
                Log("Starting Server, Please Wait...");

                screenshot.SetDefault();

                ServerInfo.IsWorking = true;

                Task.Run(() => webServer.Start());

                Thread.Sleep(500);

                Task.Factory.StartNew(() => CaptureScreenWhileWorking());
                
                Log("Server Started Successfuly!");
                Log("URL : " + ServerInfo.GetUrl());
                Log("Localhost URL : " + "http://localhost:" + numPort.Value.ToString() + "/");
                gbxServer.Enabled = false;

                btnStartServer.Tag = "stop";
                btnStartServer.Text = "Stop Server";

            }
            catch (Exception ex)
            {
                Log("Error! : " + ex.Message);
            }
        }

        #endregion

        #region ScreenSharing

        private void ScreenSharing_Changed(object sender, EventArgs e)
        {
            if (!cbPreview.Checked)
                imgPreview.Image = imgPreview.InitialImage;
        }

        private void ImgPreview_Click(object sender, EventArgs e)
        {
            imgPreview.Dock = (imgPreview.Dock == DockStyle.None) ? DockStyle.Fill : DockStyle.None;
            imgPreview.BringToFront();
        }

        private async Task CaptureScreenWhileWorking()
        {
            while (ServerInfo.IsWorking)
            {
                if (cbScreenshot.Checked)
                {
                    screenshot.HasPreview = cbPreview.Checked;
                    screenshot.CaptureScreen(cbCaptureMouse.Checked);

                    if (screenshot.HasPreview)
                        imgPreview.Image = screenshot.Preview;
                }

                await Task.Delay((int)numShotEvery.Value);
            }
        }

        #endregion
       
    }
}
