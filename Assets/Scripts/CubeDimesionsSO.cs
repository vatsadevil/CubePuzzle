using UnityEngine;
using UnityEditor;

namespace CubePuzzle
{
    [CreateAssetMenu(fileName = "CubeDimesionsSO", menuName = "Settings/CubeDimesionsSO", order = 0)]
    public class CubeDimesionsSO : ScriptableObject
    {
        public int LevelWidth;
        public int LevelHeight;
        public int LevelLength;
    }
}