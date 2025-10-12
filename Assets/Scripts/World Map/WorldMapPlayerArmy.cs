using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(AbstractCharacterController))]
	public class WorldMapPlayerArmy : MonoBehaviour
	{
		AbstractCharacterController controller;
		public AbstractCharacterController Controller
		{
			get
			{
				if(controller == null)
					controller = GetComponent<AbstractCharacterController>();
				return controller;
			}
		}

		[Min(0)] public float maxInteractDistance = 10;

		public void InteractWithNearbyPolis()
		{
			var polisMiniatures = FindObjectsByType<PolisMiniature>(FindObjectsSortMode.None)
				.Select(p => new KeyValuePair<PolisMiniature, float>(p, Vector3.Distance(p.transform.position, transform.position)))
				.Where(p => p.Value <= maxInteractDistance)
				.ToList();
			if(polisMiniatures.Count == 0)
				return;
			polisMiniatures.Sort((a, b) => (int)Mathf.Sign(a.Value - b.Value));

			var polisMiniature = polisMiniatures[0].Key;
			var polis = polisMiniature.data;

			if(polis.isControlled)
			{
				GameInstance.Instance.EnterPolis(polis.id);
			}
			else if(polis.isHostile)
			{
			}
		}
	}
}
