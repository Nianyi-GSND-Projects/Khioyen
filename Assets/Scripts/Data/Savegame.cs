using System;

namespace LongLiveKhioyen
{
	[Serializable]
	public class Savegame
	{
		public string name;
		public DateTime creationTime;
		public string version;
		public GameData data;
	}
}
