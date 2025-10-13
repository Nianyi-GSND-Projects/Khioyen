using System;

namespace LongLiveKhioyen
{
	[Serializable]
	public class Savegame
	{
		public string name;
		public DateTime lastUpdatedTime;
		public string version;
		public GameData data;
	}
}
