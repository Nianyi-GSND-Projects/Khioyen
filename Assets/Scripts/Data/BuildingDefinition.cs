using UnityEngine;

namespace LongLiveKhioyen
{
	[CreateAssetMenu(menuName = "Long Live Khioyen/Building Definition")]
	public class BuildingDefinition : ScriptableObject
	{
		public string id;
		public string[] tags;
		public Sprite figure;

		[Header("Geometry")]
		[Range(0, 3)] public int defaultOrientation;
		public Vector3 pivot;
		public bool obstructive;
		public Vector3 center;
		public Vector3 size;

		public GameObject ModelTemplate => Resources.Load<GameObject>($"Models/Buildings/{id}");

		[Header("Construction")]
		[Min(0)] public float constructionTime;
		public Economy cost;
	}
}
