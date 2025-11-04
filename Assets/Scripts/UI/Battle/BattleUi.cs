using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LongLiveKhioyen
{
    public class BattleUi : MonoBehaviour
    {
        #region Life cycle
        void Awake()
        {
            Battle.Instance.onInitialized += Initialize;
        }

        void Initialize()
        {
           
        }
        #endregion
        
        #region General
        public Battle battle;

        public void OpenPauseMenu()
        {
            GameInstance.Instance.OpenPauseMenu();
        }
        #endregion
        
        #region BottomPanel
        [Header("Bottom Panel")]
        public CanvasGroup ArrangementPanel;
        
        [Header("Bottom Button")]
        public GameObject ArrangementButtonPrefab;
        public RectTransform contentContainer;
        
        #endregion
        
        #region StageManagement
        public Button toArrangementButton;
        public Button toBattleButton;
        public void PreparationToArrangement()
        {
            toArrangementButton.gameObject.SetActive(false);
            OpenPanel(ArrangementPanel);
            toBattleButton.gameObject.SetActive(true);
            EnterArrangementMode();
        }

        public void ArrangementToBattle()
        {
            toBattleButton.gameObject.SetActive(false);
            ClosePanel(ArrangementPanel);
            ExitArrangementMode();
            EnterBattleMode();
        }

        public void ClosePanel(CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
        }
        
        public void OpenPanel(CanvasGroup canvasGroup)
        {
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1;
        }

        public void EnterArrangementMode()
        {
            battle.isInArrangementStage = true;
        }
        public void EnterBattleMode()
        {
            battle.isInBattleStage = true;
        }
        public void ExitArrangementMode()
        {
            battle.isInArrangementStage = false;
        }
        #endregion
    }
}
