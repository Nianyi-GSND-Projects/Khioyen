using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace LongLiveKhioyen
{
	[Serializable]
	public class GameData
	{
		public WorldData world;
		public List<PolisData> poleis = new();
		public string lastPoleis;
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

	[Serializable]
	public class PolisData
	{
		public string id;
		public string name;  // Needs localization.
		public Vector3 position;
		[Range(0, 359)] public float orientation;

		public bool isControlled;
		public bool isHostile;
		[ShowIf("isControlled")] public ControlledPolisData controlledData;
		[ShowIf("isHostile")] public HostilePolisData hostileData;
	}

	[Serializable]
	public class ControlledPolisData
	{
		/* Geometry */
		public Vector2Int size;

		/* Resources */
		[Serializable]
		public struct Economy
		{
			public float food;
			public float money;
			public float knowledge;
		}
		public Economy economy;

		/* Buildings */
		[Serializable]
		public struct BuildingPlacement
		{
			public string type;  // The building ID stored in the definition sheet.
			public Vector2Int position;
			[Range(0, 3)] public int orientation;  // By 90 degrees.
		}
		public List<BuildingPlacement> buildings;
	}

	[Serializable]
	public class HostilePolisData
	{
	}

	[Serializable]
	public class BuildingDefinitionData
	{
		public string id;
		public string name;  // Needs localization.
		public Bounds bounds;
		public string modelAddress;
	}
}
