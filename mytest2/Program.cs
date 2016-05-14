using System;
using Gtk;
using Griffin.WebServer;

namespace mytest2
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var moduleManager = new ModuleManager ();

			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			Application.Run ();
		}
	}
}
