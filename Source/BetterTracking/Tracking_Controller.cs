#region License
/*The MIT License (MIT)

Better Tracking

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

using System;
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
        private const string CONTROL_LOCK = "TRACKING_LOCK";

        public static OnWidgetSelect OnWidgetSelect = new OnWidgetSelect();
        public static OnWidgetAwake OnWidgetAwake = new OnWidgetAwake();

        private bool _widgetAwakeSet;
        private bool _instantStart;
        private bool _inputLock;

        private string _searchString = "";

        private SpaceTracking _TrackingStation;
        
        private GameObject _OldTrackingList;
        private GameObject _NewTrackingList;
        private Transform _ListParent;

        ReorderableList _ReorderableList;

        private ScrollRect _ScrollView;
        private Rect _ScrollViewRect;
        private Camera _CanvasCamera;
        
        private Tracking_Mode _CurrentMode = Tracking_Mode.CelestialBody;

        private DictionaryValueList<Vessel, double> _VesselManeuvers = new DictionaryValueList<Vessel, double>();
        
        private List<TrackingStationWidget> _TrackedVesselWidgets = new List<TrackingStationWidget>();

        private List<Tracking_BodyGroup> _OrderedBodyList = new List<Tracking_BodyGroup>();
        private List<VesselType> _OrderedTypeList = new List<VesselType>();

        private List<Tracking_Group> _TrackingGroups = new List<Tracking_Group>();

        private List<Tracking_Vessel> _TrackingVessels = new List<Tracking_Vessel>();

        private List<VesselGroup> _UIList = new List<VesselGroup>();
        private List<FullVesselItem> _UIVesselList = new List<FullVesselItem>();

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

            GameEvents.onNewVesselCreated.Add(new EventData<Vessel>.OnEvent(OnVesselCreate));
            GameEvents.onVesselDestroy.Add(new EventData<Vessel>.OnEvent(OnVesselDestroy));
            GameEvents.onKnowledgeChanged.Add(new EventData<GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels>>.OnEvent(OnKnowledgeChange));

            _CurrentMode = (Tracking_Mode)Tracking_Persistence.SortMode;

            StartCoroutine(WaitForTrackingStation());
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
            
            OnWidgetSelect.RemoveListener(new UnityAction<TrackingStationWidget>(OnWidgetSelected));
            OnWidgetAwake.RemoveListener(new UnityAction<TrackingStationWidget>(OnWidgetAwaken));

            GameEvents.onNewVesselCreated.Remove(new EventData<Vessel>.OnEvent(OnVesselCreate));
            GameEvents.onVesselDestroy.Remove(new EventData<Vessel>.OnEvent(OnVesselDestroy));
            GameEvents.onKnowledgeChanged.Remove(new EventData<GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels>>.OnEvent(OnKnowledgeChange));
        }

        private IEnumerator WaitForTrackingStation()
        {
            while (_TrackingStation == null)
            {
                var tracking = FindObjectsOfType<SpaceTracking>();

                if (tracking != null)
                {
                    for (int i = 0; i < tracking.Length; i++)
                    {
                        SpaceTracking space = tracking[i];

                        if (space == null)
                            continue;
                        
                        _TrackingStation = space;
                    }
                }

                if (_TrackingStation == null)
                    yield return null;
            }

            _ListParent = _TrackingStation.listContainer.parent;

            FindScrollRect();

            StartCoroutine(WaitForCamera());

            AdjustUITransforms();

            StartCoroutine(AttachSortHeader());

            _VesselToggleGroup = Instantiate(_TrackingStation.listToggleGroup);

            _OldTrackingList = _TrackingStation.listContainer.gameObject;

            _NewTrackingList = Instantiate(_OldTrackingList);

            _ReorderableList = _OldTrackingList.transform.parent.gameObject.AddComponent<ReorderableList>();
            _ReorderableList.Init(_NewTrackingList.GetComponent<LayoutGroup>(), _NewTrackingList.GetComponent<RectTransform>());
            _ReorderableList.SortType = (int)_CurrentMode;

            _NewTrackingList.transform.SetParent(_ListParent, false);

            _TrackingStation.listContainer.SetParent(null, false);

            _OrderedBodyList = OrderBodies();
            _OrderedTypeList = OrderTypes();

            Tracking_Utils.TrackingLog("Tracking Station Processed");
        }
        
        private void FindScrollRect()
        {
            _ScrollView = _TrackingStation.listContainer.GetComponentInParent<ScrollRect>();

            if (_ScrollView == null)
                Tracking_Utils.TrackingLog("Scroll Rect Not Found");
            else
                Tracking_Utils.TrackingLog("Scroll Rect Found");
        }

        private IEnumerator WaitForCamera()
        {
            while (_CanvasCamera == null)
            {
                _CanvasCamera = FindCamera();

                if (_CanvasCamera == null)
                    Tracking_Utils.TrackingLog("Canvas Camera Not Found");
                else
                    Tracking_Utils.TrackingLog("Canvas Camera Found");

                if (_CanvasCamera == null)
                    yield return null;
            }

            FindCorners();
        }

        private Camera FindCamera()
        {
            if (_TrackingStation.listContainer.parent != null)
                return _TrackingStation.listContainer.GetComponentInParent<Canvas>().worldCamera;
            else if (_NewTrackingList != null && _NewTrackingList.transform.parent != null)
                return _NewTrackingList.GetComponentInParent<Canvas>().worldCamera;

            return null;
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

            Tracking_Utils.TrackingLog("Detected Vessel List Corners");
        }

        private void AdjustUITransforms()
        {
            if (_TrackingStation == null)
                return;

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

            Tracking_Utils.TrackingLog("Squishing Sidebar UI Elements");
        }

        private IEnumerator AttachSortHeader()
        {
            Transform parent = _TrackingStation.listContainer.parent.parent.parent;

            while (Tracking_Loader.SortHeaderPrefab == null)
                yield return null;

            SortHeader sort = Instantiate(Tracking_Loader.SortHeaderPrefab).GetComponent<SortHeader>();
            sort.transform.SetParent(parent, false);
            sort.transform.SetSiblingIndex(2);
            sort.Initialize(this);

            Tracking_Utils.TrackingLog("Sort Header Inserted");
        }

        private void OnVesselCreate(Vessel vessel)
        {
            _instantStart = true;
        }

        private void OnVesselDestroy(Vessel vessel)
        {
            _instantStart = true;
            
            if (_TrackedVesselWidgets == null || _TrackedVesselWidgets.Count <= 1)
                StartCoroutine(WaitForUpdate(3));
        }

        private void OnKnowledgeChange(GameEvents.HostedFromToAction<IDiscoverable, DiscoveryLevels> knowledge)
        {
            _instantStart = true;

            if (_TrackedVesselWidgets == null || _TrackedVesselWidgets.Count <= 1)
                StartCoroutine(WaitForUpdate(3));
        }

        private IEnumerator WaitForUpdate(int frames)
        {
            int time = 0;

            while (time < frames)
            {
                time++;

                yield return new WaitForEndOfFrame();
            }
            
            _TrackedVesselWidgets.Clear();

            ParseWidgetContainer();

            if (_TrackedVesselWidgets == null || _TrackedVesselWidgets.Count <= 0)
            {
                if (string.IsNullOrEmpty(_searchString))
                    ListUpdate();
                else
                    SearchListUpdate();
            }
        }

        private void OnWidgetSelected(TrackingStationWidget widget)
        {
            if (_TrackingGroups == null || _TrackingGroups.Count <= 0)
                return;

            if (widget == null)
                return;
            else if (widget.vessel == null)
                return;

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
            if (_widgetAwakeSet || _NewTrackingList == null)
                return;
            
            _widgetAwakeSet = true;

            StartCoroutine(WidgetListReset(2));
        }

        private IEnumerator WidgetListReset(int frames)
        {
            int time = 0;

            while (time < frames)
            {
                time++;

                yield return new WaitForEndOfFrame();
            }
            
            UpdateScrollRect(_NewTrackingList.transform as RectTransform);
            
            if (_ReorderableList != null)
                _ReorderableList.SortType = (int)_CurrentMode;
            
            _TrackedVesselWidgets.Clear();

            _OrderedBodyList = OrderBodies();
            _OrderedTypeList = OrderTypes();

            ParseWidgetContainer();

            if (string.IsNullOrEmpty(_searchString))
                ListUpdate();
            else
                SearchListUpdate();

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
            
            for (int i = _TrackingStation.listContainer.childCount - 1; i >= 0; i--)
            {
                Transform t = _TrackingStation.listContainer.GetChild(i);

                TrackingStationWidget widget = t.GetComponent<TrackingStationWidget>();

                if (widget != null && widget.vessel != null)
                {
                    _TrackedVesselWidgets.Add(widget);

                    if (!_VesselManeuvers.ContainsKey(widget.vessel))
                    {
                        bool maneuver = false;

                        double time = Vessel.GetNextManeuverTime(widget.vessel, out maneuver);

                        if (maneuver)
                            _VesselManeuvers.Add(widget.vessel, time);
                        else
                            _VesselManeuvers.Add(widget.vessel, -1000000000);
                    }
                }
            }
        }

        public void ListUpdate()
        {
            _TrackingGroups.Clear();
            _TrackingVessels.Clear();
            
            switch(_CurrentMode)
            {
                case Tracking_Mode.CelestialBody:
                    _TrackingGroups = SortCelestialBodies();
                    break;
                case Tracking_Mode.Default:
                    _TrackingVessels = SortDefaultType();
                    break;
                case Tracking_Mode.VesselType:
                    _TrackingGroups = SortVesselType();
                    break;
            }
            
            ClearUI();

            GenerateUI();

            _instantStart = false;
        }

        private void SearchListUpdate()
        {
            if (_ReorderableList != null)
                _ReorderableList.SortType = 4;

            _TrackingVessels.Clear();

            _TrackingVessels = SortDefaultType();

            _TrackingVessels = SearchVessels(_TrackingVessels);
            
            ClearUI();

            GenerateSearchUI();

            _instantStart = false;
        }

        private List<Tracking_Group> SortCelestialBodies()
        {
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
                    
                    if (moonVessels.Count > 0)
                        moonGroups.Add(new Tracking_MoonGroup() { Moon = body.Moons[k], Vessels = SortWidgets(moonVessels) });
                }

                if (bodyVessels.Count > 0 || moonGroups.Count > 0)
                    vesselGroups.Add(new Tracking_Group(Tracking_Utils.LocalizeBodyName(body.Body.displayName), Tracking_Persistence.GetBodyPersistence(body.Body.flightGlobalsIndex), _instantStart, SortWidgets(bodyVessels), moonGroups, body.Body, VesselType.Unknown, Tracking_Mode.CelestialBody));
            }

            return vesselGroups;
        }

        private List<Tracking_Group> SortVesselType()
        {
            List<Tracking_Group> vesselGroups = new List<Tracking_Group>();

            int count = _OrderedTypeList.Count;

            for (int i = 0; i < count; i++)
            {
                Tracking_Group group = VesselTypeGroup(_OrderedTypeList[i]);

                if (group != null)
                    vesselGroups.Add(group);
            }
            
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
                return new Tracking_Group(Tracking_Utils.VesselTypeString(type), Tracking_Persistence.GetTypePersistence((int)type), _instantStart, SortWidgets(typeVessels), null, null, type, Tracking_Mode.VesselType);

            return null;
        }

        private List<Tracking_Vessel> SortDefaultType()
        {
            List<Tracking_Vessel> vessels = new List<Tracking_Vessel>();

            List<TrackingStationWidget> widgets = SortWidgets(_TrackedVesselWidgets);
            
            for (int i = widgets.Count - 1; i >= 0; i--)
            {
                if (widgets[i] != null)
                    vessels.Add(new Tracking_Vessel(widgets[i]));
            }

            return vessels;
        }

        private List<Tracking_Vessel> SearchVessels(List<Tracking_Vessel> vessels)
        {
            List<Tracking_Vessel> searchList = new List<Tracking_Vessel>();

            int count = vessels.Count;

            for (int i = 0; i < count; i++)
            {
                if (vessels[i].Vessel.vesselName.StringContains(_searchString, StringComparison.OrdinalIgnoreCase))
                    searchList.Add(vessels[i]);
            }

            return searchList;
        }

        private List<TrackingStationWidget> SortWidgets(List<TrackingStationWidget> widgets)
        {
            List<TrackingStationWidget> sorted = new List<TrackingStationWidget>();

            int mode = 0;
            bool asc = true;

            switch (_CurrentMode)
            {
                case Tracking_Mode.CelestialBody:
                    mode = BodySortMode;
                    asc = BodySortOrder;
                    break;
                case Tracking_Mode.VesselType:
                    mode = TypeSortMode;
                    asc = TypeSortOrder;
                    break;
                case Tracking_Mode.Default:
                    mode = StockSortMode;
                    asc = StockSortOrder;
                    break;
            }
            
            switch (mode)
            {
                case 0:
                    sorted = SortWidgetsTime(widgets, asc);
                    break;
                case 1:
                    sorted = SortWidgetsAlpha(widgets, asc);
                    break;
                case 2:
                    switch (_CurrentMode)
                    {
                        case Tracking_Mode.CelestialBody:
                            sorted = SortWidgetsType(widgets, asc);
                            break;
                        case Tracking_Mode.VesselType:
                            sorted = SortWidgetsBody(widgets, asc);
                            break;
                        case Tracking_Mode.Default:
                            sorted = SortWidgetsType(widgets, asc);
                            break;
                    }
                    break;
                case 3:
                    switch (_CurrentMode)
                    {
                        case Tracking_Mode.CelestialBody:
                            sorted = SortWidgetsType(widgets, asc);
                            break;
                        case Tracking_Mode.VesselType:
                            sorted = SortWidgetsBody(widgets, asc);
                            break;
                        case Tracking_Mode.Default:
                            sorted = SortWidgetsBody(widgets, asc);
                            break;
                    }
                    break;
            }

            return sorted;
        }

        private List<KeyValuePair<TrackingStationWidget, double>> GetManeuvers(List<TrackingStationWidget> widgets)
        {
            List<KeyValuePair<TrackingStationWidget, double>> maneuvers = new List<KeyValuePair<TrackingStationWidget, double>>();

            for (int i = widgets.Count - 1; i >= 0; i--)
            {
                if (_VesselManeuvers.ContainsKey(widgets[i].vessel))
                {
                    double t = _VesselManeuvers[widgets[i].vessel];

                    if (t > 0)
                    {
                        maneuvers.Add(new KeyValuePair<TrackingStationWidget, double>(widgets[i], t));

                        widgets.RemoveAt(i);
                    }
                }
                else
                {
                    bool maneuver = false;

                    double t = Vessel.GetNextManeuverTime(widgets[i].vessel, out maneuver);

                    if (maneuver)
                    {
                        _VesselManeuvers.Add(widgets[i].vessel, t);

                        maneuvers.Add(new KeyValuePair<TrackingStationWidget, double>(widgets[i], t));

                        widgets.RemoveAt(i);
                    }
                    else
                        _VesselManeuvers.Add(widgets[i].vessel, -1000000000);
                }
            }

            if (maneuvers.Count > 0)
                maneuvers.Sort((a, b) => b.Value.CompareTo(a.Value));

            return maneuvers;
        }

        private List<TrackingStationWidget> SortWidgetsTime(List<TrackingStationWidget> widgets, bool asc)
        {
            List<KeyValuePair<TrackingStationWidget, double>> maneuvers = GetManeuvers(widgets);

            List<TrackingStationWidget> sorted = new List<TrackingStationWidget>();

            if (asc)
                sorted = widgets;
            else
            {
                for (int i = widgets.Count - 1; i >= 0; i--)
                {
                    sorted.Add(widgets[i]);
                }
            }

            int count = maneuvers.Count;

            for (int i = 0; i < count; i++)
            {
                sorted.Add(maneuvers[i].Key);
            }

            return sorted;
        }

        private List<TrackingStationWidget> SortWidgetsAlpha(List<TrackingStationWidget> widgets, bool asc)
        {
            List<KeyValuePair<TrackingStationWidget, double>> maneuvers = GetManeuvers(widgets);

            widgets.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!asc, a.vessel.vesselName.CompareTo(b.vessel.vesselName), a.vessel.launchTime.CompareTo(b.vessel.launchTime)));

            int count = maneuvers.Count;

            for (int i = 0; i < count; i++)
            {
                widgets.Add(maneuvers[i].Key);
            }

            return widgets;
        }

        private List<TrackingStationWidget> SortWidgetsBody(List<TrackingStationWidget> widgets, bool asc)
        {
            List<KeyValuePair<TrackingStationWidget, double>> maneuvers = GetManeuvers(widgets);

            List<TrackingStationWidget> sorted = new List<TrackingStationWidget>();
            
            if (asc)
            {
                for (int i = _OrderedBodyList.Count - 1; i >= 0; i--)
                {
                    Tracking_BodyGroup group = _OrderedBodyList[i];

                    for (int k = group.Moons.Count - 1; k >= 0; k--)
                    {
                        int index = group.Moons[k].flightGlobalsIndex;

                        for (int l = widgets.Count - 1; l >= 0; l--)
                        {
                            if (widgets[l].vessel.mainBody.flightGlobalsIndex == index)
                                sorted.Add(widgets[l]);
                        }
                    }

                    for (int j = widgets.Count - 1; j >= 0; j--)
                    {
                        if (widgets[j].vessel.mainBody.flightGlobalsIndex == group.Body.flightGlobalsIndex)
                            sorted.Add(widgets[j]);
                    }
                }
            }
            else
            {
                int b = _OrderedBodyList.Count;

                for (int i = 0; i < b; i++)
                {
                    Tracking_BodyGroup group = _OrderedBodyList[i];

                    int m = group.Moons.Count;

                    for (int k = 0; k < m; k++)
                    {
                        int index = group.Moons[k].flightGlobalsIndex;

                        for (int l = widgets.Count - 1; l >= 0; l--)
                        {
                            if (widgets[l].vessel.mainBody.flightGlobalsIndex == index)
                                sorted.Add(widgets[l]);
                        }
                    }

                    for (int j = widgets.Count - 1; j >= 0; j--)
                    {
                        if (widgets[j].vessel.mainBody.flightGlobalsIndex == group.Body.flightGlobalsIndex)
                            sorted.Add(widgets[j]);
                    }
                }
            }

            int count = maneuvers.Count;

            for (int i = 0; i < count; i++)
            {
                sorted.Add(maneuvers[i].Key);
            }

            return sorted;
        }

        private List<TrackingStationWidget> SortWidgetsType(List<TrackingStationWidget> widgets, bool asc)
        {
            List<KeyValuePair<TrackingStationWidget, double>> maneuvers = GetManeuvers(widgets);

            List<TrackingStationWidget> sorted = new List<TrackingStationWidget>();
            
            if (asc)
            {
                for (int i = Tracking_Persistence.TypeOrderList.Count - 1; i >= 0; i--)
                {
                    int index = Tracking_Persistence.TypeOrderList[i];

                    for (int j = widgets.Count - 1; j >= 0; j--)
                    {
                        if (widgets[j].vessel.vesselType == (VesselType)index)
                            sorted.Add(widgets[j]);
                    }
                }
            }
            else
            {
                int t = Tracking_Persistence.TypeOrderList.Count;

                for (int i = 0; i < t; i++)
                {
                    int index = Tracking_Persistence.TypeOrderList[i];
                    
                    for (int j = widgets.Count - 1; j >= 0; j--)
                    {
                        if (widgets[j].vessel.vesselType == (VesselType)index)
                            sorted.Add(widgets[j]);
                    }
                }
            }

            int count = maneuvers.Count;

            for (int i = 0; i < count; i++)
            {
                sorted.Add(maneuvers[i].Key);
            }

            return sorted;
        }

        private void ClearUI()
        {
            for (int i = _UIList.Count - 1; i >= 0; i--)
            {
                _UIList[i].gameObject.SetActive(false);
                _UIList[i].transform.SetParent(null, false);
                DestroyImmediate(_UIList[i].gameObject);
            }

            _UIList.Clear();

            for (int i = 0; i < _UIVesselList.Count; i++)
            {
                _UIVesselList[i].gameObject.SetActive(false);
                _UIVesselList[i].transform.SetParent(null, false);
                DestroyImmediate(_UIVesselList[i].gameObject);
            }

            _UIVesselList.Clear();

            int count = _NewTrackingList.transform.childCount;

            for (int i = count - 1; i >= 0; i--)
            {
                _NewTrackingList.transform.GetChild(i).gameObject.SetActive(false);
                DestroyImmediate(_NewTrackingList.transform.GetChild(i).gameObject);
            }
        }

        private void GenerateUI()
        {
            if (_NewTrackingList == null)
                return;

            switch (_CurrentMode)
            {
                case Tracking_Mode.Default:
                    if (_TrackingVessels == null || _TrackingVessels.Count <= 0)
                        return;

                    if (Tracking_Loader.FullVesselPrefab == null)
                        return;

                    int vessels = _TrackingVessels.Count;

                    for (int i = 0; i < vessels; i++)
                    {
                        AddTrackingVessel(_TrackingVessels[i]);
                    }

                    break;
                default:
                    if (_TrackingGroups == null || _TrackingGroups.Count <= 0)
                        return;

                    if (Tracking_Loader.GroupPrefab == null)
                        return;

                    int count = _TrackingGroups.Count;

                    for (int i = 0; i < count; i++)
                    {
                        AddTrackingGroup(_TrackingGroups[i]);
                    }
                    break;
            }
        }

        private void GenerateSearchUI()
        {
            if (_NewTrackingList == null)
                return;

            if (_TrackingVessels == null || _TrackingVessels.Count <= 0)
                return;

            if (Tracking_Loader.FullVesselPrefab == null)
                return;

            int vessels = _TrackingVessels.Count;

            for (int i = 0; i < vessels; i++)
            {
                AddTrackingVessel(_TrackingVessels[i]);
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

        private void AddTrackingVessel(Tracking_Vessel vessel)
        {
            FullVesselItem UI = Instantiate(Tracking_Loader.FullVesselPrefab).GetComponent<FullVesselItem>();

            if (UI == null)
                return;

            UI.transform.SetParent(_NewTrackingList.transform, false);
            UI.Initialize(vessel);

            _UIVesselList.Add(UI);
        }

        public void ActivateDefaultSort()
        {
            _CurrentMode = Tracking_Mode.Default;
            
            StartCoroutine(WidgetListReset(1));
        }

        public void ActivateCelestialSort()
        {
            _CurrentMode = Tracking_Mode.CelestialBody;
            
            StartCoroutine(WidgetListReset(1));
        }

        public void ActivateVesselTypeSort()
        {
            _CurrentMode = Tracking_Mode.VesselType;
            
            StartCoroutine(WidgetListReset(1));
        }

        public void ActivateCustomSort()
        {
            _CurrentMode = Tracking_Mode.Custom;

            StartCoroutine(WidgetListReset(1));
        }

        private List<Tracking_BodyGroup> OrderBodies()
        {
            var allBodies = FlightGlobals.Bodies.Where(b => b.referenceBody == Planetarium.fetch.Sun && b.referenceBody != b);

            var orderedBodies = allBodies.OrderBy(b => b.orbit.semiMajorAxis).ToList();

            List<Tracking_BodyGroup> bodies = RecursiveCelestialBodies(orderedBodies);

            //for (int i = 0; i < orderedBodies.Count; i++)
            //{
            //    CelestialBody body = orderedBodies[i];

            //    List<CelestialBody> moons = new List<CelestialBody>();

            //    for (int j = 0; j < body.orbitingBodies.Count; j++)
            //    {
            //        CelestialBody moon = body.orbitingBodies[j];

            //        moons.Add(moon);

            //        for (int k = 0; k < moon.orbitingBodies.Count; k++)
            //        {
            //            CelestialBody subMoon = moon.orbitingBodies[k];

            //            moons.Add(subMoon);

            //            for (int l = 0; l < subMoon.orbitingBodies.Count; l++)
            //            {
            //                CelestialBody subSubMoon = subMoon.orbitingBodies[l];

            //                moons.Add(subSubMoon);

            //                for (int m = 0; m < subSubMoon.orbitingBodies.Count; m++)
            //                {
            //                    CelestialBody subSubSubMoon = subSubMoon.orbitingBodies[m];

            //                    moons.Add(subSubSubMoon);

            //                    for (int n = 0; n < subSubSubMoon.orbitingBodies.Count; n++)
            //                    {
            //                        CelestialBody subSubSubSubMoon = subSubSubMoon.orbitingBodies[n];

            //                        moons.Add(subSubSubSubMoon);
            //                    }
            //                }
            //            }
            //        }
            //    }

            //    bodies.Add(new Tracking_BodyGroup() { Body = body, Moons = moons });
            //}

            bool missingHome = true;

            for (int i = bodies.Count - 1; i >= 0; i--)
            {
                if (bodies[i].Body == FlightGlobals.GetHomeBody())
                {
                    missingHome = false;
                    break;
                }
            }

            if (missingHome)
            {
                List<Tracking_BodyGroup> missingHomeBodies = RecursiveCelestialBodies(new List<CelestialBody>() { FlightGlobals.GetHomeBody() });

                bodies.InsertRange(0, missingHomeBodies);
            }
            else
            {
                for (int i = bodies.Count - 1; i >= 0; i--)
                {
                    Tracking_BodyGroup body = bodies[i];

                    if (body.Body != Planetarium.fetch.Home)
                        continue;

                    bodies.RemoveAt(i);
                    bodies.Insert(0, body);
                }
            }

            bodies.Insert(1, new Tracking_BodyGroup() { Body = Planetarium.fetch.Sun, Moons = new List<CelestialBody>() });

            List<Tracking_BodyGroup> ordered = new List<Tracking_BodyGroup>();

            for (int i = 0; i < Tracking_Persistence.BodyOrderList.Count; i++)
            {
                for (int j = bodies.Count - 1; j >= 0; j--)
                {
                    int index = bodies[j].Body.flightGlobalsIndex;

                    if (index != Tracking_Persistence.BodyOrderList[i])
                        continue;
                    
                    ordered.Add(bodies[j]);
                    break;
                }
            }

            return ordered;
        }

        private List<Tracking_BodyGroup> RecursiveCelestialBodies(List<CelestialBody> bodies)
        {
            List<Tracking_BodyGroup> trackingBodies = new List<Tracking_BodyGroup>();

            for (int i = 0; i < bodies.Count; i++)
            {
                CelestialBody body = bodies[i];

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

                            for (int m = 0; m < subSubMoon.orbitingBodies.Count; m++)
                            {
                                CelestialBody subSubSubMoon = subSubMoon.orbitingBodies[m];

                                moons.Add(subSubSubMoon);

                                for (int n = 0; n < subSubSubMoon.orbitingBodies.Count; n++)
                                {
                                    CelestialBody subSubSubSubMoon = subSubSubMoon.orbitingBodies[n];

                                    moons.Add(subSubSubSubMoon);
                                }
                            }
                        }
                    }
                }

                trackingBodies.Add(new Tracking_BodyGroup() { Body = body, Moons = moons });
            }

            return trackingBodies;
        }

        private List<VesselType> OrderTypes()
        {
            List<VesselType> types = new List<VesselType>();

            for (int i = 0; i < Tracking_Persistence.TypeOrderList.Count; i++)
            {
                for (int j = 15 - 1; j >= 0; j--)
                {
                    if (j != Tracking_Persistence.TypeOrderList[i])
                        continue;

                    types.Add((VesselType)j);
                    break;
                }
            }

            return types;
        }

        public bool SelectedVessel(Vessel vessel)
        {
            if (_TrackingStation == null || _TrackingStation.SelectedVessel == null)
                return false;

            return _TrackingStation.SelectedVessel == vessel;
        }

        public int CurrentMode
        {
            get { return (int)_CurrentMode; }
        }

        public int BodySortMode
        {
            get { return Tracking_Persistence.BodyOrderMode; }
            set
            {
                Tracking_Persistence.BodyOrderMode = value;

                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public int TypeSortMode
        {
            get { return Tracking_Persistence.TypeOrderMode; }
            set
            {
                Tracking_Persistence.TypeOrderMode = value;
                
                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public int StockSortMode
        {
            get { return Tracking_Persistence.StockOrderMode; }
            set
            {
                Tracking_Persistence.StockOrderMode = value;

                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public bool BodySortOrder
        {
            get { return Tracking_Persistence.BodyAscOrder; }
            set
            {
                Tracking_Persistence.BodyAscOrder = value;

                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public bool TypeSortOrder
        {
            get { return Tracking_Persistence.TypeAscOrder; }
            set
            {
                Tracking_Persistence.TypeAscOrder = value;

                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public bool StockSortOrder
        {
            get { return Tracking_Persistence.StockAscOrder; }
            set
            {
                Tracking_Persistence.StockAscOrder = value;

                _instantStart = true;

                StartCoroutine(WidgetListReset(1));
            }
        }

        public Transform DropDownParent
        {
            get { return _ListParent.parent; }
        }
        
        public bool LockInput
        {
            get { return _inputLock; }
            set
            {
                _inputLock = value;

                if (_inputLock)
                    InputLockManager.SetControlLock(CONTROL_LOCK);
                else
                    InputLockManager.RemoveControlLock(CONTROL_LOCK);
            }
        }

        public string SearchString
        {
            get { return _searchString; }
            set
            {
                if (_searchString == value)
                    return;
                
                _searchString = value;

                _instantStart = true;

                if (string.IsNullOrEmpty(value))
                    StartCoroutine(WidgetListReset(1));
                else
                    SearchListUpdate();
            }
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
