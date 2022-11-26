using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CubePuzzle
{
    public class LevelEndController : MonoBehaviour
    {
        private const float WAIT_TIME = 2.0f;
        [SerializeField] Text coinText;
        [SerializeField] Text movesText;
        [SerializeField] Image progressBarContainer;
        [SerializeField] Image progressBarImg;
        [SerializeField] GameObject nextButtonContainer;

        private void OnEnable()
        {
            nextButtonContainer.SetActive(false);
            StartCoroutine(WaitBeforeShowNext());
        }

        public void SetData(int coins, int moves)
        {
            coinText.text = coins + " Coins";
            movesText.text = moves + " Moves";
        }

        public void VideoButtonPressed()
        {

        }
        public void NextButtonPressed()
        {
            GameManager.Instance.UICon.SetupGameScreen();
            GameManager.Instance.StartLevel();

        }
        public void MenuButtonPressed()
        {
            GameManager.Instance.UICon.SetupMainMenu();
        }

        IEnumerator WaitBeforeShowNext()
        {
            yield return new WaitForSeconds(WAIT_TIME);
            nextButtonContainer.SetActive(true);
        }
    }
}