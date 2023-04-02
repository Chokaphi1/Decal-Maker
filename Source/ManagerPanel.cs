using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uFileBrowser;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace VAM_Decal_Maker
{
    //Controls the display of Decal Level UI elements
    //split off from ImagePanel to focus class
    public class ManagerPanel : UIPanelBase
    {
        public ImagePanelBase ImagePanel { get; set; }
        public new GameObject gameObject { get { return ImagePanel.gameObject; } }
        private Button Button { get { return ImagePanel.Button; } }
        //controls the addition and removal of all panels
        public List<DecalPanel> DecalPanels { get; set; } = new List<DecalPanel>();
        private List<DecalPanel> TempDecalPanels { get; set; } = new List<DecalPanel>();
        private static Random rd = new Random();
        private RenderPanelBase renderPanel { get; set; }
        private RenderPanelNormal SelfNormRenderPanel { get; set; }
        private int retries = 0;

        private JSONStorableUrl jsnURL;
        private JSONStorableUrl jsnDecalURL;

        public override void OnDestroy()
        {
            DeregisterDAZCharacterTextureControl();
            base.OnDestroy();
        }

        //events from decal/image panel
        private void DecalPanelEvent(object o, PanelEventArgs e)
        {
            //is this event for us. Has a decal panel and it is one of ours
            if (e.DecalPanel == null || !DecalPanels.Contains(e.DecalPanel))
            {
                return;
            }
            int currentIndex;
            int newIndex;
            if (renderPanel != null)
            {
                switch (e.EventName)
                {
                    case EventEnum.DecalPanelColor:
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.DecalPanelMove:
                        RepositionDecalPanel(e.DecalPanel, e.NewPosition);
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.DecalPanelButtonUP:
                        currentIndex = e.DecalPanel.gameObject.transform.GetSiblingIndex();
                        newIndex = currentIndex - 1;
                        if (newIndex >= 0)
                        {
                            RepositionDecalPanel(e.DecalPanel, newIndex);
                            renderPanel.IsDirty = true;
                        }
                        break;

                    case EventEnum.DecalPanelButtonDOWN:
                        currentIndex = e.DecalPanel.gameObject.transform.GetSiblingIndex();
                        newIndex = currentIndex + 1;
                        if (newIndex < DecalPanels.Count)
                        {
                            RepositionDecalPanel(e.DecalPanel, newIndex);
                            renderPanel.IsDirty = true;
                        }
                        break;
                    case EventEnum.DecalPanelButtonCLOSE:
                        RemoveDecalPanel(e.DecalPanel);
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.DecalPanelButtonAdd:
                        DecalPanel d = AddDecalPanels();
                        currentIndex = e.DecalPanel.gameObject.transform.GetSiblingIndex();
                        newIndex = currentIndex + 1;
                        RepositionDecalPanel(d, newIndex);
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.DecalPanelButtonCOPY:
                        d = AddDecalPanels();
                        //Copy Function to copy source data to target
                        e.DecalPanel.CopyDataToTargetPanel(d);
                        currentIndex = e.DecalPanel.gameObject.transform.GetSiblingIndex();
                        newIndex = currentIndex + 1;
                        RepositionDecalPanel(d, newIndex);
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.ImagePanelPathChanged:
                        // JSONStorableUrl lastImageDir = DM.GetUrlJSONParam("LastImageDir");
                        //lastImageDir.FileBrowse();

                        string lastDir = DM.GetUrlParamValue("LastImageDir");
                        List<ShortCut> shortCuts = FileManagerSecure.GetShortCutsForDirectory("Custom/Atom/Person/Textures/", false, true, true, false);
                        SuperController.singleton.GetMediaPathDialog(new FileBrowserCallback(e.DecalPanel.BrowserCallBack), "", lastDir, true, true, false, null, false, shortCuts);

                        break;

                    case EventEnum.ImagePanelNormalSliderChange:
                        renderPanel.IsDirty = true;
                        break;

                    case EventEnum.ImagePanelVideoFrameUpdate:
                        //Debug.Log(DateTime.Now +  " video event ");
                        renderPanel.IsDirty = true;
                        break;

                    default:
                        //SuperController.LogError("OPERATION NOT HANDLED" + e.EventName);
                        renderPanel.IsDirty = true;
                        break;
                }
            }

        }

        //events from Core/General events
        public void CoreEvent(object o, PanelEventArgs e)
        {
            //SuperController.LogError("CORE EVENT " + e.EventName + " " + o.ToString());
            switch (e.EventName)
            {   //set Torso as active slot
                case EventEnum.CoreSetupFinished:
                    if (TextureSlot == BodyRegionEnum.Torso)
                    {
                        //set the torso UI of each as selection on each UI
                        RaiseCoreEvent(this, new PanelEventArgs(EventEnum.ManagerPanelSelection, this));
                    }
                    break;

                case EventEnum.CoreTempDecalToPerm:
                    ResetTempDecalPanelTable();
                    break;

                case EventEnum.CoreNewCharacterSelected:
                    if (e.Bool)
                    {
                        //DeregisterDAZCharacterTextureControl();
                        //RegisterDAZCharacterTextureControl();
                    }
                    UpdateSkinImage();
                    renderPanel.IsDirty = true;
                    break;

                case EventEnum.ToggleGenitalCutout:
                    if (MaterialSlot == MatSlotEnum.DecalTex && TextureSlot == BodyRegionEnum.Genitals)
                    {
                        renderPanel.IsDirty = true;
                    }
                    break;

                case EventEnum.ToggleNippleCutout:
                    if (MaterialSlot == MatSlotEnum.DecalTex && TextureSlot == BodyRegionEnum.Torso)
                    {
                        renderPanel.IsDirty = true;
                    }
                    break;

                case EventEnum.ManagerPanelButtonADD:
                    if (e.ManagerPanel == this)
                    {
                        AddDecalPanels();
                        //UI may not have the tab selected so sync
                        UpdateSelection(e);
                    }
                    break;

                case EventEnum.ManagerPanelButtonCLOSE:
                    if (e.ManagerPanel == this)
                        RemoveDecalPanel();
                    break;

                case EventEnum.ManagerPanelSelection:
                    UpdateSelection(e);
                    break;

                case EventEnum.CoreRestoreFromJSON:
                    if (e.materialSlot == MaterialSlot && e.bodyRegion == TextureSlot)
                    {
                        //SuperController.LogError("EventEnum.CoreRestoreFromJSON:" + MaterialSlot + TextureSlot + " " + DecalPanels.Count);
                        RestorePanelFromJSON(e.saveJSON, e.Bool);
                    }
                    break;

                case EventEnum.CoreResetAll:
                    int count = DecalPanels.Count;
                    for (int i = 0; i < count; i++)
                    {
                        RemoveDecalPanel();
                    }
                    break;

                case EventEnum.CoreRemoveTempPanels:
                    DM.StartCoroutine(RemoveTempDecalPanels());
                    break;

                case EventEnum.DecalPanelColor:

                    //return;
                    bool isUpdated = false;
                    foreach (DecalPanel d in DecalPanels)
                    {   //if not event from us, our ID is set and we are same ID
                        if (d != e.DecalPanel && d.linkedPanelID != "*" && d.linkedPanelID == e.DecalPanel.linkedPanelID)
                        {   //we need to copy data and not trigger a EventEnum.DecalPanelColor event to avoid loop
                            //SuperController.LogError("Copy Data from panel" + e.DecalPanel.RandomName + " To panel " + d.RandomName);
                            d.CopyDataFromTargetPanel(e.DecalPanel, false);
                            //tell material to update
                            d.UnifiedImagePanelColor(false);
                            isUpdated = true;
                        }
                    }
                    if (isUpdated)
                        //SuperController.LogError("render is dirty"); 
                        renderPanel.IsDirty = true;
                    break;


            }
        }

        private void UpdateSelection(PanelEventArgs e)
        {
            //we only want to update on the 4 UI buttons of this material slot.
            if (e.ManagerPanel == this)
            {
                Button.image.color = Color.white;
                ActivateWindow(true);
            }
            else if (e.ManagerPanel.MaterialSlot == MaterialSlot)
            {
                Button.image.color = Color.grey;
                ActivateWindow(false);
            }
        }
        //need to turn our texture and mat enums into VAM equiv strings
        private string ConvertToVAMName(string convert)
        {
            switch (convert)
            {
                default:
                case MatSlotEnum.DecalTex:
                    return "Decal";
                case MatSlotEnum.MainTex:
                    return "Diffuse";
                case MatSlotEnum.SpecTex:
                    return "Specular";
                case MatSlotEnum.GlossTex:
                    return "Gloss";
                case MatSlotEnum.BumpMap:
                    return "Normal";
                case BodyRegionEnum.Face:
                    return "face";
                case BodyRegionEnum.Limbs:
                    return "limbs";
                case BodyRegionEnum.Torso:
                    return "torso";
                case BodyRegionEnum.Genitals:
                    return "genitals";
            }
        }

        private void UrlCallBackDecal(JSONStorableString s)
        {
            //SuperController.LogError(DateTime.Now + " UrlCallBackDecal " + MaterialSlot + TextureSlot);
            if (s.val != string.Empty)
            {
                DecalPanel d = AddDecalPanels();
                d.RestoreImageFromJSON(s.val);
            }
        }

        private void UrlCallBack(JSONStorableString s)
        {
            //SuperController.LogError(DateTime.Now + " UrlCallBack " + MaterialSlot + TextureSlot);
            DM.QueueCharacterChanged();
        }

        private void RegisterDAZCharacterTextureControl()
        {
            string mat = ConvertToVAMName(MaterialSlot);
            string tex = ConvertToVAMName(TextureSlot);

            string paramName = string.Format("{0}{1}Url", tex, mat);

            DAZCharacterTextureControl dAZCharacterTextureControl = DM._dazCharacter.GetComponentInChildren<DAZCharacterTextureControl>();
            foreach (string UrlParamName in dAZCharacterTextureControl.GetUrlParamNames())
            {
                if (paramName == UrlParamName)
                {
                    JSONStorableUrl jsnURL = dAZCharacterTextureControl.GetUrlJSONParam(UrlParamName);

                    if (MaterialSlot == MatSlotEnum.DecalTex)
                    {   //override VAM Decal system
                        jsnDecalURL = jsnURL;
                        //jsnURL.setJSONCallbackFunction = UrlCallBackDecal;

                        //register diffuse texture change as well
                        //jsnURL = dAZCharacterTextureControl.GetUrlJSONParam(UrlParamName.Replace("Decal", "Diffuse"));
                        //just ask to be notified VAM Decal system
                        //jsnURL.setJSONCallbackFunction += UrlCallBack;
                    }
                    else
                    {//
                     // jsnURL.setJSONCallbackFunction += UrlCallBack;
                    }
                    this.jsnURL = jsnURL;
                }
            }
        }

        private void DeregisterDAZCharacterTextureControl()
        {
            if (this.jsnDecalURL != null)
                this.jsnDecalURL.setJSONCallbackFunction -= UrlCallBackDecal;

            if (this.jsnURL != null)
                this.jsnURL.setJSONCallbackFunction -= UrlCallBack;
        }

        private void UpdateSkinImage()
        {
            //System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            Texture2D temp = null;
            if (MaterialSlot == MatSlotEnum.DecalTex)
            {
                temp = SelfNormRenderPanel.GetGPUTexture(MatSlotEnum.MainTex, TextureSlot);
            }
            else if (MaterialSlot == MatSlotEnum.BumpMap)
            {   //convert any packed normals to rgb format
                temp = SelfNormRenderPanel.GetGPUTexture(MaterialSlot, TextureSlot);

                if (temp == null)
                    temp = SelfNormRenderPanel.BlankNormalTex;

                temp = SelfNormRenderPanel.ConvertNormal(temp);
            }
            else
            {
                temp = SelfNormRenderPanel.GetGPUTexture(MaterialSlot, TextureSlot);
            }
            ImagePanel.ApplyTexture(temp);
            // watch.Stop();
            //SuperController.LogError(MaterialSlot + " " +TextureSlot + " Skin panel took " + watch.ElapsedMilliseconds);
        }

        public ManagerPanel(Decal_Maker DM = null, Color? color = null, string TextureSlot = null, string MaterialSlot = null, bool IsNormalMap = false, bool linear = false) : base(DM, 300)
        {
            base.MaterialSlot = MaterialSlot;
            base.TextureSlot = TextureSlot;
            base.IsNormalMap = IsNormalMap;
            base.linear = linear;
            DM.GetJSONDelegate += SavePanelToJSON;

            PanelName = MaterialSlot + " " + TextureSlot;

            ImagePanel = new ImagePanelBase(DM, TextureSlot, MaterialSlot, IsNormalMap, linear);

            TextPanel textPanel = new TextPanel(gameObject, new Vector3(0, -35, 0), "Region Text")
            {
                text = TextureSlot.ToUpper() ?? "",
                alignment = TextAnchor.LowerCenter,
                color = new Color(0.196078435f, 0.196078435f, 0.196078435f, 1f),
                fontStyle = FontStyle.Bold,
                fontSize = 30,
                anchorMin = Vector2.zero,
                anchorMax = Vector2.one,
                sizeDelta = Vector2.zero,
            };

            SetLayout(1050, spacerLeft.height);

            RemoveDecalPanel();
            ActivateWindow(false);


            switch (MaterialSlot)
            {
                case MatSlotEnum.DecalTex:
                    renderPanel = new RenderPanelDecal(DM, MaterialSlot, TextureSlot);
                    break;

                case MatSlotEnum.BumpMap:
                    renderPanel = new RenderPanelNormal(DM, MaterialSlot, TextureSlot);
                    break;

                case MatSlotEnum.SpecTex:
                case MatSlotEnum.GlossTex:
                    renderPanel = new RenderPanelSpecGloss(DM, MaterialSlot, TextureSlot);
                    break;
            }
            renderPanel.DecalPanels = DecalPanels;

            //register two listner functions one for general and one to monitor child panels
            RegisterForCoreEvents(DecalPanelEvent);
            RegisterForCoreEvents(CoreEvent);

            SelfNormRenderPanel = new RenderPanelNormal(base.DM, base.MaterialSlot, base.TextureSlot);
            //reuse button from image panel
            Button.onClick.AddListener(
                () =>
                {
                    RaiseCoreEvent(this, new PanelEventArgs(EventEnum.ManagerPanelSelection, this));
                }
            );
        }

        protected override void SetLayout(float width, float height)
        {
            GridLayoutGroup glg = spacerLeft.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(width, height);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 1;
            glg.spacing = new Vector2(0, 0);
        }

        private DecalPanel RestorePanelFromJSON(JSONClass saveJSON, bool IsTemp = false)
        {
            string RandomName;
            if (saveJSON["RandomName"] != null)
            {
                RandomName = saveJSON["RandomName"];
                if (DecalPanels.Any(x => x.RandomName == RandomName))
                    RandomName = GeneratePanelName();
            }
            else
            {
                RandomName = GeneratePanelName();
            }

            DecalPanel d = AddDecalPanels(IsTemp, RandomName);
            if (saveJSON["Alpha"] != null)
            {
                d.sliderDynamic.slider.value = (float.Parse(saveJSON["Alpha"]));
            }
            if (saveJSON["H"] != null)
            {
                float h = float.Parse(saveJSON["H"]);
                float s = float.Parse(saveJSON["S"]);
                float v = float.Parse(saveJSON["V"]);
                d.jcolor.SetVal(h, s, v);
            }
            if (saveJSON["Path"] != null)
            {
                d.RestoreImageFromJSON(saveJSON["Path"]);
            }
            if (saveJSON["LinkID"] != null)
            {
                d.linkedPanelID = saveJSON["LinkID"];
            }

            d.gameObject.transform.localPosition += new Vector3(270, 0, 0);
            renderPanel.IsDirty = true;

            return d;
        }

        private JSONArray SavePanelToJSON(string MaterialSlot, string TextureSlot)
        {
            if (MaterialSlot == this.MaterialSlot && TextureSlot == this.TextureSlot)
            {
                JSONArray Panel = new JSONArray();
                foreach (DecalPanel d in DecalPanels)
                {
                    JSONClass tempDecal = d.Save();
                    Panel.Add("", tempDecal);
                }
                return Panel;
            }

            return null;
        }

        public DecalPanel AddDecalPanels(bool temp = false, string RandomName = null)
        {
            if (RandomName == null)
                RandomName = GeneratePanelName();

            DecalPanel d = new DecalPanel(spacerLeft.transform, 1050, 300, UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), DM, RandomName, MaterialSlot, TextureSlot, IsNormalMap, linear);
            d.gameObject.transform.localPosition += new Vector3(270, 0, 0);
            IncreaseDecalPanel(d, temp);
            renderPanel.IsDirty = true;

            return d;
        }

        private void IncreaseDecalPanel(DecalPanel d, bool temp = false)
        {
            DecalPanels.Add(d);

            ResizeSpacers(300);

            if (temp)
            {
                TempDecalPanels.Add(d);
            }
        }

        //unique ID to use for jsonparam
        private string GeneratePanelName()
        {
            DateTime date = DateTime.Now;
            int y = date.Year - 2000;
            int m = date.Month;
            int d = date.Day;
            string randombit = CreateString(3);

            string newName = string.Format("{0}{1}{2}{3}", y, m, d, randombit);
            //regenerate until unique or 100 tries
            if (retries > 100)
            {
                return "FAIL";
            }
            else if (DecalPanels.Any(x => x.RandomName == newName))
            {
                retries++;
                return GeneratePanelName();
            }

            return newName;
        }

        //https://stackoverflow.com/a/4616745 
        internal static string CreateString(int stringLength)
        {
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }


        private void RemoveDecalPanel()
        {
            if (DecalPanels.Any())
            {
                //get last entry and remove
                DecalPanel d = DecalPanels[DecalPanels.Count - 1];
                RemoveDecalPanel(d);
            }
        }

        public bool RemoveDecalPanel(DecalPanel d)
        {
            bool removed = DecalPanels.Remove(d);
            if (removed)
            {
                d.OnDestroy();
                GameObject.Destroy(d.gameObject);
                ResizeSpacers(-300);
                renderPanel.IsDirty = true;
            }

            return true;
        }

        public IEnumerator RemoveTempDecalPanels()
        {
            foreach (DecalPanel d in TempDecalPanels.ToList())
            {
                yield return new WaitUntil(() => RemoveDecalPanel(d));
                RemoveDecalPanel(d);
            }
        }

        public void ResetTempDecalPanelTable()
        {
            TempDecalPanels = new List<DecalPanel>();
        }

        private void RepositionDecalPanel(DecalPanel d, int index)
        {
            d.gameObject.transform.SetSiblingIndex(index);
            DecalPanels.Remove(d);
            DecalPanels.Insert(index, d);
        }

        private void ResizeSpacers(float value)
        {
            spacerLeft.height += value;
            spacerRight.height += value;
        }

        public void ActivateWindow(bool value)
        {
            spacerLeft.gameObject.SetActive(value);
            spacerRight.gameObject.SetActive(value);

        }

    }


}

