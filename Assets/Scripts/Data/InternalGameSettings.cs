using UnityEngine;

namespace LongLiveKhioyen
{
	[CreateAssetMenu(menuName = "Long Live Khioyen/Internal Game Settings")]
	public class InternalGameSettings : ScriptableObject
	{
		[Tooltip("游戏中一逻辑月对应的游戏时间（秒）。")]
		public float monthLength;
	}
}
