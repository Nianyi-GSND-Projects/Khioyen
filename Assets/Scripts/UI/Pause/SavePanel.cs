using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace LongLiveKhioyen
{
	public class SavePanel : MonoBehaviour
	{
		public Button deleteButton, loadButton, saveButton;
		public List<SaveCard> saveCards;

		#region Life cycle
		protected void Awake()
		{
			GameManager.onSavegamesChanged += UpdateCardStatus;
			foreach(var card in saveCards)
				card.onClick += OnCardClicked;

			SelectedCard = null;
		}

		protected void OnDestroy()
		{
			GameManager.onSavegamesChanged -= UpdateCardStatus;
		}

		protected void OnEnable()
		{
			GameManager.RefreshSavegames();
		}

		protected void Start()
		{
			GameManager.RefreshSavegames();
		}

		void OnCardClicked(SaveCard card)
		{
			SelectedCard = card;
		}
		#endregion

		#region State
		SaveCard selectedCard = null;
		public SaveCard SelectedCard
		{
			get => selectedCard;
			set
			{
				if(selectedCard)
					selectedCard.Selected = false;
				selectedCard = value;
				if(selectedCard)
					selectedCard.Selected = true;

				deleteButton.interactable = selectedCard?.Created ?? false;
				loadButton.interactable = selectedCard?.Created ?? false;
				saveButton.interactable = selectedCard != null;
			}
		}

		void UpdateCardStatus()
		{
			for(int i = 0; i < saveCards.Count; ++i)
			{
				var card = saveCards[i];
				card.UpdateInformation();
				card.Interactable = i == 0 || card.Created || saveCards[i - 1].Created;
			}
			SelectedCard = SelectedCard;
		}
		#endregion

		#region Functions
		public void SaveToSelected()
		{
			if(!GameManager.IsGameRunning)
			{
				Debug.LogWarning("No game instance running, cannot save.");
				return;
			}
			if(SelectedCard == null)
			{
				Debug.LogWarning("No savegame slot selected, cannot save.");
				return;
			}
			GameInstance.Instance.SaveTo(SelectedCard.FileName);
		}

		public void LoadFromSelected()
		{
			CoroutineHelper.Run(LoadSavegameCoroutine(SelectedCard.FileName));
		}

		IEnumerator LoadSavegameCoroutine(string filename)
		{
			if(GameManager.IsGameRunning)
			{
				GameManager.StopCurrentGame();
				yield return new WaitForEndOfFrame();
			}
			GameManager.LoadGame(filename);
		}

		public void DeleteSelected()
		{
			GameManager.DeleteSavegame(SelectedCard.FileName);
		}
		#endregion
	}
}
