using UnityEngine;
using UnityEngine.EventSystems;

namespace CubePuzzle
{
    public class CamControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] UIController uiControler;
        [SerializeField] CamTurnDir dir;
        private bool _buttonHeld = false;
        public void OnPointerExit(PointerEventData eventData)
        {
            _buttonHeld = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _buttonHeld = false;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _buttonHeld = true;
        }

        private void Update() 
        {
            if(_buttonHeld)
            {
                uiControler.RotateCam(dir);
            }    
        }
    }
}