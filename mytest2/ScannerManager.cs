using System;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using SSTV4.Interfaces;

namespace SSTV4
{
	public class ScannerManager
	{
		public ScannerManager()
		{
		}

		public async Task Refresh()
		{
			await Task.Yield();

			var q = (from t in Assembly.GetExecutingAssembly().GetTypes() where t.IsClass && typeof(IScanner).IsAssignableFrom(t) select t).ToList();

			var tasks = q.Select(t =>
			{
				MainClass.Logger.Info(string.Format("Running {0}", t.Name));
				var o = Activator.CreateInstance(t) as IScanner;

				try
				{
					return o.Refresh();
				}
				catch (Exception ex)
				{
					MainClass.Logger.Error(string.Format("Unexpected error during refresh: {0}",  ex.StackTrace));
					return Task.FromResult(0);					                       
				}
			});

			await Task.WhenAll(tasks.ToArray());
		}
	}
}
