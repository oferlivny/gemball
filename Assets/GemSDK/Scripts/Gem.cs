using System;
using UnityEngine;

namespace GemSDK.Unity
{
    public interface IGem
    {
        GemState state { get; }
        Quaternion rotation { get; }
        Vector3 acceleration { get; }

        void Calibrate();
        void Reconnect();
    }
}
