using UnityEngine;


namespace CubePuzzle
{
    [CreateAssetMenu(fileName = "VisualCubeDataSO", menuName = "VisualCube/VisualCubeDataSO", order = 0)]
    public class VisualCubeDataSO : ScriptableObject
    {
        public VCCategories Category;
        public string uniqueId;
        public string MeshName;
        public string IconImageName;
        public Sprite IconImage;
        public string MainTextureName;
        public int Price = 0;
        public int UnlocksAtLevel = 0;
    }
}