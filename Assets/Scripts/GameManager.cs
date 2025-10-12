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
			ReadSavegamePaths();

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
				CreateNewGameInstance();
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
				File.Create(settingsPath);
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
		static readonly List<string> savegamePaths = new();
		static readonly Action onSavegamesChanged;

		static void EnsureSavegamesDir()
		{
			if(File.Exists(savegamesDir))
				File.Delete(savegamesDir);
			if(!Directory.Exists(savegamesDir))
				Directory.CreateDirectory(savegamesDir);
		}

		static void ReadSavegamePaths()
		{
			EnsureSavegamesDir();
			foreach(string path in Directory.EnumerateFiles(savegamesDir))
			{
				if(savegamePaths.Contains(path))
					continue;
				savegamePaths.Add(path);
			}
			onSavegamesChanged?.Invoke();
		}

		public static Savegame ReadSavegame(string path)
		{
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

		public static void WriteSavegame(string path, Savegame savegame)
		{
			try
			{
				if(!File.Exists(path))
					File.Create(path);
				File.WriteAllText(path, JsonUtility.ToJson(savegame));
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

		public static void StartNewGame()
		{
			if(Instance != null)
			{
				Debug.LogError("A game instance is already running, cannot start new game.");
				return;
			}

			CreateNewGameInstance();

			SwitchScene("Polis");
		}

		private static void CreateNewGameInstance()
		{
			GameObject go = new("Game Instance");
			go.AddComponent<GameInstance>();
			Instance.Data = Resources.Load<GameDataSO>("Data/Initial Game Data").gameData;
		}

		public static void StopCurrentGame()
		{
			if(Instance == null)
			{
				Debug.LogError("No game instance is currently running, cannot stop current game.");
				return;
			}

			// TODO: Save the game.

			// Destroy the current game instance.
			UnityEngine.Object.Destroy(Instance);

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

			pauseMenu = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("UI/Pause Menu")).GetComponent<PauseMenu>();
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
