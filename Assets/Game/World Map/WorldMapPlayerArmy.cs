using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(CharacterController))]
	public class WorldMapPlayerArmy : MonoBehaviour
	{
		#region Settings
		[Min(0)] public float maxInteractDistance = 10;
		#endregion

		#region Movement
		CharacterController controller;
		CharacterController Controller
		{
			get
			{
				if(controller == null)
					controller = GetComponent<CharacterController>();
				return controller;
			}
		}

		[Min(0)] public float moveSpeed = 30;
		[Min(0)] public float rotateSpeed = 40;

		float moveInput = 0;
		float rotateInput = 0;

		public void SetPosition(Vector3 position)
		{
			Controller.enabled = false;
			transform.position = position;
			Controller.enabled = true;
		}
		#endregion

		#region Life cycle
		void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;
			Controller.Move(transform.forward * (moveInput * dt));
			transform.Rotate(Vector3.up * (rotateInput * dt));
		}
		#endregion

		#region Input handler
		protected void OnMove(InputValue value)
		{
			var raw = value.Get<float>();
			moveInput = raw * moveSpeed;
		}

		protected void OnRotate(InputValue value)
		{
			var raw = value.Get<float>();
			rotateInput = raw * rotateSpeed;
		}

		protected void OnInteract()
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
		#endregion
	}
}
