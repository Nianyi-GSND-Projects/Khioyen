using UnityEngine;
using UnityEngine.AI;
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

			if(definition.obstructive)
			{
				var obstale = gameObject.AddComponent<NavMeshObstacle>();
				obstale.carving = true;
				obstale.size = definition.size;
				obstale.center = definition.center;
			}
		}

		public bool Selected { get; set; }
	}
}
