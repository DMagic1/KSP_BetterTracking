#region License
/*The MIT License (MIT)

Better Tracking

Tracking_SubGroup - UI Interface for sub group

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
using KSP.UI.Screens;

namespace BetterTracking
{
    public class Tracking_SubGroup : IVesselSubGroup
    {
        private ISubHeaderItem _header;
        private string _title;
        private bool _isOpen;
        private bool _instant;
        private int _vesselCount;
        private List<IVesselItem> _vessels = new List<IVesselItem>();
        private Tracking_Mode _mode;
        private CelestialBody _body;

        public Tracking_SubGroup(string title, bool isOpen, bool instant, List<TrackingStationWidget> vessels, CelestialBody body, Tracking_Mode mode)
        {
            _title = title;
            _isOpen = isOpen;
            _instant = instant;
            _vesselCount = vessels.Count;
            _body = body;
            _mode = mode;

            AddHeader();
            
            AddVessels(vessels);
        }

        private void AddHeader()
        {
            _header = new Tracking_SubHeader(_title, _vesselCount, Tracking_Utils.GetHeaderObject(_body, VesselType.Unknown, _mode, true), (int)_mode);
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
        
        public IVesselItem FindVessel(Vessel vessel)
        {
            for (int i = _vessels.Count - 1; i >= 0; i--)
            {
                if (((Tracking_Vessel)_vessels[i]).Vessel == vessel)
                    return _vessels[i];
            }
            
            return null;
        }
        
        public ISubHeaderItem SubHeader
        {
            get { return _header; }
        }

        public IList<IVesselItem> Vessels
        {
            get { return _vessels; }
        }

        public bool StartOn
        {
            get { return _isOpen; }

            set
            {
                _isOpen = value;

                switch (_mode)
                {
                    case Tracking_Mode.CelestialBody:
                        Tracking_Persistence.SetBodyPersistence(_body.flightGlobalsIndex, _isOpen);
                        break;
                }
            }
        }

        public bool Instant
        {
            get { return _instant; }
        }
    }
}
