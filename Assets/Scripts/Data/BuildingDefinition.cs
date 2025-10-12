using UnityEngine;
using NaughtyAttributes;

namespace LongLiveKhioyen
{
	[CreateAssetMenu(menuName = "Long Live Khioyen/Building Definition")]
	public class BuildingDefinition : ScriptableObject
	{
		public string id;
		public string[] tags;
		public Sprite figure;

		[Header("Geometry")]
		public GameObject model;
		[Range(0, 3)] public int defaultOrientation;
		public Vector3 pivot;
		public bool obstructive;
		public Vector3 center;
		public Vector3 size;

		[Header("Construction")]
		[Min(0)] public float constructionTime;
		public Economy cost;
	}
}
