using Cinemachine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Device;

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
			IsInConstructModal = false;
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
			SpawnBuildingsFromGameData();
		}

		public bool RayToGround(Ray ray, out Vector3 ground)
		{
			var plane = new Plane(Vector3.up, Vector3.zero);
			if(!plane.Raycast(ray, out float t))
			{
				ground = default;
				return false;
			}
			ground = ray.GetPoint(t);
			return true;
		}

		public bool ScreenToGround(Vector3 screen, out Vector3 ground)
		{
			var ray = Camera.main.ScreenPointToRay(screen);
			return RayToGround(ray, out ground);
		}

		#region Building
		readonly List<Building> buildings = new();

		Building currentSelection;
		public Building SelectedBuilding
		{
			get => currentSelection;
			set
			{
				// 解除上一个的高亮
				if(currentSelection != null)
					currentSelection.Selected = false;

				currentSelection = value;

				// 新的高亮 & UI
				if(currentSelection != null)
				{
					currentSelection.Selected = true;
					// TODO: 打开信息面板 / 属性编辑器
				}
				else
				{
					// TODO: 关闭信息面板
				}
			}
		}

		void SpawnBuildingsFromGameData()
		{
			foreach(var placement in Controlled.buildings)
				SpawnBuilding(placement);
		}

		public void SpawnBuilding(ControlledPolisData.BuildingPlacement placement)
		{
			if(!GameManager.FindBuildingDefinitionByType(placement.type, out var definition))
			{
				Debug.LogWarning($"Skipping spawning building of ID \"{placement.type}\", cannot find its definition.");
				return;
			}

			var building = new GameObject().AddComponent<Building>();
			PositionBuilding(building.transform, definition.bounds, placement.position, placement.orientation);
			buildings.Add(building);
			building.definition = definition;
			building.placement = placement;
		}

		public void ConstructBuilding(string type, Vector2Int gridPosition, int orientation)
		{
			if(!GameManager.FindBuildingDefinitionByType(type, out var definition))
				return;

			ControlledPolisData.BuildingPlacement placement = new()
			{
				type = type,
				position = gridPosition,
				orientation = orientation,
				underConstruction = definition.constructionTime > 0,
				remainingConstructionTime = definition.constructionTime,
			};
			SpawnBuilding(placement);
		}

		public void PositionBuilding(Transform building, Bounds bounds, Vector2Int gridPosition, int orientation)
		{
			building.SetParent(transform, false);
			building.localPosition = grid.CellToLocalInterpolated(
				(Vector3Int)gridPosition - bounds.center + Vector3.one
			);
			building.localEulerAngles = Vector3.up * (orientation * 90);
		}
		#endregion
		#endregion

		#region Mode
		public enum Mode { Mayor, Wander }
		Mode currentMode = Mode.Mayor;
		public Mode CurrentMode => currentMode;

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

		public ConstructModal constructModal;
		public bool IsInConstructModal
		{
			get => constructModal.gameObject.activeInHierarchy;
			set
			{
				if(CurrentMode != Mode.Mayor)
					value = false;
				constructModal.gameObject.SetActive(value);
			}
		}

		#region Mayor mode
		[Header("Mayor Mode")]
		public InputGroup mayorInput;
		public CinemachineVirtualCamera mayorCamera;

		void SetMayorMode(bool enabled)
		{
			mayorCamera.enabled = enabled;
			if(enabled)
				mayorCamera.LookAt.position = Vector3.ProjectOnPlane(player.transform.position, Vector3.up);
		}
		#endregion

		#region Wander mode
		[Header("Wander Mode")]
		public InputGroup wanderInput;
		public AbstractCharacterController player;
		public CinemachineVirtualCamera wanderCamera;

		void SetWanderMode(bool enabled)
		{
			wanderCamera.enabled = enabled;
			player.gameObject.SetActive(enabled);
		}
		#endregion
		#endregion
	}
}
