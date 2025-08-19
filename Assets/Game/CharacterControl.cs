using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace LongLiveKhioyen
{
	public class CharacterControl : MonoBehaviour
	{
		public AbstractCharacterController characterController;
		public UnityEvent onInteract;

		protected void OnMove(InputValue value)
		{
			characterController.MoveInput = value.Get<float>();
		}

		protected void OnRotate(InputValue value)
		{
			characterController.RotateInput = value.Get<float>();
		}

		protected void OnInteract()
		{
			onInteract?.Invoke();
		}
	}
}
