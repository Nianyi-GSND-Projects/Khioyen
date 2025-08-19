using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class InputGroup : MonoBehaviour
	{
		public PlayerInput playerInput;
		public List<string> actionMaps = new();

		IEnumerable<InputActionMap> Maps()
		{
			foreach(var mapName in actionMaps)
			{
				var map = playerInput.actions.FindActionMap(mapName);
				if(map == null)
					continue;
				yield return map;
			}
		}

		void OnEnable()
		{
			foreach(var map in Maps())
				map.Enable();
		}

		void OnDisable()
		{
			foreach(var map in Maps())
				map.Disable();
		}
	}
}
