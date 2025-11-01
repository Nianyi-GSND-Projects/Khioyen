using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	/// <summary>用来成组管理 action map 的启用性。</summary>
	public class InputGroup : MonoBehaviour
	{
		PlayerInput playerInput;
		[SerializeField] List<string> actionMaps = new();

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

		bool inputEnabled;
		public bool InputEnabled
		{
			get => inputEnabled;
			set
			{
				inputEnabled = value;
				OnExternalStateChanged();
			}
		}

		void Awake()
		{
			playerInput = FindObjectOfType<PlayerInput>(true);
			InputEnabled = isActiveAndEnabled;
			GameInstance.Instance.onPauseStateChanged += OnExternalStateChanged;
		}

		void OnDestroy()
		{
			if(GameInstance.Instance)
				GameInstance.Instance.onPauseStateChanged -= OnExternalStateChanged;
		}

		void OnExternalStateChanged()
		{
			if(!GameInstance.Instance || !playerInput)
				return;
			bool value = !GameInstance.Instance.Paused && InputEnabled;
			foreach(var map in Maps())
			{
				if(value)
					map.Enable();
				else
					map.Disable();
			}
		}

		void OnEnable()
		{
			OnExternalStateChanged();
		}

		void OnDisable()
		{
			OnExternalStateChanged();
		}
	}
}
