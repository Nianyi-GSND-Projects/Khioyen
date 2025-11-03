using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LongLiveKhioyen
{
	public enum BattleType
	{
		Seige,
		Defend,
		Encounter
	}
	[Serializable]
	public class BattleData
	{
		public string id;
		public string name;
		public Vector2 position;
		public BattleType battleType;

		public Vector2Int battleSize;
		[Range(0, 359)] public float encounterOrientation;
		public List<BattalionCompilation> PlayerBattalions;
		public List<BattalionCompilation> EnemyBattalions;
		public List<BattalionCompilation> FriendlyBattalions;

		public string playerCommanderId;

		public BattleData()
		{
			battleType = BattleType.Encounter;
			battleSize = new Vector2Int(10, 10);
			PlayerBattalions = new ();
			PlayerBattalions.Add(new BattalionCompilation());
			EnemyBattalions = new();
			EnemyBattalions.Add(new BattalionCompilation());
		}
	}
	
	public class BattleResult
	{
		public List<string> Loot;

		public void CollectLoot(BattleData battleData)
		{
			Loot.Clear();
			foreach (var BattalionCompilation in battleData.PlayerBattalions)
			foreach (var Item in BattalionCompilation.currentInventory)
			{
				Loot.Add(Item);
			}
		}
	}

	public class BattalionCompilation
	{
		public string battalionId;
		public string battalionCommanderId;
		public int currentSolider;
		public int currentMorale;
		public int currentTraining;

		public List<string> currentInventory;
		//TODO：道具堆叠的定义
		
		public BattalionCompilation()
		{
			battalionId = "sword";
			battalionCommanderId = "null";
			currentSolider = 500;
			currentMorale = 100;
			currentTraining = 100;
			currentInventory = new List<string>();
		}
	}
}
