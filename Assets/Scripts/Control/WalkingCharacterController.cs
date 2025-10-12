using UnityEngine;
using UnityEngine.AI;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(NavMeshAgent))]
	public class WalkingCharacterController : AbstractCharacterController
	{
		NavMeshAgent agent;

		void Awake()
		{
			agent = GetComponent<NavMeshAgent>();
		}

		void FixedUpdate()
		{
			if(!agent.isOnNavMesh)
				return;

			var cam = Camera.main;
			Vector3 world = cam.transform.forward * ForwardMoveInput + cam.transform.right * LateralMoveInput;

			if(world.sqrMagnitude <= 0.0001f)
				agent.ResetPath();
			else
			{
				Vector3 target = transform.position + world.normalized * 0.5f;
				agent.SetDestination(target);
				agent.speed = moveSpeed;
			}
		}

		public override void Teleport(Vector3 position)
		{
			agent.Warp(position);
		}

		public override void FaceTowards(Vector3 forward)
		{
			agent.enabled = false;
			base.FaceTowards(forward);
			agent.enabled = true;
		}
	}
}
