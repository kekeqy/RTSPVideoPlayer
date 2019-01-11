using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RTSPVideoPlayer
{
    public partial class MainWindow : Form
    {
        private HTTPServer server;
        public MainWindow()
        {
            InitializeComponent();
            int port = 80;
            try
            {
                port = int.Parse(ConfigurationManager.AppSettings["port"]);
            }
            catch { }
            server = new HTTPServer(port);
            server.RequestPlay += Server_RequestPlay;
        }

        private void Server_RequestPlay(string obj)
        {
            try
            {
                rtspPlayer1.Invoke(new Action<string>(url =>
                {
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        rtspPlayer1.Pause();
                        rtspPlayer1.Stop();
                        rtspPlayer1.Update();
                    }
                    else
                    {
                        rtspPlayer1.Play(url);
                    }
                }), obj);
            }
            catch { }
        }

        protected override void WndProc(ref Message m)
        {
            FormWindowState previousWindowState = this.WindowState;
            base.WndProc(ref m);
            FormWindowState currentWindowState = this.WindowState;
            if (previousWindowState != currentWindowState && currentWindowState == FormWindowState.Maximized)
            {
                FormBorderStyle = FormBorderStyle.None;
            }
        }
    }
}
