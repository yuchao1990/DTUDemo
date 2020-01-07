using Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DTULib
{
    public class DTUServerHandler : ChannelHandlerAdapter
    {
        private byte[] cBuf = new byte[0];
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            IPEndPoint ipSocket = (IPEndPoint)context.Channel.RemoteAddress;
            var addr = ipSocket.Address;
            if (addr.IsIPv4MappedToIPv6)
                addr = addr.MapToIPv4();
            LogUtil.Info("通道读取，对方位于" + addr.ToString() + ":" + ipSocket.Port);
            var buffer = message as IByteBuffer;
            if (buffer != null)
            {
                //string msg = BitConverter.ToString(buffer.Array);
                //LogUtil.Info(msg);
                // 读取长度
                int len = buffer.ReadableBytes;
                byte[] curBuf = new byte[len];
                Array.ConstrainedCopy(buffer.Array, buffer.ArrayOffset, curBuf, 0, len);
                string msg = BitConverter.ToString(curBuf);
                LogUtil.Info(msg);
                //
                Unpacking(curBuf);
            }
        }

        /// <summary>
        /// 递归处理当前包
        /// </summary>
        /// <param name="curBuf"></param>
        private void Unpacking(byte[] curBuf)
        {
            // 读取长度
            int len = curBuf.Length;
            // 定义拼接数据包
            byte[] revBuf = new byte[len + cBuf.Length];
            // 拼接 缓存数据包
            Array.ConstrainedCopy(cBuf, 0, revBuf, 0, cBuf.Length);
            // 拼接 新接收数据包
            Array.ConstrainedCopy(curBuf, 0, revBuf, cBuf.Length, len);
            // 使用完重置缓存包
            cBuf = new byte[0];
            // 包长判断
            if (len >= 4)
            {
                //2 3 位为整包长度
                var packageLen = BitConverter.ToInt16(revBuf, 2);

                if (packageLen==0)
                {
                    return;
                }
                if (packageLen > revBuf.Length)
                {
                    // 缓存断包  等待下一个数据包
                    LogUtil.Info("缓存断包!");
                    cBuf = revBuf;
                }
                else
                {
                    // 根据长度 拆包
                    byte[] comBuf = new byte[packageLen];
                    Array.ConstrainedCopy(revBuf, 0, comBuf, 0, packageLen);
                    // 业务处理
                    DoSomeThing(comBuf);
                    //// 重置缓存包
                    //cBuf = new byte[0];
                    int remLen = revBuf.Length - packageLen;
                    if (remLen>0)
                    {
                        LogUtil.Info("粘包处理!");
                        // 重置当前处理包
                        curBuf = new byte[remLen];
                        Array.ConstrainedCopy(revBuf, packageLen, curBuf, 0, remLen);
                        // 递归处理剩余数据包
                        Unpacking(curBuf);
                    }                 
                }
            }
            else
            {
                // 缓存断包 等待下一个数据包
                LogUtil.Info("缓存断包!");
                cBuf = revBuf;
            }
        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
            context.Flush();
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }
        private void DoSomeThing(byte[] comBuf)
        {
            // 完整数据包 后两位 为 CRC 校验位
            bool crc = CRC.isCRCCorrect(comBuf, false, comBuf.Length);

            LogUtil.Info(crc+"------------");
            string msg = BitConverter.ToString(comBuf);
            LogUtil.Info(msg);
            Console.WriteLine(msg);

            int dataLen = BitConverter.ToInt16(comBuf, 2) - 14;
            byte[] DataByte = new byte[dataLen];
            Array.ConstrainedCopy(comBuf, 14, DataByte, 0, dataLen);

            // 0 1 头
            // 2 3 长度
            // 4 5 厂站号
            int stationId = BitConverter.ToInt16(comBuf, 4);
            // 6 7 通道号
            int channelId = BitConverter.ToInt16(comBuf, 6);
            int startAddress = BitConverter.ToInt16(comBuf, 11);
            int type = Convert.ToInt16(comBuf[9]);
            LogUtil.Info("场站号：" + stationId.ToString() + ", 通道号：" + channelId.ToString() + ", 起始地址：" + startAddress+",type："+ type);
            LogUtil.Info(stationId+":"+string.Join("-", GetRaw(DataByte)));
        }

        public float[] GetRaw(byte[] rawData)
        {
            if (rawData == null)
                return null;
            float[] result = new float[rawData.Length / 4];
            IntPtr midPtr = Marshal.AllocHGlobal(rawData.Length);
            Marshal.Copy(rawData, 0, midPtr, rawData.Length);
            Marshal.Copy(midPtr, result, 0, result.Length);
            Marshal.FreeHGlobal(midPtr);
            return result;
        }
    }
}
