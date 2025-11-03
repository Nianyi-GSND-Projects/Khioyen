using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;

namespace LongLiveKhioyen
{
	enum Stage
	{
		Preparation,
		Arrangement,
		Battle,
		Settlement
	}
	public class Battle : MonoBehaviour
	{
		static Battle instance;
		public static Battle Instance => instance;
		public System.Action onInitialized;

		#region Life cycle
		void Awake()
		{
			instance = this;
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
			
			transform.rotation = Quaternion.Euler(0, 0, 0);
			gameObject.isStatic = true;
			
			BattleMesh = ConstructBattleMesh();
			Map.GetComponent<MeshCollider>().sharedMesh = BattleMesh;
			Map.GetComponent<MeshFilter>().sharedMesh = BattleMesh;
			
			onInitialized?.Invoke();
		}
		#endregion
		
		#region Data
		#region Battle data

		// BattleData data => GameInstance.Instance.LastBattle;
		BattleData data = new();
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

					Vector3 hexCenter = hexgrid.CellToWorld((Vector3Int)cellCoord) + new Vector3(-Size.x / 2.0f *Xscale,0,-Size.y / 2.0f *Yscale);
					
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
				gridPos.x + Size.x * .5f * Xscale,
				gridPos.z + Size.y * .5f * Yscale
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
			return hexgrid.CellToLocalInterpolated(new(
				map.x - Size.x * .5f * Xscale,
				0,
				map.y - Size.y * .5f * Yscale
			));
		}

		public bool IsValidMapPosition(Vector2Int pos)
		{
			return pos.x >= 0 && pos.y >= 0 && pos.x < Size.x && pos.y < Size.y;
		}
		
		
		#endregion
		
		#region Functions
		/// <summary>给 UI 控件暴露的接口方法。</summary>
		
		
		public void ExitBattle()
		{
			GameInstance.Instance.ExitBattle();
		}
		#endregion
	}
}
