using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

// TODO: nicer background!
// TODO: onRead send TreeAttributes of arText only (decrease networking)
// Feat1: Help-Tab for book, shows more info on format, features etc.
// Feat2: Add waypoint sharing support?

namespace Books
{
    internal class BooksGui : GuiDialogGeneric
    {
        private static readonly int
            MaxTitleWidth = 240;

        private static readonly int
            MaxLines = 18;

        private static readonly int
            MaxWidth = 580;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            PageLimit = 20;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            PageNumberingFont = 18;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            PageNumberingHeight = 20;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            PageNumberingWidth = 50;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            SaveButtonOffsetX = 80;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TitleX = 20;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TitleY = 40;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TitleWidth = 250;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TitleHeight = 20;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TitleFont = 18;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TextFont = 18;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TextX = 0;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            TextY = 20;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            WindowWidth = 600;

        private static readonly int
            // PageLimit, how many pages players can have in a book,
            // network performance impact on chunk load and player join
            WindowHeight = 400;


        private static readonly string
            // Language en.json references:
            LangTextDef = "books:editor-text-default";

        private static readonly string
            // Language en.json references:
            LangTitelDef = "books:editor-titel-default";

        private static readonly string
            // Language en.json references:
            LangTitelEditor = "books:editor-titel";

        private static readonly string
            // Language en.json references:
            LangbCancel = "books:editor-cancel";

        private static readonly string
            // Language en.json references:
            LangbSave = "books:editor-save";

        private static readonly string
            // Language en.json references:
            LangbClose = "books:editor-close";

        private static readonly string
            // Language en.json references:
            LangbHelp = "books:editor-help";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            DialogNameEditor = "bookeditor";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            CompNameRead = "blockentitytextreaddialog";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            CompNameEdit = "blockentitytexteditordialog";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            IDTextArea = "text";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            IDRichtextArea = "page";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            IDTitleInput = "title";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            IDPageArea = "page-numbering";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            //IDHelpArea = "help-page",
            _bSub = "-";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            //IDHelpArea = "help-page",
            _bAdd = "+";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            //IDHelpArea = "help-page",
            _bNextPage = ">>";

        private static readonly string
            // Language en.json references:
            //LangHelpText = "books:editor-help-text",
            // IDs/dialog keys:
            //IDHelpArea = "help-page",
            _bPrevPage = "<<";

        private readonly ICoreClientAPI Capi;

        private readonly string[] Text = new string[20];

        private BlockPos BEPos;

        public bool
            didSave,
            isPaper,
            Unique;

        public Action OnCloseCancel;

        public delegate void OnTextChangedHandler(string previousText, string currentText);
        public OnTextChangedHandler OnTextChanged;

        private int
            PageCurrent;

        private string
            Title = "";

        private readonly string[] _previousText;

        private string
            CurrentPageNumbering = "1/1",
            EditTitle = "",
            // button texts:
            _bCancel = "",
            _bSave = "",
            _bClose = "",
            _bHelp = "";

        private string _previousTitle;

        public BooksGui(bool isPaper, bool unique, string booktitle, string[] text, int pagemax, ICoreClientAPI capi, string dialogTitel) : base(dialogTitel, capi)
        {
            Capi = capi;
            GetLangEntries();
            this.isPaper = isPaper;
            PageMax = pagemax;
            DeletingText();
            _previousText = text;
            text.CopyTo(Text, 0);
            _previousTitle = booktitle;
            Title = booktitle;
            Unique = unique;
        }

        public int
            PageMax { get; private set; }

        private void GetLangEntries()
        {
            EditTitle = Lang.Get(LangTitelEditor);
            _bCancel = Lang.Get(LangbCancel);
            _bSave = Lang.Get(LangbSave);
            _bClose = Lang.Get(LangbClose);
            _bHelp = Lang.Get(LangbHelp);
        }

        private void DeletingText()
        {
            Unique = false;
            for (var i = 0; i < PageLimit; i++)
            {
                Text[i] = "";
            }
            Text[0] = Lang.Get(LangTextDef);
            Title = Lang.Get(LangTitelDef);
        }

