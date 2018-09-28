using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using System;
using System.Diagnostics;
using System.Text;

namespace Game.Misc
{
	public static class LoggerProxy
	{
		[System.Flags]
		public enum LogLevel
		{
			None = 1 << 0,
			Log = 1 << 1,
			Debug = 1 << 2,
			Net = 1 << 3,
			Info = 1 << 4,
			Warn = 1 << 5,
			Error = 1 << 6,
			Fatal = 1 << 7,
			All = int.MaxValue
		}

		public static LogLevel logLevel = LogLevel.All;

		private static ILog _log;

		public static void Init( string logPath, string ip, int port )
		{
			Core.Misc.Logger.logAction = Log;
			Core.Misc.Logger.debugAction = Debug;
			Core.Misc.Logger.netAction = Net;
			Core.Misc.Logger.infoAction = Info;
			Core.Misc.Logger.warnAction = Warn;
			Core.Misc.Logger.errorAction = Error;
			Core.Misc.Logger.factalAction = Fatal;

			ILoggerRepository repository = LogManager.CreateRepository( "NETCoreRepository" );
			PatternLayout patternLayout = new PatternLayout
			{
				ConversionPattern = "%date{MM-dd HH:mm:ss,fff} [%thread] %-5level %logger - %message%newline"
			};
			patternLayout.ActivateOptions();


			RollingFileAppender fileAppender = new RollingFileAppender
			{
				File = logPath,
				AppendToFile = true,
				RollingStyle = RollingFileAppender.RollingMode.Composite,
				StaticLogFileName = false,
				DatePattern = "yyyyMMdd'.log'",
				MaxSizeRollBackups = 10,
				MaximumFileSize = "2MB",
				Layout = patternLayout
			};
			fileAppender.ActivateOptions();

			UnityAppender unityLogger = new UnityAppender
			{
				Layout = new PatternLayout()
			};
			unityLogger.ActivateOptions();
			BasicConfigurator.Configure( repository, fileAppender, unityLogger );
			_log = LogManager.GetLogger( repository.Name, "Server" );
		}

		public static void Dispose()
		{
			LogManager.Shutdown();
		}

		public static void Log( object obj )
		{
			if ( ( logLevel & LogLevel.Log ) > 0 )
			{
				_log.Debug( obj );
			}
		}

		public static void Debug( object obj )
		{
			if ( ( logLevel & LogLevel.Debug ) > 0 )
			{
				_log.Debug( obj + Environment.NewLine + GetStacks() );
			}
		}

		public static void Net( object obj )
		{
			if ( ( logLevel & LogLevel.Net ) > 0 )
			{
				_log.Debug( obj );
			}
		}

		public static void Info( object obj )
		{
			if ( ( logLevel & LogLevel.Info ) > 0 )
			{
				_log.Info( obj );
			}
		}

		public static void Warn( object obj )
		{
			if ( ( logLevel & LogLevel.Warn ) > 0 )
			{
				_log.Warn( obj );
			}
		}

		public static void Error( object obj )
		{
			if ( ( logLevel & LogLevel.Error ) > 0 )
			{
				_log.Error( obj + Environment.NewLine + GetStacks() );
			}
		}

		public static void Fatal( object obj )
		{
			if ( ( logLevel & LogLevel.Fatal ) > 0 )
			{
				_log.Fatal( obj + Environment.NewLine + GetStacks() );
			}
		}

		private static string GetStacks()
		{
			StackTrace st = new StackTrace( true );
			if ( st.FrameCount < 3 )
				return string.Empty;

			StringBuilder sb = new StringBuilder();
			int count = Math.Min( st.FrameCount, 5 );
			for ( int i = 2; i < count; i++ )
			{
				StackFrame sf = st.GetFrame( i );
				string fn = sf.GetFileName();
				int pos = fn.LastIndexOf( '\\' ) + 1;
				fn = fn.Substring( pos, fn.Length - pos );
				sb.Append( $" M:{sf.GetMethod()} in {fn}:{sf.GetFileLineNumber()},{sf.GetFileColumnNumber()}" );
				if ( i != count - 1 )
					sb.AppendLine();
			}
			return sb.ToString();
		}
	}

	class UnityAppender : AppenderSkeleton
	{
		protected override void Append( LoggingEvent loggingEvent )
		{
			string message = this.RenderLoggingEvent( loggingEvent );

			if ( Level.Compare( loggingEvent.Level, Level.Error ) >= 0 )
			{
				UnityEngine.Debug.LogError( message );
			}
			else if ( Level.Compare( loggingEvent.Level, Level.Warn ) >= 0 )
			{
				UnityEngine.Debug.LogWarning( message );
			}
			else
			{
				UnityEngine.Debug.Log( message );
			}
		}
	}
}