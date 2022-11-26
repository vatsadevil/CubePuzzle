using UnityEngine;
using System;
using System.Globalization;

namespace CubePuzzle
{
    public class DailyRewards : MonoBehaviour
    {
        public static DailyRewards Instance;
        private const string LAST_OPEN_DATE = "last_reward_open_date";
        private const string NEXT_REWARD_DATE = "next_reward_date";
        private DateTime _nextRewardDate;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            _nextRewardDate = DateTime.Today;
            string rewDate = PlayerPrefs.GetString(NEXT_REWARD_DATE, "");
            if (!string.IsNullOrEmpty(rewDate))
            {

                _nextRewardDate = DateTime.ParseExact(rewDate, "ddMMyy", CultureInfo.InvariantCulture);
            }

        }

        private void OnDisable()
        {
            // System.DateTime dt = System.DateTime.Now();

        }

        public string GetTimeRemaining()
        {
            TimeSpan remaining = _nextRewardDate - DateTime.Now;
            if (_nextRewardDate > DateTime.Now)
            {
                string h = remaining.Hours.ToString().PadLeft(2, '0');
                string m = remaining.Minutes.ToString().PadLeft(2, '0');
                string s = remaining.Seconds.ToString().PadLeft(2, '0');
                string str = string.Format("<size=200>{0} : {1} : {2}</size>", h, m, s);
                return str;
            }

            return string.Empty;
        }

        public void UpdateLastOpenedDate()
        {
            string lastDate = DateTime.Now.ToString("ddMMyy");
            PlayerPrefs.SetString(LAST_OPEN_DATE, lastDate);

            string nextRewardDate = DateTime.Now.AddDays(1).ToString("ddMMyy");
            PlayerPrefs.SetString(NEXT_REWARD_DATE, nextRewardDate);
        }
    }
}