using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CubePuzzle
{
    public class HUDController : MonoBehaviour
    {
        [SerializeField] Text levelText;
        private GameObject _shop;

        private void OnEnable()
        {
            levelText.text = "Level " + GameManager.Instance.CurrentLevel;
        }
        public void MenuPressed()
        {
            GameManager.Instance.LevelGen.ClearLevelCube();
            GameManager.Instance.UICon.SetupMainMenu();
        }
        public void RestartPressed()
        {
            GameManager.Instance.LevelGen.GenerateLevelCube();
        }
        public void ShopPressed()
        {
            _shop = Instantiate(Resources.Load<GameObject>("ShopScreen"), transform.parent);
            _shop.transform.SetAsLastSibling();
        }
    }
}