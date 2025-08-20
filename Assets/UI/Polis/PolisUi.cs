using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LongLiveKhioyen
{
	public class PolisUi : MonoBehaviour
	{
		#region Life cycle
		void Awake()
		{
			if(!polis.Initialized)
			{
				Debug.LogWarning("Cannot initialize polis UI, as the polis instance is not successfully initialized.");
				return;
			}

			UpdateTopBar();
			SwitchBottomPanel(normalPanel);
		}
		#endregion

		#region General
		public Polis polis;

		public void OpenPauseMenu()
		{
			GameManager.OpenPauseMenu();
		}

		public void DepartFromPolis()
		{
			GameInstance.Instance.DepartFromPolis();
		}
		#endregion

		#region Status Bar
		[Header("Status Bar")]
		public CanvasGroup statusBar;
		public TMP_Text polisName;
		public TMP_Text foodValue;
		public TMP_Text moneyValue;
		public TMP_Text knowledgeValue;

		void UpdateTopBar()
		{
			polisName.text = polis.Data.name;
			foodValue.text = polis.Controlled.economy.food.ToString();
			moneyValue.text = polis.Controlled.economy.money.ToString();
			knowledgeValue.text = polis.Controlled.economy.knowledge.ToString();
		}
		#endregion

		#region Bottom Area
		[Header("Bottom Area")]
		public CanvasGroup bottomArea;
		public CanvasGroup normalPanel;
		public CanvasGroup constructPanel;

		void SwitchBottomPanel(CanvasGroup panel)
		{
			bool flag = false;  // Record if any panels has been switched to.

			for(int i = 0, count = bottomArea.transform.childCount; i < count; ++i)
			{
				var child = bottomArea.transform.GetChild(i);
				bool active = child == panel.transform;
				child.gameObject.SetActive(active);
				flag |= active;
			}

			if(!flag)  // If nothing has been switched to, display the normal panel.
				SwitchBottomPanel(normalPanel);
		}

		void SetBottomAreaVisibiltiy(bool visible)
		{
			bottomArea.gameObject.SetActive(visible);
		}
		#endregion

		#region Side bar
		[Header("Side Bar")]
		public Image switchModeImage;
		public Sprite wanderModeIcon;
		public Sprite mayorModeIcon;

		public void SwitchMode()
		{
			polis.SwitchMode();
			switch(polis.CurrentMode)
			{
				case Polis.Mode.Mayor:
					switchModeImage.sprite = wanderModeIcon;
					SetBottomAreaVisibiltiy(true);
					break;
				case Polis.Mode.Wander:
					ExitConstructModal();
					switchModeImage.sprite = mayorModeIcon;
					SetBottomAreaVisibiltiy(false);
					break;
			}
		}
		#endregion

		#region Construction
		public void EnterConstructModal()
		{
			SwitchBottomPanel(constructPanel);
			polis.IsInConstructModal = true;
		}

		public void ExitConstructModal()
		{
			SwitchBottomPanel(normalPanel);
			polis.IsInConstructModal = false;
		}
		#endregion
	}
}
