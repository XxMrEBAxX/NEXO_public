using UnityEngine;
using System.Collections.Generic;

namespace BirdCase
{
    internal static class YieldInstructionCache
    {
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    
        private static Dictionary<float, WaitForSeconds> timeInteval
            = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds GetTimeInteval(float _f)
        {
            if (!timeInteval.ContainsKey(_f))
                timeInteval.Add(_f, new WaitForSeconds(_f));

            return timeInteval[_f];
        }
    }
}
