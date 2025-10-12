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
			name = definition.id;

			model = Instantiate(definition.ModelTemplate);
			model.name = "Model";
			model.transform.SetParent(transform, false);

			if(definition.obstructive)
			{
				var obstale = gameObject.AddComponent<NavMeshObstacle>();
				obstale.carving = true;
				obstale.size = definition.size;
				obstale.center = definition.center;
			}

			var collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = definition.obstructive;
			collider.size = definition.size;
			collider.center = definition.center;
		}

		public bool Selected { get; set; }
	}
}
