using UnityEngine;

namespace CubePuzzle
{
    public class GOPanel : MonoBehaviour
    {
        public void HidePanel()
        {
            Destroy(this.gameObject);
        }
    }
}