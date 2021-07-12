using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

//TODO: ALL of it /./

namespace Books
{
    internal class BooksAnimationHandler
    {
        private static readonly float
            AnimOpenSpeed = 0.8F;

        private static readonly float
            AnimCloseSpeed = 1F;

        private static readonly string
            AnimatorOpen = "animbooksopen";

        private static readonly string
            AnimatorClose = "booksclose";

        private static readonly string
            AnimOpen = "bookopen";

        private static readonly string
            AnimOpenCode = "bookopening";

        private static readonly string
            AnimClose = "bookclose";

        private static readonly string
            AnimCloseCode = "bookclosing";

        private readonly AnimationMetaData AnimMetaDataClose = new AnimationMetaData { Animation = AnimClose, Code = AnimCloseCode, AnimationSpeed = AnimCloseSpeed };


        private readonly AnimationMetaData AnimMetaDataOpen = new AnimationMetaData { Animation = AnimOpen, Code = AnimOpenCode, AnimationSpeed = AnimOpenSpeed };
        private readonly BlockEntityAnimationUtil animUtilclose;

        private readonly BlockEntityAnimationUtil animUtilopen;
        public ICoreClientAPI Capi;

        public BooksAnimationHandler(ICoreAPI api, BlockEntityBooks BE)
        {
            if (api is ICoreClientAPI)
            {
                Capi = (ICoreClientAPI)api;
                animUtilopen = new BlockEntityAnimationUtil(Capi, BE);
                animUtilclose = new BlockEntityAnimationUtil(Capi, BE);
                animUtilopen.InitializeAnimator(AnimatorOpen);
                animUtilclose.InitializeAnimator(AnimatorClose);

                // new Vec3f(Block.Shape.rotateX, Block.Shape.rotateY, Block.Shape.rotateZ);
            }
        }

        public void Open()
        {
            animUtilopen.StartAnimation(AnimMetaDataOpen);
        }


        public void Open(ICoreAPI api)
        {
            if (api.World is ICoreClientAPI)
            {
                animUtilclose.InitializeAnimator(AnimatorOpen);
                animUtilclose.StartAnimation(AnimMetaDataClose);
                //animUtilopen.InitializeAnimator(AnimatorOpen);
                //animUtilopen.StartAnimation(AnimMetaDataOpen);
            }
        }

        public void Close()
        {
            animUtilopen.StopAnimation(AnimatorOpen);
            //animUtilclose.InitializeAnimator(AnimatorOpen);
        }

        public void Close(ICoreAPI api)
        {
            if (api.World is ICoreClientAPI)
            {
                animUtilopen.StopAnimation(AnimatorOpen);
                animUtilopen.activeAnimationsByAnimCode.Clear();
                animUtilopen.InitializeAnimator(AnimatorOpen);
            }
        }

        public void Dispose()
        {
            animUtilopen.renderer.Dispose();
        }

        public bool HideDrawModel()
        {
            if (animUtilclose.activeAnimationsByAnimCode.Count > 0)
            {
                return true;
            }

            if (animUtilopen.activeAnimationsByAnimCode.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}
