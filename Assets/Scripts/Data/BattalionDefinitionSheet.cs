using UnityEngine;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
    [CreateAssetMenu(menuName = "Long Live Khioyen/Battalion Definition Sheet")]
    public class BattalionDefinitionSheet : ScriptableObject
    {
        public List<BattalionDefinition> battalionDefinitions = new();
    }
}