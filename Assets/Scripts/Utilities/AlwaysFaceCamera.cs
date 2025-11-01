using UnityEngine;

namespace LongLiveKhioyen
{
	public class AlwaysFaceCamera : MonoBehaviour
	{
		public bool flipped;

		protected void LateUpdate()
		{
			if(!Camera.main)
				return;
			Vector3 forward = Camera.main.transform.forward;
			if(flipped)
				forward = -forward;
			transform.rotation = Quaternion.LookRotation(forward);
		}
	}
}
