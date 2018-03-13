#region License
/*The MIT License (MIT)

One Window

Tracking_Utils - Utilities class

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
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KSP.Localization;
using KSP.UI.Screens;
using TMPro;

namespace BetterTracking
{
    public static class Tracking_Utils
    {
        public static string LocalizeBodyName(string input)
        {
            return Localizer.Format("<<1>>", input);
        }

        public static void TrackingLog(string message, params object[] stringObjects)
        {
            message = string.Format(message, stringObjects);
            string finalLog = string.Format("[Better_Tracking] {0}", message);
            Debug.Log(finalLog);
        }

        public static GameObject GetHeaderObject(CelestialBody body, VesselType type, Tracking_Mode mode, bool moon)
        {
            switch (mode)
            {
                case Tracking_Mode.CelestialBody:
                    return GetHeaderObject(body, moon);
                case Tracking_Mode.VesselType:
                    return GetHeaderObject(type);
            }

            return null;
        }

        private static GameObject GetHeaderObject(VesselType type)
        {
            VesselIconSprite _iconSPrite = GameObject.Instantiate(Tracking_Loader.IconPrefab);

            _iconSPrite.SetType(type);

            return _iconSPrite.gameObject;
        }

        private static GameObject GetHeaderObject(CelestialBody body, bool moon)
        {
            if (body == null || body.scaledBody == null || Tracking_RDWatch.RDPlanetPrefab == null)
                return null;

            PSystemBody pBody = GetBody(body.bodyName, PSystemManager.Instance.systemPrefab.rootBody);

            GameObject obj = GameObject.Instantiate(pBody.scaledVersion);
            GameObject.DestroyImmediate(obj.GetComponent<ScaledSpaceFader>());
            GameObject.DestroyImmediate(obj.GetComponent<MaterialSetDirection>());
            GameObject.DestroyImmediate(obj.GetComponent<SphereCollider>());
            GameObject.DestroyImmediate(GameObject.Find(obj.name + "/Atmosphere"));

            RDPlanetListItemContainer planet = GameObject.Instantiate(Tracking_RDWatch.RDPlanetPrefab);
            planet.Setup(body.bodyName, body.displayName, obj, false, 0, moon ? 0.8f : 1, 44, 0, null);
            planet.SetSelectionCallback(new RDPlanetListItemContainer.SelectionCallback(FakeCallback));

            RectTransform pRect = planet.transform as RectTransform;

            pRect.anchoredPosition = new Vector2(2, -2);
            pRect.sizeDelta = new Vector2(-4, -4);

            RawImage raw = planet.planetRawImage;
            RectTransform rect = raw.transform as RectTransform;

            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(4, -2);
            rect.sizeDelta = new Vector2(0, -4);

            GameObject.DestroyImmediate(planet.label_planetName.gameObject);

            if (!Tracking_Controller.Instance.LightAdded)
            {
                Light light = Tracking_Controller.Instance.NewTrackingList.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.range = 1000;
                light.cullingMask = planet.thumbnailCameraMask;

                Tracking_Controller.Instance.LightAdded = true;
            }

            return planet.gameObject;
        }

        private static void FakeCallback(RDPlanetListItemContainer container, bool isOn)
        {
            if (!isOn || container == null || PlanetariumCamera.fetch == null)
                return;

            PlanetariumCamera.fetch.SetTarget(container.name);
        }

        private static PSystemBody GetBody(string name, PSystemBody root)
        {
            if (root.celestialBody.bodyName == name)
                return root;

            for (int i = 0; i < root.children.Count; i++)
            {
                PSystemBody pBody = GetBody(name, root.children[i]);

                if (pBody != null)
                    return pBody;
            }

            return null;
        }

        public static void LogPrefab(Transform prefab, int spaces)
        {
            RectTransform rect = prefab as RectTransform;

            if (rect == null)
            {
                for (int i = 0; i < prefab.childCount; i++)
                {
                    LogPrefab(prefab.GetChild(i), spaces + 5);
                }

                return;
            }

            TrackingLog("{0}Prefab Element: {1}\n{0}Anchored Position: {2:N2}\n{0}Size Delta: {3:N2}\n{0}Pivot: {4:N2}\n{0}Anchor Min: {5:N3}\n{0}Anchor Max: {6:N3}"
                , Dashes(spaces), rect.name, rect.anchoredPosition, rect.sizeDelta, rect.pivot, rect.anchorMin, rect.anchorMax);

            TextMeshProUGUI tmp = rect.GetComponent<TextMeshProUGUI>();

            if (tmp != null)
            {
                TrackingLog("{0}Text Mesh Pro Elements: {1}\n{0}Font Color: {2}\n{0}Font Size: {3:N2}\n{0}Font Alignment: {4}\n{0}Font Style: {5}"
                    , Dashes(spaces), rect.name, tmp.color, tmp.fontSize, tmp.alignment, tmp.fontStyle);
            }

            for (int i = 0; i < prefab.childCount; i++)
            {
                LogPrefab(prefab.GetChild(i), spaces + 5);
            }
        }

        private static string Dashes(int count)
        {
            string s = "";

            for (int i = 0; i < count; i++)
            {
                s += "-";
            }

            s += ">";

            return s;
        }

        public static string ConcatDictionary(Dictionary<int, bool> values)
        {
            int count = values.Count;

            StringBuilder sb = StringBuilderCache.Acquire(512);

            for (int i = 0; i < count; i++)
            {
                var pair = values.ElementAt(i);

                sb.AppendFormat("{0},{1}|", pair.Key, pair.Value);

                //TrackingLog("Dictionary Key: {0} - Value: {1}", pair.Key, pair.Value);
            }

            if (sb.Length > 1)
            {
                if (sb[sb.Length - 1] == '|')
                    sb.Length -= 1;
            }

            //TrackingLog("Save Dictionary: {0}", sb.ToString());

            return sb.ToStringAndRelease();
        }

        public static Dictionary<int, bool> ParseDictionary(string text)
        {
            Dictionary<int, bool> dict = new Dictionary<int, bool>();

            string[] pairs = text.Split('|');

            for (int i = pairs.Length - 1; i >= 0; i--)
            {
                string[] pair = pairs[i].Split(',');

                int key = -1;

                if (!int.TryParse(pair[0], out key))
                    continue;

                bool value = true;

                bool.TryParse(pair[1], out value);

                if (!dict.ContainsKey(key))
                    dict.Add(key, value);
            }

            return dict;
        }

        public static string VesselTypeString(VesselType type)
        {
            switch (type)
            {
                case VesselType.Base:
                    return Localizer.Format("#autoLoc_6002178");
                case VesselType.Debris:
                    return Localizer.Format("#autoLOC_900676");
                case VesselType.EVA:
                    return Localizer.Format("#autoLOC_6003088");
                case VesselType.Flag:
                    return Localizer.Format("#autoLoc_6002179");
                case VesselType.Lander:
                    return Localizer.Format("#autoLOC_900686");
                case VesselType.Plane:
                    return Localizer.Format("#autoLOC_900685");
                case VesselType.Probe:
                    return Localizer.Format("#autoLOC_900681");
                case VesselType.Relay:
                    return Localizer.Format("#autoLOC_900687");
                case VesselType.Rover:
                    return Localizer.Format("#autoLOC_900683");
                case VesselType.Ship:
                    return Localizer.Format("#autoLOC_900684");
                case VesselType.SpaceObject:
                    return Localizer.Format("#autoLoc_6002177");
                case VesselType.Station:
                    return Localizer.Format("#autoLOC_900679");
                case VesselType.Unknown:
                    return Localizer.Format("#autoLOC_6002223");
            }

            return "";
        }

        //Extension taken from:
        //https://forum.unity.com/threads/test-if-ui-element-is-visible-on-screen.276549/#post-2978773
        /// <summary>
        /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
        /// </summary>
        /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        private static int CountCornersVisibleFrom(this RectTransform rectTransform, Camera camera, Rect view)
        {
            Vector3[] objectCorners = new Vector3[4];
            rectTransform.GetWorldCorners(objectCorners);

            int visibleCorners = 0;
            Vector3 tempScreenSpaceCorner; // Cached
            for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
            {
                tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
                if (view.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }

        /// <summary>
        /// Determines if this RectTransform is fully visible from the specified camera.
        /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsFullyVisibleFrom(this RectTransform rectTransform, Camera camera, Rect view)
        {
            return CountCornersVisibleFrom(rectTransform, camera, view) == 4; // True if all 4 corners are visible
        }

        /// <summary>
        /// Determines if this RectTransform is at least partially visible from the specified camera.
        /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
        /// </summary>
        /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
        /// <param name="rectTransform">Rect transform.</param>
        /// <param name="camera">Camera.</param>
        public static bool IsVisibleFrom(this RectTransform rectTransform, Camera camera, Rect view)
        {
            return CountCornersVisibleFrom(rectTransform, camera, view) > 0; // True if any corners are visible
        }
    }
}
