#region License
/*The MIT License (MIT)

Better Tracking

Tracking_Loader - UI loader script

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

using System.Collections;
using BetterTracking.Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KSP.UI.Screens;

namespace BetterTracking
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, true)]
    public class Tracking_Loader : MonoBehaviour
    {
        private const string prefabAssetName = "better_tracking_prefabs.btk";

        private static bool loaded;
        private static bool UILoaded;
        
        private static GameObject[] loadedPrefabs;

        private static GameObject _groupPrefab;
        private static GameObject _sortHeaderPrefab;
        private static GameObject _fullVesselPrefab;

        private Sprite _backgroundSprite;
        private Sprite _checkmarkSprite;
        private Sprite _hoverSprite;
        private Sprite _activeSprite;
        private Sprite _normalSprite;
        private Sprite _inactiveSprite;

        private static VesselIconSprite _iconPrefab;

        public static VesselIconSprite IconPrefab
        {
            get { return _iconPrefab; }
        }

        public static GameObject GroupPrefab
        {
            get { return _groupPrefab; }
        }

        public static GameObject SortHeaderPrefab
        {
            get { return _sortHeaderPrefab; }
        }

        public static GameObject FullVesselPrefab
        {
            get { return _fullVesselPrefab; }
        }

        private void Awake()
        {
            if (loaded)
            {
                Destroy(gameObject);
                return;
            }

            if (loadedPrefabs == null)
            {
                string path = KSPUtil.ApplicationRootPath + "GameData/TrackingStationEvolved/Resources/";

                AssetBundle prefabs = AssetBundle.LoadFromFile(path + prefabAssetName);

                if (prefabs != null)
                    loadedPrefabs = prefabs.LoadAllAssets<GameObject>();
            }

            StartCoroutine(WaitForTrackingList());
        }

        private IEnumerator WaitForTrackingList()
        {
            WaitForSeconds wait = new WaitForSeconds(0.1f);

            SpaceTracking _TrackingStation = null;

            while (_TrackingStation == null)
            {
                _TrackingStation = GameObject.FindObjectOfType<SpaceTracking>();

                if (_TrackingStation == null)
                    yield return wait;
            }

            processSprites(_TrackingStation);

            if (loadedPrefabs != null)
                processUIPrefabs();

            if (UILoaded)
                loaded = true;

            Tracking_Utils.TrackingLog("UI Loaded");

            Destroy(gameObject);
        }

        private void processSprites(SpaceTracking tracking)
        {
            var prefab = tracking.listItemPrefab;

            if (prefab == null)
                return;

            prefab.gameObject.AddOrGetComponent<Tracking_WidgetListener>();

            _iconPrefab = prefab.iconSprite;

            Selectable toggle = prefab.toggle.GetComponent<Selectable>();

            if (toggle == null)
                return;

            _normalSprite = toggle.image.sprite;
            _hoverSprite = toggle.spriteState.highlightedSprite;
            _activeSprite = toggle.spriteState.pressedSprite;
            _inactiveSprite = toggle.spriteState.disabledSprite;

            var images = prefab.GetComponentsInChildren<Image>();

            if (images == null || images.Length < 2)
                return;

            _backgroundSprite = images[images.Length - 2].sprite;

            _checkmarkSprite = ((Image)prefab.toggle.graphic).sprite;
        }

        private void processUIPrefabs()
        {
            for (int i = loadedPrefabs.Length - 1; i >= 0; i--)
            {
                GameObject o = loadedPrefabs[i];

                if (o == null)
                    continue;
                
                if (o.name == "HeaderGroup")
                    _groupPrefab = o;
                else if (o.name == "SortHeader")
                    _sortHeaderPrefab = o;
                else if (o.name == "FullVessel")
                    _fullVesselPrefab = o;

                processTMP(o);
                processInputFields(o);
                processUIComponents(o);
            }

            UILoaded = true;
        }
        
        private void processTMP(GameObject obj)
        {
            TextHandler[] handlers = obj.GetComponentsInChildren<TextHandler>(true);

            if (handlers == null)
                return;

            for (int i = 0; i < handlers.Length; i++)
                TMProFromText(handlers[i]);
        }

        private void TMProFromText(TextHandler handler)
        {
            if (handler == null)
                return;

            Text text = handler.GetComponent<Text>();

            if (text == null)
                return;

            string t = text.text;
            Color c = text.color;
            int i = text.fontSize;
            bool r = text.raycastTarget;
            FontStyles sty = TMPProUtil.FontStyle(text.fontStyle);
            TextAlignmentOptions align = TMPProUtil.TextAlignment(text.alignment);
            float spacing = text.lineSpacing;
            GameObject obj = text.gameObject;

            DestroyImmediate(text);

            Tracking_TMP tmp = obj.AddComponent<Tracking_TMP>();

            tmp.text = t;
            tmp.color = c;
            tmp.fontSize = i;
            tmp.raycastTarget = r;
            tmp.alignment = align;
            tmp.fontStyle = sty;
            tmp.lineSpacing = spacing;

            tmp.font = UISkinManager.TMPFont;
            tmp.fontSharedMaterial = Resources.Load("Fonts/Materials/Calibri Dropshadow", typeof(Material)) as Material;
            
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.isOverlay = false;
            tmp.richText = true;
        }

        private static void processInputFields(GameObject obj)
        {
            InputHandler[] handlers = obj.GetComponentsInChildren<InputHandler>(true);

            if (handlers == null)
                return;

            for (int i = 0; i < handlers.Length; i++)
                TMPInputFromInput(handlers[i]);
        }

        private static void TMPInputFromInput(InputHandler handler)
        {
            if (handler == null)
                return;

            InputField input = handler.GetComponent<InputField>();

            if (input == null)
                return;

            int limit = input.characterLimit;
            TMP_InputField.ContentType content = GetTMPContentType(input.contentType);
            float caretBlinkRate = input.caretBlinkRate;
            int caretWidth = input.caretWidth;
            Color selectionColor = input.selectionColor;
            GameObject obj = input.gameObject;

            RectTransform viewport = handler.GetComponentInChildren<RectMask2D>().rectTransform;
            Tracking_TMP placholder = handler.GetComponentsInChildren<Tracking_TMP>()[0];
            Tracking_TMP textComponent = handler.GetComponentsInChildren<Tracking_TMP>()[1];

            if (viewport == null || placholder == null || textComponent == null)
                return;

            DestroyImmediate(input);

            Tracking_TMP_Input tmp = obj.AddComponent<Tracking_TMP_Input>();

            tmp.textViewport = viewport;
            tmp.placeholder = placholder;
            tmp.textComponent = textComponent;

            tmp.characterLimit = limit;
            tmp.contentType = content;
            tmp.caretBlinkRate = caretBlinkRate;
            tmp.caretWidth = caretWidth;
            tmp.selectionColor = selectionColor;

            tmp.readOnly = false;
            tmp.shouldHideMobileInput = false;

            tmp.fontAsset = UISkinManager.TMPFont;
        }

        private static TMP_InputField.ContentType GetTMPContentType(InputField.ContentType type)
        {
            switch (type)
            {
                case InputField.ContentType.Alphanumeric:
                    return TMP_InputField.ContentType.Alphanumeric;
                case InputField.ContentType.Autocorrected:
                    return TMP_InputField.ContentType.Autocorrected;
                case InputField.ContentType.Custom:
                    return TMP_InputField.ContentType.Custom;
                case InputField.ContentType.DecimalNumber:
                    return TMP_InputField.ContentType.DecimalNumber;
                case InputField.ContentType.EmailAddress:
                    return TMP_InputField.ContentType.EmailAddress;
                case InputField.ContentType.IntegerNumber:
                    return TMP_InputField.ContentType.IntegerNumber;
                case InputField.ContentType.Name:
                    return TMP_InputField.ContentType.Name;
                case InputField.ContentType.Password:
                    return TMP_InputField.ContentType.Password;
                case InputField.ContentType.Pin:
                    return TMP_InputField.ContentType.Pin;
                case InputField.ContentType.Standard:
                    return TMP_InputField.ContentType.Standard;
                default:
                    return TMP_InputField.ContentType.Standard;
            }
        }
        private void processUIComponents(GameObject obj)
        {
            TrackingStyle[] styles = obj.GetComponentsInChildren<TrackingStyle>(true);

            if (styles == null)
                return;

            for (int i = 0; i < styles.Length; i++)
                processComponents(styles[i]);
        }

        private void processComponents(TrackingStyle style)
        {
            if (style == null)
                return;

            UISkinDef skin = UISkinManager.defaultSkin;

            if (skin == null)
                return;

            switch (style.StlyeType)
            {
                case TrackingStyle.StyleTypes.Toggle:
                    style.setToggle(_normalSprite, _hoverSprite, _activeSprite, _inactiveSprite, _checkmarkSprite);
                    break;
                case TrackingStyle.StyleTypes.IconBackground:
                    style.setImage(_backgroundSprite);
                    break;
                case TrackingStyle.StyleTypes.Button:
                    style.setButton(_normalSprite, _hoverSprite, _activeSprite, _inactiveSprite);
                        break;
                case TrackingStyle.StyleTypes.Window:
                    style.setImage(skin.window.normal.background);
                    break;
                case TrackingStyle.StyleTypes.Background:
                    style.setImage(skin.box.normal.background);
                    break;
                case TrackingStyle.StyleTypes.Input:
                    style.setImage(skin.textField.normal.background);
                    break;
            }
        }
    }
}
