#region License
/*The MIT License (MIT)

One Window

Tracking_Vessel - UI Interface for vessel element

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

using BetterTracking.Unity;
using BetterTracking.Unity.Interface;
using UnityEngine;
using KSP.UI.Screens;
using UnityEngine.UI;

namespace BetterTracking
{
    public class Tracking_Vessel : IVesselItem
    {
        private TrackingStationWidget _vesselWidget;
        private VesselIconSprite _iconSprite;
        private VesselItem _vesselUI;
        private SubVesselItem _subVesselUI;

        public Tracking_Vessel(TrackingStationWidget widget)
        {
            _vesselWidget = widget;
            
            _iconSprite = GameObject.Instantiate(Tracking_Loader.IconPrefab);

            _iconSprite.SetType(widget.vessel.vesselType);
        }

        public void OnVesselSelect()
        {
            if (_vesselUI != null)
                _vesselUI.SelectVessel();
            else if (_subVesselUI != null)
                _subVesselUI.SelectVessel();
        }
        
        public Vessel Vessel
        {
            get
            {
                if (_vesselWidget != null)
                    return _vesselWidget.vessel;

                return null;
            }
        }

        public string VesselName
        {
            get
            {
                if (_vesselWidget != null)
                    return _vesselWidget.textName.text;

                return "";
            }
        }

        public string VesselSituation
        {
            get
            {
                if (_vesselWidget != null)
                    return _vesselWidget.textStatus.text;

                return "";
            }
        }

        public string VesselInfo
        {
            get
            {
                if (_vesselWidget != null)
                    return _vesselWidget.textInfo.text;

                return "";
            }
        }

        public Sprite VesselIcon
        {
            get
            {
                if (_vesselWidget != null)
                    return null;

                return null;
            }
        }

        public GameObject VesselImage
        {
            get
            {
                if (_iconSprite != null)
                    return _iconSprite.gameObject;

                return null;
            }
        }

        public ToggleGroup VesselToggleGroup
        {
            get
            {
                if (Tracking_Controller.Instance != null)
                    return Tracking_Controller.Instance.TrackingToggleGroup;

                return null;
            }
        }

        public void OnIconUpdate()
        {
            if (_iconSprite == null || _vesselWidget == null)
                return;

            if (_iconSprite.vesselType != _vesselWidget.vessel.vesselType)
                _iconSprite.SetType(_vesselWidget.vessel.vesselType);
        }

        public void OnToggle(bool isOn)
        {
            if (_vesselWidget != null)
            {
                //Tracking_Utils.TrackingLog("Pass through click: {0} - {1}", _vesselWidget.vessel.vesselName, isOn);
                _vesselWidget.toggle.onValueChanged.Invoke(isOn);
                //_vesselWidget.toggle.isOn = isOn;
            }
        }

        public void OnVesselEdit()
        {
            if (_vesselWidget == null || _vesselWidget.vessel == null)
                return;

            _vesselWidget.vessel.RenameVessel();
        }
        
        public void SetUI(VesselItem item)
        {
            _vesselUI = item;
        }

        public void SetSubUI(SubVesselItem item)
        {
            _subVesselUI = item;
        }
    }
}
