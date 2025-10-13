using System;

namespace LongLiveKhioyen
{
	[Serializable]
	public class Savegame
	{
		public DateTime lastUpdatedTime;
		public GameData data;
	}
}
