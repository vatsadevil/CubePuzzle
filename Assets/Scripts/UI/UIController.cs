using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubePuzzle
{
    public class UIController : MonoBehaviour
    {
        
        [SerializeField] TouchInputHandler TIH;
        [SerializeField] Transform ControlParent;
        [SerializeField] GameObject SettingPanel;
        [SerializeField] GameObject MainMenu;
        [SerializeField] GameObject LevelEndScreen;
        [SerializeField] GameObject HUD;
        [SerializeField] GameObject SettingsScreen;
        public bool IsRotating { get; private set; }
        public bool IsZooming { get; private set; }
        LevelGenerator _levelGen;
        Vector2 _lastDelta = Vector2.zero;
        float _prevZoomVal = -1;
        GameObject _tempParent;

        private void OnEnable() {
            _levelGen = GameManager.Instance.LevelGen;
            SetupMainMenu();
        }

        public void SetupCamera(float distFromlevel)
        {
            ControlParent.position = _levelGen.GetCenter();
            Camera.main.transform.parent = ControlParent;
            Camera.main.transform.localPosition = Vector3.back * (distFromlevel);
            ControlParent.transform.rotation = Quaternion.identity;
        }

        public void ResetPressed()
        {
             _levelGen.GenerateLevelCube(true);
        }
        
        public void NewLevelPressed()
        {
            _levelGen.GenerateLevelCube();
        }
        
        public void SettingsPressed()
        {
            SettingPanel.SetActive(true);
        }
        
        public void CloseSettingsPressed()
        { 
            SettingPanel.SetActive(false);
            NewLevelPressed();
        }

        public void ShowLevelEndScreen()
        {
            LevelEndScreen.SetActive(true);
        }

        public void HideGOPanel()
        {
            LevelEndScreen.SetActive(false);
        }

        public void RotateCam(CamTurnDir dir)
        {
            return;
            switch (dir)
            {
                
                case CamTurnDir.UP:
                    ControlParent.Rotate(ControlParent.right, 1.0f, Space.World);
                    break;
                case CamTurnDir.DOWN:
                    ControlParent.Rotate(ControlParent.right, -1.0f, Space.World);
                    break;
                case CamTurnDir.RIGHT:
                    ControlParent.Rotate(ControlParent.up, -1.0f, Space.World);
                    break;
                case CamTurnDir.LEFT:
                    ControlParent.Rotate(ControlParent.up, 1.0f, Space.World);
                    break;
            }
        }
        
        private void TouchRotateStart()
        {
            Debug.Log("Touch started");
            // _tempParent = GameManager.Instance.LevelGen.GetTempParent();
            // _preRotateAngle = ControlParent.transform.rotation;
            // _preRotateEuler = ControlParent.transform.rotation.eulerAngles;
        }
        private void TouchRotateEnd()
        {
            Debug.Log("Touch Ended");
            if(_tempParent == null) return;
            
            ControlParent.transform.parent = _tempParent.transform;
            _tempParent.transform.rotation = Quaternion.identity;
            ControlParent.transform.parent = null;


            while (_tempParent.transform.childCount > 0)
            {
                _tempParent.transform.GetChild(0).parent = null;
            }
            Destroy(_tempParent);
            _lastDelta = Vector3.zero;
            IsRotating = false;
        }

        private void TouchEnded()
        {

        }

        public void RotateCam(Vector2 rotationDelta)
        {
            IsRotating = true;
            if(_tempParent == null)
            {
                _tempParent = GameManager.Instance.LevelGen.GetTempParent();
            }
            // ControlParent.transform.rotation = _preRotateAngle;

            if(_lastDelta != rotationDelta)
            {
                Vector2 delta = rotationDelta - _lastDelta;
                _tempParent.transform.Rotate(ControlParent.transform.up, delta.x * 0.2f, Space.World);
                _tempParent.transform.Rotate(ControlParent.transform.right, -delta.y * 0.2f, Space.World);
                _lastDelta = rotationDelta;
            }
        }
        
        public void ZoomCam(float val)
        {
            if(_prevZoomVal == -1)
            {
                _prevZoomVal = Camera.main.fieldOfView;
                Debug.Log("zoom val "+_prevZoomVal);
            }
            IsZooming = true;
            Camera.main.fieldOfView = _prevZoomVal * val;

        }

        public void ZoomEnd()
        {
            IsZooming = false;
            _prevZoomVal = -1;
        }

        public void SetupMainMenu()
        {
            MainMenu.SetActive(true);
            HUD.SetActive(false);
            LevelEndScreen.SetActive(false);

            TIH.OnRotateStart -= TouchRotateStart;
            TIH.OnRotateEnd -= TouchRotateEnd;
            TIH.OnRotate -= RotateCam;
            TIH.OnZoom -= ZoomCam;
            TIH.OnZoomEnd -= ZoomEnd;
            TIH.OnValidTouchEnd -= TouchEnded;
        }
        public void SetupGameScreen()
        {
            MainMenu.SetActive(false);
            HUD.SetActive(true);
            LevelEndScreen.SetActive(false);

            TIH.OnRotateStart += TouchRotateStart;
            TIH.OnRotateEnd += TouchRotateEnd;
            TIH.OnRotate += RotateCam;
            TIH.OnZoom += ZoomCam;
            TIH.OnZoomEnd += ZoomEnd;
            TIH.OnValidTouchEnd += TouchEnded;
        }

        public void SetupLevelEndScreen(int moveCounts, int coinsEarned)
        {
            MainMenu.SetActive(false);
            HUD.SetActive(false);
            LevelEndScreen.SetActive(true);
            LevelEndScreen.GetComponent<LevelEndController>().SetData(coinsEarned, moveCounts);

            TIH.OnRotateStart -= TouchRotateStart;
            TIH.OnRotateEnd -= TouchRotateEnd;
            TIH.OnRotate -= RotateCam;
            TIH.OnZoom -= ZoomCam;
            TIH.OnZoomEnd -= ZoomEnd;
            TIH.OnValidTouchEnd -= TouchEnded;
        }

        public void SetupSettinsScreen()
        {
            SettingsScreen.SetActive(true);
            HUD.SetActive(false);
            LevelEndScreen.SetActive(false);
        }


    }

    public enum CamTurnDir
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
}