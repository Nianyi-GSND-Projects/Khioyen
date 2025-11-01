using UnityEngine;
using UnityEngine.Events;

namespace LongLiveKhioyen
{
	public class Trigger : MonoBehaviour
	{
		#region Serialized fields
		public bool oneTime;
		public UnityEvent onEnter;
		public UnityEvent onExit;
		#endregion

		#region Life cycle
		protected void OnTriggerEnter(Collider other)
		{
			onEnter?.Invoke();
		}

		protected void OnTriggerExit(Collider other)
		{
			onExit?.Invoke();
			if(oneTime)
				Destroy(this);
		}
		#endregion
	}
}
