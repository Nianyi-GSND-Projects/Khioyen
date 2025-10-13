using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class PauseMenu : MonoBehaviour
	{
		public CanvasGroup mainGroup, saveGroup;
		IEnumerable<CanvasGroup> UiGroups
		{
			get
			{
				yield return mainGroup;
				yield return saveGroup;
			}
		}

		void SwitchGroup(CanvasGroup target)
		{
			foreach(var group in UiGroups)
				group.gameObject.SetActive(group == target);
		}

		public Button saveButton;

		protected void OnEnable()
		{
			OpenMainGroup();

			var isInPolis = GameInstance.Instance.CurrentMode == GameInstance.Mode.Polis;
			saveButton.interactable = isInPolis;
		}

		public void ClosePauseMenu()
		{
			GameInstance.Instance.ClosePauseMenu();
		}

		public void OpenSaveGroup()
		{
			SwitchGroup(saveGroup);
		}

		public void OpenMainGroup()
		{
			SwitchGroup(mainGroup);
		}

		public void QuitGame()
		{
			GameManager.StopCurrentGame();
		}
	}
}
