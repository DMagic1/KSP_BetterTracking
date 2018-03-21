#region License
/*The MIT License (MIT)

Better Tracking

TrackingStyle - Script for applying UI style elements

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

namespace BetterTracking.Unity
{
    public class TrackingStyle : MonoBehaviour
    {
        public enum StyleTypes
        {
            None,
            Toggle,
            IconBackground,
            Button,
            Window,
            Background,
        }

        [SerializeField]
        private StyleTypes m_StyleType = StyleTypes.None;

        public StyleTypes StlyeType
        {
            get { return m_StyleType; }
        }

        private void setSelectable(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
        {
            Selectable select = GetComponent<Selectable>();

            if (select == null)
                return;

            select.image.sprite = normal;
            select.image.type = Image.Type.Sliced;
            select.transition = Selectable.Transition.SpriteSwap;

            SpriteState spriteState = select.spriteState;
            spriteState.highlightedSprite = highlight;
            spriteState.pressedSprite = active;
            spriteState.disabledSprite = inactive;
            select.spriteState = spriteState;
        }

        public void setImage(Sprite sprite)
        {
            Image image = GetComponent<Image>();

            if (image == null)
                return;

            image.sprite = sprite;
        }

        public void setToggle(Sprite normal, Sprite highlight, Sprite active, Sprite inactive, Sprite checkmark)
        {
            setSelectable(normal, highlight, active, inactive);

            Toggle toggle = GetComponent<Toggle>();

            if (toggle == null)
                return;

            Image toggleImage = toggle.graphic as Image;

            if (toggleImage == null)
                return;

            toggleImage.sprite = checkmark;
            toggleImage.type = Image.Type.Sliced;
        }

        public void setButton(Sprite normal, Sprite highlight, Sprite active, Sprite inactive)
        {
            setSelectable(normal, highlight, active, inactive);
        }
    }
}
