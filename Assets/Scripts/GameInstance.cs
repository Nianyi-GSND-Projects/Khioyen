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
			if(instance != null && instance != this)
			{
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

		#region Life cycle
		bool initialized;
		System.Action onInitialized;
		void Start()
		{
			Initialize();
			onInitialized?.Invoke();
		}

		public void ExecuteWhenInitialized(System.Action action)
		{
			if(initialized)
				action?.Invoke();
			else
				onInitialized += action;
		}
		#endregion

		#region Serialization/deserialization
		public string LastPolis { get; private set; }

		void Initialize()
		{
			LastPolis = GameManager.LoadedGameData.lastPolis;
			initialized = true;
		}

		Savegame MakeSavegame()
		{
			GameData data = Utilities.DeepCopy(GameManager.LoadedGameData);
			data.lastPolis = LastPolis;
			return new()
			{
				name = "Khioyen - Test name",
				lastUpdatedTime = System.DateTime.Now,
				version = "0.0.0",
				data = data,
			};
		}

		public void SaveTo(string filename)
		{
			GameManager.WriteSavegame(filename, MakeSavegame());
		}
		#endregion

		#region Scene transition
		public enum Mode { Polis, WorldMap, Battle }
		Mode currentMode = Mode.Polis;
		public Mode CurrentMode
		{
			get => currentMode;
			private set
			{
				switch(currentMode = value)
				{
					case Mode.WorldMap:
						GameManager.SwitchScene("World Map");
						break;
					case Mode.Polis:
						GameManager.SwitchScene("Polis");
						break;
				}
			}
		}

		public void DepartFromPolis()
		{
			var polis = Polis.Instance;
			Debug.Log($"Departing from polis \"{polis.Id}\".");

			var data = GameManager.LoadedGameData.poleis.Find(d => d.id == polis.Id);
			polis.WriteData(data);

			CurrentMode = Mode.WorldMap;
		}

		public void EnterPolis(string polisId)
		{
			var polis = GameManager.LoadedGameData.poleis.Find(p => p.id == polisId);
			if(polis == null)
			{
				Debug.LogWarning($"Cannot enter polis \"{polisId}\", failed to find.");
				return;
			}
			Debug.Log($"Entering polis \"{polis.id}\".");

			LastPolis = polis.id;
			CurrentMode = Mode.Polis;
		}
		#endregion
	}
}
