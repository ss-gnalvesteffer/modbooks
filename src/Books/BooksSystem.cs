using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Books
{
    public class BBooks : ModSystem
    {
        // Client:
        public ICoreClientAPI Capi { get; private set; }
        public IClientNetworkChannel CChannel { get; private set; }

        // Server:
        public ICoreServerAPI Sapi { get; private set; }
        public IServerNetworkChannel SChannel { get; private set; }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return true;
        }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("BlockBooks", typeof(BlockBooks));
            api.RegisterBlockEntityClass("BlockEntityBooks", typeof(BlockEntityBooks));
            api.RegisterBlockClass("BlockPaper", typeof(BlockPaper));
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            base.StartClientSide(capi);

            var Capi = capi;
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            base.StartServerSide(sapi);

            var Sapi = sapi;
        }
    }
}
