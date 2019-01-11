using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RTSPVideoPlayer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainWindow());
            Mutex instance = new Mutex(true, "TestSingleStart", out bool createdNew); //同步基元变量 
            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindow());
                instance.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("已经启动了一个程序！");
                Application.Exit();
            }
        }
        
    }
}
