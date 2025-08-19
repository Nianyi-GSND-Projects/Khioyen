using UnityEngine;
using System;

namespace LongLiveKhioyen
{
	public class Building : MonoBehaviour
	{
		Polis polis;
		[NonSerialized] public BuildingDefinition definition;
		[NonSerialized] public ControlledPolisData.BuildingPlacement placement;
		GameObject model;

		void Start()
		{
			polis = GetComponentInParent<Polis>();
			name = definition.name;

			transform.localPosition = polis.grid.CellToLocalInterpolated(
				(Vector3Int)placement.position - definition.bounds.center + Vector3.one
			);
			transform.localEulerAngles = Vector3.up * (placement.orientation * 90);

			model = Resources.Load<GameObject>(definition.modelAddress);
			if(model == null)
			{
				Debug.LogWarning($"Cannot find the model of building \"{placement.type}\" at {definition.modelAddress}.");
				return;
			}
			model = Instantiate(model);
			model.name = "Model";
			model.transform.SetParent(transform, false);
		}
	}
}
