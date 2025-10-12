using UnityEngine;
using UnityEngine.UI;
using System;

namespace LongLiveKhioyen
{
	public class StartMenu : MonoBehaviour
	{
		[Serializable]
		public struct Buttons {
			public Button newGame, loadGame, settings, credits, exit;
		}
		public Buttons buttons;

		#region Functions
		public void StartNewGame()
		{
			GameManager.StartNewGame();
		}
		#endregion
	}
}
