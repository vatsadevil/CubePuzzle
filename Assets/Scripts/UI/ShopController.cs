using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace CubePuzzle
{
    public class ShopController : MonoBehaviour
    {
        private const string OWNED_VCS_PREF = "owned_vcs_pref";

        [SerializeField] Text coinCountText;
        [SerializeField] VisualCubeCategoryDataSO VCData;
        [SerializeField] Transform buyWithCoinSection;
        [SerializeField] Transform unlockByAdsSection;
        [SerializeField] Transform unlockByProgressSection;
        [SerializeField] Transform unlockFromGiftboxSection;

        private int _coinsAvailable;
        private List<string> _ownedVCs = new List<string>();

        private void OnEnable() 
        {
            GameManager.Instance.BlockInput = true;
            _coinsAvailable = GameManager.Instance.CurrentCoins;
            coinCountText.text = "Coins: "+GameManager.Instance.CurrentCoins;
            GetOwnedVCs();
            SetupSection1(buyWithCoinSection, VCData.CubeVisuals[0]);
        }

        

        private void SetupSection1(Transform sectionParent, VisualCubeCategory data)
        { 
            for (int i = 0; i < data.CubeVisuals.Count; i++)
            {
                GameObject btn = Instantiate(Resources.Load<GameObject>("VCButton"), sectionParent);
                btn.GetComponentInChildren<Text>().text = "BUY:" + data.CubeVisuals[i].Price;
                btn.GetComponentInChildren<Image>().sprite = data.CubeVisuals[i].IconImage;

                //check if already owned. 
                //Check if affordable. Then add listener
                if(IsOwned(data.CubeVisuals[i].uniqueId))
                {
                    btn.GetComponentInChildren<Text>().text = "OWNED";
                }
                else if (data.CubeVisuals[i].Price <= _coinsAvailable)
                {
                    Debug.Log("unique id "+data.CubeVisuals[i].uniqueId);
                    string id = data.CubeVisuals[i].uniqueId;
                    Debug.Log("id is "+id);
                    btn.GetComponent<Button>().onClick.AddListener(delegate { BuyCube(id); });
                }
            }
        }

        private void OnDisable()
        {
            GameManager.Instance.BlockInput = false;
        }

        public void Close()
        {
            Destroy(gameObject);
        }

        public void BuyCube(string id)
        {
            Debug.Log("Cube pressed "+id);
            AddToOwnedVCs(id);
        }

        private void GetOwnedVCs()
        {
            string data = PlayerPrefs.GetString(OWNED_VCS_PREF, "");
            Debug.Log("owned vc data " + data);
            _ownedVCs = data.Split('$').ToList();
        }

        private void AddToOwnedVCs(string id)
        {
            string data = PlayerPrefs.GetString(OWNED_VCS_PREF, "");
            data += id + "$";
            PlayerPrefs.SetString(OWNED_VCS_PREF, data);
            GetOwnedVCs();
        }

        private bool IsOwned(string id)
        {
            return _ownedVCs.Contains(id);
        }

    }
}