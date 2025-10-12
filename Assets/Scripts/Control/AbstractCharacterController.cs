using UnityEngine;

namespace LongLiveKhioyen
{
	public abstract class AbstractCharacterController : MonoBehaviour
	{
		[Min(0)] public float moveSpeed = 30;
		[Min(0)] public float lateralSpeed = 40;

		public virtual float ForwardMoveInput { get; set; } = 0;
		public virtual float LateralMoveInput { get; set; } = 0;

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
