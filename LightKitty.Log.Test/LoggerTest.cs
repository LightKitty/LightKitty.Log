using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LightKitty.Log.Test
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void WriteTest()
        {
            Logger.Debug("debug log");
            Logger.Info("info log");
            Logger.Warn("warn log");
            try
            {
                string str = null;
                str = str.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error("error log", ex);
            }
            Logger.Fatal("fatal log");

            Thread.Sleep(1000);
        }
    }
}