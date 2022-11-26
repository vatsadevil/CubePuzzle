using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

namespace CubePuzzle
{
    public class Cell : MonoBehaviour
    {
        [SerializeField] GameObject IdObject;
        private const string ID_OBJECT_NAME = "IdObject";
        
        public Vector3Int GridPosition { get; private set; }
        public Facing FacingDirection { get; private set; }

        private float _posOffset;
        private LevelGenerator _levelGen;
        private const float MOVE_SPEED = 0.2f;
        private const int EXIT_POS_DIST = 5;
        private const float BOP_ANIM_TIME = 0.4f;
        private const float BOP_TRIGGER_INTERVAL = 0.06f;

        private void OnEnable() 
        {
            IdObject = transform.Find(ID_OBJECT_NAME).gameObject;
        }
        
        public void Init(LevelGenerator gen, Vector3Int pos, Facing facing, float positionOffset)
        {
            _levelGen = gen;
            GridPosition = pos;
            FacingDirection = facing;
            _posOffset = positionOffset;
            transform.position = new Vector3(pos.x + ( pos.x * positionOffset), pos.y + ( pos.y * positionOffset), pos.z + ( pos.z * positionOffset));
            transform.rotation = Quaternion.LookRotation(facing.DirVector);
            UpdateGridPosition(GridPosition);

            

        }

        private void UpdateGridPosition(Vector3Int pos)
        {
            GridPosition = pos;
            IdObject.GetComponentInChildren<TextMesh>().text = pos.ToString();
        }

        private void OnMouseUp() {
            if(!GameManager.Instance.UICon.IsRotating && !GameManager.Instance.UICon.IsZooming)
            {
                MoveToNextAvailablePosition();
            }
        }

        private void OnMouseOver() 
        {
            // IdObject.SetActive(true);
        }

        private void OnMouseExit() 
        {
            // IdObject.SetActive(false);
        }

        public void MoveToNextAvailablePosition()
        {
            Debug.Log("movetonextavailablePos");
            Vector3Int NewGridPos = _levelGen.GetNextAvailableGridPos(this);
            
            if (NewGridPos.Equals(GridPosition)) 
            {
                StartCoroutine(FlashAllCells());
                return;
            }
            else if (NewGridPos.Equals(Vector3Int.one * -1))
            {
                GameManager.Instance.RegisterMove();
                // _levelGen.RemoveCell(this);
                Vector3 targetPos = transform.position + (FacingDirection.DirVector * EXIT_POS_DIST);
                _levelGen.MakeOrphan(this);
                StartCoroutine(Move(targetPos, () => { 
                    _levelGen.RemoveCell(this); 
                }));
            }
            else
            {
                GameManager.Instance.RegisterMove();
                Vector3 targetPos;
                //check if the new grid pos is an edge pos
                GridPosition = NewGridPos;
                // UpdateGridPosition(GridPosition);
                // Vector3Int edgeCheck =  _levelGen.GetNextAvailableGridPos(this);
                // if (edgeCheck.Equals(Vector3Int.one * -1))
                // {
                //     targetPos = transform.position + (FacingDirection.DirVector * EXIT_POS_DIST);
                //     StartCoroutine(Move(targetPos, () => { 
                //         _levelGen.RemoveCell(this); 
                //     }));
                //     return;
                // }
                targetPos = new Vector3(NewGridPos.x + (NewGridPos.x * _posOffset),
                                                NewGridPos.y + (NewGridPos.y * _posOffset),
                                                NewGridPos.z + (NewGridPos.z * _posOffset));
                StartCoroutine(Move(targetPos));
            }
        }

        private IEnumerator Move(Vector3 targetPos, Action OnComplete = null)
        {
            float dist = Vector3.Distance(gameObject.transform.position, targetPos);
            
            while (!Mathf.Approximately(dist, 0.0f))
            {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, targetPos, MOVE_SPEED);
                dist = Vector3.Distance(gameObject.transform.position, targetPos);
                yield return new WaitForEndOfFrame();
            }

            OnComplete?.Invoke();
        }

