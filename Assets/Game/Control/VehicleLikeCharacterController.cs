using UnityEngine;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(CharacterController))]
	public class VehicleLikeCharacterController : AbstractCharacterController
	{
		CharacterController controller;

		void Awake()
		{
			controller = GetComponent<CharacterController>();
		}

		void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;
			controller.SimpleMove(transform.forward * (ForwardMoveInput * moveSpeed));
			transform.Rotate(Vector3.up * (LateralMoveInput * lateralSpeed * dt));
		}

		public override void Teleport(Vector3 position)
		{
			controller.enabled = false;
			base.Teleport(position);
			controller.enabled = true;
		}
	}
}
