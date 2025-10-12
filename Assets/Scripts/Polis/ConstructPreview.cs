using UnityEngine;

namespace LongLiveKhioyen
{
	public class ConstructPreview : MonoBehaviour
	{
		public GameObject model;
		public System.Action onInitialized;

		protected void Awake()
		{
			validMaterial = Resources.Load<Material>("Materials/Polis/Construction_preview-valid");
			invalidMaterial = Resources.Load<Material>("Materials/Polis/Construction_preview-invalid");
		}

		protected void Start()
		{
			onInitialized?.Invoke();
		}

		public BuildingDefinition Definition
		{
			set
			{
				model = Instantiate(value.ModelTemplate);
				model.transform.SetParent(transform, false);
			}
		}

		public bool Visible
		{
			get => model.activeSelf;
			set => model.SetActive(value);
		}

		Material validMaterial, invalidMaterial;
		bool valid = true;
		public bool Valid
		{
			get => valid;
			set
			{
				valid = value;

				var mat = valid ? validMaterial : invalidMaterial;
				foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
				{
					var arr = renderer.sharedMaterials;
					for(int i = 0; i < arr.Length; ++i)
						arr[i] = mat;
					renderer.sharedMaterials = arr;
				}
			}
		}
	}
}
