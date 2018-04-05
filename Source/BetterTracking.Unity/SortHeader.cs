#region License
/*The MIT License (MIT)

Better Tracking

SortHeader - Sort button UI element

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
using BetterTracking.Unity.Interface;
using UnityEngine.EventSystems;

namespace BetterTracking.Unity
{
    public class SortHeader : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_BodyToggle = null;
        [SerializeField]
        private Toggle m_TypeToggle = null;
        [SerializeField]
        private Toggle m_CustomToggle = null;
        [SerializeField]
        private Toggle m_DefaultToggle = null;
        [SerializeField]
        private SortDropDown m_SortPrefab = null;
        [SerializeField]
        private Transform m_DrowDownAnchor = null;
        [SerializeField]
        private Image m_SortOrderImage = null;
        [SerializeField]
        private Image m_SortModeImage = null;
        [SerializeField]
        private Sprite m_SortAscIcon = null;
        [SerializeField]
        private Sprite m_SortDescIcon = null;
        [SerializeField]
        private Toggle m_SortToggle = null;
        [SerializeField]
        private GameObject m_SortOrderButton = null;
        [SerializeField]
        private InputHandler m_SearchField = null;

        public Sprite m_TimerAscIcon = null;
        public Sprite m_TimerDescIcon = null;
        public Sprite m_AlphaAscIcon = null;
        public Sprite m_AlphaDescIcon = null;
        public Sprite m_BodySortIcon = null;
        public Sprite m_TypeSortIcon = null;
        
        private ISortHeader _sortInterface;
        private Transform _dropDownParent;
        private SortDropDown _dropDown;

        private Animator _anim;

        private bool _loaded;

        private void Awake()
        {
            _anim = GetComponent<Animator>();

            if (m_SearchField != null)
                m_SearchField.OnValueChange.AddListener(new UnityEngine.Events.UnityAction<string>(OnSearchInput));
        }

        private void OnDestroy()
        {
            if (m_SearchField != null)
                m_SearchField.OnValueChange.RemoveAllListeners();
        }

        private void Update()
        {
            if (_sortInterface == null)
                return;

            if (_sortInterface.LockInput)
            {
                if (m_SearchField != null && !m_SearchField.IsFocused)
                    _sortInterface.LockInput = false;
            }
        }

        public void Initialize(ISortHeader sort)
        {
            _sortInterface = sort;

            _dropDownParent = sort.DropDownParent;

            switch(sort.CurrentMode)
            {
                case 0:
                    if (m_BodyToggle != null)
                        m_BodyToggle.isOn = true;
                    break;
                case 1:
                    if (m_TypeToggle != null)
                        m_TypeToggle.isOn = true;
                    break;
                case 2:
                    if (m_CustomToggle != null)
                        m_CustomToggle.isOn = true;
                    break;
                case 3:
                    if (m_DefaultToggle != null)
                        m_DefaultToggle.isOn = true;
                    break;
            }

            UpdateSortButton();

            _loaded = true;
        }

        public void SortBody(bool isOn)
        {
            if (!_loaded)
                return;            

            if (isOn && _sortInterface != null)
                _sortInterface.SortBody(isOn);

            UpdateSortButton();
        }

        public void SortType(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortType(isOn);

            UpdateSortButton();
        }

        public void SortCustom(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortCustom(isOn);

            UpdateSortButton();
        }

        public void SortDefault(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortDefault(isOn);

            UpdateSortButton();
        }

        private void UpdateSortButton()
        {
            if (_sortInterface == null)
                return;

            //if (_sortInterface.CurrentMode == 3)
            //{
            //    if (m_SortOrderButton != null)
            //        m_SortOrderButton.SetActive(false);

            //    if (m_SortToggle != null)
            //        m_SortToggle.gameObject.SetActive(false);

            //    return;
            //}
            //else
            //{
            //    if (m_SortOrderButton != null && !m_SortOrderButton.activeSelf)
            //        m_SortOrderButton.SetActive(true);

            //    if (m_SortToggle != null && !m_SortToggle.gameObject.activeSelf)
            //        m_SortToggle.gameObject.SetActive(true);
            //}

            if (m_SortModeImage != null && m_SortOrderImage != null)
            {
                switch (_sortInterface.CurrentMode)
                {
                    case 0:
                        switch (_sortInterface.BodySortMode)
                        {
                            case 0:
                                m_SortModeImage.sprite = _sortInterface.BodySortOrder ? m_TimerAscIcon : m_TimerDescIcon;
                                break;
                            case 1:
                                m_SortModeImage.sprite = _sortInterface.BodySortOrder ? m_AlphaAscIcon : m_AlphaDescIcon;
                                break;
                            case 2:
                            case 3:
                                m_SortModeImage.sprite = m_TypeSortIcon;
                                break;
                        }

                        m_SortOrderImage.sprite = _sortInterface.BodySortOrder ? m_SortAscIcon : m_SortDescIcon;

                        break;
                    case 1:
                        switch (_sortInterface.TypeSortMode)
                        {
                            case 0:
                                m_SortModeImage.sprite = _sortInterface.TypeSortOrder ? m_TimerAscIcon : m_TimerDescIcon;
                                break;
                            case 1:
                                m_SortModeImage.sprite = _sortInterface.TypeSortOrder ? m_AlphaAscIcon : m_AlphaDescIcon;
                                break;
                            case 2:
                            case 3:
                                m_SortModeImage.sprite = m_BodySortIcon;
                                break;
                        }

                        m_SortOrderImage.sprite = _sortInterface.TypeSortOrder ? m_SortAscIcon : m_SortDescIcon;

                        break;
                    case 3:
                        switch(_sortInterface.StockSortMode)
                        {
                            case 0:
                                m_SortModeImage.sprite = _sortInterface.StockSortOrder ? m_TimerAscIcon : m_TimerDescIcon;
                                break;
                            case 1:
                                m_SortModeImage.sprite = _sortInterface.StockSortOrder ? m_AlphaAscIcon : m_AlphaDescIcon;
                                break;
                            case 2:
                                m_SortModeImage.sprite = m_TypeSortIcon;
                                break;
                            case 3:
                                m_SortModeImage.sprite = m_BodySortIcon;
                                break;
                        }

                        m_SortOrderImage.sprite = _sortInterface.StockSortOrder ? m_SortAscIcon : m_SortDescIcon;

                        break;
                }
            }
        }

        public void ToggleSortOrder()
        {
            if (_sortInterface == null)
                return;

            switch (_sortInterface.CurrentMode)
            {
                case 0:
                    _sortInterface.BodySortOrder = !_sortInterface.BodySortOrder;
                    break;
                case 1:
                    _sortInterface.TypeSortOrder = !_sortInterface.TypeSortOrder;
                    break;
                case 3:
                    _sortInterface.StockSortOrder = !_sortInterface.StockSortOrder;
                    break;
            }

            UpdateSortButton();
        }

        public void ToggleSortMenu(bool isOn)
        {
            if (isOn)
            {
                if (_dropDown != null)
                {
                    _dropDown.gameObject.SetActive(false);
                    DestroyImmediate(_dropDown.gameObject);
                    _dropDown = null;
                }

                OpenSortMenu();
            }
            else
                CloseSortMenu();
        }

        public void OpenSortMenu()
        {
            if (m_SortPrefab == null || m_DrowDownAnchor == null || _dropDownParent == null)
                return;
            
            _dropDown = Instantiate(m_SortPrefab);
            _dropDown.gameObject.SetActive(true);

            _dropDown.transform.SetParent(m_DrowDownAnchor, false);

            _dropDown.Initialize(_sortInterface, this);

            _dropDown.transform.SetParent(_dropDownParent, true);
        }

        public void CloseSortMenu()
        {
            if (_dropDown != null)
            {
                _dropDown.gameObject.SetActive(false);
                Destroy(_dropDown.gameObject);
                _dropDown = null;
            }

            UpdateSortButton();

            _loaded = false;

            if (m_SortToggle != null)
                m_SortToggle.isOn = false;

            _loaded = true;
        }

        public void ToggleSearch(bool isOn)
        {
            if (_anim != null)
                _anim.SetBool("search", isOn);

            if (!isOn && _sortInterface != null)
            {
                _sortInterface.LockInput = false;
                _sortInterface.SearchString = "";
            }

            if (m_SearchField != null)
                m_SearchField.OnTextUpdate.Invoke("");
        }

        public void OnInputClick(BaseEventData eventData)
        {
            if (!(eventData is PointerEventData) || _sortInterface == null)
                return;

            if (((PointerEventData)eventData).button != PointerEventData.InputButton.Left)
                return;

            _sortInterface.LockInput = true;
        }

        public void OnSearchInput(string input)
        {
            if (_sortInterface == null)
                return;

            _sortInterface.SearchString = input;
        }

    }
}
