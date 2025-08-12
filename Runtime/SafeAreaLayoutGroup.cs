using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    public interface ICustomSafeAreaProvider{}
    
    [RequireComponent(typeof(RectTransform)), ExecuteAlways]
    public class SafeAreaLayoutGroup : MonoBehaviour, ILayoutGroup
    {
        public bool IsDrivingLayout => (Application.isPlaying || PreviewInEditor)
            && _canvas != null && _canvas.renderMode != RenderMode.WorldSpace;

#if UNITY_EDITOR
        public static bool PreviewInEditor = false;
#else
        public const bool PreviewInEditor = false;
#endif

        [SerializeField] private SafeAreaLayoutConfig _overrideGlobalLayoutConfig;

        public RectTransform SelfRectTransform => (RectTransform) transform;

        protected readonly Dictionary<RectTransform, Anchors> _childrenAnchors = new Dictionary<RectTransform, Anchors>();
        protected DrivenRectTransformTracker _drivenRectTransformTracker = new DrivenRectTransformTracker();
        protected readonly Vector3[] _worldCorners = new Vector3[4];
        protected Canvas _canvas;
        protected Rect _screenRect;
        private SafeAreaLayoutConfig _layoutConfig;

        private void Awake()
        {
            _layoutConfig = _overrideGlobalLayoutConfig == null ? SafeAreaLayoutProjectConfigProvider.Config : _overrideGlobalLayoutConfig; 
        }

        protected virtual void OnEnable()
        {
            _canvas = FindRootCanvas();
            RefreshChildrenAnchors();
        }

        protected virtual void OnDisable()
        {
            _canvas = null;
            ClearChildrenAnchors();
        }

        protected virtual void OnTransformChildrenChanged()
        {
            if (isActiveAndEnabled)
            {
                RefreshChildrenAnchors();
            }
        }

        protected virtual void OnTransformParentChanged()
        {
            if (isActiveAndEnabled)
            {
                _canvas = FindRootCanvas();
                RefreshChildrenAnchors();
            }
        }

        public virtual void SetLayoutHorizontal()
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

            Rect safeArea = GetSafeArea();
            float leftMargin = _layoutConfig.LeftMargin * Mathf.Max(0, safeArea.xMin - _screenRect.xMin) / horizontalSize;
            float rightMargin = _layoutConfig.RightMargin * Mathf.Max(0, _screenRect.xMax - safeArea.xMax) / horizontalSize;

            foreach ((RectTransform child, Anchors anchors) in _childrenAnchors)
            {
                anchors.WithHorizontalMargins(leftMargin, rightMargin).ApplyTo(child);
            }
        }

        public virtual void SetLayoutVertical()
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

            Rect safeArea = GetSafeArea();
            float bottomMargin = _layoutConfig.BottomMargin * Mathf.Max(0, safeArea.yMin - _screenRect.yMin) / verticalSize;
            float topMargin = _layoutConfig.TopMargin * Mathf.Max(0, _screenRect.yMax - safeArea.yMax) / verticalSize;

            foreach ((RectTransform child, _) in _childrenAnchors)
            {
                new Anchors(child).WithVerticalMargins(bottomMargin, topMargin).ApplyTo(child);
            }
        }

        public void ClearChildrenAnchors()
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
            var childrenToUntrack = new HashSet<RectTransform>(_childrenAnchors.Keys);
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
                childrenToUntrack.Remove(rectTransform);
            }

            foreach (RectTransform previousChild in childrenToUntrack)
            {
                _childrenAnchors.Remove(previousChild);
            }
            
            LayoutRebuilder.MarkLayoutForRebuild(SelfRectTransform);
        }

        protected virtual Rect GetSafeArea()
        {
            return SafeAreaUtility.GetSafeArea();
        }

        protected void RefreshScreenRect()
        {
            SelfRectTransform.GetWorldCorners(_worldCorners);

            Vector3 bottomLeft = _worldCorners[0];
            Vector3 topRight = _worldCorners[2];
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera != null)
            {
                Camera camera = _canvas.worldCamera;
                bottomLeft = camera.WorldToScreenPoint(bottomLeft);
                topRight = camera.WorldToScreenPoint(topRight);
            }
            _screenRect = Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
        }

        protected Canvas FindRootCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            LayoutRebuilder.MarkLayoutForRebuild(SelfRectTransform);
        }
#endif
    }
}
