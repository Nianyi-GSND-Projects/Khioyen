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
		void Awake()
		{
			Construct();
			// Positions the player army to the front door of last polis.
			var lastPolis = polisMiniatures.Find(pm => pm.data.id == GameInstance.Instance.LastPolis.id);
			playerArmy.Controller.Teleport(lastPolis.transform.position + lastPolis.transform.forward * departureDistance);
			playerArmy.Controller.FaceTowards(lastPolis.transform.forward);
		}
		#endregion

		#region Construction
		void Construct()
		{
			/* Poleis */
			foreach(var polisData in GameInstance.Instance.Data.poleis)
			{
				var polisMiniature = SpawnPolisMiniature(polisData);
				polisMiniatures.Add(polisMiniature);
			}
		}

		static GameObject controlledPolisMiniatureTemplate, hostilePolisMiniatureTemplate;

		PolisMiniature SpawnPolisMiniature(PolisData polisData)
		{
			if(!controlledPolisMiniatureTemplate)
				controlledPolisMiniatureTemplate = Resources.Load<GameObject>("Prefabs/World Map/Polis_miniature-controlled");
			if(!hostilePolisMiniatureTemplate)
				hostilePolisMiniatureTemplate = Resources.Load<GameObject>("Prefabs/World Map/Polis_miniature-hostile");

			GameObject go;
			if(polisData.isControlled)
				go = Instantiate(controlledPolisMiniatureTemplate);
			else if(polisData.isHostile)
				go = Instantiate(hostilePolisMiniatureTemplate);
			else throw new System.NotSupportedException();

			go.transform.SetParent(transform, false);
			go.transform.localPosition = new Vector3(polisData.position.x, 0, polisData.position.y) * GameInstance.Instance.Data.world.data3D.scale;
			go.transform.localEulerAngles = Vector3.up * polisData.orientation;

			var pm = go.GetComponent<PolisMiniature>();
			pm.data = polisData;
			return pm;
		}
		#endregion
	}
}
