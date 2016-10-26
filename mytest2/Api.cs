using System;
using System.Collections.Generic;
using System.Linq;
using Griffin.Net.Protocols.Http;
using SSTV4.DataContracts;
using SSTV4.Interfaces;

namespace SSTV4
{
	public class Api : IApi
	{
		private static IApi sm_Api;

		public Api()
		{

		}

		public static IApi Instance
		{
			get
			{
				if (sm_Api == null)
				{
					sm_Api = new Api();
				}

				return sm_Api;
			}
		}

		public object Execute(IParameterCollection form)
		{
			switch (form["method"])
			{
				case "queryMovies":
					IEnumerable<Media> movies = null;
					switch (form["params[sort]"])
					{
						case "RecentlyAdded":
							movies = Db.Instance.ListMovies(MovieSortOption.RecentlyAdded);
							break;
						case "ByName":
							movies = Db.Instance.ListMovies(MovieSortOption.ByName);
							break;
						default:
							throw new Exception("Unexpected sort type");
					}

					return movies.ToArray();

				case "queryTV":
					IEnumerable<Media> shows = null;
					switch (form["params[sort]"])
					{
						case "RecentlyAdded":
							shows = Db.Instance.ListTV(MovieSortOption.RecentlyAdded);
							break;
						case "ByName":
							shows = Db.Instance.ListTV(MovieSortOption.ByName);
							break;
						default:
							throw new Exception("Unexpected sort type");
					}

					return shows.ToArray();
					
				case "playerStart":
					var media = Db.Instance.GetItemById(int.Parse(form["params[id]"]));
					Player.Instance.Start(media);
					break;
					
				case "playerCommand":
					Player.Instance.Command((PlayerCommand)Enum.Parse(typeof(PlayerCommand), form["params[command]"]));
					break;
					
				case "playerStatus":
					return new Dictionary<string, object>() { { "state", Player.Instance.CurrentState.ToString() }, { "playing", Player.Instance.CurrentlyPlaying } };

				case "playerKill":
					Player.Instance.Kill();
					break;
				case "refresh":
					MainClass.Refresh();
					break;
					
				default:
					throw new Exception("Unexpected api method");
			}

			return null;
		}
	}
}
