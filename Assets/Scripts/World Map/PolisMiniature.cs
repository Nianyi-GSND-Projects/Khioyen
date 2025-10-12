using UnityEngine;

namespace LongLiveKhioyen
{
	public class PolisMiniature : MonoBehaviour
	{
		public PolisData data;

		void Start()
		{
			name = data.id;
			CreateModel();
		}

		void CreateModel()
		{
			var model = Instantiate(Resources.Load<GameObject>("Models/Site Miniatures/Polis"));
			model.transform.SetParent(transform, false);
		}
	}
}
