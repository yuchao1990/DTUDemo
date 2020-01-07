using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static Socket socketSend;
        public static Logger logger = LogManager.GetLogger("ConsoleApp1");
        static void Main(string[] args)
        {
            Console.WriteLine("执行开始");
            logger.Error("Hello World");
            Console.WriteLine("执行结束");

            socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32("8007"));

            socketSend.Connect(point);

            string msg = "xxxxxxxxxxxxxxxxxxxxxxxx";
            byte[] buffer = new byte[1024 * 1024 * 3];
            buffer = Encoding.UTF8.GetBytes(msg);
            socketSend.Send(buffer);

            Thread c_thread = new Thread(Received);
            c_thread.IsBackground = true;
            c_thread.Start();
            Console.ReadKey();
        }

                   /// <summary>
           /// 接收服务端返回的消息
           /// </summary>
           static void Received()
           {
               while (true)
               {
                 try
                 {
                      byte[] buffer = new byte[1024 * 1024 * 3];
                      //实际接收到的有效字节数
                     int len = socketSend.Receive(buffer);
                      if (len == 0)
                      {
                         continue;
                        }
                       string str = Encoding.UTF8.GetString(buffer, 0, len);

                    Console.WriteLine(str);
                   }
                   catch
                   {
  
  
 
                  }
              }
           }
    }
}
