using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace CubePuzzle
{
    public class JsonController : MonoBehaviour
    {
        public const string DATA_FILE_NAME = "LevelsData.json";
        private GameData _gameData;

        private void Awake() 
        {
            string str = Resources.Load<TextAsset>(Path.Combine("Data",Path.GetFileNameWithoutExtension(DATA_FILE_NAME))).ToString();
            _gameData = JsonUtility.FromJson<GameData>(str);
        }

        public CellPlacementData[] GetLevelData(int levelId)
        {
            // List<CellPlacementData> 
            LevelData ld = _gameData.AllLevels.FirstOrDefault(l => l.LevelId.Equals(levelId));
            Debug.Log("total levels "+_gameData.AllLevels.Length+" curr level "+levelId);
            foreach (var l in _gameData.AllLevels)
            {
                Debug.Log("has id "+l.LevelId);
            }
            if(ld != null)
            {
                
                return ld.Cells;
            }
            return null;
        }


        //Generation helpers
        public void InitForGeneration()
        {
            File.WriteAllText(Path.Combine(Application.dataPath, "Resources", "Data", DATA_FILE_NAME), "");
            _gameData = new GameData();
            _gameData.AllLevels = new LevelData[GameManager.TOTAL_LEVELS_TO_GENERATE];
        }

        public void AppendLevelData(int levelId, List<CellPlacementData> data)
        {
            LevelData ld = new LevelData();
            ld.LevelId = levelId;
            ld.Cells = data.ToArray();

            _gameData.AllLevels[levelId] = ld;
        }
        public void ReadFromDataPath()
        {
            string str = File.ReadAllText(Path.Combine(Application.dataPath, "Resources", "Data", DATA_FILE_NAME));
        }
        public void WriteToJson()
        {
            string str = JsonUtility.ToJson(_gameData);
            File.WriteAllText(Path.Combine(Application.dataPath, "Resources", "Data", DATA_FILE_NAME), str);
            Debug.Log("save str: "+str);
        }
    }

    [System.Serializable]
    public class GameData
    {
        public int CurrentLevel;
        public LevelData[] AllLevels;
    }

    [System.Serializable]
    public class LevelData
    {
        public int LevelId;
        public CellPlacementData[] Cells;
    }
}