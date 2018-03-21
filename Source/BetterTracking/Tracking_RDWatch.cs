#region License
/*The MIT License (MIT)

Better Tracking

Tracking_RDWatch - Script to access the R&D archives spinning planet prefab

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
using UnityEngine;
using KSP.UI.Screens;

namespace BetterTracking
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class Tracking_RDWatch : MonoBehaviour
    {
        private static RDPlanetListItemContainer _RDPlanetPrefab;

        public static RDPlanetListItemContainer RDPlanetPrefab
        {
            get { return _RDPlanetPrefab; }
        }

        private void Start()
        {
            StartCoroutine(Searching());
        }
        
        private IEnumerator Searching()
        {
            WaitForSeconds wait = new WaitForSeconds(0.1f);

            RDArchivesController RDController = null;

            while (RDController == null)
            {
                var resControllers = Resources.FindObjectsOfTypeAll<RDArchivesController>();

                if (resControllers != null)
                {
                    for (int i = resControllers.Length - 1; i >= 0; i--)
                    {
                        RDArchivesController rd = resControllers[i];

                        if (rd == null)
                            continue;
                        
                        RDController = rd;
                        break;
                    }
                }
                
                if (RDController == null)
                    yield return wait;
            }

            if (RDController != null)
            {
                _RDPlanetPrefab = RDController.planetListItemPrefab;
            }
        }
    }
}
