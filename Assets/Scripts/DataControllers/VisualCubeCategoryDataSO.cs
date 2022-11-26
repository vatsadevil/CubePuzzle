using UnityEngine;
using System.Collections.Generic;

namespace CubePuzzle
{
    public enum VCCategories
    {
        COIN,
        ADS,
        PROGRESS,
        GIFT_BOX
    }

    [CreateAssetMenu(fileName = "VisualCubeCategorySO", menuName = "VisualCube/VisualCubeCategorySO", order = 1)]
    public class VisualCubeCategoryDataSO : ScriptableObject
    {
        public List<VisualCubeCategory> CubeVisuals;

    }

    [System.Serializable]
    public class VisualCubeCategory
    {
        public string CategoryName;
        public List<VisualCubeDataSO> CubeVisuals;
    }
}