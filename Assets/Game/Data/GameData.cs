using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LongLiveKhioyen
{
	[Serializable]
	public class GameData
	{
		public WorldData world;
		public List<PolisData> poleis = new();
		public string lastPoleis;

		public GameData() { }
		public GameData(GameData source) {
			world = JsonUtility.FromJson<WorldData>(JsonUtility.ToJson(source.world));
			poleis = source.poleis.Select(p => ScriptableObject.Instantiate(p)).ToList();
			lastPoleis = source.lastPoleis;
		}
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
