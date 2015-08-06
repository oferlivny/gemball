using System;

namespace GemSDK.Unity
{
    public interface IGemService
    {
        void Connect();
        void Disconnect();
        IGem getGem();
    }
}
