using System;

namespace Common
{
    public class CRC
    {
        public CRC()
        {

        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            var res = "";
            foreach (var b in bytes)
            {
                res += b.ToString("X2") + " ";
            }
            return res;
        }

        public static string GetStringFromBytes(byte[] bytes, int length)
        {
            var res = "";
            int lengthTemp = 0;
            foreach (var b in bytes)
            {
                res += b.ToString("X2") + " ";
                lengthTemp++;
                if (lengthTemp >= length)
                {
                    break;
                }
            }
            return res;
        }

        #region CRC校验，验证抓取到的数据包是否为有效数据包

        /// <summary>
        /// CRC校验，验证抓取到的数据包是否为有效数据包
        /// </summary>
        /// <param name="b">抓取到的数据包,类型byte[]</param>
        /// <param name="isOwner">是不是7B~7D，是->true 不是->false</param>
        /// <param name="data_len">数据长度包含CRC16的两位</param>
        /// <returns>
        /// <para>有效数据包：true</para>
        /// <para>无效数据包:false</para>
        /// </returns>
        public static bool isCRCCorrect(byte[] b, bool isOwner, int data_len)
        {
            if (isOwner)
            {
                byte[] crcByte = CRC16(b);// CRC.GetCRC16(b, 1, (byte)(b[1] - 1));
                return crcByte[1] == b[b[1] - 1] && crcByte[0] == b[b[1]];
            }
            else
            {
                byte[] crcByte = CRC16(b);//CRC.GetCRC16(b, 0, (data_len - 2));
                byte[] crcByte2 = ToModbus(b);
                byte[] crcByte1 = CRC.GetCRC16(b, 0, (data_len - 2));//CRC.GetCRC16(b, 0, (data_len - 2));
                return crcByte[1] == b[data_len - 2] && crcByte[0] == b[data_len - 1];
            }
        }

        #endregion

