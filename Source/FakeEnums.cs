using System.Collections.Generic;

//lets fake enum since compiler issue
namespace VAM_Decal_Maker
{
    public static class EventEnum
    {
        //Core Panel
        public const string CoreSetupFinished = "Core-SETUP FINISHED";
        public const string CoreTempDecalToPerm = "Core Button:Panel Temp to Perm";
        public const string CoreRemoveTempPanels = "Remove Decals flagged with temp";
        public const string CoreNewCharacterSelected = "New Skin Selected";
        public const string ToggleGenitalCutout = "_toggleGenitalCutout";
        public const string ToggleNippleCutout = "_toggleNippleCutout";
        public const string CoreRestoreFromJSON = "Restore data from json";
        public const string CoreResetAll = "Reset and clear all panels";



        //ImagePanel
        public const string ImagePanelPathChanged = "ImagePanel-PathChange";
        public const string ImagePanelMaterialChanged = "ImagePanel Material Changed";
        public const string ImagePanelNormalSliderChange = "ImagePanel Normal Scale Changed";
        public const string ImagePanelSpecSliderChange = "ImagePanelSpecChanged";
        public const string ImagePanelTextureCleared = "ImagePanelTextureCleared";
        public const string ImagePanelTextureLoaded = "ImagePanelTextureLoaded";
        public const string ImagePanelVideoFrameUpdate = "ImagePanelVideoFrameUpdate";
        public const string ImagePanelVidePrepared = "ImagePanel Vide Prepared";
        public const string ImagePanelImageLoad = "Image selected for loading";
        public const string ImagePanelVideoLoad = "Video selected for loading";



        //DecalPanel
        public const string DecalPanelColor = "Color";
        public const string DecalPanelMove = "DecalPanelMove";
        public const string DecalPanelButtonUP = "DecalPanel-Move-UP";
        public const string DecalPanelButtonDOWN = "DecalPanel-Move-DOWN";
        public const string DecalPanelButtonCLOSE = "DecalPanel Close Button";
        public const string DecalPanelButtonAdd = "DecalPanel-CREATE";
        public const string DecalPanelButtonCOPY = "DecalPanel-Duplicate";
        public const string DecalPanelDELETE = "DecalPanel-DELETE";
        public const string DecalPanelLinkChanged = "DecalPanel-LinkChanged";

        //ManagerPanel
        public const string ManagerPanelSelection = "New Texture Region Selected";
        public const string ManagerPanelRestoreActive = "Restore last active panel";
        public const string ManagerPanelButtonADD = "Add new panel after last panel";
        public const string ManagerPanelButtonCLOSE = "Remove last panel";

        //HeaderPanel
        public const string HeaderPanelSelection = "New Material Region Selected";

        //preview Panel
        //public const string Scale = "Scale";
        //public const string Create = "Create";
        //public const string Delete = "Delete";
        //public const string Sort = "Sort";

    }

    public static class MatSlotEnum
    {
        public const string MainTex = "_MainTex";
        public const string DecalTex = "_DecalTex";
        public const string BumpMap = "_BumpMap";
        public const string SpecTex = "_SpecTex";
        public const string GlossTex = "_GlossTex";

        public static readonly List<string> Values = new List<string>() { MainTex, DecalTex, BumpMap, SpecTex, GlossTex };
    }
    public static class BodyRegionEnum
    {
        public const string Torso = "torso";
        public const string Face = "face";
        public const string Limbs = "limbs";
        public const string Genitals = "genitals";

        public const string EyeReflection = "relflection";
        public const string EyePupil = "pupil";
        public const string EyeIris = "iris";
        public const string EyeCornea = "cornea";
        public const string EyeSclera = "whites";

        public static readonly List<string> Values = new List<string>() { Torso, Face, Limbs, Genitals };

    }


}
