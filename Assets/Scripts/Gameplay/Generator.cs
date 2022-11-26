using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace CubePuzzle
{
    public class Generator : MonoBehaviour
    {
        [SerializeField] float offset;
        [SerializeField] private CubeDimesionsSO dimensions;
        [SerializeField] private LevelGenerator levelGen;
        private const string CUBE_CELL_OBJECT = "Cell";
        private const string CUBE_CELL_DEBUG_OBJECT = "DebugCell";
        private List<Cell> _allCells = new List<Cell>();
        private Vector3Int _currSpawnPos;
        private Facing _currSpawnFacing;
        private List<Vector3Int> _emptySpots = new List<Vector3Int>();
        private int _totalNeededCubes;

        //
        bool d_step = false;
        bool d_autoGen = false;
        List<GameObject> d_debugCells = new List<GameObject>();

        public List<Cell> GenerateNew()
        {
            _allCells.Clear();
            _totalNeededCubes = dimensions.LevelWidth * dimensions.LevelHeight * dimensions.LevelLength;
            for (int i = 0; i < dimensions.LevelLength; i++)
            {
                for (int j = 0; j < dimensions.LevelHeight; j++)
                {
                    for (int k = 0; k < dimensions.LevelWidth; k++)
                    {
                        _emptySpots.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            //Debug.Log("_totalNeededCubes "+_totalNeededCubes);
            _currSpawnPos = GetEmptyPos();
            _currSpawnFacing = null;
            for (int i = 0; i < _totalNeededCubes; i++)
            {
                StepNext2();
            }
            return _allCells;
        }

        private void StepNext()
        {
            GenerateCellAtPos(_currSpawnPos);

            if(_allCells.Count == _totalNeededCubes)
            {
                //Debug.Log("All Done");
                return;
            }

            Cell lastAdded = _allCells.Last();
            _currSpawnPos = lastAdded.GridPosition + lastAdded.FacingDirection.DirVector;
           
            // //Debug.Log("Checking currnet spawn pos " + _currSpawnPos);
            if(_currSpawnPos.x >= dimensions.LevelWidth || _currSpawnPos.x < 0
            || _currSpawnPos.y >= dimensions.LevelHeight  || _currSpawnPos.y < 0
            || _currSpawnPos.z  >= dimensions.LevelLength || _currSpawnPos.z < 0)
            {
                // //Debug.Log("Spawn pos out of bound " + _currSpawnPos);
                _currSpawnPos = GetEmptyPos();
            }
            else
            {
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(_currSpawnPos));
                if(occupant != null)
                {
                    // //Debug.Log("curr pos already occupied");
                    _currSpawnPos = GetEmptyPos(); // or get another adjacent position? 
                }
            }
            _emptySpots.Remove(_currSpawnPos);
        }

        //------------------------- GENERATION MEHTOD 1 ------------------------//
        /*
            1. find ~center/random(?) cell. 
            2. add to p1 list. 
            3. occupy p1 list. 
            4. iterate over all spawned cells and create a list of all adjacent empty cells ( non duplicate )
            5. add to p1 list
            6. Spawn with valid direction
            7. clear p1 list. 
            8. go to 4
        */

        public IEnumerator DebugGeneration(System.Action<List<Cell>> OnGenerated)
        {
            _allCells.Clear();
            _totalNeededCubes = dimensions.LevelWidth * dimensions.LevelHeight * dimensions.LevelLength;
            for (int i = 0; i < dimensions.LevelLength; i++)
            {
                for (int j = 0; j < dimensions.LevelHeight; j++)
                {
                    for (int k = 0; k < dimensions.LevelWidth; k++)
                    {
                        _emptySpots.Add(new Vector3Int(i, j, k));
                    }
                }
            }

            //Debug.Log("_totalNeededCubes " + _totalNeededCubes);
            _currSpawnPos = GetEmptyPos();
            _currSpawnFacing = null;

            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < _totalNeededCubes; i++)
            {
                // StepNext2();
                yield return StartCoroutine(StepNext2());
                d_step = false;
#if UNITY_EDITOR
                yield return new WaitUntil(() => d_step || d_autoGen);
#else
                // yield return new WaitForEndOfFrame();
#endif
            }
            OnGenerated?.Invoke(_allCells);
        }

        private IEnumerator StepNext2()
        {
            
            GenerateCellAtPos(_currSpawnPos);
            if (_allCells.Count == _totalNeededCubes)
            {
                //Debug.Log("All Done");
                // return;
                yield break;
            }

            List<Vector3Int> surfaceEmpties = new List<Vector3Int>();
            for (int i = 0; i < _allCells.Count; i++)
            {
                List<Vector3Int> emptySpots = GetAllEmptySpotsAround(_allCells[i].GridPosition);
                List<Vector3Int> uniqueEmpties = emptySpots.Except(surfaceEmpties).ToList();
                surfaceEmpties.AddRange(uniqueEmpties);
            }

            //For debugging
            // d_step = false;
            // yield return new WaitUntil(() => d_step || d_autoGen);

            // for (int i = 0; i < surfaceEmpties.Count; i++)
            // {
            //     PlaceDebugCellAtPos(surfaceEmpties[i]);
            // }

            // d_step = false;
            // yield return new WaitUntil(() => d_step || d_autoGen);
            // ClearDebugCells();
            //------------

            for (int i = 0; i < surfaceEmpties.Count; i++)
            {
                /*
                1. get all clear dirs
                2. pick one. 
                */
                Facing[] allDirections = GetAllDirections();
                List<Facing> validDirs = new List<Facing>();
                for (int j = 0; j < allDirections.Length; j++)
                {
                    bool isValidDir = IsPathClear(surfaceEmpties[i], allDirections[j]);
                    if(isValidDir)
                    {
                        validDirs.Add(allDirections[j]);
                    }
                }

                _currSpawnPos = surfaceEmpties[i];
                _currSpawnFacing = validDirs[Random.Range(0, validDirs.Count)];
                break;
            }
            //Debug.Log("Surface Empties: " + surfaceEmpties.Count);
        }


        //------------------------- GENERATION MEHTOD 2 ------------------------//
        /*
        1. Spawn all the cubes with random directions. Add to list (L1) 
        2. while(removable cells > 1) remove an open cell from L1
        3. if L1.Count >0 goto 4. else, all done.
        4. if no removalbe cells set a few to open direction. Goto 2
        */
        
        List<CellPlacementData> _unsorted = new List<CellPlacementData>();
        List<CellPlacementData> _sorted = new List<CellPlacementData>();

        // public IEnumerator DebugGeneration2(System.Action<List<Cell>> OnGenerated)
        public IEnumerator DebugGeneration2(System.Action<List<CellPlacementData>> OnGenerated)
        {
#if !UNITY_EDITOR
            d_autoGen = true;
#endif
            _allCells.Clear();
            _totalNeededCubes = dimensions.LevelWidth * dimensions.LevelHeight * dimensions.LevelLength;
            _unsorted.Clear();
            _sorted.Clear();
            for (int i = 0; i < dimensions.LevelLength; i++)
            {
                for (int j = 0; j < dimensions.LevelHeight; j++)
                {
                    for (int k = 0; k < dimensions.LevelWidth; k++)
                    {
                        Facing[] allFacings = GetAllDirections();
                        Facing dir = allFacings[Random.Range(0, allFacings.Length)];
                        Vector3Int pos = new Vector3Int(i, j, k);
                        CellPlacementData placementData = new CellPlacementData(pos, dir);
                        _unsorted.Add(placementData);
                        GenerateCellAtPos(pos, dir);
                    }
                }
            }
            //Debug.Log("Total cells "+_unsorted.Count);

            int countBeforeCleanup = _unsorted.Count;
            while (_unsorted.Count > 0)
            {
                countBeforeCleanup = _unsorted.Count;
                //Debug.Log("_unsortedCount at round start: " + countBeforeCleanup);

                // d_step = false;
                // yield return new WaitUntil(() => d_step || d_autoGen);

                for (int i = 0; i < _unsorted.Count; i++)
                {
                    CellPlacementData cpd = _unsorted[i];
                    bool isOpen = IsPathClear(cpd.Position, cpd.FacingDirection);
                    if (isOpen)
                    {
                        Cell c = _allCells.FirstOrDefault(c => c.GridPosition.Equals(cpd.Position));
                        if (c != null)
                        {
                            //debug highlight and pause
                            c.ToggleHighlight(true);
                            // d_step = false;
                            // yield return new WaitUntil(() => d_step || d_autoGen);
                            
                            _sorted.Add(cpd);
                            _unsorted[i] = null;

                            _allCells.Remove(c);
                            bool removed = c.DebugMove();
                            if (!removed)
                            {
                                //Debug.Log("Somethig wrong.");
                            }
                        }
                        else
                        {
                            //Debug.Log("something wrong. Couldn't find cell with grid pos "+cpd.Position);
                        }
                    }
                }
                _unsorted = _unsorted.Where(c => c != null).ToList();
                //Debug.Log("_unsortedCount: "+_unsorted.Count);

                if(_unsorted.Count > 0 && _unsorted.Count == countBeforeCleanup)
                {
                    //Debug.Log("Reached stuck state");
                    int cellsToOpen = (int)Mathf.Ceil(0.05f * _unsorted.Count);
                    //Debug.Log("Opening "+cellsToOpen+" cells");

                    // d_step = false;
                    // yield return new WaitUntil(() => d_step || d_autoGen);

                    List<CellPlacementData> outerCells = new List<CellPlacementData>();
                    for (int i = 0; i < _unsorted.Count; i++)
                    {
                        Facing[] allFacings = GetAllDirections();
                        for (int j = 0; j < allFacings.Length; j++)
                        {
                            Facing f = allFacings[j];
                            bool isOpen = IsPathClear(_unsorted[i].Position, f);
                            if (isOpen)
                            {
                                outerCells.Add(_unsorted[i]);
                                break;
                            }
                        }
                    }

                    //Debug.Log("Total outer cells "+outerCells.Count);
                    // d_step = false;
                    // yield return new WaitUntil(() => d_step || d_autoGen);

                    int cellsOpened = 0;
                    while(cellsOpened < cellsToOpen)
                    {
                        CellPlacementData randomOpenableCell = outerCells[Random.Range(0, outerCells.Count)];
                        Facing[] allFacings = GetAllDirections();

                        _allCells.FirstOrDefault(c => c.GridPosition.Equals(randomOpenableCell.Position)).ToggleHighlight(true);
                        
                        // d_step = false;
                        // yield return new WaitUntil(() => d_step || d_autoGen);

                        for (int i = 0; i < allFacings.Length; i++)
                        {
                            Facing f = allFacings[i];
                            bool isOpen = IsPathClear(randomOpenableCell.Position, f);
                            if (isOpen)
                            {
                                randomOpenableCell.FacingDirection = f;
                                cellsOpened++;
                                
                                //this is same as randomOpenableCell
                                CellPlacementData cellInUnsorted = _unsorted.FirstOrDefault(c => c.Position.Equals(randomOpenableCell.Position));
                                cellInUnsorted.FacingDirection = f;

                                Cell c = _allCells.FirstOrDefault(c => c.GridPosition.Equals(cellInUnsorted.Position));
                                _allCells.Remove(c);
                                bool removed = c.DebugMove();
                                if (!removed)
                                {
                                    //Debug.Log("Somethig wrong 1.");
                                }


                                // //Debug.Log("Generating new cell obj at " + randomOpenableCell.Position);
                                // GenerateCellAtPos(randomOpenableCell.Position, randomOpenableCell.FacingDirection);

                                c = _allCells.FirstOrDefault(c => c.GridPosition.Equals(randomOpenableCell.Position));
                                if(c == null)
                                {
                                    //Debug.Log("removed cell not in all cells");
                                }
                                else
                                {
                                    //Debug.Log("removed cell present in all cells");
                                }

                                //Debug.Log("Generating new cell obj at " + randomOpenableCell.Position);
                                GenerateCellAtPos(randomOpenableCell.Position, randomOpenableCell.FacingDirection);


                                break;
                            }
                        }

                        //Debug.Log("Cells opened "+cellsOpened);

                    }

                }
            }

            //Debug.Log("Total cells in sorted "+_sorted.Count);

            for (int i = 0; i < _sorted.Count; i++)
            {
                GenerateCellAtPos(_sorted[i].Position, _sorted[i].FacingDirection);
            }

            yield return new WaitForSeconds(0.1f);
            // OnGenerated?.Invoke(_allCells);
            OnGenerated?.Invoke(_sorted);
        }

        //------------------------------------------------------------------------------------------

        public List<Cell> GenerateLevelFromData(CellPlacementData[] data)
        {
            _allCells.Clear();
            for (int i = 0; i < data.Length; i++)
            {
                GenerateCellAtPos(data[i].Position, data[i].FacingDirection);
            }
            return _allCells;
        }
        private Vector3Int GetEmptyPos()
        {
            if(_emptySpots.Count==0) 
            {
                return Vector3Int.one * -1;
            }
            Vector3Int spot = _emptySpots[Random.Range(0, _emptySpots.Count)];
            _emptySpots.Remove(spot);
            return spot;
        }
        private void GenerateCellAtPos(Vector3Int pos, Facing dir = null)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>(CUBE_CELL_OBJECT));
            Cell cell = go.AddComponent<Cell>();
            Vector3Int gridPos = pos;
            dir = dir == null ? PickDirection(gridPos) : dir;
            cell.Init(
                levelGen,
                gridPos,
                dir,
                offset
            );
            _allCells.Add(cell);
        }

        private GameObject PlaceDebugCellAtPos(Vector3Int pos, Facing dir = null)
        {
            GameObject go = Instantiate(Resources.Load<GameObject>(CUBE_CELL_DEBUG_OBJECT));
            go.name = "DebugCell";
            Cell cell = go.AddComponent<Cell>();
            Vector3Int gridPos = pos;
            dir = dir == null ? PickDirection(gridPos) : dir;
            cell.Init(
                levelGen,
                gridPos,
                dir,
                offset
            );
            Destroy(cell);
            d_debugCells.Add(go);
            return go;
        }

        private Facing PickDirection(Vector3Int pos)
        {
            Facing facing = null;
            List<Facing> allFacings = new List<Facing>()
            {
                new Facing(DirectionName.RIGHT),
                new Facing(DirectionName.LEFT),
                new Facing(DirectionName.UP),
                new Facing(DirectionName.DOWN),
                new Facing(DirectionName.FRONT),
                new Facing(DirectionName.BACK)
            };

            
            bool faceToFace, pathClear;
            do
            {
                facing = allFacings[Random.Range(0, allFacings.Count)];
                allFacings.Remove(facing);
                //make sure its not face-to-face with another cell
                faceToFace = IsFaceToFace(pos, facing);
                //Make sure path is clear - front
                pathClear = IsPathClear(pos, facing);
                
                if (!faceToFace && pathClear)
                {
                    return facing;
                }
            } while (!(!faceToFace && pathClear) && allFacings.Count>0);
            return facing;

        }

        

        private bool IsFaceToFace(Vector3Int pos, Facing facing)
        {
            int numPositionsToCheck = 0;
            switch (facing.DirName)
            {
                case DirectionName.RIGHT:
                    numPositionsToCheck = dimensions.LevelWidth - pos.x - 1;
                    break;
                case DirectionName.LEFT:
                    numPositionsToCheck = pos.x;
                    break;
                case DirectionName.FRONT:
                    numPositionsToCheck = dimensions.LevelLength - pos.z - 1;
                    break;
                case DirectionName.BACK:
                    numPositionsToCheck = pos.z;
                    break;
                case DirectionName.UP:
                    numPositionsToCheck = dimensions.LevelHeight - pos.y - 1;
                    break;
                case DirectionName.DOWN:
                    numPositionsToCheck = pos.y;
                    break;
            }

            Vector3Int checkPos = pos;
            for (int i = 0; i < numPositionsToCheck; i++)
            {
                Vector3Int candidatePos = checkPos + facing.DirVector;
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(candidatePos));
                if(occupant != null)
                {
                    return occupant.FacingDirection.DirVector.Equals(facing.DirVector * -1);
                }
            }
            return false;
        }
        private bool IsPathClear(Vector3Int pos, Facing facing)
        {
            int numPositionsToCheck = 0;
            switch (facing.DirName)
            {
                case DirectionName.RIGHT:
                    numPositionsToCheck = dimensions.LevelWidth - pos.x - 1;
                    break;
                case DirectionName.LEFT:
                    numPositionsToCheck = pos.x;
                    break;
                case DirectionName.FRONT:
                    numPositionsToCheck = dimensions.LevelLength - pos.z - 1;
                    break;
                case DirectionName.BACK:
                    numPositionsToCheck = pos.z;
                    break;
                case DirectionName.UP:
                    numPositionsToCheck = dimensions.LevelHeight - pos.y - 1;
                    break;
                case DirectionName.DOWN:
                    numPositionsToCheck = pos.y;
                    break;
            }
            // ClearDebugCells();
            // PlaceDebugCellAtPos(pos, new Facing(DirectionName.UP));
            //Debug.Log("Checking if path clear from "+pos+" in dir "+facing.DirName.ToString());

            Vector3Int checkPos = pos;
            for (int i = 0; i < numPositionsToCheck; i++)
            {
                Vector3Int candidatePos = checkPos + facing.DirVector;
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(candidatePos));
                if(occupant != null)
                {
                    //Debug.LogFormat("Pos {0} is NOT free", candidatePos);
                    return false;
                }
                //Debug.LogFormat("Pos {0} is free. Not a part of {1} remaining cells", candidatePos, _allCells.Count);
                // PlaceDebugCellAtPos(candidatePos, new Facing(DirectionName.UP));
                checkPos = candidatePos;
            }
            return true;
        }

        private Facing[] GetAllDirections()
        {
            Facing up = new Facing(DirectionName.UP);
            Facing down = new Facing(DirectionName.DOWN);
            Facing left = new Facing(DirectionName.LEFT);
            Facing right = new Facing(DirectionName.RIGHT);
            Facing front = new Facing(DirectionName.FRONT);
            Facing back = new Facing(DirectionName.BACK);

            Facing[] allDirections = new Facing[] { up, down, left, right, front, back };
            return allDirections;
        }
        
        private bool HasEmptySpotsAround(Vector3Int pos)
        {
            foreach (Facing dir in GetAllDirections())
            {
                Vector3Int adjCell = pos + dir.DirVector;
                if (adjCell.x >= dimensions.LevelWidth || adjCell.x < 0
                    || adjCell.y >= dimensions.LevelHeight || adjCell.y < 0
                    || adjCell.z >= dimensions.LevelLength || adjCell.z < 0)
                {
                    continue;
                }
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(adjCell));
                if (occupant == null)
                {
                    return true;
                }
            }

            return false;
        }

        private List<Vector3Int> GetAllEmptySpotsAround(Vector3Int pos)
        {
            List<Vector3Int> emptyAdjacents = new List<Vector3Int>();
            foreach (Facing dir in GetAllDirections())
            {
                Vector3Int adjCell = pos + dir.DirVector;
                if (adjCell.x >= dimensions.LevelWidth || adjCell.x < 0
                    || adjCell.y >= dimensions.LevelHeight || adjCell.y < 0
                    || adjCell.z >= dimensions.LevelLength || adjCell.z < 0)
                {
                    continue;
                }
                Cell occupant = _allCells.FirstOrDefault(c => c.GridPosition.Equals(adjCell));
                if (occupant == null)
                {
                    emptyAdjacents.Add(adjCell);
                }
            }
            return emptyAdjacents;
        }

        private void Update() {
            if(Input.GetKeyDown(KeyCode.L))
            {
                d_step = true;
            }
            if(Input.GetKeyDown(KeyCode.A))
            {
                d_autoGen = !d_autoGen;
            }
            
        }

        public List<Cell> GetAllCells()
        {
            return _allCells;
        }

        private void ClearDebugCells()
        { 
            foreach (var item in d_debugCells)
            {
                if(item != null)
                    Destroy(item);
            }
            d_debugCells.Clear();
        }
    }

    public class Candidate
    {
        public Vector3Int Position;
        public List<Facing> OpenDirections;

        public Candidate(Vector3Int pos)
        {
            Position = pos;
            OpenDirections = new List<Facing>();
        }
    }
    
    [System.Serializable]
    public class CellPlacementData
    {
        public Vector3Int Position;
        public Facing FacingDirection;
        public CellPlacementData(Vector3Int pos, Facing facing)
        {
            Position = pos;
            FacingDirection = facing;
        }
    }
}