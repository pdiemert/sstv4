using System;
using System.Collections.Generic;

namespace SSTV4
{
	public class Configuration
	{
		public Configuration()
		{
		}

		public string MediaDirs = @"\\192.168.0.110\vast\downloads";
		public int Port = 8881;
		public bool Fullscreen = false;

		public Dictionary<string, bool> MediaExtensions = new Dictionary<string, bool>()
			{ {"3GP", true}, {"3IV", true}, {"ASF", true}, {"AVI", true}, {"CPK", true}, {"DAT", true}, {"DIVX", true}, {"DV", true}, {"FLAC", true}, {"FLI", true}, {"FLV", true}, {"H264", true}, {"I263", true}, {"M2TS", true}, {"M4V", true}, {"MKV", true}, {"MOV", true}, {"MP2", true}, {"MP4", true}, {"MPEG", true}, {"MPG", true}, {"MPG2", true}, {"MPG4", true}, {"NSV", true}, {"NUT", true}, {"NUV", true}, {"OGG", true}, {"OGM", true}, {"QT", true}, {"RM", true}, {"RMVB", true}, {"VCD", true}, {"VFW", true}, {"VOB", true}, {"WEBM", true}, {"WMV", true} };
		public Dictionary<string, bool> KeeperExtensions = new Dictionary<string, bool>()
			{ {"SRT", true}, {"IDX", true}, {"SUB", true} };
		public Dictionary<string, bool> IgnoreExtensions = new Dictionary<string, bool>()
			{ {"DS_STORE", true} };

	}
}
