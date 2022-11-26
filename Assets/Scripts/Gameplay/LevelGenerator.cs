using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CubePuzzle
{
    public class LevelGenerator : MonoBehaviour
    {
        private const string CUBE_CELL_OBJECT = "Cell";
        [SerializeField] Transform levelParent;
        [SerializeField] CubeDimesionsSO dimesions;
        [SerializeField] float offset;

        private UIController _uiController;
        private LevelValidator _validator;
        private Generator _generator;
        private JsonController _jCon;
        private List<Cell> _allCells = new List<Cell>();
        private List<Cell> _cache = new List<Cell>();


        private void Start() 
        {

            _generator = GetComponent<Generator>();
            _jCon = GetComponent<JsonController>();

            _validator = GetComponent<LevelValidator>();
            _validator.Init(this);

            _uiController = GameManager.Instance.UICon;
            // GenerateLevelCube();
        }
        
        public void GenerateLevelCube(bool loadFromCache = false)
        {
            // bool valid = false;
            // int breaker = 0;
            // while (!valid && breaker<100)
            // {
            //     GenerateNewLevel(loadFromCache);
            //     valid = _validator.QuickValidate();
            //     breaker++;
            // }
            // Debug.Log("Valid " + valid+" breaker "+breaker);

            //For testing
            GenerateNewLevel(loadFromCache);

        }

        private void GenerateNewLevel(bool loadFromCache = false)
        {
            ClearLevelCube();
            if (!loadFromCache)
            {
                _cache.Clear();
                _allCells = _generator.GenerateLevelFromData(_jCon.GetLevelData(GameManager.Instance.CurrentLevel));
                _cache = new List<Cell>(_allCells);
            }
            else
            {
                if (_cache.Count == 0)
                {
                    loadFromCache = false;
                }
                else
                {
                    _allCells = new List<Cell>(_cache);
                }
            }

            // _allCells = _generator.GenerateNew();
            // StartCoroutine(_generator.DebugGeneration2(OnGenerationComplete));
            // _allCells = _generator.GenerateLevelFromData(_jCon.GetLevelData(GameManager.Instance.CurrentLevel));
            // _cache = new List<Cell>(_allCells);
            _uiController.SetupCamera(dimesions.LevelLength * 4.0f);
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.V))
            {
                _allCells = _generator.GetAllCells();
                StartCoroutine(_validator.Validate(OnValidationComplete));
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                GenerateLevelCube(true);
            }
        }
        
        private void OnGenerationComplete(List<CellPlacementData> cells)
        {
            // _allCells = cells;
        }
        private void OnValidationComplete(bool result)
        {
            Debug.Log("Validation success: "+result);
        }

        public Vector3 GetCenter()
        { 
            return new Vector3 (
                ((dimesions.LevelWidth-1) + ((dimesions.LevelWidth - 1) * offset)) / 2.0f,
                ((dimesions.LevelHeight-1) + ((dimesions.LevelHeight - 1) * offset)) / 2.0f,
                ((dimesions.LevelLength-1) + ((dimesions.LevelLength - 1) * offset)) / 2.0f
            );
        }
        public Vector3Int GetNextAvailableGridPos(Cell cell)
        {
            Vector3Int availablePos = cell.GridPosition;

            int numPositionsToCheck = 0;
            switch (cell.FacingDirection.DirName)
            {
                case DirectionName.RIGHT:
                    numPositionsToCheck = dimesions.LevelWidth - cell.GridPosition.x - 1;
                    break;
                case DirectionName.LEFT:
                    numPositionsToCheck = cell.GridPosition.x;
                    break;
                case DirectionName.FRONT:
                    numPositionsToCheck = dimesions.LevelLength - cell.GridPosition.z - 1;
                    break;
                case DirectionName.BACK:
                    numPositionsToCheck = cell.GridPosition.z;
                    break;
                case DirectionName.UP:
                    numPositionsToCheck = dimesions.LevelHeight - cell.GridPosition.y - 1;
                    break;
                case DirectionName.DOWN:
                    numPositionsToCheck = cell.GridPosition.y;
                    break;
            }

            if(numPositionsToCheck == 0)
            {
                return Vector3Int.one * -1;
            }

            for (int i = 0; i < numPositionsToCheck; i++)
            {
                Vector3Int candidatePos = availablePos + cell.FacingDirection.DirVector;
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(candidatePos));
                if(occupant == null)
                {
                    availablePos = candidatePos;
                    if(i == numPositionsToCheck - 1)
                    {
                        return Vector3Int.one * -1;
                    }
                }
                else
                {
                    break;
                }
            }
            
            return availablePos;
        }

        public List<Cell> CellsInPath(Cell cell)
        {
            List<Cell> cellsToFlash = new List<Cell>();
            cellsToFlash.Add(cell);
            Vector3Int availablePos = cell.GridPosition;

            int numPositionsToCheck = 0;
            switch (cell.FacingDirection.DirName)
            {
                case DirectionName.RIGHT:
                    numPositionsToCheck = dimesions.LevelWidth - cell.GridPosition.x - 1;
                    break;
                case DirectionName.LEFT:
                    numPositionsToCheck = cell.GridPosition.x;
                    break;
                case DirectionName.FRONT:
                    numPositionsToCheck = dimesions.LevelLength - cell.GridPosition.z - 1;
                    break;
                case DirectionName.BACK:
                    numPositionsToCheck = cell.GridPosition.z;
                    break;
                case DirectionName.UP:
                    numPositionsToCheck = dimesions.LevelHeight - cell.GridPosition.y - 1;
                    break;
                case DirectionName.DOWN:
                    numPositionsToCheck = cell.GridPosition.y;
                    break;
            }

            for (int i = 0; i < numPositionsToCheck; i++)
            {
                Vector3Int candidatePos = availablePos + cell.FacingDirection.DirVector;
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(candidatePos));
                availablePos = candidatePos;
                cellsToFlash.Add(occupant);
            }
            return cellsToFlash;
        }

        private void CheckGameEnd()
        {
            if(_allCells.Count == 0)
            {
                GameManager.Instance.LevelEnded();
            }
        }

        public void MakeOrphan(Cell cell)
        {
            _allCells.Remove(cell);
        }

        public void RemoveCell(Cell cell)
        {
            Destroy(cell.gameObject);
            CheckGameEnd();
        }
        
        public void ClearLevelCube()
        {
            foreach (var item in _allCells)
            {
                Destroy(item.gameObject);
            }
            _allCells.Clear();
        }

        public GameObject GetTempParent()
        {
            GameObject tempParent = new GameObject();
            tempParent.transform.position = GetCenter();
            tempParent.name = "TempParent";
            foreach (var item in _allCells)
            {
                item.transform.parent = tempParent.transform;
            }
            return tempParent;

        }

        public List<Cell> GetAllCellsDebug()
        {
            return _allCells;
        }

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}