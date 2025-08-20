using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace LongLiveKhioyen
{
	public class MayorMode : MonoBehaviour
	{
		#region Settings
		public Polis polis;

		[Header("Settings")]
		public float panSpeed = 1;
		public float zoomSpeed = 1;
		public Vector2 zoomRange = new(2, 100);
		public float rotateSpeed = 1;
		#endregion

		#region Input states
		Vector2 pointerScreenPosition;
		public Vector2 PointerScreenPosition => pointerScreenPosition;
		bool isPointerOverGameObjects;

		bool isPrimaryButtonDown, isSecondaryButtonDown;
		float lastPrimaryClickTime;
		Vector2 primaryStartScreenPosition;
		#endregion

		#region Life cycle
		void Update()
		{
			isPointerOverGameObjects = EventSystem.current?.IsPointerOverGameObject() ?? false;
		}
		#endregion

		#region Input handlers
		protected void OnPoint(InputValue value)
		{
			var raw = value.Get<Vector2>();
			pointerScreenPosition = raw;
		}

		protected void OnDrag(InputValue value)
		{
			var raw = value.Get<Vector2>();

			if(isPrimaryButtonDown)
				Pan(raw);
			if(isSecondaryButtonDown)
				Rotate(raw);
		}

		protected void OnScroll(InputValue value)
		{
			var raw = value.Get<float>();
			Zoom(raw);
		}

		protected void OnPrimaryClick(InputValue value)
		{
			var raw = value.isPressed;
			isPrimaryButtonDown = raw;

			if(raw)
			{
				lastPrimaryClickTime = Time.realtimeSinceStartup;
				primaryStartScreenPosition = pointerScreenPosition;
			}
			else
			{
				float elapsedTime = Time.realtimeSinceStartup - lastPrimaryClickTime;
				Vector2 mouseMoved = primaryStartScreenPosition - pointerScreenPosition;
				if(
					elapsedTime <= 0.3f &&
					mouseMoved.magnitude <= 5 &&
					!isPointerOverGameObjects
				)
					BroadcastMessage("Interact", pointerScreenPosition);
			}
		}

		protected void OnSecondaryClick(InputValue value)
		{
			var raw = value.isPressed;
			isSecondaryButtonDown = raw;
		}
		#endregion

		#region Functions
		void Pan(Vector2 screenDelta)
		{
			if(!polis.ScreenToGround(pointerScreenPosition - screenDelta, out var to))
				return;
			if(!polis.ScreenToGround(pointerScreenPosition, out var from))
				return;
			polis.mayorCamera.LookAt.position += (to - from) * panSpeed;
		}

		void Zoom(float scrollY)
		{
			var follow = polis.mayorCamera.Follow;

			float z = -follow.localPosition.z;
			z *= Mathf.Exp(-scrollY * zoomSpeed);
			z = Mathf.Clamp(z, zoomRange.x, zoomRange.y);

			follow.localPosition = Vector3.back * z;
		}

		void Rotate(Vector2 screenDelta)
		{
			screenDelta.y *= -1;
			screenDelta *= rotateSpeed;

			var root = polis.mayorCamera.LookAt;
			var euler = root.localEulerAngles;
			euler.x = Mathf.Clamp(euler.x + screenDelta.y, 0, 90);
			euler.y += screenDelta.x;
			root.localEulerAngles = euler;
		}

		protected void Interact(Vector2 screenPos)
		{
			if(polis.IsInConstructModal)
				return;

			var ray = Camera.main.ScreenPointToRay(screenPos);
			if(!Physics.Raycast(ray, out var hit, Mathf.Infinity))
				return;

			var hitBuilding = hit.collider.GetComponentInParent<Building>();
			polis.SelectedBuilding = hitBuilding;
		}
		#endregion
	}
}
