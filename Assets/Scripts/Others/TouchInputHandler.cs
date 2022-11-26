using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CubePuzzle
{
    public class TouchInputHandler : MonoBehaviour
    {
        public Action OnRotateStart;
        public Action<Vector2> OnRotate;
        public Action OnRotateEnd;
        public Action OnValidTouchEnd;
        public Action<float> OnZoom;
        public Action OnZoomEnd;
        Vector2 _touchStartPos = Vector2.one * -1;
        float _initialDist = -1;
        bool _wasZooming = false;
        bool _wasTouching = false;
        bool _blockedTouch = false;

        private void Update()
        {
            if(GameManager.Instance.BlockInput) 
            {
                // _blockedTouch = true;
                return;
            }
            if (Input.touchCount == 2)
            {
                // if(_blockedTouch) return;
                // Debug.Log("touch count 2 _wasZooming "+_wasZooming);
                Vector2 t1Pos = Input.GetTouch(0).position;
                Vector2 t2Pos = Input.GetTouch(1).position;
                float dist = Vector2.Distance(t1Pos, t2Pos);

                if (_initialDist == -1)
                {
                    _initialDist = dist;
                }
                OnZoom?.Invoke((_initialDist / dist));
                _wasZooming = true;
                _wasTouching = false;
                CancelTouch();
                return;
            }
            else if (Input.touchCount == 1 || Input.GetMouseButton(0))
            {
                // Debug.Log("touch count 1 _wasZooming "+_wasZooming);
                if (_wasZooming) return;
                // if (_blockedTouch) return;
                _wasTouching = true;
                if (_touchStartPos.Equals(Vector2.one * -1))
                {
                    _touchStartPos = Input.mousePosition;
                    OnRotateStart?.Invoke();
                }
                float deltax = _touchStartPos.x - Input.mousePosition.x;
                float deltay = _touchStartPos.y - Input.mousePosition.y;
                float dist = Vector2.Distance(_touchStartPos, Input.mousePosition);
                if (dist > 10.0f)
                {
                    OnRotate?.Invoke(new Vector2(deltax, deltay));
                }

                return;
            }
            else if(Input.touchCount == 0 || Input.GetMouseButtonUp(0))
            {
                
                if(_wasZooming)
                {
                    _wasZooming = false;
                    if (_initialDist != -1)
                    {
                        _initialDist = -1;
                    }
                    OnZoomEnd.Invoke();
                    Debug.Log("touch zoom ended");
                    return;
                }
                if(_wasTouching)
                {
                    Debug.Log("was touching");
                    CancelTouch();
                }
                // _blockedTouch = false;

            }
        }

        private void CancelTouch()
        {
            if (_touchStartPos != Vector2.one * -1)
            {
                _touchStartPos = Vector2.one * -1;
                OnRotateEnd?.Invoke();
            }
            
            _wasTouching = false;
            Debug.Log("touch ended");
        }

    }
}