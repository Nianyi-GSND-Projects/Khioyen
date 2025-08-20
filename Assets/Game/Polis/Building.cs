using UnityEngine;
using System;

namespace LongLiveKhioyen
{
	public class Building : MonoBehaviour
	{
		[NonSerialized] public BuildingDefinition definition;
		[NonSerialized] public ControlledPolisData.BuildingPlacement placement;
		GameObject model;

		void Start()
		{
			name = definition.name;

			model = GameManager.GetBuildingModelTemplate(definition);
			if(model == null)
			{
				Debug.LogWarning($"Cannot find the model of building \"{placement.type}\" at {definition.modelAddress}.");
				return;
			}
			model = Instantiate(model);
			model.name = "Model";
			model.transform.SetParent(transform, false);
		}

		public bool Selected { get; set; }
	}
}
