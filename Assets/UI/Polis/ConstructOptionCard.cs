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
		Polis Polis => Polis.Instance;

		CanvasGroup group;
		[SerializeField] Button button;
		[SerializeField] TMP_Text text;
		[SerializeField] Image image;

		public Action<ConstructOptionCard> onSelected, onHovered, onUnhovered;

		protected void Awake()
		{
			group = GetComponent<CanvasGroup>();
			Polis.onEconomyDataChanged += OnEconomyDataChanged;
		}

		protected void Start()
		{
			text.text = buildingDefinition.name;
			image.sprite = buildingDefinition.figure;
			button.onClick.AddListener(() => onSelected?.Invoke(this));
		}

		protected void OnDestroy()
		{
			if(Polis)
				Polis.onEconomyDataChanged -= OnEconomyDataChanged;
		}

		void OnEconomyDataChanged()
		{
			if(buildingDefinition.cost <= Polis.ControlledData.economy)
			{
				group.interactable = true;
				group.alpha = 1;
			}
			else
			{
				group.interactable = false;
				group.alpha = 0.5f;
			}
		}

		public void OnPointerEnter(PointerEventData eventData) => onHovered?.Invoke(this);

		public void OnPointerExit(PointerEventData eventData) => onUnhovered?.Invoke(this);
	}
}
