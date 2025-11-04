using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongLiveKhioyen
{
    public class ArrangementModal : MonoBehaviour
    {
        public ArrangementMode arrangementMode;
        
        Battle Battle=> Battle.Instance;
        
        public LayoutGroup ArrangementLayoutGroup;
        
        #region Life cycle
		void OnEnable()
		{
			
			//ShowCostPreview = false;
			//Polis.onEconomyDataChanged += OnEconomyDataChanged;
			//Polis.onBuildingOccupancyChanged += UpdatePreviewModel;
		}
  //
  //       void OnDisable()
  //       {
  //           SelectedBuildingType = null;
  //       }
  //       
  //       void OnEconomyDataChanged()
  //       {
  //           if(SelectedBuildingType != null)
  //           {
  //               if(!(SelectedBuildingType.cost <= Polis.Economy))
  //                   SelectedBuildingType = null;
  //           }
  //       }
        #endregion
  //       
         #region Input handlers
  //
		// protected void OnDrag()
		// {
		// 	UpdatePreviewModel();
		// }
		 #endregion
  //
		 #region UI

		 public void InitializeUi()
		 {
			 GenerateUi();
			 Debug.Log("Arrangement Modal Enabled");
			 SelectedReserveTeam = null;
		 }
		void GenerateUi()
		{
			
			List<Transform> children = new();
			for(int i = 0; i < ArrangementLayoutGroup.transform.childCount; ++i)
				children.Add(ArrangementLayoutGroup.transform.GetChild(i));
			foreach(var child in children)
				Destroy(child.gameObject);
  
			var cardTemplate = Resources.Load<GameObject>("Prefabs/UI/Battle/Battalion_Arrangement");
			foreach(var reserveTeam in Battle.data.playerReserveTeams)
			{
				var card = Instantiate(cardTemplate).GetComponent<BattalionArrangementUi>();
				card.reserveTeam = reserveTeam;
				card.transform.SetParent(ArrangementLayoutGroup.transform, false);
  
				card.onSelected += OnBattalionCardSelected;
				card.onHovered += OnBattalionCardHovered;
				card.onUnhovered += OnBattalionCardUnhovered;
			}
			ArrangementLayoutGroup.CalculateLayoutInputHorizontal();
		}
  //
		void OnBattalionCardSelected(BattalionArrangementUi card)
		{
			SelectedReserveTeam = card.reserveTeam;
		}
  
		void OnBattalionCardHovered(BattalionArrangementUi card)
		{
			hoveredReserveTeam = card.reserveTeam;
			//ShowCostPreview = true;
		}
  
		void OnBattalionCardUnhovered(BattalionArrangementUi card)
		{
			//ShowCostPreview = false;
		}
		 #endregion

		#region Selection
		ReserveTeam selectedReserveTeam, hoveredReserveTeam;
		//ArrangementPreview preview;
		public ReserveTeam SelectedReserveTeam
		{
			get => selectedReserveTeam;
			set
			{
				if(value == selectedReserveTeam)
					return;

				// if(preview != null)
				// {
				// 	Destroy(preview.gameObject);
				// 	preview = null;
				// }

				selectedReserveTeam = value;

				if(selectedReserveTeam != null)
				{
					// preview = new GameObject("Construction Preview").AddComponent<ConstructPreview>();
					// preview.Definition = selectedBuildingType;
					// preview.transform.SetParent(Polis.transform, false);
					// preview.onInitialized += UpdatePreviewModel;
				}
			}
		}

		// void UpdatePreviewModel()
		// {
		// 	if(preview == null)
		// 		return;
		//
		// 	Vector3 groundPos;
		// 	Vector2Int mapPos = default;
		// 	bool PositionMakesSense()
		// 	{
		// 		if(!Polis.ScreenToGround(mayorMode.PointerScreenPosition, out groundPos))
		// 			return false;
		// 		if(!Polis.IsValidMapPosition(mapPos = Polis.WorldToMapInt(groundPos)))
		// 			return false;
		// 		return true;
		// 	}
		// 	if(!PositionMakesSense())
		// 	{
		// 		preview.Visible = false;
		// 		return;
		// 	}
		// 	preview.Visible = true;
		//
		// 	BuildingPlacement placement = new()
		// 	{
		// 		position = mapPos,
		// 		orientation = orientation,
		// 	};
		// 	preview.Valid = Polis.ValidateBuildingPlacement(SelectedBuildingType, placement);
		// 	Polis.PositionBuilding(preview.transform, SelectedBuildingType, placement);
		// }
		#endregion
		
		#region Actions

		public void TryPlaceReserveTeam()
		{
			if (!Battle.ScreenToGround(arrangementMode.PointerScreenPosition, out Vector3 groundPosition))
			{
				Debug.LogWarning("Position not valid." + Battle.WorldToMapInt(groundPosition));
				return;
			}
			
			if (SelectedReserveTeam == null)
			{
				Debug.LogWarning("No reserve team selected." + Battle.WorldToMapInt(groundPosition));
				return;
			}
			
			
			
			if (selectedReserveTeam.placed == true)
			{
				Debug.LogWarning("Reserve team already placed." + Battle.WorldToMapInt(groundPosition));
				return;
			}

			BattalionCompilation compilation = new()
			{
				//position = Battle.WorldToMapInt(groundPosition),
				position = Battle.WorldToMapInt(groundPosition),
			};
			if (!Battle.ValidateArrangementPlacement(compilation.position))
			{
				Debug.LogWarning($"Cannot place {compilation.battalionId} at {compilation.position}.");
				return;
			}
			Battle.PlacingBattalion(SelectedReserveTeam, compilation.position);
		}

		#endregion
    }
}
