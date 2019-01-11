using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace RTSPVideoPlayer
{
    public class HTTPServer
    {
        private readonly HttpListener listener;
        public event Action<string> RequestPlay;
        public HTTPServer(int port)
        {
            listener = new HttpListener();
            IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    listener.Prefixes.Add(string.Format("http://{0}:{1}/", address.ToString(), port));
                }
            }
            listener.Prefixes.Add(string.Format("http://{0}:{1}/", "127.0.0.1", port));
            listener.Prefixes.Add(string.Format("http://{0}:{1}/", "localhost", port));
            listener.Start();
            listener.BeginGetContext(Handle, null);
        }
        private void Handle(IAsyncResult ar)
        {
            //继续异步监听
            listener.BeginGetContext(Handle, null);
            //获得context对象
            HttpListenerContext context = listener.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            context.Response.AppendHeader("Access-Control-Allow-Headers", "x-requested-with");
            context.Response.AppendHeader("Access-Control-Allow-Method", "GET,POST");
            context.Response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
            context.Response.AddHeader("Content-type", "text/plain");//添加响应头信息
            context.Response.ContentEncoding = Encoding.UTF8;
            try
            {
                if (request.RawUrl.StartsWith("/?url="))
                {
                    string url = request.QueryString["url"];
                    RequestPlay?.Invoke(url);
                    byte[] buffer = Encoding.UTF8.GetBytes("OK");
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            finally
            {
                response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
                response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
                response.Close();
            }
        }
    }
}