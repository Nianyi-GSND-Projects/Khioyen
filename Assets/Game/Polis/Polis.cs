using UnityEngine;

namespace LongLiveKhioyen
{
	public class Polis : MonoBehaviour
	{
		#region Life cycle
		bool initialized = false;
		public bool Initialized => initialized;
		PolisData data;
		public PolisData Data => data;
		public ControlledPolisData Controlled => data?.controlledData;

		void Awake()
		{
			var gameData = GameInstance.Instance?.Data;
			if(gameData == null)
			{
				Debug.LogWarning("Cannot initialize polis, no game currently running.");
				return;
			}
			data = gameData.poleis.Find(p => p.id == gameData.lastPoleis);
			if(data == null)
			{
				Debug.LogWarning("Cannot initialize polis, failed to find the last polis by ID.");
				return;
			}
			if(!data.isControlled || data.controlledData == null)
			{
				Debug.LogWarning($"Cannot initialize polis \"{data.name}\", because it is not controlled.");
				return;
			}
			name = $"{data.name} (Polis)";
			initialized = true;

			Construct();
		}
		#endregion

		#region Construction
		public GameObject ground;
		public Grid grid;

		void Construct()
		{
			// TODO: Procedural polis generation.

			/* Orientation */
			transform.rotation = Quaternion.Euler(0, Data.orientation, 0);
			gameObject.isStatic = true;

			/* Ground */
			ground.transform.localScale = new Vector3(Controlled.size.x, Controlled.size.y, 1);
			ground.isStatic = true;

			/* Buildings */
			SpawnBuildings();
		}

		void SpawnBuildings()
		{
			foreach(var placement in Controlled.buildings)
			{
				var definition = GameManager.BuildingDefinitions.Find(d => d.id == placement.type);
				if(definition == null)
				{
					Debug.LogWarning($"Skipping spawning building of ID \"{placement.type}\", cannot find its definition.");
					continue;
				}
				var model = Resources.Load<GameObject>(definition.modelAddress);
				if(model == null)
				{
					Debug.LogWarning($"Skipping spawning building of ID \"{placement.type}\", cannot find its model at {definition.modelAddress}.");
					continue;
				}

				var building = Instantiate(model);
				building.transform.SetParent(transform, false);
				building.transform.localPosition = grid.CellToLocalInterpolated(
					(Vector3Int)placement.position - definition.bounds.center + Vector3.one
				);
				building.transform.localEulerAngles = Vector3.up * (placement.orientation * 90);
			}
		}
		#endregion
	}
}
