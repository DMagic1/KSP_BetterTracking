#region License
/*The MIT License (MIT)

One Window

Tracking_Persistence - Scenario Module for sort persistence

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

namespace BetterTracking
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    public class Tracking_Persistence : ScenarioModule
    {
        private static Dictionary<int, bool> _bodyPersistence = new Dictionary<int, bool>();
        private static Dictionary<int, bool> _typePersistence = new Dictionary<int, bool>();
        
        private static int _sortMode = 0;
        
        public static bool GetBodyPersistence(int index)
        {
            if (_bodyPersistence.ContainsKey(index))
                return _bodyPersistence[index];
            else
                _bodyPersistence.Add(index, true);

            return true;
        }

        public static void SetBodyPersistence(int index, bool isOn)
        {
            if (_bodyPersistence.ContainsKey(index))
                _bodyPersistence[index] = isOn;
            else
                _bodyPersistence.Add(index, isOn);
        }

        public static bool GetTypePersistence(int index)
        {
            if (_typePersistence.ContainsKey(index))
                return _typePersistence[index];
            else
                _typePersistence.Add(index, true);

            return true;
        }

        public static void SetTypePersistence(int index, bool isOn)
        {
            if (_typePersistence.ContainsKey(index))
                _typePersistence[index] = isOn;
            else
                _typePersistence.Add(index, isOn);
        }
        
        public static int SortMode
        {
            get { return _sortMode; }
            set { _sortMode = value; }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (node.HasValue("BodyPersistence"))
                _bodyPersistence = Tracking_Utils.ParseDictionary(node.GetValue("BodyPersistence"));

            if (node.HasValue("TypePersistence"))
                _typePersistence = Tracking_Utils.ParseDictionary(node.GetValue("TypePersistence"));

            if (node.HasValue("SortMode"))
                node.TryGetValue("SortMode", ref _sortMode);
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("BodyPersistence", Tracking_Utils.ConcatDictionary(_bodyPersistence));

            node.AddValue("TypePersistence", Tracking_Utils.ConcatDictionary(_typePersistence));

            node.AddValue("SortMode", _sortMode);
        }
    }
}
