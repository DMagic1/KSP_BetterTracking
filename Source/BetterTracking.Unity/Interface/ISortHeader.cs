#region License
/*The MIT License (MIT)

Better Tracking

ISortHeader - Interface for sort button UI element

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

namespace BetterTracking.Unity.Interface
{
    public interface ISortHeader
    {
        int CurrentMode { get; }

        int BodySortMode { get; set; }

        int TypeSortMode { get; set; }

        int StockSortMode { get; set; }

        bool BodySortOrder { get; set; }

        bool TypeSortOrder { get; set; }

        bool StockSortOrder { get; set; }

        bool LockInput { get; set; }

        string SearchString { get; set; }

        Transform DropDownParent { get; }

        void SortBody(bool isOn);

        void SortType(bool isOn);

        void SortCustom(bool isOn);

        void SortDefault(bool isOn);
    }
}
