using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

namespace LongLiveKhioyen
{
	public static class GameManager
	{
		#region Life cycle
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		static void OnGameStart()
		{
			/* Local data */
			ReadSettings();
			RefreshSavegames();
			Debug.Log(string.Join(",", savegameFilenames));

			/* Built-in resources */
			buildingDefinitionSheet = UnityEngine.Object.Instantiate(Resources.Load<BuildingDefinitionSheet>("Data/Building Definitions"));
			if(buildingDefinitionSheet == null)
			{
				Debug.LogError("Cannot construct buildings, cannot fetch the initial building definition sheet.");
				return;
			}

			/* Scene */
			SceneManager.activeSceneChanged += OnSceneChanged;
#if DEBUG
			// Use initial game data if not starting from the menu scene.
			if(SceneManager.GetActiveScene().buildIndex != 0)
				StartNewGame();
#endif
		}
		#endregion

		#region Built-in resources
		static BuildingDefinitionSheet buildingDefinitionSheet;
		public static List<BuildingDefinition> BuildingDefinitions => buildingDefinitionSheet?.buildingDefinitions;
		public static bool FindBuildingDefinitionByType(string type, out BuildingDefinition definition)
		{
			definition = BuildingDefinitions.Find(d => d.id == type);
			return definition != null;
		}
		#endregion

		#region Local data
		#region Settings
		static readonly string settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
		static GameSettings settings = null;
		static readonly Action onSettingsChanged;

		static void EnsureSettingsPath()
		{
			if(Directory.Exists(settingsPath))
				Directory.Delete(settingsPath, true);
			if(!File.Exists(settingsPath))
				File.Create(settingsPath).Close();
		}

		static void ReadSettings()
		{
			if(File.Exists(settingsPath))
			{
				try
				{
					settings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(settingsPath));
				}
				catch(Exception e)
				{
					Debug.LogWarning($"Failed to read game settings at {settingsPath}.");
					Debug.LogError(e);
					settings = null;
				}
			}

			if(settings == null)
			{
				settings = new();
				WriteSettings();
			}

			onSettingsChanged?.Invoke();
		}

		public static void WriteSettings()
		{
			try
			{
				EnsureSettingsPath();
				File.WriteAllText(settingsPath, JsonUtility.ToJson(settings));
			}
			catch(Exception e)
			{
				Debug.LogWarning($"Failed to write game settings to {settingsPath}.");
				Debug.LogError(e);
			}
		}
		#endregion

		#region Savegames
		static readonly string savegamesDir = Path.Combine(Application.persistentDataPath, "savegames");
		static readonly List<string> savegameFilenames = new();
		public static IList<string> SavegameFilenames => savegameFilenames;
		public static Action onSavegamesChanged;

		static void EnsureSavegamesDir()
		{
			if(File.Exists(savegamesDir))
				File.Delete(savegamesDir);
			if(!Directory.Exists(savegamesDir))
				Directory.CreateDirectory(savegamesDir);
		}

		static string SavegameFileNameToPath(string filename)
		{
			return Path.Join(savegamesDir, filename);
		}

		public static void RefreshSavegames()
		{
			EnsureSavegamesDir();
			savegameFilenames.Clear();
			foreach(string path in Directory.EnumerateFiles(savegamesDir))
			{
				var filename = Path.GetRelativePath(savegamesDir, path);
				RecordSavegameFileName(filename);
			}
			onSavegamesChanged?.Invoke();
		}

		static void RecordSavegameFileName(string filename)
		{
			if(savegameFilenames.Contains(filename))
				return;
			savegameFilenames.Add(filename);
		}

		public static Savegame ReadSavegame(string filename)
		{
			var path = SavegameFileNameToPath(filename);
			try
			{
				var savegame = JsonUtility.FromJson<Savegame>(File.ReadAllText(path));
				return savegame;
			}
			catch(Exception e)
			{
				Debug.LogWarning($"Failed to read the savegame at {path}.");
				Debug.LogError(e);
				return null;
			}
		}

		public static void WriteSavegame(string filename, Savegame savegame)
		{
			var path = SavegameFileNameToPath(filename);
			try
			{
				if(!File.Exists(path))
					File.Create(path).Close();
				File.WriteAllText(path, JsonUtility.ToJson(savegame));
				Debug.Log($"Wrote savegame to {path}.");
				RecordSavegameFileName(filename);
				onSavegamesChanged?.Invoke();
			}
			catch(Exception e)
			{
				Debug.LogWarning($"Failed to write savegame \"{savegame.name}\" to {path}.");
				Debug.LogError(e);
			}
		}
		#endregion
		#endregion

		#region Scene
		public static void SwitchScene(string sceneName)
		{
			SceneManager.LoadScene(sceneName);
		}

		static void OnSceneChanged(Scene previous, Scene after)
		{
			// Update environmental lighting.
			DynamicGI.UpdateEnvironment();
		}
		#endregion

		#region Game instance
		public static GameInstance Instance => GameInstance.Instance;
		public static GameData LoadedGameData { get; private set; }

		public static void StartNewGame()
		{
			if(Instance != null)
			{
				Debug.LogError("A game instance is already running, cannot start new game.");
				return;
			}

			StartGameInstanceWithData(Utilities.DeepCopy(Resources.Load<GameDataSO>("Data/Initial Game Data").gameData));
		}

		public static void LoadGame(string filename)
		{
			if(!savegameFilenames.Contains(filename))
			{
				Debug.LogError($"No savegame named {filename} exists, cannot load game.");
				return;
			}
			try
			{
				var savegame = ReadSavegame(filename);
				StartGameInstanceWithData(savegame.data);
			}
			catch(Exception e)
			{
				Debug.LogError($"Failed to load savegame from {filename}.");
				Debug.LogError(e);
				return;
			}
		}

		static void StartGameInstanceWithData(GameData data)
		{
			GameObject go = new("Game Instance");
			go.AddComponent<GameInstance>();
			LoadedGameData = data;
			SwitchScene("Polis");
		}

		public static void StopCurrentGame()
		{
			if(Instance == null)
			{
				Debug.LogError("No game instance is currently running, cannot stop current game.");
				return;
			}

			// Destroy the current game instance.
			UnityEngine.Object.Destroy(Instance);
			LoadedGameData = null;
			SwitchScene("Start Menu");
		}
		#endregion

		#region Pause menu
		static PauseMenu pauseMenu;
		public static void OpenPauseMenu()
		{
			if(Instance == null)
			{
				Debug.LogWarning("Pause menu can only be opened when a game instance is running.");
				return;
			}
			if(pauseMenu != null)
				return;

			pauseMenu = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UI/Pause/Pause Menu")).GetComponent<PauseMenu>();
			Time.timeScale = 0.0f;
		}

		public static void ClosePauseMenu()
		{
			if(pauseMenu == null)
				return;

			UnityEngine.Object.Destroy(pauseMenu.gameObject);
			pauseMenu = null;
			Time.timeScale = 1.0f;
		}
		#endregion
	}
}