        private void UpdatingText()
        {
            if (DialogTitle == DialogNameEditor)
            {
                Composers[CompNameEdit]
                    .GetTextArea(IDTextArea)
                    .SetValue(Text[PageCurrent]);
            }
            else // to keep richtext functionality
            {
                ReadGui(BEPos, Capi);
            }
        }

        private void UpdatingCurrentPageNumbering()
        {
            var temp_page = 0;
            string
                updatedCurrentPageNumbering = "",
                currentPage = "",
                dividerLayout = "/",
                lastPage = "";

            // Display purpose only: 1 to PageMax+1 instead of 0 to PageMax, e.g. 0/9 is 1/10
            temp_page = PageCurrent + 1;
            currentPage = temp_page.ToString();
            temp_page = PageMax;
            lastPage = temp_page.ToString();

            updatedCurrentPageNumbering = string.Concat(
                currentPage,
                dividerLayout,
                lastPage
            );

            CurrentPageNumbering = updatedCurrentPageNumbering;
            if (DialogTitle == DialogNameEditor)
            {
                Composers[CompNameEdit]
                    .GetDynamicText(IDPageArea)
                    .SetNewText(CurrentPageNumbering, false, true);
            }
            else
            {
                Composers[CompNameRead]
                    .GetDynamicText(IDPageArea)
                    .SetNewText(CurrentPageNumbering, false, true);
            }
        }

        private bool SavingInputTemporary()
        {
            if (DialogTitle == DialogNameEditor)
            {
                Title = Composers[CompNameEdit]
                    .GetTextInput(IDTitleInput)
                    .GetText();
                Text[PageCurrent] = Composers[CompNameEdit]
                    .GetTextArea(IDTextArea)
                    .GetText();
            }
            return true;
        }

