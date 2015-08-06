using UnityEngine;
using System;
using System.Collections;

namespace GemSDK.Unity
{
    /// <summary>
    /// Represents connection to remote Gem Service regardless of platform
    /// </summary>
    public class GemService
    {
        private static IGemService instance; 
     
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static IGemService Instance
        {
            get
            {
                if (instance == null)
                {
                    #if (UNITY_ANDROID)
                        instance = new GemAndroidService();
                    #elif (UNITY_ANDROID || UNITY_EDITOR)
                    instance = new GemWindowsService();
                    #else
                        Debug.Log("GemSDK doesn't support this platform");
                    #endif
                }     
                
                return instance;
            }
        }
    }
}