        // CRC-16/MODBUS
        public static byte[] GetCRC16(byte[] data, int start, int datalen)
        {
            byte[] crc = new byte[2];
            int i;

            byte CRC16Lo = 0xff, CRC16Hi = 0xff, CH = 0xA0, CL = 0x01;
            byte SaveHi, SaveLo;
            byte Flag;

            for (i = start; i < datalen; i++)  //0 to Data_Len-1 
            //for(i=1;i< datalen-1; i++)  //1 to datalen-1
            {
                CRC16Lo = (byte)(CRC16Lo ^ data[i]);
                for (Flag = 0; Flag < 8; Flag++) //0 to 7
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);
                    CRC16Lo = (byte)(CRC16Lo >> 1);
                    if ((SaveHi & 0x01) == 0x01)
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);
                    }
                    if ((SaveLo & 0x01) == 0x01)
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            crc[0] = CRC16Hi;
            crc[1] = CRC16Lo;
            return crc;
        }
        public static byte[] ToModbus(byte[] byteData)
        {
            byte[] CRC = new byte[2];   // CRC-16 returns 2 bytes
            UInt16 CRC_Chk = 0xFFFF;    // The Initial Value of CRC-16 modbus

            // Check every Bytes 剔除后两位 CRC校验位
            for (int i = 0; i < byteData.Length-2; i++)
            {
                // CRC_Chk XOR with byte[i]
                CRC_Chk ^= Convert.ToUInt16(byteData[i]);

                // Check every bits in byte
                for (int j = 0; j < 8; j++)
                {
                    // 若右移位元 = 1 , 則 XOR 0xA001
                    if ((CRC_Chk & 0x0001) == 1)
                    {
                        CRC_Chk >>= 1;
                        CRC_Chk ^= 0xA001; // 0xA001 is reverse from poly : 0x8005H
                    }
                    else
                    {
                        CRC_Chk >>= 1;
                    }
                }
            }
            // 計算結果須交換位置
            CRC[1] = (byte)((CRC_Chk & 0xFF00) >> 8);
            CRC[0] = (byte)(CRC_Chk & 0x00FF);

            return CRC;
        }

        /// <summary>
        /// CRC码计算,返回数组中,0为低位,1为高位
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] CRC16(byte[] data)
        {
            byte CRC16Lo = 0;
            byte CRC16Hi = 0;
            byte CL = 0;
            byte CH = 0;
            byte SaveHi = 0;
            byte SaveLo = 0;
            // short i = 0;
            short Flag = 0;
            byte[] ReturnData = new byte[1 + 1];
            // CRC寄存器
            // 多项式码&HA001
            CRC16Lo = (byte)0xFF;
            CRC16Hi = (byte)0xFF;
            CL = (byte)0x1;
            CH = (byte)0xA0;
            for (int i = 0; i <= data.Length - 3; i++)
            {
                CRC16Lo = Convert.ToByte(CRC16Lo ^ data[i]);
                // 每一个数据与CRC寄存器进行异或
                for (Flag = (short)0;
                Flag <= 7.0;
                Flag = Convert.ToInt16(Flag + 1))
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = Convert.ToByte(CRC16Hi / 2);
                    // 高位右移一位
                    CRC16Lo = Convert.ToByte(CRC16Lo / 2);
                    // 低位右移一位
                    if ((SaveHi & 0x1) == 0x1)
                    {
                        // 如果高位字节最后一位为1
                        CRC16Lo = Convert.ToByte(CRC16Lo | 0x80);
                        // 则低位字节右移后前面补1
                    }
                    // 否则自动补0
                    if ((SaveLo & 0x1) == 0x1)
                    {
                        // 如果LSB为1，则与多项式码进行异或
                        CRC16Hi = Convert.ToByte(CRC16Hi ^ CH);
                        CRC16Lo = Convert.ToByte(CRC16Lo ^ CL);
                    }
                    // Debug.Print Str(i) & ":", CRC16Lo, CRC16Hi
                }
                // Debug.Print CRC16Lo, CRC16Hi
            }
            ReturnData[0] = CRC16Lo;
            // CRC低位
            ReturnData[1] = CRC16Hi;
            // CRC高位
            return ReturnData;
        }

        public static byte[] CRC16(byte[] data, byte start, byte Data_Len)
        {
            byte CRC16Lo = 0xff, CRC16Hi = 0xff, CH = 0xA0, CL = 0x01;
            byte SaveHi, SaveLo;
            byte i, Flag;
            byte[] CRC = new byte[2];
            for (i = start; i < Data_Len; i++)  //0 to Data_Len-1
            {
                CRC16Lo = (byte)(CRC16Lo ^ data[i]);
                for (Flag = 0; Flag < 8; Flag++) //0 to 7
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);
                    CRC16Lo = (byte)(CRC16Lo >> 1);
                    if ((SaveHi & 0x01) == 0x01)
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);
                    }
                    if ((SaveLo & 0x01) == 0x01)
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            CRC[0] = CRC16Hi;
            CRC[1] = CRC16Lo;
            return CRC;
        }

        /// <summary>
        /// 累加取模校验
        /// </summary>
        /// <param name="b">待校验数据包</param>
        /// <param name="isDouble">是否两个fe起始</param>
        /// <param name="data_len">数据长度</param>
        /// <returns></returns>
        public static bool isADDCorrect(byte[] b, bool isDouble, int data_len)
        {
            int count = 0;
            int startindex = isDouble ? 2 : 1;
            for (int i = startindex; i <= data_len - 3; i++)
            {
                count += b[i];
            }
            return (count % 256) == b[data_len - 2];
        }

        public static void getADDCorrect(byte[] b, int data_len)
        {
            int count = 0;
            for (int i = 0; i < data_len; i++)
            {
                count += b[i];
            }
            byte result = (byte)(count % 256);
        }

        /// <summary>
        /// 累加取模校验 从起始符开始累加
        /// </summary>
        /// <param name="b">待校验数据包</param>
        /// <param name="data_len">数据长度</param>
        /// <returns></returns>
        public static bool isADDCorrectDLT645(byte[] b, int data_len)
        {
            int count = 0;
            for (int i = 0; i <= data_len - 3; i++)
            {
                count += b[i];
            }
            return (count % 256) == b[data_len - 2];
        }
    }
}
