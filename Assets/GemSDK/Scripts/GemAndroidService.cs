using UnityEngine;
using System.Collections;

#if UNITY_ANDROID

namespace GemSDK.Unity
{
    public class GemAndroidService : IGemService
    {
        private AndroidJavaObject service;
        private AndroidJavaObject activity;

        internal GemAndroidService()
        {
            service = new AndroidJavaClass("com.gemsense.gemsdk.GemService").CallStatic<AndroidJavaObject>("getDefault");
            activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        }

        public IGem getGem(GemRole role)
        {
            return new AndroidGem((int)role, activity);
        }

        public IGem getGem()
        {
            return new AndroidGem((int)GemRole.Primary, activity);
        }

        public void releaseGem()
        {

        }

        public void Connect()
        {
            try
            {
                service.Call<bool>("bindService", activity);
            }
            catch
            {
                throw new GemServiceNotInstalledException();
            }
        }

        public void Disconnect()
        {
            service.Call("unbindService", activity);
        }
    }
}

#endif