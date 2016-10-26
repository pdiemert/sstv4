using System;
using Gtk;
using Griffin.WebServer;
using Griffin.WebServer.Files;
using System.Net;
using log4net;
using log4net.Appender;
using log4net.Core;
using Griffin.WebServer.Modules;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using Gdk;

namespace SSTV4
{
	class MainClass
	{
		private static readonly ILog sm_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static MainWindow m_window;
		private static ScannerManager m_scanner;
		private static Configuration m_config;

		public static ILog Logger
		{
			get
			{
				return sm_log;
			}
		}

		private static MainWindow MainWindow
		{
			get
			{
				return m_window;
			}
		}

		public static Configuration Configuration
		{
			get
			{
				return m_config;
			}
		}

		public static bool IsOSX
		{
			get
			{
				return System.Environment.OSVersion.Platform == PlatformID.MacOSX || System.Environment.OSVersion.Platform == PlatformID.Unix;
			}
		}

		public static void Main(string[] args)
		{
			//var e = System.Environment.;

			m_config = new Configuration() { Port = 8881, Fullscreen = true };

			if (IsOSX)
			{
				m_config.MediaDirs = @"/Volumes/vast/downloads";
			}
			else
			{
				m_config.MediaDirs = @"\\192.168.0.110\vast\downloads";
			}

			log4net.Config.BasicConfigurator.Configure(new DisplayAppender());

			Application.Init();
			m_window = new MainWindow();

			Logger.Info("Starting SSTV");

			m_scanner = new ScannerManager();
			m_scanner.Refresh().ContinueWith((t) =>
			{
				Logger.Info("SSTV Running");
			});

			//win.Fullscreen ();
			//win.Show ();
			StartWebServer();

			Application.Run();
		}

		public static void Refresh()
		{
			Db.Instance.Clear();
			m_scanner.Refresh().ContinueWith((t) =>
			{
				Logger.Info("Scan complete");
			});

			//win.Fullscreen ();

		}

		private static void StartWebServer()
		{
			// Module manager handles all modules in the server
			var moduleManager = new ModuleManager();

			// Add the module
			moduleManager.Add(new WebModule());

//			var fileService = new DiskFileService("/", Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\..\WebUI"));
//			var module = new FileModule(fileService) { AllowFileListing = false };

//			moduleManager.Add(module);

			// And start the server.
			var server = new HttpServer(moduleManager);

			server.Start(IPAddress.Any, Configuration.Port);

			Logger.InfoFormat("Listening on port {0}", server.LocalPort);
		}

		public class DisplayAppender : AppenderSkeleton
		{
			protected override void Append(LoggingEvent loggingEvent)
			{
				if (MainWindow != null)
				{
					MainWindow.OutputType ot;
					switch (loggingEvent.Level.Name)
					{
						case "ERROR":
							ot = MainWindow.OutputType.Error;
							break;
						case "WARN":
							ot = MainWindow.OutputType.Warn;
							break;
						default:
							ot = MainWindow.OutputType.Info;
							break;
					}

					MainWindow.AddOutput(loggingEvent.RenderedMessage, ot);
				}
			}
		}
	}
}
