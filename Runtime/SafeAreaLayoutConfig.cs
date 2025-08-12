using UnityEngine;

namespace Gilzoide.SafeAreaLayout
{
    [CreateAssetMenu(fileName = "SafeAreaLayoutConfig", menuName = "SafeAreaLayout/Config")]
    public class SafeAreaLayoutConfig : ScriptableObject
    {
        public float TopMargin = 1f;
        public float BottomMargin = 1f;
        public float LeftMargin = 1f;
        public float RightMargin = 1f;
    }
}