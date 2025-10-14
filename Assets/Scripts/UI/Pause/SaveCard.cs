using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace LongLiveKhioyen
{
	public class SaveCard : MonoBehaviour, IPointerClickHandler
	{
		public string saveName;
		public string FileName => $"{saveName}.json";
		public Button button;
		public GameObject newSaveGroup, infoGroup;
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
				newSaveGroup.SetActive(!created);
				infoGroup.SetActive(created);
				title.text = created ? saveName : string.Empty;
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
