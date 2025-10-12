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
		GameData data;
		GameData Data => data;

		void Awake()
		{
			data = GameInstance.Instance?.Data;
			if(data == null)
			{
				Debug.LogWarning("Cannot initialize polis, no game currently running.");
				return;
			}
			initialized = true;

			Construct();
			// Positions the player army to the front door of last polis.
			var lastPolis = polisMiniatures.Find(m => m.data.id == Data.lastPoleis);
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
			foreach(var polisData in Data.poleis)
			{
				var model = Instantiate(Resources.Load<GameObject>("World Map/Dummy Polis Miniature"));
				var polisMiniature = model.GetComponent<PolisMiniature>();
				polisMiniature.data = polisData;
				polisMiniatures.Add(polisMiniature);

				model.transform.SetParent(transform, false);
				model.transform.localPosition = new Vector3(polisData.position.x, 0, polisData.position.y) * data.world.data3D.scale;
				model.transform.localEulerAngles = Vector3.up * polisData.orientation;
			}
		}
		#endregion
	}
}
