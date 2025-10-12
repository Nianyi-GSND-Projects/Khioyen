using UnityEngine;

namespace LongLiveKhioyen
{
	public class ConstructPreview : MonoBehaviour
	{
		public GameObject model;
		Material validMaterial, invalidMaterial;
		bool valid = true;

		protected void Awake()
		{
			validMaterial = Resources.Load<Material>("Materials/Polis/Construction_preview-valid");
			invalidMaterial = Resources.Load<Material>("Materials/Polis/Construction_preview-invalid");
		}

		public void SetBuildingType(BuildingDefinition definition)
		{
			model = Instantiate(definition.ModelTemplate);
			model.transform.SetParent(transform, false);

			Visible = Visible;
			Valid = Valid;
		}

		public bool Visible
		{
			get => model.activeSelf;
			set => model.SetActive(value);
		}

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
