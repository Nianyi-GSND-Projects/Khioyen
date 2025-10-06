using UnityEngine;

namespace LongLiveKhioyen
{
	[CreateAssetMenu(menuName = "Long Live Khioyen/Building Definition")]
	public class BuildingDefinition : ScriptableObject
	{
		public string typeId;
		public new string name;  // Needs localization.
		public string[] tags;
		public Sprite figure;

		[Header("Geometry")]
		public Bounds bounds;
		public GameObject model;

		[Header("Construction")]
		[Min(0)] public float constructionTime;
		public Economy cost;
	}
}
