using System;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	[Serializable]
	public class GameData
	{
		public WorldData world;
		public List<PolisData> poleis = new();
		public string lastPolis;
	}

	[Serializable]
	public class WorldData
	{
		[Serializable]
		public struct WorldData3D
		{
			public float scale;
			public string terrainAddress;
			public string skyboxAddress;
		}
		public WorldData3D data3D;

		[Serializable]
		public struct WorldData2D
		{
			public float scale;
			public string mapAddress;
		}
		public WorldData2D data2D;
	}
}
