using Vintagestory.API.Client;
using Vintagestory.API.Server;

namespace Books
{
    internal class BooksNetworkHandler
    {
        private ICoreClientAPI Capi;
        private ICoreServerAPI Sapi;

        public BooksNetworkHandler(ICoreClientAPI capi)
        {
            Capi = capi;
        }

        public BooksNetworkHandler(ICoreServerAPI sapi)
        {
            Sapi = sapi;
        }

        public void SendToClient(ICoreClientAPI capi)
        {
            Capi = capi;
        }

        public void OnReceive()
        {
        }
    }
}
