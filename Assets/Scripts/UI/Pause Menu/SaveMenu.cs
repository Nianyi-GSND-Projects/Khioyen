using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class SaveMenu : MonoBehaviour
	{
		public Button saveButton;
		public List<SaveCard> saveCards;

		#region Life cycle
		protected void Awake()
		{
			GameManager.onSavegamesChanged += UpdateCardStatus;
			foreach(var card in saveCards)
				card.onClick += OnCardClicked;
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

		SaveCard selectedCard = null;
		public SaveCard SelectedCard
		{
			get => selectedCard;
			set
			{
				if(value == selectedCard)
					return;

				if(selectedCard)
					selectedCard.Selected = false;
				selectedCard = value;
				if(selectedCard)
					selectedCard.Selected = true;

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
		}

		public void Save()
		{
			if(SelectedCard == null)
			{
				Debug.LogWarning("No savegame slot selected, cannot save.");
				return;
			}
			GameInstance.Instance.SaveTo(SelectedCard.FileName);
		}
	}
}
