#region License
/*The MIT License (MIT)

One Window

Tracking_Controller - Main tracking station logic controller

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
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using KSP.UI.Screens;
using BetterTracking.Unity;
using BetterTracking.Unity.Interface;

namespace BetterTracking
{
    public class OnWidgetSelect : UnityEvent<TrackingStationWidget> { }
    public class OnWidgetAwake : UnityEvent<TrackingStationWidget> { }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class Tracking_Controller : MonoBehaviour, ISortHeader
    {
        public static OnWidgetSelect OnWidgetSelect = new OnWidgetSelect();
        public static OnWidgetAwake OnWidgetAwake = new OnWidgetAwake();

        private bool _widgetAwakeSet;

        private SpaceTracking _TrackingStation;
        
        private GameObject _OldTrackingList;
        private GameObject _NewTrackingList;
        private Transform _ListParent;

        private ScrollRect _ScrollView;
        private Rect _ScrollViewRect;
        private Camera _CanvasCamera;

        //private TrackingStationWidget _WidgetPrefab;

        private Tracking_Mode _CurrentMode = Tracking_Mode.CelestialBody;
        
        private List<TrackingStationWidget> _TrackedVesselWidgets = new List<TrackingStationWidget>();
        private List<Tracking_BodyGroup> _OrderedBodyList = new List<Tracking_BodyGroup>();

        private List<Tracking_Group> _TrackingGroups = new List<Tracking_Group>();

        private List<VesselGroup> _UIList = new List<VesselGroup>();

        private ToggleGroup _VesselToggleGroup;

        private bool _lightAdded;

        private static Tracking_Controller _instance;

        public static Tracking_Controller Instance
        {
            get { return _instance; }
        }

        public GameObject NewTrackingList
        {
            get { return _NewTrackingList; }
        }

        public ToggleGroup TrackingToggleGroup
        {
            get { return _VesselToggleGroup; }
        }
        
        public Rect TrackingScrollView
        {
            get { return _ScrollViewRect; }
        }

        public Camera CanvasCamera
        {
            get { return _CanvasCamera; }
        }

        public bool LightAdded
        {
            get { return _lightAdded; }
            set { _lightAdded = value; }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            OnWidgetSelect.AddListener(new UnityAction<TrackingStationWidget>(OnWidgetSelected));
            OnWidgetAwake.AddListener(new UnityAction<TrackingStationWidget>(OnWidgetAwaken));

            _CurrentMode = (Tracking_Mode)Tracking_Persistence.SortMode;

            StartCoroutine(WaitForTrackingStation());
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
            
            OnWidgetSelect.RemoveListener(new UnityAction<TrackingStationWidget>(OnWidgetSelected));
            OnWidgetAwake.RemoveListener(new UnityAction<TrackingStationWidget>(OnWidgetAwaken));
        }

        private IEnumerator WaitForTrackingStation()
        {
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            //Tracking_Utils.TrackingLog("Looking for Space Tracking...");

            int count = 0;

            while(_TrackingStation == null)
            {
                //Tracking_Utils.TrackingLog("Tracking Check: {0}", count);
                count++;
                var tracking = FindObjectsOfType<SpaceTracking>();

                if (tracking != null)
                {
                    for (int i = 0; i < tracking.Length; i++)
                    {
                        SpaceTracking space = tracking[i];

                        if (space == null)
                            continue;

                        //Tracking_Utils.TrackingLog("Space Tracking Logged: {0}", i);

                        _TrackingStation = space;
                    }
                }

                if (_TrackingStation == null)
                    yield return null;
            }

            FindScrollRect();

            FindCamera();

            FindCorners();

            AdjustUITransforms();

            StartCoroutine(AttachSortHeader());

            _VesselToggleGroup = Instantiate(_TrackingStation.listToggleGroup);

            _ListParent = _TrackingStation.listContainer.parent.transform;

            _OldTrackingList = _TrackingStation.listContainer.gameObject;

            _NewTrackingList = Instantiate(_OldTrackingList);

            if (_CurrentMode != Tracking_Mode.Default)
            {
                _NewTrackingList.transform.SetParent(_ListParent, false);

                _TrackingStation.listContainer.SetParent(null, false);
            }

            _OrderedBodyList = OrderBodies();

            Tracking_Utils.TrackingLog("Tracking Station Processed");
            
        }

        private IEnumerator AttachSortHeader()
        {
            Transform parent = _TrackingStation.listContainer.parent.parent.parent;

            while (Tracking_Loader.SortHeaderPrefab == null)
                yield return null;
            
            //Tracking_Utils.TrackingLog("Starting Sort Header...");

            SortHeader sort = Instantiate(Tracking_Loader.SortHeaderPrefab).GetComponent<SortHeader>();
            sort.transform.SetParent(parent, false);
            sort.transform.SetSiblingIndex(2);
            sort.Initialize(this);
        }

        private void AdjustUITransforms()
        {
            if (_TrackingStation == null)
                return;

            //Tracking_Utils.TrackingLog("Adjusting UI Transforms...");

            RectTransform listRect = _TrackingStation.listContainer.parent.parent as RectTransform;

            if (listRect != null)
            {
                listRect.anchoredPosition = new Vector2(5, -96);
                listRect.sizeDelta = new Vector2(280, -177);
            }

            RectTransform headerRect = _TrackingStation.listContainer.parent.parent.parent.GetChild(2) as RectTransform;

            if (headerRect != null)
            {
                headerRect.anchoredPosition = new Vector2(5, -72);
                headerRect.sizeDelta = new Vector2(266, 25);
            }
        }

        private void FindScrollRect()
        {
            _ScrollView = _TrackingStation.listContainer.GetComponentInParent<ScrollRect>();
        }

        private void FindCamera()
        {
            _CanvasCamera = _TrackingStation.listContainer.GetComponentInParent<Canvas>().worldCamera;
        }

        private void FindCorners()
        {
            if (_ScrollView == null || _CanvasCamera == null)
                return;

            Vector3[] objectCorners = new Vector3[4];
            ((RectTransform)_ScrollView.transform).GetWorldCorners(objectCorners);
            
            Vector3 bl = _CanvasCamera.WorldToScreenPoint(objectCorners[0]);
            
            float x = bl.x;
            float y = bl.y;

            bl = _CanvasCamera.WorldToScreenPoint(objectCorners[2]);
            
            float width = bl.x - x;
            float height = bl.y - y;

            _ScrollViewRect = new Rect(x, y, width, height);
        }
        
        private void OnWidgetSelected(TrackingStationWidget widget)
        {
            if (_TrackingGroups == null || _TrackingGroups.Count <= 0)
                return;

            if (widget == null)
            {
                //Tracking_Utils.TrackingLog("Widget Toggle Clicked - Widget Null");
                return;
            }
            else if (widget.vessel == null)
            {
                //Tracking_Utils.TrackingLog("Widget Toggle Clicked - Vessel Null");
                return;
            }
            //else
                //Tracking_Utils.TrackingLog("Widget Toggle Clicked: {0}", widget.vessel.vesselName);

            for (int i = _TrackingGroups.Count - 1; i >= 0; i--)
            {
                IVesselItem vessel = _TrackingGroups[i].FindVessel(widget.vessel);

                if (vessel != null)
                {
                    vessel.OnVesselSelect();
                    break;
                }
            }
        }

        private void OnWidgetAwaken(TrackingStationWidget widget)
        {
            if (_widgetAwakeSet || _NewTrackingList == null || _CurrentMode == Tracking_Mode.Default)
                return;

            //Tracking_Utils.TrackingLog("Tracking Widget Started; Processed List");

            _widgetAwakeSet = true;

            StartCoroutine(WidgetListReset());
        }

        private IEnumerator WidgetListReset()
        {
            yield return new WaitForEndOfFrame();

            UpdateScrollRect(_NewTrackingList.transform as RectTransform);

            _TrackedVesselWidgets.Clear();

            ParseWidgetContainer();

            ListUpdate();

            _widgetAwakeSet = false;
        }

        public void UpdateScrollRect(RectTransform rect)
        {
            if (_ScrollView != null)
            {
                _ScrollView.content = rect;
            }
        }

        private void ParseWidgetContainer()
        {
            int count = _TrackingStation.listContainer.childCount;

            Tracking_Utils.TrackingLog("Processing {0} Vessel Widgets", count);

            //Tracking_Utils.TrackingLog("Widget Parent: {0}", _TrackingStation.listContainer.name);

            //if (_TrackingStation.listContainer.parent != null)
            //    Tracking_Utils.TrackingLog("Widget List Parent: {0}", _TrackingStation.listContainer.parent.name);

            for (int i = 0; i < count; i++)
            {
                Transform t = _TrackingStation.listContainer.GetChild(i);

                TrackingStationWidget widget = t.GetComponent<TrackingStationWidget>();


                if (widget != null)
                    _TrackedVesselWidgets.Add(widget);
            }
        }

        public void ListUpdate()
        {
            _TrackingGroups.Clear();
            
            switch(_CurrentMode)
            {
                case Tracking_Mode.CelestialBody:
                    _TrackingGroups = SortCelestialBodies();
                    break;
                case Tracking_Mode.Default:

                    break;
                case Tracking_Mode.VesselType:
                    _TrackingGroups = SortVesselType();
                    break;
            }

            ClearUI();

            GenerateUI();
        }

        private List<Tracking_Group> SortCelestialBodies()
        {
            //Tracking_Utils.TrackingLog("Sorting Celestial Body List");

            List<Tracking_Group> vesselGroups = new List<Tracking_Group>();

            int count = _OrderedBodyList.Count;

            for (int i = 0; i < count; i++)
            {
                Tracking_BodyGroup body = _OrderedBodyList[i];

                List<TrackingStationWidget> bodyVessels = new List<TrackingStationWidget>();

                int vessels = _TrackedVesselWidgets.Count;

                for (int j = 0; j < vessels; j++)
                {
                    if (_TrackedVesselWidgets[j].vessel.mainBody == body.Body)
                        bodyVessels.Add(_TrackedVesselWidgets[j]);
                }

                //Tracking_Utils.TrackingLog("Body: {0} With {1} Vessels", Tracking_Utils.LocalizeBodyName(body.Body.displayName), bodyVessels.Count);

                List<Tracking_MoonGroup> moonGroups = new List<Tracking_MoonGroup>();

                int moons = body.Moons.Count;

                for (int k = 0; k < moons; k++)
                {
                    List<TrackingStationWidget> moonVessels = new List<TrackingStationWidget>();

                    for (int l = 0; l < vessels; l++)
                    {
                        if (_TrackedVesselWidgets[l].vessel.mainBody == body.Moons[k])
                            moonVessels.Add(_TrackedVesselWidgets[l]);
                    }

                    //Tracking_Utils.TrackingLog("Moon: {0} With {1} Vessels", Tracking_Utils.LocalizeBodyName(body.Moons[k].displayName), moonVessels.Count);

                    if (moonVessels.Count > 0)
                        moonGroups.Add(new Tracking_MoonGroup() { Moon = body.Moons[k], Vessels = moonVessels });
                }

                if (bodyVessels.Count > 0 || moonGroups.Count > 0)
                    vesselGroups.Add(new Tracking_Group(Tracking_Utils.LocalizeBodyName(body.Body.displayName), Tracking_Persistence.GetBodyPersistence(body.Body.flightGlobalsIndex), bodyVessels, moonGroups, body.Body, VesselType.Unknown, Tracking_Mode.CelestialBody));
            }

            return vesselGroups;
        }

        private List<Tracking_Group> SortVesselType()
        {
            List<Tracking_Group> vesselGroups = new List<Tracking_Group>();
            
            Tracking_Group group = VesselTypeGroup(VesselType.Ship);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.EVA);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Probe);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Plane);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Lander);
            if (group != null)
                vesselGroups.Add(group);
            
            group = VesselTypeGroup(VesselType.Station);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Base);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Relay);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Flag);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.SpaceObject);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Debris);
            if (group != null)
                vesselGroups.Add(group);

            group = VesselTypeGroup(VesselType.Unknown);
            if (group != null)
                vesselGroups.Add(group);

            return vesselGroups;
        }

        private Tracking_Group VesselTypeGroup(VesselType type)
        {
            int vessels = _TrackedVesselWidgets.Count;

            List<TrackingStationWidget> typeVessels = new List<TrackingStationWidget>();

            for (int i = 0; i < vessels; i++)
            {
                if (_TrackedVesselWidgets[i].vessel.vesselType == type)
                    typeVessels.Add(_TrackedVesselWidgets[i]);
            }

            if (typeVessels.Count > 0)
                return new Tracking_Group(Tracking_Utils.VesselTypeString(type), Tracking_Persistence.GetTypePersistence((int)type), typeVessels, null, null, type, Tracking_Mode.VesselType);

            return null;
        }

        private void ClearUI()
        {
            for (int i = _UIList.Count - 1; i >= 0; i--)
            {
                _UIList[i].gameObject.SetActive(false);
                _UIList[i].transform.SetParent(null, false);
                Destroy(_UIList[i].gameObject);
            }

            _UIList.Clear();

            int count = _NewTrackingList.transform.childCount;

            for (int i = count - 1; i >= 0; i--)
            {
                _NewTrackingList.transform.GetChild(i).gameObject.SetActive(false);
                Destroy(_NewTrackingList.transform.GetChild(i).gameObject);
            }
        }

        private void GenerateUI()
        {
            if (_TrackingGroups == null || _TrackingGroups.Count <= 0)
                return;

            if (Tracking_Loader.GroupPrefab == null || _NewTrackingList == null)
                return;

            int count = _TrackingGroups.Count;

            for (int i = 0; i < count; i++)
            {
                AddTrackingGroup(_TrackingGroups[i]);
            }
        }

        private void AddTrackingGroup(Tracking_Group group)
        {
            VesselGroup UI = Instantiate(Tracking_Loader.GroupPrefab).GetComponent<VesselGroup>();

            if (UI == null)
                return;

            UI.transform.SetParent(_NewTrackingList.transform, false);
            UI.Initialize(group);

            _UIList.Add(UI);
        }

        public void ActivateDefaultSort()
        {
            if (_CurrentMode != Tracking_Mode.Default)
            {
                _OldTrackingList.transform.SetParent(_ListParent, false);

                _NewTrackingList.transform.SetParent(null, false);

                UpdateScrollRect(_OldTrackingList.transform as RectTransform);
            }

            _CurrentMode = Tracking_Mode.Default;

            GameEvents.OnMapViewFiltersModified.Fire(MapViewFiltering.VesselTypeFilter.All);

            ClearUI();

            //StartCoroutine(WidgetListReset());
        }

        public void ActivateCelestialSort()
        {
            if (_CurrentMode == Tracking_Mode.Default)
            {
                _NewTrackingList.transform.SetParent(_ListParent, false);

                _OldTrackingList.transform.SetParent(null, false);
            }

            _CurrentMode = Tracking_Mode.CelestialBody;

            //_lightAdded = false;

            StartCoroutine(WidgetListReset());
        }

        public void ActivateVesselTypeSort()
        {
            if (_CurrentMode == Tracking_Mode.Default)
            {
                _NewTrackingList.transform.SetParent(_ListParent, false);

                _OldTrackingList.transform.SetParent(null, false);
            }

            _CurrentMode = Tracking_Mode.VesselType;

            StartCoroutine(WidgetListReset());
        }

        public void ActivateCustomSort()
        {
            if (_CurrentMode == Tracking_Mode.Default)
            {
                _NewTrackingList.transform.SetParent(_ListParent, false);

                _OldTrackingList.transform.SetParent(null, false);
            }

            _CurrentMode = Tracking_Mode.Custom;

            StartCoroutine(WidgetListReset());
        }

        private List<Tracking_BodyGroup> OrderBodies()
        {
            List<Tracking_BodyGroup> bodies = new List<Tracking_BodyGroup>();

            var allBodies = FlightGlobals.Bodies.Where(b => b.referenceBody == Planetarium.fetch.Sun && b.referenceBody != b);

            var orderedBodies = allBodies.OrderBy(b => b.orbit.semiMajorAxis).ToList();

            for (int i = 0; i < orderedBodies.Count; i++)
            {
                CelestialBody body = orderedBodies[i];
                
                List<CelestialBody> moons = new List<CelestialBody>();

                for (int j = 0; j < body.orbitingBodies.Count; j++)
                {
                    CelestialBody moon = body.orbitingBodies[j];

                    moons.Add(moon);

                    for (int k = 0; k < moon.orbitingBodies.Count; k++)
                    {
                        CelestialBody subMoon = moon.orbitingBodies[k];

                        moons.Add(subMoon);

                        for (int l = 0; l < subMoon.orbitingBodies.Count; l++)
                        {
                            CelestialBody subSubMoon = subMoon.orbitingBodies[l];

                            moons.Add(subSubMoon);
                        }
                    }
                }

                bodies.Add(new Tracking_BodyGroup() { Body = body, Moons = moons });
            }

            for (int i = bodies.Count - 1; i >= 0; i--)
            {
                Tracking_BodyGroup body = bodies[i];

                if (body.Body != Planetarium.fetch.Home)
                    continue;

                bodies.RemoveAt(i);
                bodies.Insert(0, body);
            }

            bodies.Insert(1, new Tracking_BodyGroup() { Body = Planetarium.fetch.Sun, Moons = new List<CelestialBody>() });

            return bodies;
        }

        public int CurrentMode
        {
            get { return (int)_CurrentMode; }
        }

        public void SortBody(bool isOn)
        {
            if (isOn && _CurrentMode != Tracking_Mode.CelestialBody)
                ActivateCelestialSort();

            Tracking_Persistence.SortMode = (int)_CurrentMode;
        }

        public void SortType(bool isOn)
        {
            if (isOn && _CurrentMode != Tracking_Mode.VesselType)
                ActivateVesselTypeSort();

            Tracking_Persistence.SortMode = (int)_CurrentMode;
        }

        public void SortCustom(bool isOn)
        {
            if (isOn && _CurrentMode != Tracking_Mode.Custom)
                ActivateCustomSort();

            Tracking_Persistence.SortMode = (int)_CurrentMode;
        }

        public void SortDefault(bool isOn)
        {
            if (isOn && _CurrentMode != Tracking_Mode.Default)
                ActivateDefaultSort();

            Tracking_Persistence.SortMode = (int)_CurrentMode;
        }
    }
}
