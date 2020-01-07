using Common;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTULib
{
    public class DTUServer
    {
        private static List<IChannel> channel = null;

        public static void Start()
        {
            SetConsoleLogger();
            //var ports = SaveDataHelper.PortPlantList.Select(l => l.Port).Distinct();
            //主工作线程组，设置为1个线程  
            var bossGroup = new MultithreadEventLoopGroup(1);
            //工作线程组，默认为内核数*2的线程数  
            var workerGroup = new MultithreadEventLoopGroup();
            try
            {
                //声明一个服务端Bootstrap，每个Netty服务端程序，都由ServerBootstrap控制，  
                //通过链式的方式组装需要的参数  
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup); // 设置主和工作线程组  
                bootstrap.Channel<TcpServerSocketChannel>(); // 设置通道模式为TcpSocket
                bootstrap.ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    //工作线程连接器 是设置了一个管道，服务端主线程所有接收到的信息都会通过这个管道一层层往下传输  
                    //同时所有出栈的消息 也要这个管道的所有处理器进行一步步处理  
                    IChannelPipeline pipeline = channel.Pipeline;

                    //pipeline.AddLast(new IdleStateHandler(5, 0, 0));//第一个参数为读，第二个为写，第三个为读写全部
                    pipeline.AddLast(new LoggingHandler("test"));
                    pipeline.AddLast(new DTUServerHandler());
                }));

                // bootstrap绑定到指定端口的行为 就是服务端启动服务，同样的Serverbootstrap可以bind到多个端口  
                //var tasks = ports.Select(bootstrap.BindAsync).ToArray();

                var tasks = bootstrap.BindAsync(8007);
                Task.WaitAll(tasks);
                var test=tasks.Result;
                Console.ReadLine();
                test.CloseAsync();
                //channel = tasks.Select(s => s.Result).ToList();
            }
            catch (Exception ex)
            {
                LogUtil.Error("连接错误：" + ex.Message);
            }
            //finally
            //{
            //    //关闭线程组，先打开的后关闭
            //    bossGroup.ShutdownGracefullyAsync();
            //    workerGroup.ShutdownGracefullyAsync();
            //}
        }

        public static void Stop()
        {
            if (channel != null)
            {
                var tasks = channel.Select(c => c.CloseAsync()).ToArray();
                Task.WaitAll(tasks);
            }
        }
        public static void SetConsoleLogger() => InternalLoggerFactory.DefaultFactory.AddProvider(new ConsoleLoggerProvider((s, level) => true, false));
    }
}
