using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V20;

namespace LongLiveKhioyen
{
	public enum Stage
	{
		Preparation,
		Arrangement,
		Battle,
		Settlement
	}
	public class Battle : MonoBehaviour
	{
		AudioSource audioSource;
		static Battle instance;
		public static Battle Instance => instance;
		public System.Action onInitialized;
		public Stage currentStage;
		#region Life cycle
		void Awake()
		{
			instance = this;
			audioSource = GetComponent<AudioSource>();
		}

		void OnDestroy()
		{
			instance = null;
		}

		void Start()
		{
			GameInstance.Instance.ExecuteWhenInitialized(Initialize);
		}
		void Initialize()
		{
			ChangeStage(Stage.Preparation);
			transform.rotation = Quaternion.Euler(0, 0, 0);
			gameObject.isStatic = true;
			
			BattleMesh = ConstructBattleMesh();
			arrangementOccupancy = new Battalion[Size.x, Size.y];
			
			//TODO:从出征队伍列表中读取部队
			AnchorPosition = MapToWorld(new Vector2Int(data.battleSize.x/2, data.battleSize.y/2));
			BattleTest();
			Map.GetComponent<MeshCollider>().sharedMesh = BattleMesh;
			Map.GetComponent<MeshFilter>().sharedMesh = BattleMesh;
			onInitialized?.Invoke();
		}

		
		#endregion
		
		#region Test

		public void BattleTest()
		{
			ReserveTeam testTeam = CreateDefaultReserveTeam();
			if(testTeam == null) 
				Debug.LogError("Create default reserve team failed.");
			else 
				data.playerReserveTeams.Add(testTeam);
			//向战斗预备队中加入默认部队
		}
		#endregion
		
		#region Stages
		
		public bool isInArrangementModal = false;
		public bool isInBattleModal = false;
		public bool isReserveTeamSelected = false;
		public bool isBattalionSelected = false;
		public void ChangeStage(Stage stage)
		{
			OnExitStage(currentStage);
			currentStage = stage;
			OnEnterStage(currentStage);
		}

		
		
		void OnEnterStage(Stage stage)
		{
			switch (stage)
			{
				case Stage.Arrangement:
					Debug.Log("OnEnter: 布置阶段");
					break;
				case Stage.Battle:
					Debug.Log("OnEnter: 战斗阶段");
					break;
				case Stage.Settlement:
					Debug.Log("OnEnter: 结算阶段");
					break;
			}
		}
		
		void OnExitStage(Stage stage)
		{
			switch (stage)
			{
				case Stage.Arrangement:
					Debug.Log("OnExit: 布置阶段");
					break;
				case Stage.Battle:
					Debug.Log("OnExit: 战斗阶段");
					break;
			}
		}
		
		#region Preparation
		
		
		
		#endregion
		
		#region Arrangement
		
		public ArrangementModal arrangementModal;
		public void PlacingBattalion(ReserveTeam reserveTeam, Vector2Int mapPosition)
		{
			if (!data.playerReserveTeams.Contains(reserveTeam))
			{
				Debug.Log("Battalion name: " + reserveTeam.battalionDefinition.battalionId + "Don't exist in your reserve teams.");
				return;
			}

			if (reserveTeam.placed)
			{
				Debug.Log("Battalion name: " + reserveTeam.battalionDefinition.battalionId + "already placed.");
				return;
			}

			BattalionCompilation compilation = new()
			{
				//TODO:填入有意义数据
				battalionId = 0,
				position = mapPosition,
				battalionDefinition = reserveTeam.battalionDefinition,
				battalionCommander = reserveTeam.battalionCommander,
				currentSoliders = reserveTeam.currentSoliders,
				currentMurale = reserveTeam.currentMurale,
				currentTraining = reserveTeam.currentTraining,
			};
			
			SpawnBattalion(compilation);
			data.PlayerBattalions.Add(compilation);
			reserveTeam.placed = true;
			ClearReserveTeamSelection();
		}
		
		public void MovingBattalionArrangement(Vector2Int mapPosition)
		{
			if (!isBattalionSelected)
			{
				Debug.Log("No battalion selected.");
				return;
			}
			BattalionCompilation compilation = SelectedBattalion.Compilation;
			arrangementOccupancy[compilation.position.x, compilation.position.y] = null;
			compilation.position = mapPosition;
			SelectedBattalion.transform.localPosition = MapToLocal(compilation.position) - new Vector3(0, 0, 0.5f);
			arrangementOccupancy[compilation.position.x, compilation.position.y] = SelectedBattalion;
			
			ClearBattalionSelection();
		}

