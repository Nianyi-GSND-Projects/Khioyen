using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
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

			var modifier = gameObject.AddComponent<NavMeshModifier>();
			modifier.overrideArea = true;
			modifier.area = NavMesh.GetAreaFromName("Not Walkable");
		}

		public bool Selected { get; set; }
	}
}
