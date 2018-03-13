#region License
/*The MIT License (MIT)

One Window

ScalableVerticalLayoutGroup - Customized vertical layout group to support scaling animations

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
    public class ScalableVerticalLayoutGroup : VerticalLayoutGroup, ILayoutElement
    {
        public float _scale = 1;

        new protected void CalcAlongAxis(int axis, bool isVertical)
        {
            float num = ((axis != 0) ? padding.vertical : padding.horizontal);
            bool controlSize = (axis != 0) ? m_ChildControlHeight : m_ChildControlWidth;
            bool childForceExpand = (axis != 0) ? childForceExpandHeight : childForceExpandWidth;
            float num2 = num;
            float num3 = num;
            float num4 = 0f;
            bool flag = isVertical ^ axis == 1;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];

                GetChildSizes(child, axis, controlSize, childForceExpand, out float num5, out float num6, out float num7);

                if (flag)
                {
                    num2 = Mathf.Max(num5 + num, num2);
                    num3 = Mathf.Max(num6 + num, num3);
                    num4 = Mathf.Max(num7, num4);
                }
                else
                {
                    num2 += num5 + spacing;
                    num3 += num6 + spacing;
                    num4 += num7;
                }
            }
            if (!flag && rectChildren.Count > 0)
            {
                num2 -= spacing;
                num3 -= spacing;
            }
            num3 = Mathf.Max(num2, num3);
            SetLayoutInputForAxis(num2 * _scale, num3 * _scale, num4, axis);
        }

        private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
        {
            if (!controlSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0f;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }
            if (childForceExpand)
            {
                flexible = Mathf.Max(flexible, 1f);
            }
        }
        
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, true);
        }
    }
}