		public void ClearReserveTeamSelection()
		{
			SelectedReserveTeam = null;
			isReserveTeamSelected = false;
		}
		
		public void ClearBattalionSelection()
		{
			SelectedBattalion = null;
			isBattalionSelected = false;
		}
		
		#endregion
		
		#region Battle
		
		
		#endregion
		
		#region Settlement

		
		#endregion
		
		#endregion
		
		#region Data
		#region Battle data

		// BattleData data => GameInstance.Instance.LastBattle;
		public BattleData data = new();
		public Vector2Int Size => data.battleSize;
		public string Id => data.id;
		#endregion
		
		#region Game data
		/// <summary>
		/// 会被 <c>GameInstance</c> 在退出战役时调用；
		/// 返回值会被应用到实际游戏数据上。
		/// </summary>
		public BattleResult YieldResult()
		{
			//结算战役。
			BattleResult result = new BattleResult();
			result.CollectLoot(data);
			return result;
		}
		#endregion
		#endregion
		
		#region Map Generation

		Mesh BattleMesh;
		public GameObject Map;
		public Grid hexgrid;
		public float Xscale;
		public float Yscale;
		public float SizeScale;
		Mesh ConstructBattleMesh()
		{
			if (hexgrid == null)
			{
				Debug.LogError("Hex Grid component is not assigned!");
				return null;
			}
			Mesh mesh = new(){name = $"Battle mesh ({Id})"};
			Dictionary<Vector3, int> vertexIndexMap = new Dictionary<Vector3, int>();
			List<Vector3> vertices = new List<Vector3>();
			List<int> indices = new List<int>();
			float hexWidth = hexgrid.cellSize.x ;
			float hexHeight = hexgrid.cellSize.y;
			float hexOuterRadius = hexHeight / 2f * SizeScale; 
			
			for (int y = -1; y < Size.y+1; y++)
			{
				for (int x = -1; x < Size.x+1; x++)
				{
					Vector2Int cellCoord = new Vector2Int(x, y);

					 //Vector3 hexCenter = hexgrid.CellToWorld((Vector3Int)cellCoord) + new Vector3(-Size.x / 2.0f *Xscale,0,-Size.y / 2.0f *Yscale);
					Vector3 hexCenter = hexgrid.CellToWorld((Vector3Int)cellCoord);
					Vector3[] corners = new Vector3[6];
					for (int i = 0; i < 6; i++)
					{
						float angleDeg = 60 * i + 30; 
						float angleRad = Mathf.Deg2Rad * angleDeg;
						corners[i] = new Vector3(
							hexCenter.x + hexOuterRadius * Mathf.Cos(angleRad),
							0, 
							hexCenter.z + hexOuterRadius * Mathf.Sin(angleRad)
						);
					}

					int centerIndex = GetOrAddVertex(hexCenter, vertexIndexMap, vertices);
					for (int i = 0; i < 6; i++)
					{
						int p1_index = GetOrAddVertex(corners[i], vertexIndexMap, vertices);
						int p2_index = GetOrAddVertex(corners[(i + 1) % 6], vertexIndexMap, vertices);
						indices.Add(centerIndex);
						indices.Add(p2_index);
						indices.Add(p1_index);
					}
				}
			}
			mesh.vertices = vertices.ToArray();
			mesh.SetIndices(indices, MeshTopology.Triangles, 0);
			
			Vector2[] uvs = new Vector2[vertices.Count];
			for (int i = 0; i < vertices.Count; i++)
			{
				uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
			}
			mesh.uv = uvs;
			
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			return mesh;
		}
		
		private int GetOrAddVertex(Vector3 vertex, Dictionary<Vector3, int> map, List<Vector3> list)
		{
			if (map.TryGetValue(vertex, out int index))
			{
				return index; 
			}
			else
			{
				int newIndex = list.Count;
				map[vertex] = newIndex;
				list.Add(vertex);
				return newIndex; 
			}
		}
		
		#endregion
		
		#region Control mode
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

