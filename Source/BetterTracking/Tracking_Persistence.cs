#region License
/*The MIT License (MIT)

Better Tracking

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
using System.Linq;

namespace BetterTracking
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
    public class Tracking_Persistence : ScenarioModule
    {
        //Key is celestial body index; value is expanded status
        private static Dictionary<int, bool> _bodyPersistence = new Dictionary<int, bool>();

        private static List<int> _bodyOrderList = null;
        private static List<int> _typeOrderList = new List<int>(15) { 7, 11, 3, 8, 6, 5, 4, 9, 10, 13, 14, 12, 1, 0, 2 };

        //Key is vessel type index; value is expanded status
        private static Dictionary<int, bool> _typePersistence = new Dictionary<int, bool>();
        
        private static int _sortMode = 0;

        private static int _bodyOrderMode = 0;
        private static int _typeOrderMode = 0;
        private static int _stockOrderMode = 0;

        private static bool _bodyAscOrder = false;
        private static bool _typeAscOrder = false;
        private static bool _stockAscOrder = false;

        public static int GetBodyOrder(int index, int fallback)
        {
            if (_bodyOrderList.Contains(index))
                return _bodyOrderList.IndexOf(index);
            else
                _typeOrderList.Insert(fallback, index);

            return fallback;
        }

        public static void SetBodyOrder(int index, int order, int oldIndex)
        {
            int old = -1;
            if (_bodyOrderList.Contains(oldIndex))
                old = _bodyOrderList.IndexOf(oldIndex);

            if (!_bodyOrderList.Contains(index))
            {
                if (old >= 0)
                    order = old + 1;
                
                _bodyOrderList.Insert(order, index);
            }
            else
            {
                if (old >= 0)
                    order = old;

                int current = _typeOrderList.IndexOf(index);

                if (current > order)
                    order++;

                _bodyOrderList.Remove(index);
                _bodyOrderList.Insert(order, index);
            }
        }

        public static List<int> BodyOrderList
        {
            get { return _bodyOrderList; }
        }
        
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
        
        public static int GetTypeOrder(int index, int fallback)
        {
            if (_typeOrderList.Contains(index))
                return _typeOrderList.IndexOf(index);
            else
                _typeOrderList.Insert(fallback, index);

            return fallback;
        }

        public static void SetTypeOrder(int index, int order, int oldIndex)
        {
            int old = -1;
            if (_typeOrderList.Contains(oldIndex))
                old = _typeOrderList.IndexOf(oldIndex);

            if (!_typeOrderList.Contains(index))
            {
                if (old >= 0)
                    order = old + 1;
                
                _typeOrderList.Insert(order, index);
            }
            else
            {
                if (old >= 0)
                    order = old;

                int current = _typeOrderList.IndexOf(index);

                if (current > order)
                    order++;
                
                _typeOrderList.Remove(index);
                _typeOrderList.Insert(order, index);
            }
        }

        public static List<int> TypeOrderList
        {
            get { return _typeOrderList; }
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

        public static int BodyOrderMode
        {
            get { return _bodyOrderMode; }
            set { _bodyOrderMode = value; }
        }

        public static int TypeOrderMode
        {
            get { return _typeOrderMode; }
            set { _typeOrderMode = value; }
        }

        public static int StockOrderMode
        {
            get { return _stockOrderMode; }
            set { _stockOrderMode = value; }
        }

        public static bool BodyAscOrder
        {
            get { return _bodyAscOrder; }
            set { _bodyAscOrder = value; }
        }

        public static bool TypeAscOrder
        {
            get { return _typeAscOrder; }
            set { _typeAscOrder = value; }
        }

        public static bool StockAscOrder
        {
            get { return _stockAscOrder; }
            set { _stockAscOrder = value; }
        }

        public override void OnAwake()
        {
            base.OnAwake();

            if (_bodyOrderList != null)
                return;

            _bodyOrderList = new List<int>();

            var allBodies = FlightGlobals.Bodies.Where(b => b.referenceBody == Planetarium.fetch.Sun && b.referenceBody != b);

            var orderedBodies = allBodies.OrderBy(b => b.orbit.semiMajorAxis).ToList();
            
            for (int i = orderedBodies.Count - 1; i >= 0; i--)
            {
                CelestialBody body = orderedBodies[i];

                if (body != Planetarium.fetch.Home)
                    continue;

                orderedBodies.RemoveAt(i);
                orderedBodies.Insert(0, body);
            }

            for (int i = 0; i < orderedBodies.Count; i++)
            {
                _bodyOrderList.Add(orderedBodies[i].flightGlobalsIndex);
            }

            _bodyOrderList.Insert(1, Planetarium.fetch.Sun.flightGlobalsIndex);
        }

        private void FallbackBodyCheck()
        {
            var allBodies = FlightGlobals.Bodies.Where(b => b.referenceBody == Planetarium.fetch.Sun && b.referenceBody != b);

            var orderedBodies = allBodies.OrderBy(b => b.orbit.semiMajorAxis).ToList();

            for (int i = orderedBodies.Count - 1; i >= 0; i--)
            {
                CelestialBody body = orderedBodies[i];

                if (body != Planetarium.fetch.Home)
                    continue;

                orderedBodies.RemoveAt(i);
                orderedBodies.Insert(0, body);
            }

            orderedBodies.Insert(1, Planetarium.fetch.Sun);

            for (int i = orderedBodies.Count - 1; i >= 0; i--)
            {
                GetBodyOrder(orderedBodies[i].flightGlobalsIndex, i);
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            if (node.HasValue("BodyPersistence"))
                _bodyPersistence = Tracking_Utils.ParseDictionary(node.GetValue("BodyPersistence"));

            if (node.HasValue("BodyOrderList"))
                _bodyOrderList = Tracking_Utils.ParseList(node.GetValue("BodyOrderList"));

            if (node.HasValue("TypePersistence"))
                _typePersistence = Tracking_Utils.ParseDictionary(node.GetValue("TypePersistence"));

            if (node.HasValue("TypeOrderList"))
                _typeOrderList = Tracking_Utils.ParseList(node.GetValue("TypeOrderList"));

            if (node.HasValue("SortMode"))
                node.TryGetValue("SortMode", ref _sortMode);

            if (node.HasValue("BodyOrderMode"))
                node.TryGetValue("BodyOrderMode", ref _bodyOrderMode);

            if (node.HasValue("TypeOrderMode"))
                node.TryGetValue("TypeOrderMode", ref _typeOrderMode);

            if (node.HasValue("StockOrderMode"))
                node.TryGetValue("StockOrderMode", ref _stockOrderMode);

            if (node.HasValue("BodyAscOrder"))
                node.TryGetValue("BodyAscOrder", ref _bodyAscOrder);

            if (node.HasValue("TypeAscOrder"))
                node.TryGetValue("TypeAscOrder", ref _typeAscOrder);

            if (node.HasValue("StockAscOrder"))
                node.TryGetValue("StockAscOrder", ref _stockAscOrder);

            FallbackBodyCheck();
        }

        public override void OnSave(ConfigNode node)
        {
            node.AddValue("BodyPersistence", Tracking_Utils.ConcatDictionary(_bodyPersistence));

            node.AddValue("BodyOrderList", Tracking_Utils.ConcatList(_bodyOrderList));

            node.AddValue("TypePersistence", Tracking_Utils.ConcatDictionary(_typePersistence));

            node.AddValue("TypeOrderList", Tracking_Utils.ConcatList(_typeOrderList));

            node.AddValue("SortMode", _sortMode);

            node.AddValue("BodyOrderMode", _bodyOrderMode);
            node.AddValue("TypeOrderMode", _typeOrderMode);
            node.AddValue("StockOrderMode", _stockOrderMode);

            node.AddValue("BodyAscOrder", _bodyAscOrder);
            node.AddValue("TypeAscOrder", _typeAscOrder);
            node.AddValue("StockAscOrder", _stockAscOrder);
        }
    }
}
