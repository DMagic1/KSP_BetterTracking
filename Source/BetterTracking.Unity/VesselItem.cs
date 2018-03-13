#region License
/*The MIT License (MIT)

One Window

VesselItem - Vessel UI element

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
    public class VesselItem : MonoBehaviour
    {
        [SerializeField]
        private TextHandler m_NameText = null;
        [SerializeField]
        private TextHandler m_SituationText = null;
        [SerializeField]
        private TextHandler m_InfoText = null;
        [SerializeField]
        private Transform m_VesselIcon = null;
        [SerializeField]
        private Image m_ConnectorIcon = null;
        [SerializeField]
        private Sprite m_EndConnector = null;
        [SerializeField]
        private Sprite m_DoubleConnector = null;
        [SerializeField]
        private Toggle m_Toggle = null;

        //private bool _selfClick;
        private IVesselItem _vesselInterface;

        public void Initialize(IVesselItem vessel, bool last)
        {
            if (vessel == null)
                return;

            _vesselInterface = vessel;

            if (m_Toggle != null)
                m_Toggle.group = vessel.VesselToggleGroup;

            if (m_NameText != null)
                m_NameText.OnTextUpdate.Invoke(vessel.VesselName);

            if (m_SituationText != null)
                m_SituationText.OnTextUpdate.Invoke(vessel.VesselSituation);

            if (m_InfoText != null)
                m_InfoText.OnTextUpdate.Invoke(vessel.VesselInfo);

            //if (m_VesselIcon != null)
            //    m_VesselIcon.sprite = vessel.VesselIcon;

            if (m_ConnectorIcon != null)
                m_ConnectorIcon.sprite = last ? m_EndConnector : m_DoubleConnector;

            AssignVesselSprite(vessel.VesselImage);

            vessel.SetUI(this);
        }

        public void SelectVessel()
        {
            if (m_Toggle == null)// || _selfClick)
                return;

            bool on = m_Toggle.isOn;

            m_Toggle.group.SetAllTogglesOff();

            if (!on)
                m_Toggle.isOn = true;
        }
        
        private void AssignVesselSprite(GameObject obj)
        {
            if (obj == null || m_VesselIcon == null)
                return;

            obj.transform.SetParent(m_VesselIcon, false);
        }

        public void OnVesselToggle(bool isOn)
        {
            if (_vesselInterface == null)
                return;

            //_selfClick = true;

            if (isOn)
            {
                _vesselInterface.OnToggle(isOn);
                //Debug.Log("[BTK] Toggle On: " + _vesselInterface.VesselName);
            }
            //else
                //Debug.Log("[BTK] Toggle Off " + _vesselInterface.VesselName);
            //_selfClick = false;
        }

        public void VesselEdit()
        {
            if (_vesselInterface == null)
                return;

            _vesselInterface.OnVesselEdit();
        }

        private void Update()
        {
            if (_vesselInterface == null)
                return;

            if (m_NameText != null)
                m_NameText.OnTextUpdate.Invoke(_vesselInterface.VesselName);

            if (m_SituationText != null)
                m_SituationText.OnTextUpdate.Invoke(_vesselInterface.VesselSituation);

            if (m_InfoText != null)
                m_InfoText.OnTextUpdate.Invoke(_vesselInterface.VesselInfo);
        }
    }
}
