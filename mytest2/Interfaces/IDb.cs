using System;
using System.Collections.Generic;
using SSTV4.DataContracts;

namespace SSTV4
{
	public interface IDb
	{
		void AddOrUpdateMedia(Media m);
		IEnumerable<Media> ListMovies(MovieSortOption option);
		IEnumerable<Media> ListTV(MovieSortOption sort);
		Media GetItemById(int id);
		void Clear();
	}

	public enum MovieSortOption
	{
		RecentlyAdded,
		ByName
	}
}
