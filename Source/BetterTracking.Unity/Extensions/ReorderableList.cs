/// Credit Ziboo
/// Sourced from - http://forum.unity3d.com/threads/free-reorderable-list.364600/

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BetterTracking.Unity
{
    [RequireComponent(typeof(RectTransform)), DisallowMultipleComponent]
    [AddComponentMenu("UI/Extensions/Re-orderable list")]
    public class ReorderableList : MonoBehaviour
    {
        [Tooltip("Child container with re-orderable items in a layout group")]
        public LayoutGroup ContentLayout;
        [Tooltip("Parent area to draw the dragged element on top of containers. Defaults to the root Canvas")]
        public RectTransform DraggableArea;

        [Tooltip("Can items be dragged from the container?")]
        public bool IsDraggable = true;
        [Tooltip("Should the draggable components be removed or cloned?")]
        public bool CloneDraggedObject = false;

        [Tooltip("Can new draggable items be dropped in to the container?")]
        public bool IsDropable = true;
        

        [Header("UI Re-orderable Events")]
        public ReorderableListHandler OnElementDropped = new ReorderableListHandler();
        public ReorderableListHandler OnElementGrabbed = new ReorderableListHandler();
        public ReorderableListHandler OnElementRemoved = new ReorderableListHandler();
        public ReorderableListHandler OnElementAdded = new ReorderableListHandler();

        private RectTransform _content;
        private ReorderableListContent _listContent;

        private int _sortType;

        public RectTransform Content
        {
            get
            {
                if (_content == null)
                {
                    _content = ContentLayout.GetComponent<RectTransform>();
                }
                return _content;
            }
        }

        public int SortType
        {
            get { return _sortType; }
            set { _sortType = value; }
        }

        Canvas GetCanvas()
        {
            Transform t = transform;
            Canvas canvas = null;
        

            int lvlLimit = 100;
            int lvl = 0;

            while (canvas == null && lvl < lvlLimit)
            {
                canvas = t.gameObject.GetComponent<Canvas>();
                if (canvas == null)
                {
                    t = t.parent;
                }

                lvl++;
            }
            return canvas;
        }
        
        public void Init(LayoutGroup content, RectTransform area)
        {
            ContentLayout = content;

            DraggableArea = transform.root.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();

            _listContent = ContentLayout.gameObject.AddComponent<ReorderableListContent>();
            _listContent.Init(this);
        }
        
        #region Nested type: ReorderableListEventStruct

        [Serializable]
        public struct ReorderableListEventStruct
        {
            public GameObject DroppedObject;
            public int FromIndex;
            public ReorderableList FromList;
            public bool IsAClone;
            public GameObject SourceObject;
            public int ToIndex;
            public ReorderableList ToList;

            public void Cancel()
            {
                SourceObject.GetComponent<ReorderableListElement>().isValid = false;
            }
        }

        #endregion

        #region Nested type: ReorderableListHandler

        [Serializable]
        public class ReorderableListHandler : UnityEvent<ReorderableListEventStruct>
        {
        }
        
        #endregion
    }
}