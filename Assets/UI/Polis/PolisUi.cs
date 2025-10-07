using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace LongLiveKhioyen
{
	public class PolisUi : MonoBehaviour
	{
		#region Life cycle
		void Awake()
		{
			localizedPolisName = new("Polis Names", "");
			localizedPolisName.StringChanged += s => polisName.text = s;

			polis.onEconomyDataChanged += UpdateTopBar;
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
		LocalizedString localizedPolisName;
		public TMP_Text foodValue;
		public TMP_Text moneyValue;
		public TMP_Text knowledgeValue;

		void UpdateTopBar()
		{
			localizedPolisName.TableEntryReference = polis.Data.id;
			localizedPolisName.RefreshString();
			foodValue.text = polis.ControlledData.economy.food.ToString();
			moneyValue.text = polis.ControlledData.economy.money.ToString();
			knowledgeValue.text = polis.ControlledData.economy.knowledge.ToString();
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
