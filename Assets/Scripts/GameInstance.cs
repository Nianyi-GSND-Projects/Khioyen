using UnityEngine;
using UnityEngine.InputSystem;

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
		public GameData Data { get; set; }

		void Initialize()
		{
			LastPolis = Data.lastPolis;
			initialized = true;
		}

		Savegame MakeSavegame()
		{
			return new()
			{
				lastUpdatedTime = System.DateTime.Now,
				data = Data,
			};
		}

		public void SaveTo(string filename)
		{
			GameManager.WriteSavegame(filename, MakeSavegame());
		}
		#endregion

		#region Scene transition
		/// <summary>最后进入过的城池。</summary>
		/// <remarks>城池内/战斗场景的调度类可读取此值来知道初始化哪个城池。</remarks>
		public string LastPolis
		{
			get => Data.lastPolis;
			private set => Data.lastPolis = value;
		}

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
					case Mode.Battle:
						GameManager.SwitchScene("Battle");
						break;
					default:
						throw new System.NotSupportedException();
				}
			}
		}

		public void EnterPolis(string polisId)
		{
			var polis = Data.poleis.Find(p => p.id == polisId);
			if(polis == null)
			{
				Debug.LogWarning($"Cannot enter polis \"{polisId}\", failed to find.");
				return;
			}

			if(polis.isControlled)
			{
				LastPolis = polis.id;
				Debug.Log($"Entering polis \"{LastPolis}\".");
				CurrentMode = Mode.Polis;
			}
			else if(polis.isHostile)
			{
				LastPolis = polis.id;
				Debug.Log($"Attacking polis \"{LastPolis}\".");
				CurrentMode = Mode.Battle;
			}
			else throw new System.NotSupportedException();
		}

		/// <summary>从我方城池出征。</summary>
		public void DepartFromPolis()
		{
			Debug.Log($"Departing from polis \"{LastPolis}\".");
			CurrentMode = Mode.WorldMap;
		}

		/// <summary>停止进攻敌方城池，回到大地图。</summary>
		public void ExitBattle()
		{
			Debug.Log($"Exiting battle against polis \"{LastPolis}\".");
			ApplyBattleResult(Battle.Instance.YieldResult());
			CurrentMode = Mode.WorldMap;
		}

		/// <summary>应用战役结算成果。</summary>
		void ApplyBattleResult(BattleResult result)
		{
			// TODO
		}
		#endregion

		#region Pause

		PauseMenu pauseMenu;
		public void OpenPauseMenu()
		{
			if(Instance == null)
			{
				Debug.LogWarning("Pause menu can only be opened when a game instance is running.");
				return;
			}
			if(pauseMenu != null)
				return;

			pauseMenu = Instantiate(Resources.Load<GameObject>("Prefabs/UI/Pause/Pause Menu")).GetComponent<PauseMenu>();
			GameManager.Paused = true;
		}

		public void ClosePauseMenu()
		{
			if(pauseMenu == null)
				return;

			Destroy(pauseMenu.gameObject);
			pauseMenu = null;
			GameManager.Paused = false;
		}
		#endregion
	}
}
