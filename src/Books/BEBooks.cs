using System;
using System.IO;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

// TODO : Privileges?
// TODO : Networkhandler?
// TODO : save authorID?

namespace Books
{
    public class BlockEntityBooks : BlockEntity
    {
        private class UserInfo
        {
            public IPlayer Player { get; set; }
            public ItemSlot EquippedItemSlot { get; set; }
            public int InitialWritingUtensilDurability { get; set; }
        }

        private static readonly int
            PageLimit = 20;

        private static readonly string
            // ID/dialog keys:
            IdDialogBookEditor = "bookeditor";

        private static readonly string
            // ID/dialog keys:
            IdDialogBookReader = "bookreader";

        private static readonly string
            // ID/dialog keys:
            // control falgs for read write gui
            FlagRead = "R";

        private static readonly string
            // ID/dialog keys:
            // control falgs for read write gui
            FlagWrite = "W";

        private static readonly string
            // ID/dialog keys:
            // control falgs for read write gui
            NetworkName = "BlockEntityTextInput";

        public string[] PageTexts = new string[PageLimit];

        public string[] ArPageNames = new string[PageLimit];

        private BooksAnimationHandler _bookAnim;
        public ICoreClientAPI Capi;

        public bool IsPaper;
        public bool Unique;

        public int PageMax = 1; // current page/pagemax <= pagelimit

        private UserInfo _currentUserInfo;

        public ICoreServerAPI Sapi;

        public string
            Title = "",
            Author = "";

        public BlockEntityBooks()
        {
        }


        public BlockEntityBooks(BlockPos blockPos, bool isPaper)
        {
            this.IsPaper = isPaper;
            DeletingText();
            Pos = blockPos;
        }

        public BlockEntityBooks(bool isUnique, bool isPaper, int pageMax, string title, string author, string[] text, BlockPos blockPos)
        {
            this.IsPaper = isPaper;
            Unique = isUnique;
            Pos = blockPos;
            PageMax = pageMax;
            Author = author;
            DeletingText();
            PageTexts = text;
            Title = title;
        }

        public BlockEntityBooks(ICoreServerAPI sapi)
        {
            DeletingText();
            Sapi = sapi;
        }

        public BlockEntityBooks(ICoreClientAPI capi)
        {
            DeletingText();
            Capi = capi;
        }

        public void NamingPages()
        {
            // naming for saving in tree attributes, e.g. page1
            string
                updatedPageName = "page",
                tempNumbering = "";

            for (var i = 1; i <= PageLimit; i++)
            {
                tempNumbering = i.ToString();
                ArPageNames[i - 1] = string.Concat(
                    updatedPageName,
                    tempNumbering
                );
            }
        }

        public void DeletingText()
        {
            for (var i = 0; i < PageLimit; i++)
            {
                PageTexts[i] = "";
            }
        }


        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (Api is ICoreClientAPI && !IsPaper)
            {
                return _bookAnim.HideDrawModel();
            }
            return false;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            Api = api;

            if (api is ICoreClientAPI && !IsPaper)
            {
                _bookAnim = new BooksAnimationHandler(api as ICoreClientAPI, this);
            }

            if (ArPageNames == null)
            {
                NamingPages();
            }
            if (!Unique)
            {
                DeletingText();
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            // TODO: rewrite to only send data on read
            // only always load title info!
            Unique = tree.GetBool("unique");
            PageMax = tree.GetInt("PageMax", 1);
            Title = tree.GetString("title", "");
            if (ArPageNames[0] == null)
            {
                NamingPages();
            }
            if (!Unique)
            {
                DeletingText();
            }
            for (var i = 0; i < PageMax; i++)
            {
                PageTexts[i] = tree.GetString(ArPageNames[i], "");
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetBool("unique", Unique);
            tree.SetInt("PageMax", PageMax);
            tree.SetString("title", Title);
            if (ArPageNames[0] == null)
            {
                NamingPages();
            }
            if (!Unique)
            {
                DeletingText();
            }
            // TODO: rewrite to only send data on read
            // only always load title and maxpage number info!
            for (var i = 0; i < PageMax; i++)
            {
                tree.SetString(ArPageNames[i], PageTexts[i]);
            }
        }

        public override void OnBlockBroken()
        {
            // unregister renderer?
            if (Api is ICoreClientAPI && !IsPaper)
            {
                _bookAnim.Dispose();
            }
            // keep data
            // base.OnBlockBroken(); 
        }


        public void OnRightClick(IPlayer byPlayer, bool isPaper)
        {
            var controlRw = FlagRead;

            var hotbarSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (isPaper)
            {
                this.IsPaper = isPaper;
            }

            if (PageTexts[0] == null)
            {
                DeletingText();
            }

            if (byPlayer.Entity?.Controls?.Sprint == true)
            {
                if (Api is ICoreClientAPI && !isPaper)
                {
                    _bookAnim.Close(Api);
                }
            }

            if (byPlayer.Entity?.Controls?.Sneak == true)
            {
                if (DoesPlayerHaveWritingUtensilEquipped(byPlayer, out var writingUtensilSlot))
                {
                    _currentUserInfo = new UserInfo
                    {
                        Player = byPlayer,
                        EquippedItemSlot = writingUtensilSlot,
                        InitialWritingUtensilDurability = writingUtensilSlot.Itemstack.Collectible.Durability,
                    };
                    controlRw = FlagWrite;
                }
            }

            if (Api.World is IServerWorldAccessor)
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    var writer = new BinaryWriter(ms);
                    writer.Write(NetworkName);
                    writer.Write(PageMax);
                    for (var i = 0; i < PageMax; i++)
                    {
                        writer.Write(PageTexts[i]);
                    }
                    writer.Write(Title);
                    writer.Write(controlRw);
                    writer.Write(Unique);

                    data = ms.ToArray();
                }

                ((ICoreServerAPI)Api).Network.SendBlockEntityPacket(
                    (IServerPlayer)byPlayer,
                    Pos.X, Pos.Y, Pos.Z,
                    (int)EnumBookPacketId.OpenDialog,
                    data
                );
            }
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            // TODO: populate BooksNetworkHandler:
            if (packetid == (int)EnumBookPacketId.SaveBook)
            {
                var previousBookTextLength = PageTexts.Sum(pageText => pageText?.Length ?? 0);
                using (var ms = new MemoryStream(data))
                {
                    var reader = new BinaryReader(ms);
                    PageMax = reader.ReadInt32();
                    for (var i = 0; i < PageMax; i++)
                    {
                        PageTexts[i] = reader.ReadString();
                    }
                    Title = reader.ReadString();
                    Unique = reader.ReadBoolean();
                }
                NamingPages();
                MarkDirty(true);
                Api.World.BlockAccessor.GetChunkAtBlockPos(Pos.X, Pos.Y, Pos.Z).MarkModified();
            }
        }