        public void Flash(Vector3 dir)
        {
            Animator anim = GetComponent<Animator>();
            anim.SetTrigger("TriggerFlash");

            Transform internalContainer = transform.GetChild(0);
            float bopDist = 0.30f;
            Vector3 targetPos = internalContainer.position + (dir * bopDist);
            Vector3 localStartPos = internalContainer.localPosition;
            Vector3 localTargetPos = transform.InverseTransformPoint(targetPos);
            internalContainer.DOLocalPath(new Vector3[] { localStartPos, localTargetPos, localStartPos }, BOP_ANIM_TIME);
            
        }

        private IEnumerator FlashAllCells()
        {
            // transform.Translate(transform.forward * 0.20f, Space.World);
            // GameManager.Instance.BlockInput = true;
            List<Cell> blockers = _levelGen.CellsInPath(this);
            for (int i = 0; i < blockers.Count; i++)
            {
                if (blockers[i] != null)
                {
                    blockers[i].Flash(transform.forward);
                    // blockers[i].transform.Translate(transform.forward * 0.20f, Space.World);
                    yield return new WaitForSeconds(BOP_TRIGGER_INTERVAL);
                }
            }
            yield return new WaitForSeconds(BOP_ANIM_TIME);
            // GameManager.Instance.BlockInput = false;

        }

        public bool DebugMove()
        {
            Vector3Int NewGridPos = _levelGen.GetNextAvailableGridPos(this);
            
            if (NewGridPos.Equals(GridPosition)) 
            {
                return false;
            }
            else if (NewGridPos.Equals(Vector3Int.one * -1))
            {
                GameManager.Instance.RegisterMove();
                // _levelGen.RemoveCell(this);
                Vector3 targetPos = transform.position + (FacingDirection.DirVector * EXIT_POS_DIST);
                _levelGen.MakeOrphan(this);
                _levelGen.RemoveCell(this);
                return true;
            }
            else
            {
                GameManager.Instance.RegisterMove();
                Vector3 targetPos;
                //check if the new grid pos is an edge pos
                GridPosition = NewGridPos;
                // UpdateGridPosition(GridPosition);
                // Vector3Int edgeCheck =  _levelGen.GetNextAvailableGridPos(this);
                // if (edgeCheck.Equals(Vector3Int.one * -1))
                // {
                //     targetPos = transform.position + (FacingDirection.DirVector * EXIT_POS_DIST);
                //     StartCoroutine(Move(targetPos, () => { 
                //         _levelGen.RemoveCell(this); 
                //     }));
                //     return;
                // }
                targetPos = new Vector3(NewGridPos.x + (NewGridPos.x * _posOffset),
                                                NewGridPos.y + (NewGridPos.y * _posOffset),
                                                NewGridPos.z + (NewGridPos.z * _posOffset));
                gameObject.transform.position = targetPos;
                return false;
            }
        }

        public void ToggleHighlight(bool show = true)
        {
            transform.Find("highlightObj")?.gameObject.SetActive(show);
        }

    }
    
    [System.Serializable]
    public class Facing
    {
        public Vector3Int DirVector;// { get; private set; }
        public DirectionName DirName;// { get; private set; }

        public Facing(DirectionName dir)
        {
            DirName = dir;
            switch (dir)
            {
                case DirectionName.FRONT:
                    DirVector = new Vector3Int(0, 0, 1);
                    break;
                case DirectionName.BACK:
                    DirVector = new Vector3Int(0, 0, -1);
                    break;
                case DirectionName.LEFT:
                    DirVector = new Vector3Int(-1, 0, 0);
                    break;
                case DirectionName.RIGHT:
                    DirVector = new Vector3Int(1, 0, 0);
                    break;
                case DirectionName.UP:
                    DirVector = new Vector3Int(0, 1, 0);
                    break;
                case DirectionName.DOWN:
                    DirVector = new Vector3Int(0, -1, 0);
                    break;
            }
        }
    }

    [System.Serializable]
    public enum DirectionName 
    {
        FRONT,
        BACK,
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
}