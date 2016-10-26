using System;
namespace SSTV4.DataContracts
{
	public class Media
	{
		public Media()
		{
		}

		public int Id { get; set; }
		public string Path { get; set; }
		public string Title { get; set; }
		public long Size { get; set; }
		public MediaType Type { get; set; }
		public string Ext { get; set; }
		public string Series { get; set; }
		public int Season { get; set; }
		public int Episode { get; set; }
		public int AirYear { get; set; }
		public int AirMonth { get; set; }
		public int AirDay { get; set; }
		public int Year { get; set; }
		public int ParseIdx { get; set; }
		public DateTime DateAdded { get; set; }

		private string m_key;

		public string Key
		{
			get
			{
				if (m_key == null)
				{
					string key;

					switch (Type)
					{
						case MediaType.Movie:
							key = Title + ":" + Year.ToString();
							break;
						case MediaType.TV:
							if (Season == 0)
								key = Series + ":" + AirYear.ToString() + "-" + AirMonth.ToString() + "-" + AirDay.ToString();
							else
								key = Series + ":S" + Season.ToString() + "E" + Episode.ToString();
							break;
						default:
							throw new Exception("Unexpected media type");
					}

					m_key = key;
				}

				return m_key;
			}
		}

		public override string ToString()
		{
			return string.Format("[Media: Path={0}, Title={1}, MediaType={2}, Ext={3}, Series={4}, Season={5}, Episode={6}, AirYear={7}, AirMonth={8}, AirDay={9}, Year={10}]", Path, Title, Type, Ext, Series, Season, Episode, AirYear, AirMonth, AirDay, Year);
		}

	}

	public enum MediaType
	{
		TV,
		Movie
	}
}
