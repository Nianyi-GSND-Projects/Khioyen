using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace LongLiveKhioyen
{
	public class Polis : MonoBehaviour
	{
		static Polis instance;
		public static Polis Instance => instance;

		#region Unity life cycle
		void Awake()
		{
			instance = this;

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
				Debug.LogWarning($"Cannot initialize polis \"{data.id}\", because it is not controlled.");
				return;
			}
			name = $"{data.id} (Polis)";

			player.gameObject.SetActive(false);
		}

		void Start()
		{
			SwitchToMode(Mode.Mayor);
			IsInConstructModal = false;

			// TODO: Procedural polis generation.

			/* Orientation */
			transform.rotation = Quaternion.Euler(0, Data.orientation, 0);
			gameObject.isStatic = true;

			/* Ground */
			groundMesh = ConstructGroundMesh();
			ground.GetComponent<MeshFilter>().sharedMesh = groundMesh;
			ground.GetComponent<MeshCollider>().sharedMesh = groundMesh;

			/* Buildings */
			SpawnBuildingsFromGameData();
		}

		void OnDestroy()
		{
			Destroy(groundMesh);

			instance = null;
		}

		void Update()
		{
			if(isNavMeshDirty)
			{
				isNavMeshDirty = false;
				UpdateGroundNavMesh();
			}
		}
		#endregion

		#region Data
		PolisData data;
		public PolisData Data => data;
		public ControlledPolisData ControlledData => data?.controlledData;

		#region Economy
		public System.Action onEconomyDataChanged;
		IEnumerator EmitOnEconomyDataChangedOnNextFrame()
		{
			yield return new WaitForEndOfFrame();
			onEconomyDataChanged?.Invoke();
		}

		public bool CheckResourceAffordance(Economy cost)
		{
			return cost <= ControlledData.economy;
		}

		public bool TryCostResource(Economy cost, bool actuallyCost = true)
		{
			if(!CheckResourceAffordance(cost))
				return false;
			if(actuallyCost)
			{
				ControlledData.economy = ControlledData.economy - cost;
				StartCoroutine(nameof(EmitOnEconomyDataChangedOnNextFrame));
			}
			return true;
		}
		#endregion
		#endregion

		#region Construction
		[Header("Construction")]
		public GameObject ground;
		public Grid grid;
		Mesh groundMesh;
		public NavMeshSurface navMeshSurface;

		#region Ground
		Mesh ConstructGroundMesh()
		{
			Mesh mesh = new() { name = $"Ground mesh ({Data.id})" };
			Dictionary<(int, int), (int, Vector2)> vertices = new();
			float mx = ControlledData.size.x * -.5f, my = ControlledData.size.y * -.5f;
			for(int x = 0; x <= ControlledData.size.x; ++x)
			{
				for(int y = 0; y <= ControlledData.size.y; ++y)
					vertices[(x, y)] = (vertices.Count, new(x + mx, y + my));
			}
			mesh.vertices = vertices.Values.Select(pair => new Vector3(pair.Item2.x, 0, pair.Item2.y)).ToArray();
			mesh.uv = vertices.Values.Select(pair => pair.Item2).ToArray();
			List<int> indices = new();
			for(int x = 0; x < ControlledData.size.x; ++x)
			{
				for(int y = 0; y < ControlledData.size.y; ++y)
				{
					indices.Add(vertices[(x, y)].Item1);
					indices.Add(vertices[(x, y + 1)].Item1);
					indices.Add(vertices[(x + 1, y)].Item1);
					indices.Add(vertices[(x + 1, y)].Item1);
					indices.Add(vertices[(x, y + 1)].Item1);
					indices.Add(vertices[(x + 1, y + 1)].Item1);
				}
			}
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
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

		bool isNavMeshDirty = true;

		void UpdateGroundNavMesh()
		{
			navMeshSurface.RemoveData();
			navMeshSurface.BuildNavMesh();
		}
		#endregion

		#region Grid
		public Vector2Int GridToMap(Vector3Int grid)
		{
			return new(grid.x, grid.z);
		}
		public Vector3Int MapToGrid(Vector2Int map)
		{
			return new(map.x, 0, map.y);
		}
		#endregion

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
					Debug.Log($"Seleted {currentSelection}.", currentSelection);
				}
				else
				{
					// TODO: 关闭信息面板
				}
			}
		}

		void SpawnBuildingsFromGameData()
		{
			foreach(var placement in ControlledData.buildings)
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
			PositionBuilding(building.transform, definition.pivot, placement.position, placement.orientation);
			buildings.Add(building);
			building.definition = definition;
			building.placement = placement;

			isNavMeshDirty = true;
		}

		public void ConstructBuilding(string type, Vector2Int mapPosition, int orientation)
		{
			if(!GameManager.FindBuildingDefinitionByType(type, out var definition))
				return;

			ControlledPolisData.BuildingPlacement placement = new()
			{
				type = type,
				position = mapPosition,
				orientation = orientation,
				underConstruction = definition.constructionTime > 0,
				remainingConstructionTime = definition.constructionTime,
			};
			SpawnBuilding(placement);
		}

		public void PositionBuilding(Transform building, Vector3 pivot, Vector2Int mapPosition, int orientation)
		{
			building.SetParent(transform, false);
			building.localPosition = grid.CellToLocalInterpolated(
				MapToGrid(mapPosition) - pivot + Vector3.one
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

		public Transform anchor;
		public Vector3 AnchorPosition
		{
			get => anchor.position;
			set => anchor.position = value;
		}
		public Vector3 AnchorEulers
		{
			get => anchor.eulerAngles;
			set => anchor.eulerAngles = value;
		}
		public float MayorDistance
		{
			get => -mayorCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;
			set
			{
				var composer = mayorCamera.GetCinemachineComponent<CinemachineTransposer>();
				var offset = composer.m_FollowOffset;
				offset.z = -value;
				composer.m_FollowOffset = offset;
			}
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
		[SerializeField] CinemachineVirtualCamera mayorCamera;

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
		[SerializeField] CinemachineVirtualCamera wanderCamera;

		void SetWanderMode(bool enabled)
		{
			wanderCamera.enabled = enabled;
			player.gameObject.SetActive(enabled);
			player.GetComponent<NavMeshAgent>().enabled = enabled;
			if(enabled)
			{
				player.Teleport(mayorCamera.LookAt.position);
				player.FaceTowards(Camera.main.transform.forward);
			}
		}
		#endregion
		#endregion
	}
}
