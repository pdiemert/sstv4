using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SSTV4.DataContracts;

namespace SSTV4
{
	public class Db : IDb
	{
		private static IDb sm_db;

		private Dictionary<string, Media> m_media = new Dictionary<string, Media>();
		private int m_pk;

		public Db()
		{
		}

		public static IDb Instance
		{
			get
			{
				if (sm_db == null)
				{
					sm_db = new Db();
				}

				return sm_db;
			}
		}

		public void Clear()
		{
			m_media.Clear();
		}

		public void AddOrUpdateMedia(Media m)
		{
			if (!m_media.ContainsKey(m.Key))
			{
				m.Id = Interlocked.Increment(ref m_pk);

				m_media.Add(m.Key, m);
			}
			else
			{
				m_media[m.Key] = m;
			}
		}

		public IEnumerable<Media> ListMovies(MovieSortOption sort)
		{
			var items = m_media.Where((kvp) => kvp.Value.Type == MediaType.Movie).Select(kvp => kvp.Value);

			switch (sort)
			{
				case MovieSortOption.ByName:
					items = items.OrderBy(m => m.Title);
					break;
				case MovieSortOption.RecentlyAdded:
					items = items.OrderByDescending(m => m.DateAdded);
					break;
			}

			return items;
		}

		public IEnumerable<Media> ListTV(MovieSortOption sort)
		{
			var items = m_media.Where((kvp) => kvp.Value.Type == MediaType.TV).Select(kvp => kvp.Value);

			switch (sort)
			{
				case MovieSortOption.ByName:
					items = items.OrderBy(m => m.Series);
					break;
				case MovieSortOption.RecentlyAdded:
					items = items.OrderByDescending(m => m.DateAdded);
					break;
			}

			return items;
		}

		public Media GetItemById(int id)
		{
			return m_media.SingleOrDefault(kvp => kvp.Value.Id == id).Value;
		}
	}
}
