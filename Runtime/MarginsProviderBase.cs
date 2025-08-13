using UnityEngine;

namespace Gilzoide.SafeAreaLayout
{
    public abstract class MarginsProviderBase : MonoBehaviour
    {
        public abstract float TopMargin { get; }
        public abstract float BottomMargin { get; }
        public abstract float LeftMargin { get; }
        public abstract float RightMargin { get; }

        public abstract void SetLayoutGroup(SafeAreaLayoutGroup safeAreaLayoutGroup);
    }
}