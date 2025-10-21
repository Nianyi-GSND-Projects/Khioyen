using UnityEngine;

namespace LongLiveKhioyen
{
	public abstract class AbstractCharacterController : MonoBehaviour
	{
		[SerializeField] Animator animator;

		[Min(0)] public float moveSpeed = 30;
		[Min(0)] public float lateralSpeed = 40;

		bool isMoving = false;
		public bool IsMoving
		{
			get => isMoving;
			set
			{
				isMoving = value;
				if(animator)
					animator.SetBool("Is Moving", isMoving);
			}
		}

		Vector2 moveInput;
		public virtual float ForwardMoveInput {
			get => moveInput.y;
			set
			{
				moveInput.y = value;
				IsMoving = moveInput.sqrMagnitude != 0;
			}
		}
		public virtual float LateralMoveInput
		{
			get => moveInput.x;
			set
			{
				moveInput.x = value;
				IsMoving = moveInput.sqrMagnitude != 0;
			}
		}

		public virtual void Teleport(Vector3 position)
		{
			transform.position = position;
		}

		public virtual void FaceTowards(Vector3 forward)
		{
			transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
		}
	}
}
