using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using SSTV4.DataContracts;

namespace SSTV4
{
	public class FileNameParser
	{
		public FileNameParser()
		{
		}

		private class Pattern
		{
			public string Regex { get; set; }
			public MediaType MediaType { get; set; }
			public string[] Fields { get; set; }
			public bool Discard { get; set; }
			public bool UsesAirTime { get; set; }
		}

		private static Pattern[] sm_patterns =
		{
			new Pattern()
			{
				Regex = @"^.*sample(?:[-\.]\w*)?\.\w*$",
				Discard = true
			},
			new Pattern()
			{
				Regex = @"^(.*)(\d\d\d\d)\.(\d\d)\.(\d\d)(.*)(?:720p|1080p).*\.(\w*)$",
				Fields = new[] { "Series", "AirYear", "AirMonth", "AirDay", "Title", "Ext" },
				MediaType = MediaType.TV
			},
			new Pattern()
			{
				Regex = @"^(.*)(\d\d\d\d)\.(\d\d)\.(\d\d).*\.(\w*)$",
				Fields = new[] { "Series", "AirYear", "AirMonth", "AirDay", "Ext" },
				MediaType = MediaType.TV
			},
			new Pattern()
			{
				Regex = @"^(.*).*[S|s](\d\d?)[E|e](\d\d?).*\.(\w*)$",
				Fields = new[] { "Series", "Season", "Episode", "Ext" },
				MediaType = MediaType.TV
			},
			new Pattern()
			{
				Regex = @"^([\w -\.]+)\.(?:720p|1080p)\.(?:HDTV|hdtv|bluray)\.x264(?:-\w*)?\.(\w*)$",
				Fields = new[] { "Title", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^([\w -\.]+)[ -\.](\d\d\d\d)[ -\.](?:720p|1080p)[\.-][\w-\.]*\.(\w*)$",
				Fields = new[] { "Title", "Year", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^([\w -\.]+)[ -\.](?:720p|1080p)[\.-][\w-\.]*\.(\w*)$",
				Fields = new[] { "Title", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^([\w -\.]+)[ -\.](?:720p|1080p)\.(\w*)$",
				Fields = new[] { "Title", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^([\w -\.]+)[ -\.](?:HD)?x264\.(\w*)$",
				Fields = new[] { "Title", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^(.*)(\d\d\d\d).*\.(\w*)$",
				Fields = new[] { "Title", "Year", "Ext" },
				MediaType = MediaType.Movie
			},
			new Pattern()
			{
				Regex = @"^([\w\d -\.]+)\.(\w*)$",
				Fields = new[] { "Title", "Ext" },
				MediaType = MediaType.Movie
			},
		};

		/// <summary>
		/// Returns if successfully parsed
		/// Out media will be null if item should be discarded
		/// </summary>
		public static bool TryParse(string path, out Media mOut)
		{
			mOut = null;

			var filename = Path.GetFileName(path);

			var idxParse = 0;
			foreach (var p in sm_patterns)
			{
				idxParse++;

				var re = new Regex(p.Regex);

				var match = re.Match(filename);

				if (match.Success)
				{
					if (p.Discard)
					{
						return true;
					}

					var m = new Media();

					m.Path = path;
					m.Type = p.MediaType;
					m.ParseIdx = idxParse;

					var type = m.GetType();
					var idx = 1;
					foreach (var f in p.Fields)
					{
						var val = match.Groups[idx++].Value;

						if (f == "Ext")
						{
							val = val.ToUpper();
						}

						// Special case: clean certain fields
						if (f == "Title" || f == "Series")
						{
							val = Clean(val);
						}

						var prop = type.GetProperty(f);

						var code = Type.GetTypeCode(prop.PropertyType);
						if (code == TypeCode.Int32)
						{
							prop.SetValue(m, int.Parse(val));
						}
						else
						{
							prop.SetValue(m, val);
						}
					}

					mOut = m;

					return true;
				}
			}

			return false;
		}

		private static string Clean(string str)
		{
			str = str.Replace('.', ' ').Trim();

			return str;
		}
	}
}
/*
new Pattern()
{
	// 0 /Volumes/c$/Media/TV/2 Broke Girls/2.Broke.Girls.S03E18.720p.HDTV.X264-DIMENSION/couple.of.poor.bitches.318.720p-dimension.sample.mkv
	Regex = @".* (?:\.| -)sample\.(\w *)$",
	Discard = true
},
new Pattern()
{   // 1
	Regex = @".*-s\.(\w*)$",
		Discard = true
},
new Pattern()
{   
	// 2 /Volumes/c$/Media/TV/Bizarre Foods/S06/Bizarre.Foods.S06E21.Sardinia.HDTV.XviD-MOMENTUM/sample-bizarre.foods.s06e21.hdtv.xvid-momentum.avi
	Regex = @".*[\/\\].*sample-.*\.(\w*)$",
	Discard = true
},
new Pattern()
{   
	// 3 /Volumes/c$/Media/Movies/Saving Private Ryan 1998 1080p BluRay AC3 x264 estres/Promo/Teaser.mp4
	Regex = @".*[\/\\]teaser\.(\w*)$",
	Discard = true
},
new Pattern()
{   
	// 4 /Volumes/c$/Media/Movies/Enemy 2013 1080p WEBRIP x264 AC3-EVE Read Nfo/Enemy.2013.1080p.WEBRIP.x264.AC3-EVE/Sample.mp4
	Regex = @".*[\/\\]sample\.(\w*)$",
	Discard = true
},
new Pattern()
{   
	// 5
	Regex = @".*[\/\\][^\/\\\.]*[\/\\]_UNPACK_(.*)\.S\d+E\d+.*[\/\\].*S\d{1,3}E\d{1,3}.*\.\w*$",
	Discard = true
},
new Pattern()
{   
	// 6 /Volumes/c$/Media/TV/2 Broke Girls/2.Broke.Girls.S03E18.720p.HDTV.X264-DIMENSION/2.Broke.Girls.S03E18.720p.HDTV.X264-DIMENSION.mkv
	Regex = @".*[\/\\][^/\.]*[\/\\](.*)\.S\d+E\d+.*[\/\\].*S(\d{1,3})E(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] {"series", "season", "episode", "ext"}
},
new Pattern()
{   
	// 7
	Regex = @".*[\/\\](.*)\.(\d{1,2})x(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] {"series.", "season", "episode", "ext"}
},
new Pattern()
{   
	// 8 /Volumes/c$/Media/TV/2 Broke Girls/2 Broke Girls - 3x15 - And the Icing on the Cake.mkv
	// /Volumes/c$/Media/TV/House of Cards (2013)/House of Cards (US) - 2x01 - Chapter 14.mkv

	Regex = @".*[\/\\]([^\/\\]*(?: ?\(\d*\))?) +- *(\d{1,3})x(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] { "series", "season", "episode", "ext" }
},
new Pattern()
{   
	// 9 /Volumes/c$/Media/TV/Inside Comedy/Inside Comedy - S01E05 - Larry David.avi
	Regex = @".*[\/\\]([^\/\\]*(?: ?\(\d*\))?) +- *S(\d{1,3})E(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] {"series", "season", "episode", "ext"}
},
new Pattern()
{   
	// 10 /Volumes/c$/Media/TV/60 Minutes/60.Minutes.AU.2013.10.27.One.Direction.PDTV.x264-FUtV/60.minutes.au.2013.10.27.one.direction.pdtv.x264-futv.mp4

	Regex = @".*[\/\\](.*)\.(\d\d\d\d)\.(\d{1,2})\.(\d\d{1,2}).*\.(\w*)$",
	MediaType = MediaType.TV,
	UsesAirTime = true,
	Fields = new[] {"series", "airyear", "airmonth", "airday", "ext"}
},
new Pattern()
{   
	// 11 /Volumes/c$/Media/TV/60 Minutes/60.Minutes.US.2014.01.26.Jay.Leno.HDTV.x264.mp4

	Regex = @".*[\/\\](.*)[\/\\].*(\d\d\d\d)\.(\d{1,2})\.(\d{1,2}).*\.(\w*)$",
	MediaType = MediaType.TV,
	UsesAirTime = true,
	Fields = new[] {"series", "airyear", "airmonth", "airday", "ext"}
},
new Pattern()
{   
	// 12 /Volumes/c$/Media/TV/House of Cards (2013)/House.of.Cards.2013.S01E12.720p.BluRay.x264-Green.mkv

	Regex = @".*[\/\\](.*)\.\d\d\d\d\.s(\d{1,3})e(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] { "series", "season", "episode#", "ext"}
},
new Pattern()
{   
	// 13 /Volumes/c$/Media/TV/Angry Boys/angry.boys.s01e04.720p.hdtv.x264-bia.mkv

	Regex = @".*[\/\\](?:[^-]*-)?(.*)\.s(\d{1,3})e(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] { "series", "season", "episode", "ext" }
},
new Pattern()
{   // 14 /Volumes/c$/Media/TV/Little Britain/little.britain.s01.e01.ws.ac3.dvdrip.xvid-m00tv.avi

	Regex = @".*[\/\\](?:[^-]*-)?(.*)\.s(\d{1,3})\.e(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] { "series", "season", "episode", "ext" }
},
new Pattern()
{
	// 15 /Volumes/c$/Media/TV/Monty Python's Flying Circus/MPFC-1.01.XviD.DVDRip.[rus.eng]_weconty.avi

	Regex = @".*[\/\\](.*)[\/\\]\w\w\w\w-(\d{1,3})\.(\d{1,3}).*\.(\w*)$",
	MediaType = MediaType.TV,
	Fields = new[] {"series", "season#", "episode", "ext" }
},
new Pattern()
{   
	// 16 /Volumes/c$/Media/Movies/Upside Down 2012 720p BRRip AC3 x264 MacGuffin/macguffin-upsdow720p/Upside Down 2012 720p BRRip AC3 x264 MacGuffin.mkv

	Regex = @".*[\/\\](.*) \((\d\d\d\d)\)[\/\\][^\/\\]*\.(\w*)$$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 17 /Volumes/c$/Media/Movies/Saving Private Ryan 1998 1080p BluRay AC3 x264 estres/Saving Private Ryan (1998)(1080p)/Saving Private Ryan (1998).mkv

	Regex = @".*[\/\\](.*) *\((\d\d\d\d)\)\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 18 /Volumes/c$/Media/Movies/The.Secret.Life.of.Walter.Mitty.2013.720p.WEB-DL.DD5.1.H.264-PHD/The.Secret.Life.Of.Walter.Mitty.2013.720p.WEB-DL.DD5.1.H.264-PHD.mkv

	Regex = @".*[\/\\](?:_UNPACK_)?(?:.*-)?(.*)\.(\d\d\d\d)\..*\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 19 /Volumes/c$/Media/Movies/The Jerk (1979) 720p/The Jerk (1979) 720p NL Subs/The Jerk (1979) 720p NL Subs.mkv

	Regex = @".*[\/\\](.*) \((\d\d\d\d)\) .*\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 20 /Volumes/c$/Media/Movies/Wolf Children (2012)/Wolf Children.mkv

	Regex = @".*[\/\\](.*) \((\d\d\d\d)\)[\/\\].*\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 21 /Volumes/c$/Media/Movies/Upside Down 2012 720p BRRip AC3 x264 MacGuffin/macguffin-upsdow720p/Upside Down 2012 720p BRRip AC3 x264 MacGuffin.mkv

	Regex = @".*[\/\\](.*) (\d\d\d\d) .*\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "year", "ext"}
},
new Pattern()
{   
	// 22 /Volumes/c$/Media/Movies/Louis C.K. Oh.My.God.720p.HDTV.x264.AC3-Riding High.mkv

	Regex = @".*[\/\\](.*)\.(?:720p|1080p)\..*\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "ext"}
},
new Pattern()
{   
	// 23 Volumes/c$/Media/Movies/A Journey to Planet Sanity (HD).mkv

	Regex = @".*[\/\\](.*)(?:-.*)\.(\w*)$",
	MediaType = MediaType.Movie,
	Fields = new[] {"title", "ext"}
}
*/
