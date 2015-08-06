using UnityEngine;
using System.Collections;
using System;

namespace GemSDK.Unity
{
    public enum GemState
    {
        Connecting = 2,
        Connected = 3,
        Disconnecting = 1,
        Disconnected = 0
    }

    public enum GemError
    {
        ConnectingTimeout = 404
    }

    public enum GemRole
    {
        Primary = 0,
        Secondary = 1
    }

    public class GemServiceNotInstalledException : Exception
    {
        public GemServiceNotInstalledException()
            : base("GemService not found. Please install Gemsense app")
        { }
    }
}