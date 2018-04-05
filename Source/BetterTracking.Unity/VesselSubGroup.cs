#region License
/*The MIT License (MIT)

Better Tracking

VesselSubGroup - Sub group UI element

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
    public class VesselSubGroup : MonoBehaviour
    {
        [SerializeField]
        private Transform m_VesselGroup = null;
        [SerializeField]
        private SubVesselItem m_VesselPrefab = null;
        [SerializeField]
        private Transform m_HeaderTransform = null;
        [SerializeField]
        private SubHeaderItem m_HeaderPrefab = null;

        [HideInInspector]
        public bool _animating = true;

        private List<SubVesselItem> _vesselList = new List<SubVesselItem>();
        private Animator _anim;

        private bool _final;

        private VesselGroup _parent;
        private IVesselSubGroup _groupInterface;

        private Coroutine _animRoutine;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (_anim == null || _groupInterface == null || !_groupInterface.StartOn)
                return;

            //Debug.Log("[BTK] Sub Group Enable: " + _groupInterface.StartOn);

            _anim.SetBool("open", true);

            if (_animRoutine != null)
            {
                StopCoroutine(_animRoutine);
                _animRoutine = null;
            }

            _animating = false;

            _animRoutine = StartCoroutine(WaitForExpand());
        }

        public void Initialize(IVesselSubGroup group, VesselGroup parent, bool last)
        {
            if (group == null)
                return;

            _groupInterface = group;
            _parent = parent;

            _final = last;

            ClearUI();

            AddHeader(group.SubHeader);

            AddVessels(group.Vessels);

            //Debug.Log("[BTK] Sub Group Initialize: " + group.StartOn);

            if (_anim != null)
            {
                _anim.SetBool("open", group.StartOn);
                _anim.SetBool("instant", group.Instant && group.StartOn);
            }

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

        private void AddHeader(ISubHeaderItem header)
        {
            if (m_HeaderTransform == null || m_HeaderPrefab == null || header == null)
                return;

            SubHeaderItem obj = Instantiate(m_HeaderPrefab);
            obj.transform.SetParent(m_HeaderTransform, false);
            obj.transform.SetAsFirstSibling();
            obj.Initialize(header, this, _parent, _final, _groupInterface.StartOn);
        }

        private void AddVessels(IList<IVesselItem> vessels)
        {
            if (m_VesselGroup == null || m_VesselPrefab == null)
                return;

            ClearUI();
            
            for (int i = vessels.Count - 1; i >= 0; i--)
            {
                AddVessel(vessels[i], i == 0);
            }
        }

        private void AddVessel(IVesselItem vessel, bool last)
        {
            if (vessel == null)
                return;

            SubVesselItem obj = Instantiate(m_VesselPrefab);
            obj.transform.SetParent(m_VesselGroup, false);
            obj.Initialize(vessel, last, _final);

            _vesselList.Add(obj);
        }

        public void ToggleGroup(bool isOn)
        {
            if (_anim != null)
            {
                _anim.SetBool("open", isOn);
                _anim.SetBool("instant", false);
            }

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
