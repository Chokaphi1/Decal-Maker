namespace VAM_Decal_Maker
{
    //root class that provide the basic setup and anchor for a window
    //fix this to derive
    public class UIPanelBase : PanelBase
    {
        public UIDynamic spacerLeft { get; private set; }
        protected UIDynamic spacerRight { get; set; }

        public UIPanelBase(Decal_Maker DM, int size = 550) : base(DM)
        {
            //create a UI Spacers to Hold our Panels
            spacerLeft = DM.CreateSpacer(false);
            spacerLeft.height = size;
            spacerRight = DM.CreateSpacer(true);
            spacerRight.height = size;
        }
    }
}