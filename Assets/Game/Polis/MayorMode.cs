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

		#region Private states
		Vector2 pointerPosition;
		bool isPrimaryButtonDown, isSecondaryButtonDown;
		bool isPointerOverGameObjects;
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
			pointerPosition = raw;
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

			if(!isPointerOverGameObjects)
				Interact(pointerPosition);
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
			static Vector3 ScreenToGround(Vector2 screen)
			{
				var ray = Camera.main.ScreenPointToRay(screen);
				var plane = new Plane(Vector3.up, Vector3.zero);
				if(plane.Raycast(ray, out float t))
					return ray.GetPoint(t);
				return default;
			}

			Vector3 worldDelta = ScreenToGround(pointerPosition - screenDelta) - ScreenToGround(pointerPosition);
			polis.mayorCamera.LookAt.position += worldDelta * panSpeed;
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

		void Interact(Vector2 screenPos)
		{
			var ray = Camera.main.ScreenPointToRay(screenPos);
			if(!Physics.Raycast(ray, out var hit, Mathf.Infinity))
				return;

			var hitBuilding = hit.collider.GetComponentInParent<Building>();
			polis.SelectedBuilding = hitBuilding;
		}
		#endregion
	}
}
