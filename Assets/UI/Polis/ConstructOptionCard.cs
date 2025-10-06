using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace LongLiveKhioyen
{
	public class ConstructOptionCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[NonSerialized] public BuildingDefinition buildingDefinition;

		[SerializeField] Button button;
		[SerializeField] TMP_Text text;
		[SerializeField] Image image;

		public Action<ConstructOptionCard> onSelected, onHovered, onUnhovered;

		void Start()
		{
			text.text = buildingDefinition.name;
			image.sprite = buildingDefinition.figure;
			button.onClick.AddListener(() => onSelected?.Invoke(this));
		}

		public void OnPointerEnter(PointerEventData eventData) => onHovered?.Invoke(this);

		public void OnPointerExit(PointerEventData eventData) => onUnhovered?.Invoke(this);
	}
}
