using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace LongLiveKhioyen
{
	public class CharacterControl : MonoBehaviour
	{
		public AbstractCharacterController characterController;
		public UnityEvent onInteract;

		protected void OnForwardMove(InputValue value)
		{
			characterController.ForwardMoveInput = value.Get<float>();
		}

		protected void OnLateralMove(InputValue value)
		{
			characterController.LateralMoveInput = value.Get<float>();
		}

		protected void OnInteract()
		{
			onInteract?.Invoke();
		}
	}
}
