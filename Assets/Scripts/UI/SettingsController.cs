using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace CubePuzzle
{
    public class SettingsController : MonoBehaviour
    {
        public Action OnClosed;
        [SerializeField] Toggle soundToggle;
        [SerializeField] Toggle vibratioToggle;
        private const string SOUND_ENABLED_FLAG = "sound_enabled_flag";
        private const string VIBRATION_ENABLED_FLAG = "vibration_enabled_flag";

        private void OnEnable()
        {
            soundToggle.onValueChanged.AddListener(VolumeToggled);
            vibratioToggle.onValueChanged.AddListener(VibrationToggled);
            soundToggle.isOn = PlayerPrefs.GetInt(SOUND_ENABLED_FLAG, 1) == 1;
            vibratioToggle.isOn = PlayerPrefs.GetInt(VIBRATION_ENABLED_FLAG, 1) == 1;
        }

        private void VolumeToggled(bool val)
        {
            PlayerPrefs.SetInt(SOUND_ENABLED_FLAG, val ? 1 : 0);
        }

        private void VibrationToggled(bool val)
        {
            PlayerPrefs.SetInt(VIBRATION_ENABLED_FLAG, val ? 1 : 0);
        }

        public void NoAdsPressed()
        {

        }

        public void RateNowPressed()
        {

        }

        public void ClosePressed()
        {
            OnClosed?.Invoke();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            soundToggle.onValueChanged.RemoveAllListeners();
            vibratioToggle.onValueChanged.RemoveAllListeners();
        }


    }
}