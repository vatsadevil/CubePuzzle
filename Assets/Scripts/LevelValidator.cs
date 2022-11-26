using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubePuzzle
{
    public class LevelValidator : MonoBehaviour
    {
        private CubeDimesionsSO _dims;
        private LevelGenerator _levelGen;
        private List<Cell> _allCells;
        public void Init(LevelGenerator gen)
        {
            _levelGen = gen;
        }
        

        public IEnumerator Validate(System.Action<bool> OnComplete)
        {

            _allCells = _levelGen.GetAllCellsDebug();
            while (_allCells.Count > 0)
            {
                bool removed = false;
                int count = _allCells.Count;
                for (int i = 0; i < _allCells.Count; i++)
                {
                    Cell c = _allCells[i];
                    removed = c.DebugMove();
                    if (removed)
                    {
                        yield return new WaitForEndOfFrame();
                        break;
                    }
                }

                _allCells = _levelGen.GetAllCellsDebug();
                if(count <= _allCells.Count)
                {
                    Debug.Log("Nothing was removed " + count);
                    OnComplete.Invoke(false);
                    yield break;
                }
                yield return new WaitForEndOfFrame();
            }

            OnComplete.Invoke(true);
        }

        public bool QuickValidate()
        {
            _allCells = _levelGen.GetAllCellsDebug();
            while (_allCells.Count > 0)
            {
                bool removed = false;
                int count = _allCells.Count;
                for (int i = 0; i < _allCells.Count; i++)
                {
                    Cell c = _allCells[i];
                    removed = c.DebugMove();
                    if (removed)
                    {
                        break;
                    }
                }

                _allCells = _levelGen.GetAllCellsDebug();
                if (count <= _allCells.Count)
                {
                    Debug.Log("Nothing was removed " + count);
                    return false;
                }
            }
            return true;
        }
    }
}