        public override void OnReceivedServerPacket(int packetid, byte[] data)
        {
            // TODO: populate BooksNetworkHandler, sloppy for now:
            if (packetid == (int)EnumBookPacketId.OpenDialog)
            {
                using (var ms = new MemoryStream(data))
                {
                    var reader = new BinaryReader(ms);

                    var dialogClassName = reader.ReadString();
                    PageMax = reader.ReadInt32();
                    for (var i = 0; i < PageMax; i++)
                    {
                        PageTexts[i] = reader.ReadString();
                    }
                    Title = reader.ReadString();
                    var controlRw = reader.ReadString();
                    var unique = reader.ReadBoolean();

                    var clientWorld = (IClientWorldAccessor)Api.World;


                    if (controlRw.Equals(FlagWrite))
                    {
                        var bGuiWrite = new BooksGui(IsPaper, unique, Title, PageTexts, PageMax, Api as ICoreClientAPI, IdDialogBookEditor);
                        bGuiWrite.WriteGui(Pos, Api as ICoreClientAPI);
                        bGuiWrite.OnTextChanged = (previousText, currentText) =>
                        {
                            var writingUtensil = _currentUserInfo.EquippedItemSlot?.Itemstack?.Collectible;
                            if (writingUtensil == null)
                            {
                                bGuiWrite.TryClose();
                            }
                            else
                            {
                                _currentUserInfo.EquippedItemSlot.Itemstack.Collectible.DamageItem(
                                    Api.World,
                                    _currentUserInfo.Player.Entity,
                                    _currentUserInfo.EquippedItemSlot,
                                    Math.Abs(currentText.Length - previousText.Length)
                                );
                            }
                        };
                        bGuiWrite.OnCloseCancel = () =>
                        {
                            if (Api is ICoreClientAPI && !IsPaper)
                            {
                                _bookAnim.Close();
                            }
                            (Api as ICoreClientAPI)?.Network
                                .SendBlockEntityPacket(
                                    Pos.X,
                                    Pos.Y,
                                    Pos.Z,
                                    (int)EnumBookPacketId.CancelEdit
                                );
                            _currentUserInfo = null;
                        };
                        bGuiWrite.TryOpen();
                    }
                    else
                    {
                        var bGuiRead = new BooksGui(IsPaper, unique, Title, PageTexts, PageMax, Api as ICoreClientAPI, IdDialogBookReader);
                        bGuiRead.ReadGui(Pos, Api as ICoreClientAPI);
                        bGuiRead.OnCloseCancel = () =>
                        {
                            if (Api is ICoreClientAPI && !IsPaper)
                            {
                                _bookAnim.Close();
                            }
                            (Api as ICoreClientAPI)
                                .Network
                                .SendBlockEntityPacket(
                                    Pos.X, Pos.Y, Pos.Z,
                                    (int)EnumBookPacketId.CancelEdit);
                        };
                        bGuiRead.TryOpen();
                    }
                    if (Api is ICoreClientAPI && !IsPaper)
                    {
                        _bookAnim.Open(Api);
                    }
                }
            }
        }

        private static bool DoesPlayerHaveWritingUtensilEquipped(IPlayer player, out ItemSlot writingUtensilSlot)
        {
            writingUtensilSlot = player.InventoryManager.ActiveHotbarSlot;
            return writingUtensilSlot?.Itemstack?.ItemAttributes?["writing_utensil"].Exists == true;
        }
    }

    public enum EnumBookPacketId
    {
        OpenDialog = 5301,
        SaveBook = 5302,
        CancelEdit = 5303
    }
}
