using UnityEngine;
using UnityEngine.SceneManagement;

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

		#region Scene transition
		public void DepartFromPolis()
		{
			Debug.Log($"Departing rom polis \"{Data.lastPoleis}\".");
			// TODO
			SceneManager.LoadScene("World Map");
		}

		public void EnterPolis(string polisId)
		{
			var polis = Data.poleis.Find(p => p.id == polisId);
			if(polis == null)
			{
				Debug.LogWarning($"Cannot enter polis \"{polisId}\", failed to find.");
				return;
			}
			Debug.Log($"Entering polis \"{polis.id}\".");

			// TODO
			Data.lastPoleis = polis.id;

			SceneManager.LoadScene("Polis");
		}
		#endregion
	}
}
