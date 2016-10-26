using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SSTV4.DataContracts;
using SSTV4.Interfaces;

namespace SSTV4.Scanners
{
	public class FileScanner : IScanner
	{
		private Dictionary<string, bool> m_extraFiles = new Dictionary<string, bool>();
		private Dictionary<string, List<string>> m_nonMediaFiles = new Dictionary<string, List<string>>();
		private Dictionary<string, Media> m_media = new Dictionary<string, Media>();
		private List<Media> m_dupes = new List<Media>();

		public FileScanner()
		{
		}

		public async Task Refresh()
		{
			await ScanDir(MainClass.Configuration.MediaDirs);

			Reconcile();

			foreach (var kvp in m_media)
			{
				Db.Instance.AddOrUpdateMedia(kvp.Value);
			}

			foreach (var m in m_media)
			{
				MainClass.Logger.InfoFormat("{0} : {1}", m.Value.Key, m.Value.Path);
			}

			foreach (var n in m_nonMediaFiles)
			{
				foreach (var p in n.Value)
				{
					MainClass.Logger.WarnFormat("Non-media: {0}", p);
				}
			}

			foreach (var e in m_extraFiles)
			{
				MainClass.Logger.WarnFormat("Extra: {0}", e.Key);
			}

			MainClass.Logger.InfoFormat("Found {0} media items, {1} non-media item groups, {2} extra files", m_media.Count, m_nonMediaFiles.Count, m_extraFiles.Count);
		}

		private void Reconcile()
		{
			// For dupes, keep the largest
			foreach (var dupe in m_dupes)
			{
				var existing = m_media[dupe.Key];

				if (dupe.Size > existing.Size)
				{
					// Toss out related non-media
					RemoveNonMediaFiles(existing.Path);

					m_extraFiles[existing.Path] = true;

					m_media[dupe.Key] = dupe;
				}
			}
		}

		private string NonMediaKey(string path)
		{
			return Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path);
		}

		private void AddNonMediaFile(string path)
		{
			// Group by file name without extension
			var key = NonMediaKey(path);

			List<string> lst;
			if (m_nonMediaFiles.TryGetValue(key, out lst))
			{
				lst.Add(path);
			}
			else
			{
				lst = new List<string>();
				lst.Add(path);
				m_nonMediaFiles.Add(key, lst);
			}
		}

		private void RemoveNonMediaFiles(string path)
		{
			var key = NonMediaKey(path);

			List<string> lst;
			if (m_nonMediaFiles.TryGetValue(key, out lst))
			{
				foreach (var file in lst)
				{
					AddExtraFile(file);
				}

				m_nonMediaFiles.Remove(key);
			}
		}

		private void AddExtraFile(string path)
		{
			if (!m_extraFiles.ContainsKey(path))
			{
				m_extraFiles.Add(path, true);
			}
		}

		private void AddMedia(Media m)
		{
			Media existing;
			if (m_media.TryGetValue(m.Key, out existing))
			{
				m_dupes.Add(m);
			}
			else
			{
				m_media.Add(m.Key, m);
			}
		}

		private Task ScanDir(string dir)
		{
			var files = Directory.GetFileSystemEntries(dir);

			foreach (var file in files)
			{
				try
				{
					if ((File.GetAttributes(file) & FileAttributes.Directory) == FileAttributes.Directory)
					{
						ScanDir(file);
					}
					else
					{
						var ext = Path.GetExtension(file).ToUpper().TrimStart(new[] { '.' });

						if (!MainClass.Configuration.MediaExtensions.ContainsKey(ext))
						{
							if (!MainClass.Configuration.KeeperExtensions.ContainsKey(ext))
							{
								if (MainClass.Configuration.IgnoreExtensions.ContainsKey(ext))
								{
									continue;
								}

								AddExtraFile(file);
							}
							else
							{
								AddNonMediaFile(file);
							}
						}
						else
						{
							Media m;
							if (!FileNameParser.TryParse(file, out m))
							{
								MainClass.Logger.ErrorFormat("Could not parse: {0}", file);
							}
							else
							{
								if (m == null)
								{
									AddExtraFile(file);
								}
								else
								{
									var fi = new FileInfo(file);
									m.Size = fi.Length;
									m.DateAdded = fi.LastWriteTime;

									AddMedia(m);
								}
							}
						}
					}
				}
				catch (PathTooLongException)
				{
					// Always discard
					AddExtraFile(file);
				}
				catch (Exception ex)
				{
					MainClass.Logger.ErrorFormat("Could not parse: {0}: {1}", file, ex.Message);
				}
			}

			return Task.FromResult(0);
		}
	}
}
