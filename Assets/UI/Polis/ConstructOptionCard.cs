using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace LongLiveKhioyen
{
	public class ConstructOptionCard : MonoBehaviour
	{
		[NonSerialized] public BuildingDefinition buildingDefinition;

		public Button button;
		public TMP_Text text;
		public Image image;

		void Start()
		{
			text.text = buildingDefinition.name;
			image.sprite = buildingDefinition.figure;
			button.onClick.AddListener(() => FindObjectOfType<ConstructModal>().SelectedBuildingType = buildingDefinition.typeId);
		}
	}
}
