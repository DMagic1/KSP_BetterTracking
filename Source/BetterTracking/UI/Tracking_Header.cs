#region License
/*The MIT License (MIT)

One Window

Tracking_Header - UI Interface for group header element

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

using BetterTracking.Unity.Interface;
using UnityEngine;

namespace BetterTracking
{
    public class Tracking_Header : IHeaderItem
    {
        private string _title;
        private int _vesselCount;
        private int _moonCount;
        private int _mode;
        private GameObject _headerImage;
        private RectTransform _headerRect;

        public Tracking_Header(string title, int vesselCount, int moonCount, GameObject obj, int mode)
        {
            _title = title;
            _vesselCount = vesselCount;
            _moonCount = moonCount;
            _mode = mode;
            _headerImage = obj;

            _headerRect = obj.transform as RectTransform;
        }

        public string HeaderName
        {
            get { return _title; }
        }

        public string HeaderInfo
        {
            get
            {
                if (_moonCount > 0)
                    return string.Format("{0}(+{1})", _vesselCount, _moonCount);
                else
                    return _vesselCount.ToString();
            }
        }

        public GameObject HeaderImage
        {
            get { return _headerImage; }
        }

        public void Update()
        {
            if (_mode > 0 || _headerRect == null || Tracking_Controller.Instance == null)
                return;

            if (_headerRect.IsFullyVisibleFrom(Tracking_Controller.Instance.CanvasCamera, Tracking_Controller.Instance.TrackingScrollView))
            {
                if (!_headerImage.activeSelf)
                    _headerImage.SetActive(true);
            }
            else
            {
                if (_headerImage.activeSelf)
                    _headerImage.SetActive(false);
            }
        }
    }
}
