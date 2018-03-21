#region License
/*The MIT License (MIT)

Better Tracking

SortDropDown - Sort button drop down UI menu

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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BetterTracking.Unity.Interface;

namespace BetterTracking.Unity
{
    public class SortDropDown : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private Toggle m_TimeSortToggle = null;
        [SerializeField]
        private Toggle m_AlphaSortToggle = null;
        [SerializeField]
        private Toggle m_BodyTypeSortToggle = null;
        [SerializeField]
        private Image m_TimerSortImage = null;
        [SerializeField]
        private Image m_AlphaSortImage = null;
        [SerializeField]
        private Image m_BodyTypeSortImage = null;

        private ISortHeader _sortInterface;
        private SortHeader _sortHeader;
        private int _sortType;
        private bool _loaded;
        private bool _mouseOver;
        
        public void Initialize(ISortHeader sort, SortHeader parent)
        {
            _sortInterface = sort;
            _sortHeader = parent;

            _sortType = sort.CurrentMode;

            switch (_sortType)
            {
                case 0:
                    if (m_TimeSortToggle != null)
                        m_TimeSortToggle.isOn = sort.BodySortMode == 0 ? true : false;

                    if (m_AlphaSortToggle != null)
                        m_AlphaSortToggle.isOn = sort.BodySortMode == 1 ? true : false;

                    if (m_BodyTypeSortToggle != null)
                        m_BodyTypeSortToggle.isOn = sort.BodySortMode == 2 ? true : false;

                    if (m_TimerSortImage != null)
                        m_TimerSortImage.sprite = sort.BodySortOrder ? _sortHeader.m_TimerAscIcon : _sortHeader.m_TimerDescIcon;

                    if (m_AlphaSortImage != null)
                        m_AlphaSortImage.sprite = sort.BodySortOrder ? _sortHeader.m_AlphaAscIcon : _sortHeader.m_AlphaDescIcon;

                    if (m_BodyTypeSortImage != null)
                        m_BodyTypeSortImage.sprite = _sortHeader.m_TypeSortIcon;

                    break;
                case 1:
                    if (m_TimeSortToggle != null)
                        m_TimeSortToggle.isOn = sort.TypeSortMode == 0 ? true : false;

                    if (m_AlphaSortToggle != null)
                        m_AlphaSortToggle.isOn = sort.TypeSortMode == 1 ? true : false;

                    if (m_BodyTypeSortToggle != null)
                        m_BodyTypeSortToggle.isOn = sort.TypeSortMode == 2 ? true : false;
                    
                    if (m_TimerSortImage != null)
                        m_TimerSortImage.sprite = sort.TypeSortOrder ? _sortHeader.m_TimerAscIcon : _sortHeader.m_TimerDescIcon;

                    if (m_AlphaSortImage != null)
                        m_AlphaSortImage.sprite = sort.TypeSortOrder ? _sortHeader.m_AlphaAscIcon : _sortHeader.m_AlphaDescIcon;

                    if (m_BodyTypeSortImage != null)
                        m_BodyTypeSortImage.sprite = _sortHeader.m_BodySortIcon;

                    break;
            }

            _loaded = true;
        }

        public void ToggleTimeSort(bool isOn)
        {
            if (_sortInterface == null || !_loaded)
                return;

            if (isOn)
            {
                switch (_sortType)
                {
                    case 0:
                        _sortInterface.BodySortMode = 0;
                        break;
                    case 1:
                        _sortInterface.TypeSortMode = 0;
                        break;
                }
            }

            Close();
        }

        public void ToggleAlphaSort(bool isOn)
        {
            if (_sortInterface == null || !_loaded)
                return;

            if (isOn)
            {
                switch (_sortType)
                {
                    case 0:
                        _sortInterface.BodySortMode = 1;
                        break;
                    case 1:
                        _sortInterface.TypeSortMode = 1;
                        break;
                }
            }

            Close();
        }

        public void ToggleBodyTypeSort(bool isOn)
        {
            if (_sortInterface == null || !_loaded)
                return;

            if (isOn)
            {
                switch (_sortType)
                {
                    case 0:
                        _sortInterface.BodySortMode = 2;
                        break;
                    case 1:
                        _sortInterface.TypeSortMode = 2;
                        break;
                }
            }

            Close();
        }

        private void Update()
        {
            if (_mouseOver)
                return;

            if (Input.GetMouseButtonUp(0))
                Close();
        }

        private void Close()
        {
            if (_sortHeader != null)
                _sortHeader.CloseSortMenu();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _mouseOver = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _mouseOver = true;
        }
    }
}
