using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class Building : MonoBehaviour
	{
		public BuildingPlacement Placement { get; set; }
		public BuildingDefinition Definition { get; set; }

		#region Life cycle
		public Action onInitialized;
		public Action onConstructionStatusChange;

		protected void Start()
		{
			name = Definition.id;

			model = Instantiate(Definition.ModelTemplate);
			model.name = "Model";
			model.transform.SetParent(transform, false);
			foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
				legacyMaterials[renderer] = renderer.sharedMaterials;
			constructionMaterial = Resources.Load<Material>("Materials/Polis/Construction_site");

			Vector3 size = new(Definition.size.x, 0, Definition.size.y);
			Vector3 center = new Vector3(Definition.center.x, 0, Definition.center.y) - size * .5f;
			size.y = 1;

			// For building selection.
			var collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = Definition.obstructive;
			collider.size = size;
			collider.center = center;

			if(Definition.obstructive)
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

			if(dirty)
				UpdateVisualState();
		}
		#endregion

		#region Visual state
		bool dirty = true;

		GameObject model;
		readonly Dictionary<Renderer, Material[]> legacyMaterials = new();
		Material constructionMaterial;

		public void UpdateVisualState()
		{
			if(Placement.underConstruction)
			{
				foreach(var renderer in legacyMaterials.Keys)
					renderer.sharedMaterial = constructionMaterial;
			}
			else
			{
				foreach(var (renderer, mats) in legacyMaterials)
					renderer.sharedMaterials = mats;
			}

			dirty = false;
		}

		public bool UnderConstruction
		{
			get => Placement.underConstruction;
			set
			{
				Placement.underConstruction = value;
				dirty = true;
			}
		}

		public float RemainingConstructionTime
		{
			get => Placement.remainingConstructionTime;
			set
			{
				Placement.remainingConstructionTime = Mathf.Max(0, value);
				if(Placement.remainingConstructionTime == 0)
					UnderConstruction = false;
			}
		}

		public bool Selected { get; set; }
		#endregion
	}
}
