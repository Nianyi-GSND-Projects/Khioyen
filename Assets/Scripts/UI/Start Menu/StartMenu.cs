using UnityEngine;

namespace LongLiveKhioyen
{
	[RequireComponent(typeof(ManyPanelUi))]
	public class StartMenu : MonoBehaviour
	{
		ManyPanelUi panels;
		public CanvasGroup mainPanel;

		protected void Awake()
		{
			panels = GetComponent<ManyPanelUi>();
			panels.SwitchToPanel(mainPanel);
		}

		public void StartNewGame()
		{
			GameManager.StartNewGame();
		}

		public void ExitGame()
		{
			GameManager.ExitGame();
		}
	}
}
