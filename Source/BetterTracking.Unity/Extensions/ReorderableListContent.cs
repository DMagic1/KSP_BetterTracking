/// Credit Ziboo
/// Sourced from - http://forum.unity3d.com/threads/free-reorderable-list.364600/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetterTracking.Unity
{
    public class ReorderableListContent : MonoBehaviour
    {
        private List<Transform> _cachedChildren;
        private List<ReorderableListElement> _cachedListElement;
        private ReorderableListElement _ele;
        private ReorderableList _extList;
        private RectTransform _rect;

        private void OnEnable()
        {
            if(_rect)StartCoroutine(RefreshChildren());
        }

        public void OnTransformChildrenChanged()
        {
            if (this.isActiveAndEnabled && _extList != null && _extList.SortType < 2)
                StartCoroutine(RefreshChildren());
        }

        public void Init(ReorderableList extList)
        {
            _extList = extList;
            _rect = GetComponent<RectTransform>();
            _cachedChildren = new List<Transform>();
            _cachedListElement = new List<ReorderableListElement>();

            StartCoroutine(RefreshChildren());
        }

        private IEnumerator RefreshChildren()
        {
            yield return new WaitForEndOfFrame();
            
            if (_rect.childCount > 0)
            {
                var first = _rect.GetChild(0);

                if (first != null && first.gameObject.name != "Fake")
                {
                    VesselGroup group = first.GetComponent<VesselGroup>();

                    int timer = 0;

                    while (group == null)
                    {
                        if (timer > 20)
                            yield break;

                        group = first.GetComponent<VesselGroup>();
                        timer++;

                        if (group == null)
                            yield return null;
                    }
                    
                    while (!group.Initialized || group.Header == null)
                    {
                        yield return null;
                    }
                    
                    //Handle new chilren
                    for (int i = 0; i < _rect.childCount; i++)
                    {
                        if (_rect.GetChild(i) == null)
                            continue;

                        if (_cachedChildren.Contains(_rect.GetChild(i)))
                            continue;

                        VesselGroup child = _rect.GetChild(i).GetComponent<VesselGroup>();

                        if (child != null)
                        {
                            _ele = child.Header.DragHandle.AddComponent<ReorderableListElement>();
                            _ele.Init(_extList);
                        }
                        
                        _cachedChildren.Add(_rect.GetChild(i));
                        _cachedListElement.Add(_ele);
                    }
                }
            }
            
            ////HACK a little hack, if I don't wait one frame I don't have the right deleted children
            yield return null;
            
            //Remove deleted child
            for (int i = _cachedChildren.Count - 1; i >= 0; i--)
            {
                if (_cachedChildren[i] == null)
                {
                    _cachedChildren.RemoveAt(i);
                    _cachedListElement.RemoveAt(i);
                }
            }
        }
    }
}