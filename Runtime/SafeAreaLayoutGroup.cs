using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class SafeAreaLayoutGroup : MonoBehaviour, ILayoutGroup
    {
#if UNITY_EDITOR
        public static bool PreviewInEditor = false;
#else
        public const bool PreviewInEditor = false;
#endif

        [SerializeField] private SafeAreaLayoutConfig _overrideGlobalLayoutConfig;
        [FormerlySerializedAs("_customMargins")] [FormerlySerializedAs("_marginsProviders")] [SerializeField] private MarginsProviderBase[] _additionalMargins;
        private readonly Dictionary<RectTransform, Anchors> _childrenAnchors = new();
        private readonly HashSet<RectTransform> _childrenToUntrack = new();
        private DrivenRectTransformTracker _drivenRectTransformTracker;
        private readonly Vector3[] _worldCorners = new Vector3[4];
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;
        private Rect _screenRect;
        private SafeAreaLayoutConfig _layoutConfig;
        
        private bool IsDrivingLayout => (Application.isPlaying || PreviewInEditor)
                                       && _canvas != null && _canvas.renderMode != RenderMode.WorldSpace;
        
        public RectTransform RectTransform => (RectTransform)transform;

        private void Awake()
        {
            _layoutConfig = _overrideGlobalLayoutConfig == null ? SafeAreaLayoutProjectConfigProvider.Config : _overrideGlobalLayoutConfig;
            if (_additionalMargins != null)
            {
                foreach (MarginsProviderBase marginsProvider in _additionalMargins)
                {
                    if (marginsProvider == null)
                    {
                        continue;
                    }
                    marginsProvider.SetLayoutGroup(this);
                }
            }
        }

        private void OnEnable()
        {
            if (_canvas == null)
            {
                RefreshCanvas();
            }
        }

        private void OnDisable()
        {
            _canvas = null;
            ClearChildrenAnchors();
        }

        private void OnTransformChildrenChanged()
        {
            if (isActiveAndEnabled)
            {
                RefreshChildrenAnchors();
            }
        }

        private void OnTransformParentChanged()
        {
            if (isActiveAndEnabled)
            {
               RefreshCanvas();
            }
            else
            {
                _canvas = null;
            }
        }

        private void RefreshCanvas()
        {
            _canvas = FindRootCanvas();
            _canvasRectTransform = _canvas == null ? null : _canvas.GetComponent<RectTransform>();
            RefreshChildrenAnchors();
        }

        public void SetLayoutHorizontal()
        {
            if (!IsDrivingLayout)
            {
                return;
            }

            RefreshScreenRect();
            float horizontalSize = _screenRect.size.x;
            if (horizontalSize <= 0)
            {
                return;
            }

            Rect safeArea = SafeAreaUtility.GetSafeArea();
            float leftMargin = _layoutConfig.LeftMargin * Mathf.Max(0, safeArea.xMin - _screenRect.xMin);
            float rightMargin = _layoutConfig.RightMargin * Mathf.Max(0, _screenRect.xMax - safeArea.xMax);
            
            if (_additionalMargins != null)
            {
                foreach (var marginsProvider in _additionalMargins)
                {
                    if (marginsProvider == null)
                    {
                        continue;
                    }
                    leftMargin = Mathf.Max(leftMargin, marginsProvider.LeftMargin);
                    rightMargin = Mathf.Max(rightMargin, marginsProvider.RightMargin);
                }
            }

            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.WithHorizontalMargins(leftMargin / horizontalSize, rightMargin / horizontalSize).ApplyTo(child);
            }
        }

        public void SetLayoutVertical()
        {
            if (!IsDrivingLayout)
            {
                return;
            }

            float verticalSize = _screenRect.size.y;
            if (verticalSize <= 0)
            {
                return;
            }

            Rect safeArea = SafeAreaUtility.GetSafeArea();

            float bottomMargin = _layoutConfig.BottomMargin * Mathf.Max(0, safeArea.yMin - _screenRect.yMin);
            float topMargin = _layoutConfig.TopMargin * Mathf.Max(0, _screenRect.yMax - safeArea.yMax);

            if (_additionalMargins != null)
            {
                foreach (var marginsProvider in _additionalMargins)
                {
                    if (marginsProvider == null)
                    {
                        continue;
                    }
                    bottomMargin = Mathf.Max(bottomMargin, marginsProvider.BottomMargin);
                    topMargin = Mathf.Max(topMargin, marginsProvider.TopMargin);
                }
            }

            foreach ((RectTransform child, _) in _childrenAnchors)
            {
                new Anchors(child).WithVerticalMargins(bottomMargin / verticalSize, topMargin / verticalSize).ApplyTo(child);
            }
        }

        private void ClearChildrenAnchors()
        {
            _drivenRectTransformTracker.Clear();
            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.ApplyTo(child);
            }

            _childrenAnchors.Clear();
        }

        public void RefreshChildrenAnchors()
        {
            if (!IsDrivingLayout)
            {
                ClearChildrenAnchors();
                return;
            }

            _drivenRectTransformTracker.Clear();
            _childrenToUntrack.Clear();
            
            foreach (var children in _childrenAnchors)
            {
                _childrenToUntrack.Add(children.Key);
            }
            
            foreach (Transform child in transform)
            {
                if (!(child is RectTransform rectTransform)
                    || child.TryGetComponent(out ILayoutIgnorer ignorer) && ignorer.ignoreLayout)
                {
                    continue;
                }

                _drivenRectTransformTracker.Add(this, rectTransform, DrivenTransformProperties.Anchors);
                if (!_childrenAnchors.ContainsKey(rectTransform))
                {
                    _childrenAnchors[rectTransform] = new Anchors(rectTransform);
                }

                _childrenToUntrack.Remove(rectTransform);
            }

            foreach (RectTransform previousChild in _childrenToUntrack)
            {
                _childrenAnchors.Remove(previousChild);
            }

            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }

        private void RefreshScreenRect()
        {
            RectTransform.GetWorldCorners(_worldCorners);

            Vector3 bottomLeft = _worldCorners[0];
            Vector3 topRight = _worldCorners[2];
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera != null)
            {
                Camera canvasCamera = _canvas.worldCamera;
                bottomLeft = canvasCamera.WorldToScreenPoint(bottomLeft);
                topRight = canvasCamera.WorldToScreenPoint(topRight);
            }

            _screenRect = Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        public float ConvertToPixels(float uiUnits)
        {
            if (!IsDrivingLayout)
            {
                return uiUnits;
            }

            float pixelsScaleFactor = Screen.height / _canvasRectTransform.rect.height;
            return uiUnits * pixelsScaleFactor;
        }

        private Canvas FindRootCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
        }
#endif
    }
}