        public void WriteGui(BlockPos pos, ICoreClientAPI Capi)
        {
            ElementBounds
                TitleAreaBounds = ElementBounds
                    .Fixed(TitleX, TitleY, TitleWidth, TitleHeight),
                TextAreaBounds = ElementBounds
                    .Fixed(TextX, TextY, WindowWidth, WindowHeight),
                ClippingBounds = TextAreaBounds
                    .ForkBoundingParent()
                    .WithFixedPosition(0, 50),
                AddPageButtonBounds = ElementBounds
                    .FixedSize(0, 0)
                    .FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 + 10, 0)
                    .WithFixedPadding(3, 2),
                SubPageButtonBounds = ElementBounds
                    .FixedSize(0, 0)
                    .FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 - 10, 0)
                    .WithFixedPadding(4, 2),
                CancelButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(0, 0)
                    .WithFixedPadding(10, 2),
                SaveButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(SaveButtonOffsetX, 0)
                    .WithFixedPadding(10, 2),
                NextPageButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 + 29, 0)
                    .WithFixedPadding(3, 2),
                PrevPageButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 - 40, 0)
                    .WithFixedPadding(4, 2),
                PageNumberingAreaBounds = ElementBounds
                    .FixedSize(PageNumberingWidth, PageNumberingHeight)
                    .FixedUnder(ClippingBounds, 2 * 5)
                    .WithAlignment(EnumDialogArea.RightFixed),
                bgBounds = ElementBounds
                    .Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding),
                dialogBounds = ElementStdBounds
                    .AutosizedMainDialog
                    .WithAlignment(EnumDialogArea.RightMiddle)
                    .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

            double
                TextareaFixedY = TextAreaBounds.fixedY,
                TitleAreaFixedY = TitleAreaBounds.fixedY;

            BEPos = pos;

            //flag_RW = flag_W;

            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(
                ClippingBounds,
                CancelButtonBounds,
                SaveButtonBounds,
                SubPageButtonBounds,
                AddPageButtonBounds,
                NextPageButtonBounds,
                PrevPageButtonBounds,
                PageNumberingAreaBounds
            );

            Composers[CompNameEdit] = capi.Gui
                .CreateCompo(CompNameEdit, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(EditTitle, OnTitleBarClose)
                .AddTextInput(
                    TitleAreaBounds,
                    OnBookTextChanged,
                    CairoFont.TextInput().WithFontSize(TitleFont),
                    IDTitleInput)
                .BeginChildElements(bgBounds)
                .BeginClip(ClippingBounds)
                .AddTextArea(
                    TextAreaBounds,
                    OnBookTextChanged,
                    CairoFont.TextInput().WithFontSize(TextFont),
                    IDTextArea)
                .EndClip()
                .AddSmallButton(Lang.Get(_bSave), OnButtonSave, SaveButtonBounds)
                .AddSmallButton(Lang.Get(_bSub), OnButtonSub, SubPageButtonBounds)
                .AddSmallButton(Lang.Get(_bAdd), OnButtonAdd, AddPageButtonBounds)
                .AddSmallButton(Lang.Get(_bNextPage), OnButtonNextPage, NextPageButtonBounds)
                .AddSmallButton(Lang.Get(_bPrevPage), OnButtonPrevPage, PrevPageButtonBounds)
                .AddDynamicText(
                    CurrentPageNumbering,
                    CairoFont.TextInput().WithFontSize(PageNumberingFont),
                    EnumTextOrientation.Center,
                    PageNumberingAreaBounds,
                    IDPageArea)
                .EndChildElements()
                .Compose();


            Composers[CompNameEdit]
                .GetTextArea(IDTextArea)
                .SetMaxLines(MaxLines);
            Composers[CompNameEdit]
                .GetTextArea(IDTextArea)
                .SetMaxWidth((int)(MaxWidth * RuntimeEnv.GUIScale));
            Composers[CompNameEdit]
                .GetTextInput(IDTitleInput)
                .SetMaxWidth(MaxTitleWidth);

            if (Text.Length > 0)
            {
                Composers[CompNameEdit]
                    .GetTextArea(IDTextArea)
                    .SetValue(Text[PageCurrent]);
            }
            if (Title.Length > 0)
            {
                Composers[CompNameEdit]
                    .GetTextInput(IDTitleInput)
                    .SetValue(Title);
            }
            if (CurrentPageNumbering.Length > 0)
            {
                Composers[CompNameEdit]
                    .GetDynamicText(IDPageArea)
                    .SetNewText(CurrentPageNumbering, false, true);
            }
            UpdatingCurrentPageNumbering();
        }

        private bool OnButtonNextPage()
        {
            if (PageCurrent < PageMax - 1)
            {
                SavingInputTemporary();
                PageCurrent += 1;
                UpdatingText();
                UpdatingCurrentPageNumbering();
            }
            return true;
        }

        private bool OnButtonPrevPage()
        {
            if (PageCurrent > 0)
            {
                SavingInputTemporary();
                PageCurrent -= 1;
                UpdatingText();
                UpdatingCurrentPageNumbering();
            }
            return true;
        }

        private bool OnButtonSub()
        {
            if (PageMax > 1)
            {
                Text[PageMax] = "";
                PageMax -= 1;
            }
            // Need to return to prev. page if currently displayed lastpage was deleted
            if (PageCurrent >= PageMax)
            {
                UpdatingText();
                OnButtonPrevPage();
            }
            UpdatingCurrentPageNumbering();

            return true;
        }

        private bool OnButtonAdd()
        {
            if (isPaper)
            {
                if (PageMax == 2)
                {
                    return true;
                }
            }

            if (PageMax < PageLimit)
            {
                PageMax += 1;
                Text[PageMax - 1] = "";
                UpdatingCurrentPageNumbering();
            }

            return true;
        }

        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            if (DialogTitle == DialogNameEditor)
            {
                Composers[CompNameEdit]
                    .FocusElement(Composers[CompNameEdit]
                        .GetTextArea(IDTextArea)
                        .TabIndex);
            }
            else
            {
                Composers[CompNameRead]
                    .FocusElement(Composers[CompNameRead]
                        .GetRichtext(IDRichtextArea)
                        .TabIndex);
            }
        }

        private void OnBookTextChanged(string value)
        {
            if (DialogTitle == DialogNameEditor)
            {
                var titleArea = Composers[CompNameEdit].GetTextInput(IDTitleInput);
                var titleText = titleArea.GetText();
                var textArea = Composers[CompNameEdit].GetTextArea(IDTextArea);
                Text[PageCurrent] = textArea.GetText();
                OnTextChanged?.Invoke($"{_previousTitle} | {string.Join("\n", _previousText)}", $"{titleText} | {string.Join("\n", Text)}");
                _previousTitle = titleText;
                _previousText[PageCurrent] = Text[PageCurrent];
            }
        }

        private void OnTitleBarClose()
        {
            OnButtonCancel();
        }

        private bool OnButtonSave()
        {
            // OnButtonSave commits text to block
            // making it unique
            if (DialogTitle == DialogNameEditor)
            {
                SavingInputTemporary();
                Unique = true;

                byte[] data;
                using (var ms = new MemoryStream())
                {
                    var writer = new BinaryWriter(ms);
                    writer.Write(PageMax);
                    for (var i = 0; i < PageMax; i++)
                    {
                        writer.Write(Text[i]);
                    }
                    writer.Write(Title);
                    writer.Write(Unique);
                    data = ms.ToArray();
                }
                capi
                    .Network
                    .SendBlockEntityPacket(BEPos.X, BEPos.Y, BEPos.Z, (int)EnumBookPacketId.SaveBook, data);

                didSave = true;
                TryClose();
                Dispose();
            }
            return true;
        }

        private bool OnButtonCancel()
        {
            TryClose();
            return true;
        }

        public override void OnGuiClosed()
        {
            if (!didSave)
            {
                OnButtonSave();
                OnCloseCancel?.Invoke();
            }
            base.OnGuiClosed();
        }

        // TODO: ReadGui
        // Need of implementation: only send data of individual page on reading
        // not on chunk load/player join!
        // reduce network traffic
        // only send book title info
        // //////>> populate BooksNetworkHandler
        // see: GuiDialogJournal.Paginate
        public void ReadGui(BlockPos Pos, ICoreClientAPI Capi)
        {
            BEPos = Pos;

            ElementBounds
                textAreaBounds = ElementBounds
                    .Fixed(0, 0, WindowWidth, WindowHeight),
                ClippingBounds = textAreaBounds
                    .ForkBoundingParent()
                    .WithFixedPosition(0, 30),
                CancelButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(0, 0)
                    .WithFixedPadding(10, 2),
                NextPageButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 + 29, 0)
                    .WithFixedPadding(3, 2),
                PrevPageButtonBounds = ElementBounds
                    .FixedSize(0, 0).FixedUnder(ClippingBounds, 2 * 5)
                    .WithFixedAlignmentOffset(WindowWidth / 2 - 40, 0)
                    .WithFixedPadding(4, 2),
                PageNumberingAreaBounds = ElementBounds
                    .FixedSize(PageNumberingWidth, PageNumberingHeight)
                    .FixedUnder(ClippingBounds, 2 * 5)
                    .WithAlignment(EnumDialogArea.RightFixed),
                bgBounds = ElementBounds
                    .Fill
                    .WithFixedPadding(GuiStyle.ElementToDialogPadding),
                dialogBounds = ElementStdBounds
                    .AutosizedMainDialog
                    .WithAlignment(EnumDialogArea.RightMiddle)
                    .WithFixedAlignmentOffset(-GuiStyle.DialogToScreenPadding, 0);

            var textareaFixedY = textAreaBounds.fixedY;

            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(
                ClippingBounds,
                CancelButtonBounds,
                NextPageButtonBounds,
                PrevPageButtonBounds,
                PageNumberingAreaBounds);

            Composers[CompNameRead] = capi.Gui
                .CreateCompo(CompNameRead, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(Title, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                .BeginClip(ClippingBounds)
                // TODO:
                .AddRichtext(
                    Text[PageCurrent],
                    CairoFont.TextInput().WithFontSize(TextFont),
                    textAreaBounds,
                    IDRichtextArea)
                .EndClip()
                .AddSmallButton(Lang.Get(_bClose), OnButtonCancel, CancelButtonBounds)
                .AddSmallButton(Lang.Get(_bNextPage), OnButtonNextPage, NextPageButtonBounds)
                .AddSmallButton(Lang.Get(_bPrevPage), OnButtonPrevPage, PrevPageButtonBounds)
                .AddDynamicText(
                    CurrentPageNumbering,
                    CairoFont.TextInput().WithFontSize(PageNumberingFont),
                    EnumTextOrientation.Center,
                    PageNumberingAreaBounds,
                    IDPageArea)
                .EndChildElements()
                .Compose();

            UpdatingCurrentPageNumbering();
        }
    }
}
