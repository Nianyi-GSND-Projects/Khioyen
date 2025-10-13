using UnityEngine;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class WorldMap : MonoBehaviour
	{
		#region Fields
		readonly List<PolisMiniature> polisMiniatures = new();
		public WorldMapPlayerArmy playerArmy;
		/** <summary>The distance the player army will spawn from last polis.</summary> */
		[Min(0)] public float departureDistance = 10;
		#endregion

		#region Life cycle
		bool initialized = false;
		public bool Initialized => initialized;

		void Awake()
		{
			if(GameManager.LoadedGameData == null)
			{
				Debug.LogWarning("Cannot initialize polis, no game currently running.");
				return;
			}
			initialized = true;

			Construct();
			// Positions the player army to the front door of last polis.
			var lastPolis = polisMiniatures.Find(m => m.data.id == GameManager.LoadedGameData.lastPolis);
			if(lastPolis == null)
			{
				Debug.LogWarning("No last polis found.");
			}
			else
			{
				playerArmy.Controller.Teleport(lastPolis.transform.position + lastPolis.transform.forward * departureDistance);
				playerArmy.Controller.FaceTowards(lastPolis.transform.forward);
			}
		}
		#endregion

		#region Construction
		void Construct()
		{
			/* Poleis */
			foreach(var polisData in GameManager.LoadedGameData.poleis)
			{
				var polisMiniature = SpawnPolisMiniature(polisData);
				polisMiniatures.Add(polisMiniature);
			}
		}

		PolisMiniature SpawnPolisMiniature(PolisData polisData)
		{
			GameObject go = new();
			go.transform.SetParent(transform, false);
			go.transform.localPosition = new Vector3(polisData.position.x, 0, polisData.position.y) * GameManager.LoadedGameData.world.data3D.scale;
			go.transform.localEulerAngles = Vector3.up * polisData.orientation;

			var pm = go.AddComponent<PolisMiniature>();
			pm.data = polisData;
			return pm;
		}
		#endregion
	}
}
