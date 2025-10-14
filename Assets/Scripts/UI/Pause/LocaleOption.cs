using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.EventSystems;

namespace LongLiveKhioyen
{
	public class LocaleOption : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField] Button button;
		[SerializeField] TMP_Text text;

		Locale locale;
		public Locale Locale
		{
			get => locale;
			set
			{
				locale = value;
				text.text = LocalizationSettings.StringDatabase.GetLocalizedString(
					"Common Translations",
					"locale-name",
					locale
				);
			}
		}

		#region Life cycle
		protected void Start()
		{
			LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
			UpdateStatus();
		}

		protected void OnDestroy()
		{
			LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		}

		public void OnPointerClick(PointerEventData _)
		{
			LocalizationSettings.SelectedLocale = locale;
		}

		protected void OnLocaleChanged(Locale _)
		{
			UpdateStatus();
		}
		#endregion

		public void UpdateStatus()
		{
			Color = Selected ? selectedColor : unselectedColor;
		}

		bool Selected => LocalizationSettings.SelectedLocale == locale;

		static Color selectedColor = Color.yellow, unselectedColor = Color.white;
		Color Color
		{
			set
			{
				var colors = button.colors;
				colors.normalColor = value;
				colors.highlightedColor = value;
				colors.selectedColor = value;
				button.colors = colors;
			}
		}
	}
}
