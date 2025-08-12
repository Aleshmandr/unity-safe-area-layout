using UnityEngine;

namespace Gilzoide.SafeAreaLayout
{
    public static class SafeAreaLayoutProjectConfigProvider
    {
        private static SafeAreaLayoutConfig _cachedConfig;

        public static SafeAreaLayoutConfig Config
        {
            get
            {
                if (_cachedConfig != null)
                {
                    return _cachedConfig;
                }

                _cachedConfig = Resources.Load<SafeAreaLayoutConfig>(SafeAreaLayoutProjectConfigConstants.ResourceName);
                
#if !UNITY_EDITOR
                // In player/runtime, if no asset found in Resources, create an in-memory default
                if (_cachedConfig == null)
                {
                    _cachedConfig = ScriptableObject.CreateInstance<SafeAreaLayoutConfig>();
                    _cachedConfig.name = "RuntimeDefaultSafeAreaLayoutConfig";
                }
#endif

                return _cachedConfig;
            }
        }
    }
}