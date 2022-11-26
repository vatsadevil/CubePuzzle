using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CubePuzzle
{
    public class DRController : MonoBehaviour
    {
        [SerializeField] Animator boxAnim;
        [SerializeField] GameObject rewardImage;
        [SerializeField] GameObject claimButton;
        private bool _canClose = false;

        private void OnEnable() {
            _canClose = false;
            rewardImage.SetActive(false);
        }
        public void ClaimPressed()
        {
            claimButton.SetActive(false);
            boxAnim.gameObject.SetActive(false);
            rewardImage.SetActive(true);
            rewardImage.GetComponentInChildren<Text>().text = "10 Coins";
            StartCoroutine(PlayAnim());
        }

        private IEnumerator PlayAnim()
        {
            yield return new WaitForSeconds(0.10f);
            DailyRewards.Instance.UpdateLastOpenedDate();
            _canClose = true;
        }

        public void Close()
        {
            if(_canClose)
            {
                Destroy(gameObject);
            }
        }
    }
}