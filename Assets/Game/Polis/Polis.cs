using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

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
			SwitchToMode(Mode.Mayor);
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

		readonly List<Building> buildings = new();
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

				var building = new GameObject().AddComponent<Building>();
				building.transform.SetParent(transform, false);
				buildings.Add(building);
				building.definition = definition;
				building.placement = placement;
			}
		}
		#endregion

		#region Mode
		public enum Mode { Mayor, Wander }
		Mode currentMode = Mode.Mayor;
		public Mode CurrentMode => currentMode;

		[Header("Mayor Mode")]
		public InputGroup mayorInput;
		public CinemachineVirtualCamera mayorCamera;

		[Header("Wander Mode")]
		public InputGroup wanderInput;
		public AbstractCharacterController player;
		public CinemachineVirtualCamera wanderCamera;

		public void SwitchMode()
		{
			switch(currentMode)
			{
				case Mode.Mayor:
					SwitchToMode(Mode.Wander);
					break;
				case Mode.Wander:
					SwitchToMode(Mode.Mayor);
					break;
			}
		}

		public void SwitchToMode(Mode mode)
		{
			if(mode != Mode.Mayor)
				SetMayorMode(false);
			if(mode != Mode.Wander)
				SetWanderMode(false);

			if(mode == Mode.Mayor)
				SetMayorMode(true);
			if(mode == Mode.Wander)
				SetWanderMode(true);

			currentMode = mode;
		}

		void SetMayorMode(bool enabled)
		{
			mayorCamera.enabled = enabled;
		}

		void SetWanderMode(bool enabled)
		{
			wanderCamera.enabled = enabled;
			player.gameObject.SetActive(enabled);
		}
		#endregion
	}
}
