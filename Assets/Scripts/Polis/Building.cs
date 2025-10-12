using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class Building : MonoBehaviour
	{
		[NonSerialized] public BuildingDefinition definition;
		GameObject model;
		readonly Dictionary<Renderer, Material[]> legacyMaterials = new();
		Material constructionMaterial;

		public Action onInitialized;
		public Action onConstructionStatusChange;

		protected void Start()
		{
			name = definition.id;

			model = Instantiate(definition.ModelTemplate);
			model.name = "Model";
			model.transform.SetParent(transform, false);
			foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
				legacyMaterials[renderer] = renderer.sharedMaterials;
			constructionMaterial = Resources.Load<Material>("Materials/Polis/Construction_site");

			Vector3 size = new(definition.size.x, 0, definition.size.y);
			Vector3 center = new Vector3(definition.center.x, 0, definition.center.y) - size * .5f;
			size.y = 1;

			// For building selection.
			var collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = definition.obstructive;
			collider.size = size;
			collider.center = center;

			if(definition.obstructive)
			{
				var obstale = gameObject.AddComponent<NavMeshObstacle>();
				obstale.carving = true;
				obstale.size = size;
				obstale.center = center;
			}

			onInitialized?.Invoke();
		}

		protected void Update()
		{
			if(UnderConstruction)
				RemainingConstructionTime -= Time.deltaTime;
		}

		bool underConstruction;
		public bool UnderConstruction
		{
			get => underConstruction;
			set
			{
				if(value == underConstruction)
					return;
				underConstruction = value;

				if(underConstruction)
				{
					foreach(var renderer in legacyMaterials.Keys)
						renderer.sharedMaterial = constructionMaterial;
				}
				else
				{
					foreach(var (renderer, mats) in legacyMaterials)
						renderer.sharedMaterials = mats;
				}

				onConstructionStatusChange?.Invoke();
			}
		}

		float remainingConstructionTime;
		public float RemainingConstructionTime
		{
			get => remainingConstructionTime;
			set
			{
				if(value < 0)
					value = 0;
				remainingConstructionTime = value;
				if(value == 0)
					UnderConstruction = false;
			}
		}

		public bool Selected { get; set; }
	}
}
