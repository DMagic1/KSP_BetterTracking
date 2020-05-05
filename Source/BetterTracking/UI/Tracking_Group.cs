#region License
/*The MIT License (MIT)

Better Tracking

Tracking_Group - UI Interface for primary tracking station group

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

using System.Collections.Generic;
using BetterTracking.Unity.Interface;
using KSP.UI;
using KSP.UI.Screens;

namespace BetterTracking
{
    public class Tracking_Group : IVesselGroup
    {
        private Tracking_Header _header;
        private string _title;
        private bool _isOpen;
        private bool _instant;
        private List<IVesselItem> _vessels = new List<IVesselItem>();
        private List<IVesselSubGroup> _subGroups = new List<IVesselSubGroup>();
        private Tracking_Mode _mode;
        private CelestialBody _body;
        private VesselType _type;

        public Tracking_Group(string title, bool isOpen, bool instant, List<TrackingStationWidget> vessels, List<Tracking_MoonGroup> subGroups, CelestialBody body, VesselType type, Tracking_Mode mode)
        {
            _title = title;
            _isOpen = isOpen;
            _instant = instant;
            _mode = mode;
            _body = body;
            _type = type;

            int _subVesselCount = 0;

            if (subGroups != null && subGroups.Count > 0)
            {
                for (int i = subGroups.Count - 1; i >= 0; i--)
                {
                    _subVesselCount += subGroups[i].Vessels.Count;
                }
            }

            AddHeader(vessels.Count, _subVesselCount);

            AddSubGroups(subGroups);

            AddVessels(vessels);
        }

        private void AddHeader(int vessels, int subVessels)
        {
            _header = new Tracking_Header(_title, vessels, subVessels, Tracking_Utils.GetHeaderObject(_body, _type, _mode, false), (int)_mode, ToggleOrbits);
        }

        private void AddSubGroups(List<Tracking_MoonGroup> subGroups)
        {
            if (subGroups == null)
                return;

            int count = subGroups.Count;

            for (int i = 0; i < count; i++)
            {
                AddSubGroup(subGroups[i]);
            }
        }

        private void AddSubGroup(Tracking_MoonGroup subGroup)
        {
            Tracking_SubGroup sub = new Tracking_SubGroup(Tracking_Utils.LocalizeBodyName(subGroup.Moon.displayName), Tracking_Persistence.GetBodyPersistence(subGroup.Moon.flightGlobalsIndex), _instant, subGroup.Vessels, subGroup.Moon, _mode);

            _subGroups.Add(sub);
        }

        private void AddVessels(List<TrackingStationWidget> vessels)
        {
            int count = vessels.Count;

            for (int i = 0; i < count; i++)
            {
                AddVessel(vessels[i]);
            }
        }

        private void AddVessel(TrackingStationWidget widget)
        {
            Tracking_Vessel vessel = new Tracking_Vessel(widget);

            _vessels.Add(vessel);
        }

        private void ToggleOrbits(bool OrbitsOn)
        {
            for (int i = _vessels.Count - 1; i >= 0; i--)
            {
                Tracking_Vessel vessel = (Tracking_Vessel)_vessels[i];

                vessel.OnToggleAllOrbits(OrbitsOn);
            }
        }

        public IVesselItem FindVessel(Vessel vessel)
        {
            for (int i = _vessels.Count - 1; i >= 0; i--)
            {
                if (((Tracking_Vessel)_vessels[i]).Vessel == vessel)
                    return _vessels[i];
            }

            for (int i = _subGroups.Count - 1; i >= 0; i--)
            {
                IVesselItem v = ((Tracking_SubGroup)_subGroups[i]).FindVessel(vessel);

                if (v != null)
                    return v;
            }

            return null;
        }

        public void UpdatePosition(int order, int old)
        {
            switch(_mode)
            {
                case Tracking_Mode.CelestialBody:
                    int index = _body == null ? 100 : _body.flightGlobalsIndex;

                    Tracking_Persistence.SetBodyOrder(index, order, old);
                    break;
                case Tracking_Mode.VesselType:
                    Tracking_Persistence.SetTypeOrder((int)_type, order, old);
                    break;
            }
        }

        public IHeaderItem Header
        {
            get { return _header; }
        }

        public IList<IVesselItem> Vessels
        {
            get { return _vessels; }
        }

        public IList<IVesselSubGroup> SubGroups
        {
            get { return _subGroups; }
        }

        public bool StartOn
        {
            get { return _isOpen; }

            set
            {
                _isOpen = value;
                
                switch(_mode)
                {
                    case Tracking_Mode.CelestialBody:
                        Tracking_Persistence.SetBodyPersistence(_body.flightGlobalsIndex, _isOpen);
                        break;
                    case Tracking_Mode.VesselType:
                        Tracking_Persistence.SetTypePersistence((int)_type, _isOpen);
                        break;
                }
            }
        }

        public bool Instant
        {
            get { return _instant; }
        }

        public int Index
        {
            get
            {
                switch (_mode)
                {
                    case Tracking_Mode.CelestialBody:
                        return _body == null ? 100 : _body.flightGlobalsIndex;
                    case Tracking_Mode.VesselType:
                        return (int)_type;
                }

                return 100;
            }
        }

        public float MasterScale
        {
            get { return UIMasterController.Instance.uiScale; }
        }
    }
}
