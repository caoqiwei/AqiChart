using log4net;

namespace AqiChart.Client.Common
{
    /// <summary>
    /// 日志类
    /// </summary>
    public abstract class LogHelper
    {
        //系统框架日志
        //Type sss = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

        public static string Name { get; set; } = "MMLogger";

        private static ILog Logger => LogManager.GetLogger(Name);


        //框架内部方法

        #region 调试信息
        /// <summary>
        /// 调试信息
        /// </summary>
        /// <param name="message">描述信息</param>
        /// <param name="exception">异常信息</param>
        public static void Debug(object message, Exception exception = null)
        {
            Logger.Debug(message, exception);
        }
        public static void DebugFormat(string format, params object[] args)
        {
            Logger.DebugFormat(format, args);
        }
        public static void DebugFormat(Exception exception, string format, params object[] args)
        {
            Logger.DebugFormat(format, args, exception);
        }
        public static void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.DebugFormat(formatProvider, format, args);
        }
        public static void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.DebugFormat(formatProvider, format, args, exception);
        }
        #endregion

        #region 一般信息
        /// <summary>
        /// 一般信息
        /// </summary>
        /// <param name="message">描述信息</param>
        /// <param name="exception">异常信息</param>
        public static void Info(object message, Exception exception = null)
        {
            Logger.Info(message, exception);
        }
        public static void InfoFormat(string format, params object[] args)
        {
            Logger.InfoFormat(format, args);
        }
        public static void InfoFormat(Exception exception, string format, params object[] args)
        {
            Logger.InfoFormat(format, args, exception);
        }
        public static void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.InfoFormat(formatProvider, format, args);
        }
        public static void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.InfoFormat(formatProvider, format, args, exception);
        }
        #endregion

        #region 警告
        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="message">描述信息</param>
        /// <param name="exception">异常信息</param>
        public static void Warn(object message, Exception exception = null)
        {
            Logger.Warn(message, exception);
        }
        public static void WarnFormat(string format, params object[] args)
        {
            Logger.WarnFormat(format, args);
        }
        public static void WarnFormat(Exception exception, string format, params object[] args)
        {
            Logger.WarnFormat(format, args, exception);
        }
        public static void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.WarnFormat(formatProvider, format, args);
        }
        public static void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.WarnFormat(formatProvider, format, args, exception);
        }
        #endregion

        #region 一般错误
        /// <summary>
        /// 一般错误
        /// </summary>
        /// <param name="message">描述信息</param>
        /// <param name="exception">异常信息</param>
        public static void Error(object message, Exception exception = null)
        {
            Logger.Error(message, exception);
        }
        public static void ErrorFormat(string format, params object[] args)
        {
            Logger.ErrorFormat(format, args);
        }
        public static void ErrorFormat(Exception exception, string format, params object[] args)
        {
            Logger.ErrorFormat(format, args, exception);
        }
        public static void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.ErrorFormat(formatProvider, format, args);
        }
        public static void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.ErrorFormat(formatProvider, format, args, exception);
        }
        #endregion

        #region 致命错误
        /// <summary>
        /// 致命错误
        /// </summary>
        /// <param name="message">描述信息</param>
        /// <param name="exception">异常信息</param>
        public static void Fatal(object message, Exception exception = null)
        {
            Logger.Fatal(message, exception);
        }
        public static void FatalFormat(string format, params object[] args)
        {
            Logger.FatalFormat(format, args);
        }
        public static void FatalFormat(Exception exception, string format, params object[] args)
        {
            Logger.FatalFormat(format, args, exception);
        }
        public static void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.FatalFormat(formatProvider, format, args);
        }
        public static void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            Logger.FatalFormat(formatProvider, format, args, exception);
        }
        #endregion 
    }
}
