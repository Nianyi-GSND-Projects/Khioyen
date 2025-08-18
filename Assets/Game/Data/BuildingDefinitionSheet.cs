using UnityEngine;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	[CreateAssetMenu(menuName = "Long Live Khioyen/Building Definition Sheet")]
	public class BuildingDefinitionSheet : ScriptableObject
	{
		public List<BuildingDefinitionData> buildingDefinitions = new();
	}
}
