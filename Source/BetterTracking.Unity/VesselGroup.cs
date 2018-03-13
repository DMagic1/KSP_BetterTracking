#region License
/*The MIT License (MIT)

One Window

VesselGroup - Vessel group UI element

Copyright (C) 2018 DMagic
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BetterTracking.Unity.Interface;

namespace BetterTracking.Unity
{
    public class VesselGroup : MonoBehaviour
    {
        [SerializeField]
        private Transform m_VesselGroup = null;
        [SerializeField]
        private VesselItem m_VesselPrefab = null;
        [SerializeField]
        private Transform m_HeaderTransform = null;
        [SerializeField]
        private HeaderItem m_HeaderPrefab = null;
        [SerializeField]
        private VesselSubGroup m_SubGroupPrefab = null;
        [SerializeField]
        private Transform m_SubGroupTransform = null;

        [HideInInspector]
        public bool _animating = true;

        private List<VesselItem> _vesselList = new List<VesselItem>();
        private Animator _anim;

        private IVesselGroup _groupInterface;

        private Coroutine _animRoutine;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        public void Initialize(IVesselGroup group)
        {
            if (group == null)
                return;

            //Debug.Log("[BTK] Initialize Vessel Group...");

            _groupInterface = group;

            ClearUI();

            AddHeader(group.Header);

            AddSubGroups(group.SubGroups);

            AddVessels(group.Vessels);

            if (_anim != null)
                _anim.SetBool("open", group.StartOn);

            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }

            _animating = false;

            _animRoutine = StartCoroutine(WaitForExpand());
        }

        private void ClearUI()
        {
            for (int i = _vesselList.Count - 1; i >= 0; i--)
            {
                _vesselList[i].gameObject.SetActive(false);
                Destroy(_vesselList[i].gameObject);
            }

            _vesselList.Clear();
        }

        private void AddHeader(IHeaderItem header)
        {
            if (m_HeaderTransform == null || m_HeaderPrefab == null || header == null)
                return;

            HeaderItem obj = Instantiate(m_HeaderPrefab);
            obj.transform.SetParent(m_HeaderTransform, false);
            obj.Initialize(header, this, _groupInterface.StartOn);
        }

        private void AddSubGroups(IList<IVesselSubGroup> subGroups)
        {
            if (m_SubGroupTransform == null || m_SubGroupPrefab == null || subGroups == null)
                return;

            int count = subGroups.Count;

            for (int i = 0; i < count; i++)
            {
                AddSubGroup(subGroups[i], i >= count - 1 && _groupInterface.Vessels != null &&_groupInterface.Vessels.Count <= 0);
            }
        }

        private void AddSubGroup(IVesselSubGroup subGroup, bool last)
        {
            if (subGroup == null)
                return;

            VesselSubGroup obj = Instantiate(m_SubGroupPrefab);
            obj.transform.SetParent(m_SubGroupTransform, false);
            obj.Initialize(subGroup, last);
        }

        private void AddVessels(IList<IVesselItem> vessels)
        {
            if (m_VesselGroup == null || m_VesselPrefab == null)
                return;

            ClearUI();

            int count = vessels.Count;

            for (int i = 0; i < count; i++)
            {
                AddVessel(vessels[i], i >= count - 1);
            }
        }

        private void AddVessel(IVesselItem vessel, bool last)
        {
            if (vessel == null)
                return;

            VesselItem obj = Instantiate(m_VesselPrefab);
            obj.transform.SetParent(m_VesselGroup, false);
            obj.Initialize(vessel, last);

            _vesselList.Add(obj);
        }

        public void ToggleGroup(bool isOn)
        {
            if (_anim != null)
                _anim.SetBool("open", isOn);

            if (_groupInterface != null)
                _groupInterface.StartOn = isOn;

            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }

            _animating = false;

            _animRoutine = StartCoroutine(WaitForExpand());
        }

        private IEnumerator WaitForExpand()
        {
            while (!_animating)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

                yield return null;
            }
            
            _animRoutine = null;
        }
    }
}
