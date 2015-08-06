using System.Threading;
using System;
using GemSDK;

namespace GemSDK.Unity
{
    public class GemWindowsService : IGemService
    {
        private WindowsGem windowsGem;
        
        public GemWindowsService()
        {
            windowsGem = new WindowsGem();
        }

        public void Connect()
        {
            windowsGem.Start();
        }

        public void Disconnect()
        {
            windowsGem.Stop(false);
        }

        public IGem getGem()
        {
            return windowsGem;
        }
    }
}