using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LightKitty.Log
{
    /// <summary>
    /// log. 日志
    /// </summary>
    public static class Logger
    {
        #region private fileds

        /// <summary>
        /// Write lock. 写日志锁
        /// </summary>
        private static readonly object fileLock = new object();

        /// <summary>
        /// Log folder path. 日志文件夹路径
        /// </summary>
        private static string folderPath = "log\\";

        #endregion

        #region fileds

        /// <summary>
        /// The lowest write log level (Default is Debug). 最低写日志级别（默认为Debug）
        /// </summary>
        public static LogLevel WriteLogLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Whether to write to the console at the same time. 是否同时写控制台
        /// </summary>
        public static bool IsConsoleWrite = false;

        #endregion

        #region public methods

        /// <summary>
        /// Set log folder path. 设置日志文件夹路径
        /// </summary>
        /// <param name="path">Log Folder Path. 日志文件夹路径</param>
        public static void SetFolderPath(string path)
        {
            folderPath = (path.EndsWith("\\") || path.EndsWith("/")) ? path : path + "\\";
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); //判断并创建日志文件夹
        }

        /// <summary>
        /// Write debug log (Not write in release). 写调试日志（Release版本不会写）
        /// </summary>
        /// <param name="msg">Log message. 日志信息</param>
        /// <param name="ex">Exception. 异常</param>
        public static void Debug(string msg, Exception ex = null)
        {
            if (WriteLogLevel > LogLevel.Debug) return;
            Task.Run(() =>
            {
                Write(LogLevel.Debug, msg, ex?.ToString());
            });
        }

        /// <summary>
        /// Write info log. 写信息日志
        /// </summary>
        /// <param name="msg">Log message. 日志信息</param>
        /// <param name="ex">Exception. 异常</param>
        public static void Info(string msg, Exception ex = null)
        {
            if (WriteLogLevel > LogLevel.Info) return;
            Task.Run(() =>
            {
                Write(LogLevel.Info, msg, ex?.ToString());
            });
        }

        /// <summary>
        /// Write warn log. 写警告日志
        /// </summary>
        /// <param name="msg">Log message. 日志信息</param>
        /// <param name="ex">Exception. 异常</param>
        public static void Warn(string msg, Exception ex = null)
        {
            if (WriteLogLevel > LogLevel.Warn) return;
            Task.Run(() =>
            {
                Write(LogLevel.Warn, msg, ex?.ToString());
            });
        }

        /// <summary>
        /// Write error log. 写错误日志
        /// </summary>
        /// <param name="msg">Log message. 日志信息</param>
        /// <param name="ex">Exception. 异常</param>
        public static void Error(string msg, Exception ex = null)
        {
            if (WriteLogLevel > LogLevel.Error) return;
            Task.Run(() =>
            {
                Write(LogLevel.Error, msg, ex?.ToString());
            });
        }

        /// <summary>
        /// Write faltal log. 写毁灭日志
        /// </summary>
        /// <param name="msg">Log message. 日志信息</param>
        /// <param name="ex">Exception. 异常</param>
        public static void Fatal(string msg, Exception ex = null)
        {
            if (WriteLogLevel > LogLevel.Fatal) return;
            Task.Run(() =>
            {
                Write(LogLevel.Fatal, msg, ex?.ToString());
            });
        }

        /// <summary>
        /// Obtain the latest logs. 获取最新日志
        /// </summary>
        /// <param name="lineCount">Line number. 行数</param>
        /// <returns></returns>
        public static string GetLatestLog(int lineCount = 10)
        {
            string path = folderPath + GetName(); //日志路径
            if (!File.Exists(path))
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            lock (fileLock)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string[] lines = new string[lineCount];
                    for (int i = 0; i < lineCount; i++)
                    {
                        lines[i] = reader.ReadLine();
                    }

                    // 现在lines数组中包含了文件的最后几行，你可以对其进行处理
                    foreach (string line in lines)
                    {
                        builder.AppendLine(line);
                    }
                }
            }
            return builder.ToString();
        }

        #endregion

        #region private methods

        /// <summary>
        /// Write log. 写日志
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="msg"></param>
        /// <param name="exMsg"></param>
        private static void Write(LogLevel logLevel, string msg, string exMsg)
        {
            string content = GetContent(logLevel, msg, exMsg);//格式化日志
            if(IsConsoleWrite) Console.WriteLine(content); //写控制台
            Write(GetName(), content); //写日志
        }

        /// <summary>
        /// Write log. 写日志
        /// </summary>
        /// <param name="name"></param>
        /// <param name="msg"></param>
        private static void Write(string name, string msg)
        {
            string path = folderPath + name; //日志路径
            lock (fileLock)
            { //保证单线程写磁盘文件
                if (File.Exists(path))
                { //存在日志文件
                    using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8)) //追加
                    {
                        sw.WriteLine(msg); //写日志
                    }
                }
                else
                { //不存在日志文件
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath); //判断并创建日志文件夹
                    using (var fs = File.Create(path)) //创建日志文件
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(msg); //写日志
                    } 
                }
            }
        }

        /// <summary>
        /// Get log file name. 获取日志文件名称
        /// </summary>
        /// <returns></returns>
        private static string GetName()
        {
            return DateTime.Now.ToString("yyyyMMdd") + ".log";
        }

        /// <summary>
        /// Format log content. 格式化日志内容
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="msg"></param>
        /// <param name="exMsg"></param>
        /// <returns></returns>
        private static string GetContent(LogLevel logLevel, string msg, string exMsg)
        {
            if (string.IsNullOrEmpty(exMsg))
            {
                return GetHead(logLevel) + msg;
            }
            else
            {
                return GetHead(logLevel) + msg + Environment.NewLine + exMsg;
            }
        }

        /// <summary>
        /// Log head text. 日志头部文本
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        private static string GetHead(LogLevel logLevel)
        {
            return $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} {logLevel.ToString()}] ";
        }

        #endregion
    }

    /// <summary>
    /// Log level. 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level. 调试级别
        /// </summary>
        Debug,

        /// <summary>
        /// Info level. 信息级别
        /// </summary>
        Info,

        /// <summary>
        /// Warn level. 警告级别
        /// </summary>
        Warn,

        /// <summary>
        /// Error level, 错误级别
        /// </summary>
        Error,

        /// <summary>
        /// Fatal level. 毁灭级别
        /// </summary>
        Fatal
    };
}
