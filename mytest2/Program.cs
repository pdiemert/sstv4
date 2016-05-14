using System;
using Gtk;
using Griffin.WebServer;
using Griffin.WebServer.Files;
using System.Net;
using System.Runtime.InteropServices;
using Griffin.Logging;


namespace mytest2
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			StartWebServer ();

			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Fullscreen ();
			win.
			//win.Show ();
			Application.Run ();
		}

		private static void StartWebServer()
		{
			return;
			// Module manager handles all modules in the server
			var moduleManager = new ModuleManager();

			// Let's serve our downloaded files (Windows 7 users)
			var fileService = new DiskFileService("/", string.Format(@"C:\Users\{0}\Downloads", Environment.UserName));

			// Create the file module and allow files to be listed.
			var module = new FileModule(fileService) {AllowFileListing = true};

			// Add the module
			moduleManager.Add(module);
			//moduleManager.Add(new MyModule());

			//moduleManager.Add(new MyModule2());
			// And start the server.
			var server = new HttpServer(moduleManager);
			server.Start(IPAddress.Any, 0);
			Console.WriteLine("PORT " + server.LocalPort);
		}
	}
}
