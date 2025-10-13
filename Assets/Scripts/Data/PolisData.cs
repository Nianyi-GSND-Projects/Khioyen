using UnityEngine;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LongLiveKhioyen
{
	[Serializable]
	public class PolisData
	{
		public string id;
		public Vector2 position;
		[Range(0, 359)] public float orientation;

		public bool isControlled;
		public bool isHostile;
		[ShowIf("isControlled")] public ControlledPolisData controlledData;
		[ShowIf("isHostile")] public HostilePolisData hostileData;
	}

	[Serializable]
	public struct Economy
	{
		public float food;
		public float money;
		public float knowledge;

		public override string ToString()
		{
			return $"(food:{food}, money:{money}, knowledge:{knowledge})";
		}

		static IEnumerable<(float, float)> ValuePairs(in Economy a, in Economy b)
		{
			return new (float, float)[] {
				(a.food, b.food),
				(a.money, b.money),
				(a.knowledge, b.knowledge),
			};
		}
		static bool Compare(in Economy a, in Economy b, Func<float, float, bool> comparer)
		{
			return ValuePairs(a, b)
				.Select(pair => comparer.Invoke(pair.Item1, pair.Item2))
				.Aggregate((a, b) => a && b);
		}

		public static bool operator <(in Economy a, in Economy b) => Compare(a, b, (a, b) => a < b);
		public static bool operator >(in Economy a, in Economy b) => Compare(a, b, (a, b) => a > b);
		public static bool operator <=(in Economy a, in Economy b) => Compare(a, b, (a, b) => a <= b);
		public static bool operator >=(in Economy a, in Economy b) => Compare(a, b, (a, b) => a >= b);

		public static Economy operator -(in Economy a, in Economy b)
		{
			Economy result = a;
			result.food -= b.food;
			result.money -= b.money;
			result.knowledge -= b.knowledge;
			return result;
		}
	}

	[Serializable]
	public class ControlledPolisData
	{
		/* Geometry */
		public Vector2Int size;

		/* Resources */
		public Economy economy;

		/* Buildings */
		public List<BuildingPlacement> buildings;
	}

	[Serializable]
	public class BuildingPlacement
	{
		public string id;  // The building ID stored in the definition sheet.
		public Vector2Int position;
		[Range(0, 3)] public int orientation;  // By 90 degrees.

		public bool underConstruction;
		public float remainingConstructionTime;
	}

	[Serializable]
	public class HostilePolisData
	{
	}
}