		[SerializeField] CinemachineVirtualCamera camera;
		public float CameraDistance
		{
			get => -camera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.z;
			set
			{
				var composer = camera.GetCinemachineComponent<CinemachineTransposer>();
				var offset = composer.m_FollowOffset;
				offset.z = -value;
				composer.m_FollowOffset = offset;
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
		#endregion
		
		#region Grid
		public Vector2 WorldToMap(Vector3 world)
		{
			Vector3Int gridPos = hexgrid.WorldToCell(world);
			return new(
				gridPos.x ,
				gridPos.y 
			);
		}
		public Vector2Int WorldToMapInt(Vector3 world)
		{
			//return Vector2Int.FloorToInt(WorldToMap(world));
			Vector3Int gridPos = hexgrid.WorldToCell(world);
			return new Vector2Int(gridPos.x, gridPos.y);
		}
		public Vector3 MapToWorld(Vector2Int map)
		{
			//return transform.localToWorldMatrix.MultiplyPoint(MapToLocal(map));
			Vector3Int gridPos = new Vector3Int(map.x, map.y, 0);
			return hexgrid.GetCellCenterWorld(gridPos);
		}
		public Vector3 MapToLocal(Vector2 map)
		{
			return hexgrid.CellToLocalInterpolated(new(
				map.x,
				map.y,
				0
			));
		}

		public bool IsValidMapPosition(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < Size.x && pos.y < Size.y;
		}
		
		public bool ValidateArrangementPlacement(Vector2Int placement)
		{
			
				if(!IsValidMapPosition(placement))
					return false;
				if(arrangementOccupancy[placement.x, placement.y] != null)
					return false;
			
			return true;
		}
		
		#endregion
		
		#region Battalions
		
		Battalion[,] arrangementOccupancy;
		readonly List<Battalion> battalions = new();
		public System.Action onArrangementOccupancyChanged;
		public ReserveTeam currentReserveTeam;
		Battalion currentBattalion;
		public BattalionDefinition defaultReserveTeamDefinition;
		public ReserveTeam CreateDefaultReserveTeam()
		{
			ReserveTeam newTeam = new ReserveTeam();

			newTeam.battalionDefinition = defaultReserveTeamDefinition;
			newTeam.battalionCommander = new ();
			newTeam.currentSoliders = newTeam.battalionDefinition.defaultMaxSolider;
			newTeam.currentMurale = newTeam.battalionDefinition.defaultMaxMorale;
			newTeam.currentTraining = 100;
			return newTeam;
		}
		
		public Battalion SelectedBattalion
		{
			get => currentBattalion;
			set
			{
				if (currentBattalion != null)
					currentBattalion.Selected = false;
				
				currentBattalion = value;

				if (currentBattalion != null)
				{
					currentBattalion.Selected = true;
					//TODO: 打开行动面板
				}
			}
		}

		public ReserveTeam SelectedReserveTeam
		{
			get => currentReserveTeam;
			set
			{
				if (value == currentReserveTeam)
					return;

				// if(preview != null)
				// {
				// 	Destroy(preview.gameObject);
				// 	preview = null;
				// }

				currentReserveTeam = value;

				if (currentReserveTeam != null)
				{
					// preview = new GameObject("Construction Preview").AddComponent<ConstructPreview>();
					// preview.Definition = selectedBuildingType;
					// preview.transform.SetParent(Polis.transform, false);
					// preview.onInitialized += UpdatePreviewModel;
				}
			}
		}

		Battalion SpawnBattalion(BattalionCompilation compilation)
		{
			
			var battalion = new GameObject().AddComponent<Battalion>();
			PositionBattalion(battalion.transform, compilation.battalionDefinition,compilation);
			audioSource.PlayOneShot(compilation.battalionDefinition.SelectedSoundEffect);
			battalions.Add(battalion);
			battalion.Compilation = compilation;
			battalion.Definition = compilation.battalionDefinition;
			arrangementOccupancy[compilation.position.x, compilation.position.y] = battalion;
			return battalion;
		}

		public void PositionBattalion(Transform battalion, BattalionDefinition definition, BattalionCompilation compilation)
		{
			battalion.SetParent(transform, false);
			battalion.localPosition = MapToLocal(compilation.position) - new Vector3(0, 0, 0.5f);
		}
		#endregion
		
		#region Functions
		
		public void ProceedToNextStage()
		{
			switch(currentStage)
			{
				case Stage.Preparation:
					ChangeStage(Stage.Arrangement);
					break;
				case Stage.Arrangement:
					ChangeStage(Stage.Battle);
					break;
				case Stage.Battle:
					ChangeStage(Stage.Settlement);
					break;
				default:
					break;
			}
		}
		
		
		public void ExitBattle()
		{
			GameInstance.Instance.ExitBattle();
		}
		
		
		#endregion
	}
}
