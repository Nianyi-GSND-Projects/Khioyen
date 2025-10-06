using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class ConstructModal : MonoBehaviour
	{
		public MayorMode mayorMode;
		int orientation;
		Polis Polis => mayorMode.polis;
		public LayoutGroup constructOptionsUi;

		#region Life cycle
		void OnEnable()
		{
			GenerateUi();
			SelectedBuildingType = null;
			ShowCostPreview = false;
		}

		void OnDisable()
		{
			SelectedBuildingType = null;
		}
		#endregion

		#region Input handlers
		protected void OnRotateBuilding()
		{
			orientation = (orientation + 1) % 4;
			UpdatePreviewModelPose();
		}

		protected void OnDrag()
		{
			UpdatePreviewModelPose();
		}
		#endregion

		#region UI
		void GenerateUi()
		{
			List<Transform> children = new();
			for(int i = 0; i < constructOptionsUi.transform.childCount; ++i)
				children.Add(constructOptionsUi.transform.GetChild(i));
			foreach(var child in children)
				Destroy(child.gameObject);

			var cardTemplate = Resources.Load<GameObject>("Construct Option Card");
			foreach(var definition in GameManager.BuildingDefinitions)
			{
				var card = Instantiate(cardTemplate).GetComponent<ConstructOptionCard>();
				card.buildingDefinition = definition;
				card.transform.SetParent(constructOptionsUi.transform, false);

				card.onSelected += OnConstructionCardSelected;
				card.onHovered += OnConstructionCardHovered;
				card.onUnhovered += OnConstructionCardUnhovered;
			}
			constructOptionsUi.CalculateLayoutInputHorizontal();
		}

		void OnConstructionCardSelected(ConstructOptionCard card)
		{
			SelectedBuildingType = card.buildingDefinition.typeId;
		}

		void OnConstructionCardHovered(ConstructOptionCard card)
		{
			hoveredBuildingType = card.buildingDefinition;
			ShowCostPreview = true;
		}

		void OnConstructionCardUnhovered(ConstructOptionCard card)
		{
			ShowCostPreview = false;
		}
		#endregion

		#region Selection
		BuildingDefinition selectedBuildingType, hoveredBuildingType;
		GameObject previewModel;
		public string SelectedBuildingType
		{
			get => selectedBuildingType?.typeId;
			set
			{
				if(previewModel != null)
				{
					Destroy(previewModel);
					previewModel = null;
				}

				if(!GameManager.FindBuildingDefinitionByType(value, out selectedBuildingType))
					return;

				if(selectedBuildingType.model == null)
					return;

				orientation = 0;
				previewModel = Instantiate(selectedBuildingType.model);
				previewModel.transform.SetParent(Polis.transform, false);
				UpdatePreviewModelPose();
			}
		}

		void UpdatePreviewModelPose()
		{
			if(previewModel == null)
				return;

			if(!Polis.ScreenToGround(mayorMode.PointerScreenPosition, out var groundPos))
			{
				previewModel.SetActive(false);
				return;
			}

			var gridPos = Polis.grid.WorldToCell(groundPos);
			Polis.PositionBuilding(previewModel.transform, selectedBuildingType.bounds, (Vector2Int)gridPos, orientation);
		}
		#endregion

		#region Cost preview
		[SerializeField] CostPreviewPanel costPreviewPanel;
		bool ShowCostPreview
		{
			get => costPreviewPanel.gameObject.activeSelf;
			set
			{
				if(hoveredBuildingType == null)
					value = false;
				if(value == ShowCostPreview)
					return;

				costPreviewPanel.gameObject.SetActive(value);
				if(value)
					costPreviewPanel.UpdateCostData(hoveredBuildingType.cost);
			}
		}
		#endregion

		#region Actions
		protected void Interact()
		{
			TryPlaceBuilding();
		}

		void TryPlaceBuilding()
		{
			if(selectedBuildingType == null)
				return;
			if(!Polis.ScreenToGround(mayorMode.PointerScreenPosition, out Vector3 groundPosition))
				return;
			if(!Polis.TryCostResource(selectedBuildingType.cost))
			{
				Debug.LogWarning(
					$"Not enough resources to build a {selectedBuildingType.name}!\n" +
					$"Required: {selectedBuildingType.cost}, current: {Polis.ControlledData.economy}."
				);
				return;
			}
			var gridPosition = (Vector2Int)Polis.grid.WorldToCell(groundPosition);
			Polis.ConstructBuilding(selectedBuildingType.typeId, gridPosition, orientation);
		}
		#endregion
	}
}
