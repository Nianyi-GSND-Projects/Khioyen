using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LongLiveKhioyen
{
    public class Battalion : MonoBehaviour
    {
        public BattalionCompilation Compilation { get; set; }
        public BattalionDefinition Definition { get; set; }
        
        
        #region Life cycle
        protected void Start()
        {
            name = Definition.battalionId;
            model = Instantiate(Definition.ModelTemplate);
            model.name = "Model";
            model.transform.SetParent(transform, false);
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
                legacyMaterials[renderer] = renderer.sharedMaterials;
            selectingMaterial = Resources.Load<Material>("Materials/Polis/Construction_site");
            actionDoneMaterial= Resources.Load<Material>("Materials/Polis/Construction_site");
            // TODO:改成实际材质
            Vector3 size = new(1, 1, 1);
            Vector3 center = new(0,0,0);
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = size;
            collider.center = center;
            UpdateVisualState();
        }
        #endregion

        #region Visual state
        GameObject model;
        readonly Dictionary<Renderer, Material[]> legacyMaterials = new();
        
        Material selectingMaterial;
        
        Material actionDoneMaterial;

        public void UpdateVisualState()
        {
            if(Compilation.selected)
            {
                foreach(var renderer in legacyMaterials.Keys)
                    renderer.sharedMaterial = selectingMaterial;
            }
            else if (Compilation.actionDone)
            {
                foreach(var renderer in legacyMaterials.Keys)
                    renderer.sharedMaterial = actionDoneMaterial;
            }
            else
            {
                foreach(var (renderer, mats) in legacyMaterials)
                    renderer.sharedMaterials = mats;
            }
        }

        public bool Selected
        {
            get => Compilation.selected;
            set
            {
                Compilation.selected = value;
                UpdateVisualState();
            }
        }

        public bool ActionDone
        {
            get => Compilation.actionDone;
            set
            {
                Compilation.actionDone = value;
                UpdateVisualState();
            }
        }
        #endregion
    }
}
