using UnityEngine;
using System.Collections;

#if UNITY_ANDROID

namespace GemSDK.Unity
{
    internal class AndroidGem : AndroidJavaProxy, IGem 
    {
        private bool bound;
        private AndroidJavaObject activity;
        private AndroidJavaObject gemWrapper;

        public volatile GemState _state = GemState.Disconnected;

        public GemState state
        {
            get
            {
                return _state;
            }
        }

        public void Reconnect()
        {
            gemWrapper.Call("reconnect");   
        }

        internal AndroidGem(int role, AndroidJavaObject activity)
            : base("com.gemsense.gemsdk.unity.UnityCallback")
        {
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                gemWrapper = new AndroidJavaObject("com.gemsense.gemsdk.unity.GemWrapper", role, this);   
            }));
        }

        public void Release()
        {
            gemWrapper.Call("release");
        }

        public Quaternion rotation {
            get {
                if (gemWrapper != null)
                {
                    float[] q = gemWrapper.Call<float[]>("getLastQuaternion");
                    return new Quaternion(q[1], q[2], q[3], q[0]);
                }
                else return new Quaternion(0, 0, 0, 1);
            }
        }

        public Vector3 acceleration
        {
            get
            {
                if (gemWrapper != null)
                {
                    float[] a = gemWrapper.Call<float[]>("getLastAcceleration");
                    return new Vector3(a[1], a[2], a[3]);
                }
                else return new Vector3(0, 0, 0);
            }
        }

        public void Calibrate()
        {
            gemWrapper.Call("calibrate");
        }

        #region UnityCallback
        public void onStateChanged(int state)
        {
            this._state = (GemState)state;
            Debug.Log("new connection status is " + ((GemState)state).ToString().ToUpper());
        }

        public void onErrorOccurred(int errorCode)
        {
            Debug.Log("error " + ((GemError)errorCode).ToString().ToUpper());
            //Gem wasn't found during scan timeout
            if ((GemError)errorCode == GemError.ConnectingTimeout)
            {
                Reconnect();
            }
        }

        #endregion
    }
}

#endif
