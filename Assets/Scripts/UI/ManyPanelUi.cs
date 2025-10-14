using UnityEngine;
using System.Collections.Generic;

namespace LongLiveKhioyen
{
	public class ManyPanelUi : MonoBehaviour
	{
		public List<CanvasGroup> panels;

		public void SwitchToPanel(CanvasGroup target)
		{
			foreach(var group in panels)
				group.gameObject.SetActive(group == target);
		}
	}
}
