using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
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
            WaitForSeconds wait = new WaitForSeconds(0.2f);

            RDArchivesController RDController = null;

            while (RDController == null)
            {
                var resControllers = Resources.FindObjectsOfTypeAll<RDArchivesController>();

                if (resControllers != null)
                {
                    //TrackingController.TrackingLog("Checking Resource RD Controllers...");

                    for (int i = resControllers.Length - 1; i >= 0; i--)
                    {
                        RDArchivesController rd = resControllers[i];

                        if (rd == null)
                            continue;

                        //TrackingController.TrackingLog("RD Resource Controller Found");

                        RDController = rd;
                        break;
                    }
                }
                
                yield return wait;
            }

            if (RDController != null)
            {
                _RDPlanetPrefab = RDController.planetListItemPrefab;

                //TrackingController.TrackingLog("RD Planet Prefab Assigned");

                //Tracking_Utils.LogPrefab(_RDPlanetPrefab.transform, 0);
            }
        }
    }
}
