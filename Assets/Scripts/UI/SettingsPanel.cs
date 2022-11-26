using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CubePuzzle
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] Slider slider;
        [SerializeField] Text valText;
        [SerializeField] CubeDimesionsSO dimesions;
        private LevelGenerator _levelGen;

        private void OnEnable() 
        {
            _levelGen = GetComponent<LevelGenerator>();
        }
        private void Start() 
        {
            slider.onValueChanged.AddListener(ValueChanged);
            valText.text = slider.value.ToString();
        }

        private void ValueChanged(float val)
        {
            _levelGen.ClearCache();
            valText.text = slider.value.ToString();
            
            dimesions.LevelWidth = (int)val;
            dimesions.LevelHeight = (int)val;
            dimesions.LevelLength = (int)val;
        }
    }

}