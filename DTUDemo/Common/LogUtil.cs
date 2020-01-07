﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class LogUtil
    {
        /// <summary>
        /// 声明nlog对象
        /// </summary>
        public static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 打印调试信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public static void Debug(string message)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("调试信息：" + message);
            }
        }

        /// <summary>
        /// 打印跟踪信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public static void Info(string message)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("信息:" + message);
            }
        }

        /// <summary>
        /// 打印警告信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public static void Warn(string message)
        {
            if (log.IsWarnEnabled)
            {
                log.Warn("警告：" + message);
            }
        }

        /// <summary>
        /// 打印错误信息
        /// </summary>
        /// <param name="message">信息内容</param>
        public static void Error(string message)
        {
            if (log.IsErrorEnabled)
            {
                log.Error("错误信息：" + message);
            }
        }
    }
}
