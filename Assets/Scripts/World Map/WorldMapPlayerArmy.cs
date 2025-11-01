using UnityEngine;
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

		PolisMiniature focusedMiniature;

		protected void OnTriggerEnter(Collider other)
		{
			if(!other.TryGetComponent<PolisMiniature>(out var miniature))
				return;
			focusedMiniature = miniature;
		}

		protected void OnTriggerExit(Collider other)
		{
			if(!other.TryGetComponent<PolisMiniature>(out var miniature))
				return;
			if(miniature == focusedMiniature)
				focusedMiniature = null;
		}

		public void InteractWithNearbyPolis()
		{
			if(focusedMiniature == null)
				return;
			var polis = focusedMiniature.data;
			GameInstance.Instance.EnterPolis(polis.id);
		}
	}
}
