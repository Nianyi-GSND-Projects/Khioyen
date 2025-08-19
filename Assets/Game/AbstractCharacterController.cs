using UnityEngine;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(CharacterController))]
	public class AbstractCharacterController : MonoBehaviour
	{
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

		public float MoveInput { get; set; } = 0;
		public float RotateInput { get; set; } = 0;

		public void Teleport(Vector3 position)
		{
			Controller.enabled = false;
			transform.position = position;
			Controller.enabled = true;
		}

		public void FaceTowards(Vector3 forward)
		{
			transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}

		void FixedUpdate()
		{
			float dt = Time.fixedDeltaTime;
			Controller.SimpleMove(transform.forward * (MoveInput * moveSpeed));
			transform.Rotate(Vector3.up * (RotateInput * rotateSpeed * dt));
		}
	}
}
