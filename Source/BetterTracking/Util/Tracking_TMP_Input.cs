#region License
/*The MIT License (MIT)

Better Tracking

Tracking_TMP_Input - Text Mesh Pro Input field extension

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

using UnityEngine.Events;
using TMPro;
using BetterTracking.Unity;

namespace BetterTracking
{
    public class Tracking_TMP_Input : TMP_InputField
    {
        private InputHandler _handler;

        new private void Awake()
        {
            base.Awake();

            _handler = GetComponent<InputHandler>();

            onValueChanged.AddListener(new UnityAction<string>(valueChanged));

            _handler.OnTextUpdate.AddListener(new UnityAction<string>(UpdateText));
        }

        private void Update()
        {
            if (_handler != null)
                _handler.IsFocused = isFocused;
        }

        private void valueChanged(string s)
        {
            if (_handler == null)
                return;

            _handler.Text = s;

            _handler.OnValueChange.Invoke(s);
        }

        private void UpdateText(string t)
        {
            text = t;
        }
    }
}
