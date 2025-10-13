using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;

namespace LongLiveKhioyen
{
	public class Polis : MonoBehaviour
	{
		static Polis instance;
		public static Polis Instance => instance;

		#region Life cycle
		public System.Action onInitialized;

		void Awake()
		{
			instance = this;
		}

		void Start()
		{
			GameInstance.Instance.ExecuteWhenInitialized(Initialize);
		}

		void Initialize()
		{
			Data = GetPolisData();
			if(Data == null)
				player.gameObject.SetActive(false);

			SwitchToMode(Mode.Mayor);
			IsInConstructModal = false;

			/* Procedural polis generation */

			// Orientation
			transform.rotation = Quaternion.Euler(0, Data.orientation, 0);
			gameObject.isStatic = true;

			// Ground
			groundMesh = ConstructGroundMesh();
			ground.GetComponent<MeshFilter>().sharedMesh = groundMesh;
			ground.GetComponent<MeshCollider>().sharedMesh = groundMesh;

			// Walls
			ConstructWalls();

			// Buildings.
			buildingOccupancy = new Building[Size.x, Size.y];
			foreach(var placement in Data?.controlledData.buildings)
				SpawnBuilding(placement);

			// Initialize Navmesh.
			navMeshSurface.RemoveData();
			navMeshSurface.BuildNavMesh();

			onInitialized?.Invoke();
		}

		void OnDestroy()
		{
			Destroy(groundMesh);

			instance = null;
		}
		#endregion

		#region Data
		public PolisData Data { get; private set; }
		PolisData GetPolisData()
		{
			if(GameInstance.Instance.Data == null)
			{
				Debug.LogWarning("Cannot initialize polis, no game currently running.");
				return null;
			}
			var data = GameInstance.Instance.Data.poleis.Find(p => p.id == GameInstance.Instance.Data.lastPolis);
			if(data == null)
			{
				Debug.LogWarning("Cannot initialize polis, failed to find the last polis by ID.");
				return null;
			}
			if(!data.isControlled || data.controlledData == null)
			{
				Debug.LogWarning($"Cannot initialize polis \"{data.id}\", because it is not controlled.");
				return null;
			}
			return data;
		}

		public string Id => Data.id;
		public Vector2Int Size => Data.controlledData.size;
		public Economy Economy
		{
			get => Data.controlledData.economy;
			set => Data.controlledData.economy = value;
		}

		#region Economy
		public System.Action onEconomyDataChanged;

		public bool CheckResourceAffordance(Economy cost)
		{
			return cost <= Economy;
		}

