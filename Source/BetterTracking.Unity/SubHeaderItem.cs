#region License
/*The MIT License (MIT)

One Window

SubHeaderItem - Sub header UI element

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
    public class SubHeaderItem : MonoBehaviour
    {
        [SerializeField]
        private TextHandler m_NameText = null;
        [SerializeField]
        private TextHandler m_InfoText = null;
        [SerializeField]
        private Transform m_HeaderIconTransform = null;
        [SerializeField]
        private Image m_HeaderIcon = null;
        [SerializeField]
        private Image m_ConnectorIcon = null;
        [SerializeField]
        private Sprite m_EndConnector = null;
        [SerializeField]
        private Sprite m_DoubleConnector = null;
        [SerializeField]
        private Toggle m_HeaderToggle = null;

        private ISubHeaderItem _headerInterface;
        private VesselSubGroup _parent;

        private bool _loaded;

        public void Initialize(ISubHeaderItem header, VesselSubGroup group, bool last, bool startOn)
        {
            if (header == null || group == null)
                return;

            _parent = group;

            _headerInterface = header;

            if (m_NameText != null)
                m_NameText.OnTextUpdate.Invoke(header.HeaderName);

            if (m_InfoText != null)
                m_InfoText.OnTextUpdate.Invoke(header.HeaderInfo);

            if (m_ConnectorIcon != null)
                m_ConnectorIcon.sprite = last ? m_EndConnector : m_DoubleConnector;

            AssignHeaderObject(header.HeaderImage);

            if (m_HeaderToggle != null)
                m_HeaderToggle.isOn = startOn;

            _loaded = true;
        }

        private void AssignHeaderObject(GameObject obj)
        {
            if (obj == null || m_HeaderIconTransform == null)
                return;

            obj.transform.SetParent(m_HeaderIconTransform, false);
        }

        public void OnHeaderToggle(bool isOn)
        {
            if (_parent == null || !_loaded)
                return;

            _parent.ToggleGroup(isOn);
        }

        private void Update()
        {
            if (_headerInterface != null)
                _headerInterface.Update();
        }
    }
}
