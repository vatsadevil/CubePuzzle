using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace CubePuzzle
{
    public class MainMenuController : MonoBehaviour
    {
        Action OnPlayPressed;
        Action OnSettingsPressed;
        [SerializeField] GameObject rewardButton;
        [SerializeField] Text countDownText;
        private GameObject _drScreen;
        private bool _rewardReady = false;

        private void OnEnable() {
            _rewardReady = string.IsNullOrEmpty(DailyRewards.Instance.GetTimeRemaining());
            rewardButton.GetComponent<Animator>().SetBool("IsReady", _rewardReady);
        }
        public void PlayPressed()
        {
            OnPlayPressed?.Invoke();
            gameObject.SetActive(false);
            GameManager.Instance.StartLevel();
        } 
        
        public void SettingsPressed()
        {
            OnSettingsPressed?.Invoke();
            GameManager.Instance.UICon.SetupSettinsScreen();
        }

        public void DailyRewardPressed()
        {
            if (_rewardReady)
            {
                rewardButton.GetComponent<Animator>().SetBool("IsReady", false);
                _drScreen = Instantiate(Resources.Load<GameObject>("DailyRewardScreen"), transform.parent);
                rewardButton.SetActive(false);
            }
            else
            {
                rewardButton.GetComponent<Animator>().SetTrigger("OnClick");
            }
                
        }

        private void Update() 
        {
            if (!_rewardReady)
            {
                string val = DailyRewards.Instance.GetTimeRemaining();
                countDownText.text = "Remaining\n" + val;
                _rewardReady = string.IsNullOrEmpty(val);
                if(_rewardReady)
                {
                    OnEnable();
                }
            }
        }

    }
}