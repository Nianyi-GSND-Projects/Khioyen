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

			model = Instantiate(definition.model);
			model.name = "Model";
			model.transform.SetParent(transform, false);
		}

		public bool Selected { get; set; }
	}
}
