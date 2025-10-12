using UnityEngine;
using UnityEngine.AI;
using System;

namespace LongLiveKhioyen
{
	public class Building : MonoBehaviour
	{
		[NonSerialized] public BuildingDefinition definition;
		[NonSerialized] public BuildingPlacement placement;
		GameObject model;

		void Start()
		{
			name = definition.id;

			model = Instantiate(definition.ModelTemplate);
			model.name = "Model";
			model.transform.SetParent(transform, false);

			Vector3 size = new(definition.size.x, 0, definition.size.y);
			Vector3 center = new Vector3(definition.center.x, 0, definition.center.y) - size * .5f;

			if(definition.obstructive)
			{
				var obstale = gameObject.AddComponent<NavMeshObstacle>();
				obstale.carving = true;
				obstale.size = size;
				obstale.center = center;
			}

			var collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = definition.obstructive;
			collider.size = size;
			collider.center = center;
		}

		public bool Selected { get; set; }
	}
}
