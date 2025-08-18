using UnityEngine;

namespace LongLiveKhioyen
{
	public class GameInstance : MonoBehaviour
	{
		#region Singleton
		static GameInstance instance;
		public static GameInstance Instance => instance;

		void Awake()
		{
			if(instance != null && instance != this) {
				Destroy(this);
				return;
			}
			instance = this;
			DontDestroyOnLoad(this);
		}

		void OnDestroy()
		{
			Destroy(gameObject);
			if(instance == this)
				instance = null;
		}
		#endregion

		#region Serialization/deserialization
		GameData data;
		public GameData Data
		{
			get => data;
			set
			{
				data = JsonUtility.FromJson<GameData>(JsonUtility.ToJson(value));
			}
		}
		#endregion
	}
}
