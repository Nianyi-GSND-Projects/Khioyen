using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class SaveCard : MonoBehaviour, IPointerClickHandler
	{
		public string saveName;
		public string FileName => $"{saveName}.json";
		public Button button;
		public CanvasGroup newSaveGroup, infoGroup;
		public TMP_Text title;

		#region Life cycle
		protected void Awake()
		{
			Interactable = false;
			Created = false;
			Selected = false;
		}
		#endregion

		public bool Interactable
		{
			get => button.interactable;
			set => button.interactable = value;
		}

		bool created;
		public bool Created
		{
			get => created;
			set
			{
				created = value;
				newSaveGroup.gameObject.SetActive(!created);
				infoGroup.gameObject.SetActive(created);
				title.text = created ? FileName : string.Empty;
			}
		}

		static Color unselectedColor = Color.white;
		static Color selectedColor = Color.Lerp(Color.yellow, Color.white, 0.5f);
		public bool Selected
		{
			set
			{
				var colors = button.colors;
				var color = value ? selectedColor : unselectedColor;
				colors.normalColor = color;
				colors.selectedColor = color;
				colors.highlightedColor = color;
				button.colors = colors;
			}
		}

		public System.Action<SaveCard> onClick;
		public void OnPointerClick(PointerEventData eventData)
		{
			if(Interactable)
				onClick?.Invoke(this);
		}

		public void UpdateInformation()
		{
			Created = GameManager.SavegameFilenames.Contains(FileName);
		}
	}
}