		public bool TryCostResource(Economy cost, bool actuallyCost = true)
		{
			if(!CheckResourceAffordance(cost))
				return false;
			if(actuallyCost)
			{
				Economy = Economy - cost;
				onEconomyDataChanged?.Invoke();
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
			Mesh mesh = new() { name = $"Ground mesh ({Id})" };
			Dictionary<(int, int), (int, Vector2)> vertices = new();
			float mx = Size.x * -.5f, my = Size.y * -.5f;
			for(int x = -1; x <= Size.x + 1; ++x)
			{
				for(int y = -1; y <= Size.y + 1; ++y)
					vertices[(x, y)] = (vertices.Count, new(x + mx, y + my));
			}
			mesh.vertices = vertices.Values.Select(pair => new Vector3(pair.Item2.x, 0, pair.Item2.y)).ToArray();
			mesh.uv = vertices.Values.Select(pair => pair.Item2).ToArray();
			List<int> indices = new();
			for(int x = -1; x < Size.x + 1; ++x)
			{
				for(int y = -1; y < Size.y + 1; ++y)
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

		struct WallConstructionParams
		{
			public int length;
			public Vector3 offset;
			public Vector3 space;
			public int orientation;
		}
		void ConstructWalls()
		{
			var sectionTemplate = Resources.Load<GameObject>("Models/Polis/Wall_section");
			var cornerTemplate = Resources.Load<GameObject>("Models/Polis/Wall_corner");
			var root = new GameObject("Walls").transform;
			root.SetParent(transform, false);
			var ps = new WallConstructionParams[] {
				new() {
					length = Size.x,
					offset = new Vector3(-Size.x + 1, 0, -Size.y - 1) * .5f,
					space = Vector3.right,
					orientation = 0,
				},
				new() {
					length = Size.y,
					offset = new Vector3(+Size.x + 1, 0, -Size.y + 1) * .5f,
					space = Vector3.forward,
					orientation = 3,
				},
				new() {
					length = Size.x,
					offset = new Vector3(+Size.x - 1, 0, +Size.y + 1) * .5f,
					space = Vector3.left,
					orientation = 2,
				},
				new() {
					length = Size.y,
					offset = new Vector3(-Size.x - 1, 0, +Size.y - 1) * .5f,
					space = Vector3.back,
					orientation = 1,
				},
			};
			void MakeWall(GameObject template, Vector3 pos, int orientation)
			{
				var model = Instantiate(template);
				model.transform.SetParent(root, false);
				model.transform.SetLocalPositionAndRotation(
					pos,
					Quaternion.Euler(Vector3.up * (90 * orientation))
				);
				var obstacle = model.AddComponent<NavMeshObstacle>();
				obstacle.carving = true;
				obstacle.size = new Vector3(1, 4, 1);
				obstacle.center = Vector3.up * 2;
			}
			foreach(var p in ps)
			{
				for(int i = 0; i < p.length; ++i)
					MakeWall(sectionTemplate, i * p.space + p.offset, p.orientation);
				MakeWall(cornerTemplate, -1 * p.space + p.offset, p.orientation);
			}
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

		Vector3 ClosestWalkablePosition(Vector3 reference)
		{
			int areaMask = 1 << NavMesh.GetAreaFromName("Walkable");
			NavMeshHit hit;
			if(NavMesh.SamplePosition(reference, out hit, 0.1f, areaMask))
				return hit.position;
			if(NavMesh.SamplePosition(reference, out hit, Size.magnitude, areaMask))
				return hit.position;
			Debug.LogWarning("Failed to find closest walkable position on the NavMesh.");
			return transform.position;
		}
		#endregion

		#region Grid
		public Vector2 WorldToMap(Vector3 world)
		{
			Vector3Int gridPos = grid.WorldToCell(world);
			return new(
				gridPos.x + Size.x * .5f,
				gridPos.z + Size.y * .5f
			);
		}
		public Vector2Int WorldToMapInt(Vector3 world)
		{
			return Vector2Int.FloorToInt(WorldToMap(world));
		}
		public Vector3 MapToWorld(Vector2 map)
		{
			return transform.localToWorldMatrix.MultiplyPoint(MapToLocal(map));
		}
		public Vector3 MapToLocal(Vector2 map)
		{
			return grid.CellToLocalInterpolated(new(
				map.x - Size.x * .5f,
				0,
				map.y - Size.y * .5f
			));
		}

		public bool IsValidMapPosition(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < Size.x && pos.y < Size.y;
		}
		#endregion

		#region Building
		readonly List<Building> buildings = new();
		Building[,] buildingOccupancy;
		public System.Action onBuildingOccupancyChanged;

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

		Building SpawnBuilding(BuildingPlacement placement)
		{
			if(!GameManager.FindBuildingDefinitionByType(placement.id, out var definition))
			{
				Debug.LogWarning($"Skipping spawning building of ID \"{placement.id}\", cannot find its definition.");
				return null;
			}

			var building = new GameObject().AddComponent<Building>();
			PositionBuilding(building.transform, definition, placement);
			buildings.Add(building);
			building.Placement = placement;
			building.Definition = definition;

			foreach(var pos in YieldBuildingOccupancy(definition, placement))
				buildingOccupancy[pos.x, pos.y] = building;

			return building;
		}

		IEnumerable<Vector2Int> YieldBuildingOccupancy(BuildingDefinition definition, BuildingPlacement placement)
		{
			// GPT gen

			// Local helper to rotate a grid vector by k quarter turns around +Y (same as Transform.Rotate(0, k*90, 0)).
			// Mapping follows Unity's left-handed transform convention:
			//   0: (x, y) -> ( x,  y)
			//   1: (x, y) -> ( y, -x)
			//   2: (x, y) -> (-x, -y)
			//   3: (x, y) -> (-y,  x)
			static Vector2Int Rot90(Vector2Int v, int quarterTurns)
			{
				quarterTurns = ((quarterTurns % 4) + 4) % 4; // normalize to {0,1,2,3}
				return quarterTurns switch
				{
					0 => v,
					1 => new Vector2Int(v.y, -v.x),
					2 => new Vector2Int(-v.x, -v.y),
					3 => new Vector2Int(-v.y, v.x),
					_ => v // unreachable
				};
			}

			var size = definition.size;          // rectangle size in cells at orientation=0
			var pivot = definition.pivot;         // rotation pivot in local (orientation=0) cell coords
			var origin = placement.position;       // world/grid coords where the pivot is placed
			int rot = placement.orientation & 3;

			// Enumerate every cell of the footprint (orientation=0 local space),
			// rotate the offset around the pivot, then translate to world/grid space.
			for(int ly = 0; ly < size.y; ly++)
			{
				for(int lx = 0; lx < size.x; lx++)
				{
					// Local cell (lx, ly) relative to pivot
					var deltaLocal = new Vector2Int(lx - pivot.x, ly - pivot.y);

					// Rotate around pivot and translate by 'origin'
					var deltaWorld = Rot90(deltaLocal, rot);
					yield return origin + deltaWorld;
				}
			}
		}

		public bool ValidateBuildingPlacement(BuildingDefinition definition, BuildingPlacement placement)
		{
			foreach(var pos in YieldBuildingOccupancy(definition, placement))
			{
				if(!IsValidMapPosition(pos))
					return false;
				if(buildingOccupancy[pos.x, pos.y] != null)
					return false;
			}
			return true;
		}

		public void PositionBuilding(Transform building, BuildingDefinition definition, BuildingPlacement placement)
		{
			building.SetParent(transform, false);
			Vector2 planar = (Vector2)definition.size - definition.center - definition.pivot;
			building.localPosition = MapToLocal(placement.position) + new Vector3(planar.x, 0, planar.y);
			building.localEulerAngles = Vector3.up * (placement.orientation * 90);
		}

		public void ConstructBuilding(string type, Vector2Int mapPosition, int orientation)
		{
			if(!GameManager.FindBuildingDefinitionByType(type, out var definition))
				return;
			BuildingPlacement placement = new()
			{
				id = type,
				position = mapPosition,
				orientation = orientation,
				underConstruction = definition.constructionTime > 0,
				remainingConstructionTime = definition.constructionTime,
			};
			SpawnBuilding(placement);
			Data.controlledData.buildings.Add(placement);
		}
		#endregion
		#endregion

		#region Control mode
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

		[Header("Control Mode")]
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
		[SerializeField] CinemachineVirtualCamera mayorCamera;

		void SetMayorMode(bool enabled)
		{
			mayorCamera.enabled = enabled;
			if(enabled)
				AnchorPosition = Vector3.ProjectOnPlane(player.transform.position, Vector3.up);
		}
		#endregion

		#region Wander mode
		[SerializeField] AbstractCharacterController player;
		[SerializeField] CinemachineVirtualCamera wanderCamera;

		void SetWanderMode(bool enabled)
		{
			wanderCamera.enabled = enabled;
			player.gameObject.SetActive(enabled);
			player.GetComponent<NavMeshAgent>().enabled = enabled;
			if(enabled)
			{
				AnchorPosition = ClosestWalkablePosition(AnchorPosition);
				player.Teleport(AnchorPosition);
				player.FaceTowards(Camera.main.transform.forward);
			}
		}
		#endregion
		#endregion
	}
}
