using UnityEngine;
using System;
using System.Collections.Generic;

namespace CubePuzzle
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public UIController UICon;
        public LevelGenerator LevelGen;
        public int CurrentLevel { get; private set; }
        public int CurrentCoins { get; private set; }
        public bool BlockInput { get; set; }

        [SerializeField] CubeDimesionsSO dimesions;
        private const string CURR_LEVEL_PREF = "curr_level_id";
        private const string CURR_COIN_COUNT_PREF = "curr_coin_count_id";
        private int _moveCounter;

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

        private void OnEnable() {
            PlayerPrefs.SetInt(CURR_LEVEL_PREF, 0);
            PlayerPrefs.SetInt(CURR_COIN_COUNT_PREF, 1000);
            CurrentLevel = PlayerPrefs.GetInt(CURR_LEVEL_PREF, 0);
            CurrentCoins = PlayerPrefs.GetInt(CURR_COIN_COUNT_PREF, 0);
            Debug.Log("coins "+CurrentCoins);
            Debug.Log("CurrentLevel "+CurrentLevel);
        }

        public void StartLevel()
        {
            CurrentLevel = PlayerPrefs.GetInt(CURR_LEVEL_PREF, 1);
            SetDimensionsForLevel(CurrentLevel);
            LevelGen.GenerateLevelCube();
            _moveCounter = 0;
            UICon.SetupGameScreen();
            

            // GenerateLevel();
        }
        public void RegisterMove()
        {
            _moveCounter++;
        }
        public void LevelEnded()
        {
            int coinsEarned = 10;
            PlayerPrefs.SetInt(CURR_COIN_COUNT_PREF, CurrentCoins + coinsEarned);
            CurrentCoins = PlayerPrefs.GetInt(CURR_COIN_COUNT_PREF, 0);
            Debug.Log("levelend coins " + CurrentCoins);
            UICon.SetupLevelEndScreen(_moveCounter, coinsEarned);
            PlayerPrefs.SetInt(CURR_LEVEL_PREF, CurrentLevel + 1);
        }

        private void SetDimensionsForLevel(int level)
        {
            int size = 3;
            if(level < 20)
            {
                size = 3;
            }
            else if(level < 30)
            {
                size = 4;
            }
            else if (level < 40)
            {
                size = 5;
            }
            else
            {
                size = 6;
            }

            dimesions.LevelWidth = size;
            dimesions.LevelHeight = size;
            dimesions.LevelLength = size;
        }


        //Level generation helpers
        [SerializeField] Generator generator;
        [SerializeField] JsonController jCon;
        public const int TOTAL_LEVELS_TO_GENERATE = 50;
        private int totalLevelsGenerated = 0;
        
        private void GenerateLevel()
        {
            jCon.InitForGeneration();
            GenerateNext();
        }

        private void GenerateNext()
        {
            Debug.Log("Generating level " + totalLevelsGenerated);
            SetDimensionsForLevel(totalLevelsGenerated);
            StartCoroutine(generator.DebugGeneration2(OnGenerationComplete));
        }

        private void OnGenerationComplete(List<CellPlacementData> cells)
        {
            Debug.Log("Saving level " + totalLevelsGenerated);
            jCon.AppendLevelData(totalLevelsGenerated, cells);

            totalLevelsGenerated++;
            if(totalLevelsGenerated < TOTAL_LEVELS_TO_GENERATE)
            {
                GenerateNext();
            }
            else
            {
                jCon.WriteToJson();
                Debug.Log("All Done!");
            }
        }

        private void Update() 
        {
            if(Input.GetKeyDown(KeyCode.G))
            {
                GenerateLevel();
            }
        }
    }
}