using UnityEngine;

namespace LongLiveKhioyen
{
	public class PolisMiniature : MonoBehaviour
	{
		public PolisData data;

		void Start()
		{
			name = data.name;
		}
	}
}
