#region License
/*The MIT License (MIT)

One Window

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

        private ISortHeader _sortInterface;

        private bool _loaded;

        public void Initialize(ISortHeader sort)
        {
            _sortInterface = sort;

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

            _loaded = true;
        }

        public void SortBody(bool isOn)
        {
            if (!_loaded)
                return;            

            if (isOn && _sortInterface != null)
                _sortInterface.SortBody(isOn);
        }

        public void SortType(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortType(isOn);
        }

        public void SortCustom(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortCustom(isOn);
        }
        public void SortDefault(bool isOn)
        {
            if (!_loaded)
                return;

            if (isOn && _sortInterface != null)
                _sortInterface.SortDefault(isOn);
        }
    }
}
