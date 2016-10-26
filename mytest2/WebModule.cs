using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Griffin.WebServer;
using Griffin.WebServer.Modules;
using SSTV4.DataContracts;
using System.Reflection;

namespace SSTV4
{
	public class WebModule : IWorkerModule
	{
		public WebModule()
		{
		}

		public void BeginRequest(IHttpContext context)
		{
		}

		public void EndRequest(IHttpContext context)
		{
		}

		private Stream StreamForString(string str)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);

			writer.Write(str);
			writer.Flush();
			stream.Position = 0;
			return stream;			
		}

		public void HandleRequestAsync(IHttpContext context, Action<IAsyncModuleResult> callback)
		{
			if (!context.Request.Uri.AbsolutePath.StartsWith("/api", StringComparison.InvariantCulture))
			{
				// Just serve the content
				var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Combine("..", "..", "WebUI"), context.Request.Uri.AbsolutePath.TrimStart('/'));
				Console.WriteLine(path);
				var ext = Path.GetExtension(path);

				if (!File.Exists(path))
				{
					context.Response.StatusCode = 404;
					callback(new AsyncModuleResult(context, ModuleResult.Stop));
					return;
				}

				var fs = new FileStream(path, FileMode.Open);

				context.Response.ContentType = MimeTypes.MimeTypeMap.GetMimeType(ext);
				context.Response.Body = fs;

				callback(new AsyncModuleResult(context, ModuleResult.Stop));
				return;
			}
			
			try
			{
				var res = Api.Instance.Execute(context.Request.Form);

				if (res != null)
				{
					context.Response.ContentType = "application/json";
					context.Response.Body = StreamForString(JsonConvert.SerializeObject(res));
				}
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = 500;
				context.Response.Body = StreamForString(ex.Message);

			}

			callback(new AsyncModuleResult(context, ModuleResult.Stop));
		}
		   
		private void ServeUI(IHttpContext context)
		{
			var index = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "WebUI", "index.htm");

			var text = File.ReadAllText(index);

			var fs = new FileStream(index, FileMode.Open);

			context.Response.ContentType = "text/html";
			context.Response.Body = fs;
		}
	}
}
