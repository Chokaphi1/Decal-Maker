using MeshVR;
using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VAM_Decal_Maker.PathHelper;

namespace VAM_Decal_Maker
{
    public class Decal_Maker : MVRScript
    {
        private const string pluginName = "DecalMaker";
        private const string pluginVersion = "RC 3";

        //person script is attatched too
        private Atom _parentAtom; //VAM parent
        private bool _SetupFinished = false;
        private bool _ShadersLoaded = false;
        private bool _SetupError = false;
        private bool _processingCharacterChange = false;
        private Texture[] gpuDecalArray;
        private Texture[] gpuSpecArray;
        private Texture[] gpuGlossArray;
        private Texture[] gpuDiffuseArray;
        private Texture[] gpuNormalArray;
        public DAZCharacter _dazCharacter { get; private set; }
        private DAZCharacterSelector _dazCharacterSelector;

        public Shader _customBulkDecalShader { get; private set; }
        public Shader _customSpecGlossShader { get; private set; }
        public Shader _customNormShader { get; private set; }
        public Shader _customPackedNormShader { get; private set; }
        public Shader _customUINormalMapShader { get; private set; }
        public Shader _customUISpecularMapShader { get; private set; }
        public Shader _customUIGenitalMakerShader { get; private set; }
        public JSONClass _savedData { get; set; }
        public static Dictionary<string, Texture2D> resourceTextures = new Dictionary<string, Texture2D>();

        public DAZMergedSkinV2 _dazSkin { get; private set; }
        //Events
        public Action OnUpDateAction;
        public Action OnDestroyAction;
        //A public event for listeners to subscribe to
        //Note that by using the generic EventHandler<T> event type
        //we do not need to declare a separate delegate type.
        public event EventHandler<PanelEventArgs> CoreEvent;
        //delegate to get saveJSON from DecalManager
        public delegate JSONArray GetJSONSaveDelegate(string MaterialSlot, string TextureSlot);
        public GetJSONSaveDelegate GetJSONDelegate;


        public bool _isMale;
        public string _uvSetName;

        //UI
        public JSONStorableBool _toggleNippleCutout;
        public JSONStorableBool _toggleGenitalCutout;
        private UIDynamicButton _resetButton;
        private UIDynamicButton _saveButton;
        private UIDynamicButton _clearAllButton;
        private UIDynamicButton _prefabButton;
        private UIDynamicButton _savePresetButton;
        private UIDynamicButton _loadPresetButton;
        private UIDynamicToggle _toggleNipple;
        private UIDynamicToggle _toggleGenital;
        private HeaderPanel _headerPanel;
        //GenitalMaker _genitalMakerPanel;


        private readonly bool _Debug = false;

        public void ToggleGensCallBack(bool value)
        {
            OnCoreChange(this, new PanelEventArgs(EventEnum.ToggleGenitalCutout, value));
        }
        public void ToggleNipsCallBack(bool value)
        {
            OnCoreChange(this, new PanelEventArgs(EventEnum.ToggleNippleCutout, value));
        }

        public override void Init()
        {
            try
            {
                if (containingAtom.mainController == null)
                {
                    SuperController.LogError("Please add this plugin to a PERSON atom.");
                    return;
                }
                _parentAtom = containingAtom;
                _parentAtom.mainController.interactableInPlayMode = true;


                //setup all values before finishing UI
                StartCoroutine(CharacterChanged());
                //use vam's built in loader for assets then load shader from bundle instead of smuggling them in as a unity Atom
                AssetLoader.AssetBundleFromFileRequest req = new AssetLoader.AssetBundleFromFileRequest()
                {
                    path = "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/chokpahi-decal.assetbundle",
                    callback = LoadAssetBundle,
                };
                AssetLoader.QueueLoadAssetBundleFromFile(req);

                _clearAllButton = CreateButton("Clear Everything");
                _prefabButton = CreateButton("USE Prefab", true);

                PrefabPanel prefabPanel = new PrefabPanel(this);

                _resetButton = CreateButton("Reset Texture");
                _saveButton = CreateButton("This Space 4 Rent", true);

                _loadPresetButton = CreateButton("Load Preset");
                _savePresetButton = CreateButton("Save Preset", true);

                _toggleNippleCutout = new JSONStorableBool("Nipple Cutouts ON", true, new JSONStorableBool.SetBoolCallback(ToggleNipsCallBack));
                _toggleNipple = CreateToggle(_toggleNippleCutout);
                _toggleNippleCutout.storeType = JSONStorableParam.StoreType.Full;
                RegisterBool(_toggleNippleCutout);


                _toggleGenitalCutout = new JSONStorableBool("Genital Cutouts ON", true, new JSONStorableBool.SetBoolCallback(ToggleGensCallBack));
                _toggleGenital = CreateToggle(_toggleGenitalCutout, true);
                RegisterBool(_toggleGenitalCutout);


                RegisterBool(_toggleNippleCutout);
                RegisterBool(_toggleGenitalCutout);
                GetBoolJSONParam("Nipple Cutouts ON");

                JSONStorableUrl lastImagePath = new JSONStorableUrl("LastImageDir", "Custom/Atom/Person/Textures");
                RegisterUrl(lastImagePath);
                lastImagePath.isStorable = false;
                //assign listners
                _clearAllButton.button.onClick.AddListener(ResetAll);

                _prefabButton.button.onClick.AddListener(prefabPanel.OnClick);

                string presetsPath = "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Presets";
                if (!FileManagerSecure.DirectoryExists(presetsPath))
                {
                    FileManagerSecure.CreateDirectory(presetsPath);
                }
                if (!FileManagerSecure.DirectoryExists(presetsPath + "/PreFabs"))
                {
                    FileManagerSecure.CreateDirectory(presetsPath + "/PreFabs");
                }

                _savePresetButton.button.onClick.AddListener(() =>
                {
                    SuperController.singleton.fileBrowserUI.defaultPath = presetsPath;
                    SuperController.singleton.fileBrowserUI.showFiles = true;
                    SuperController.singleton.fileBrowserUI.showDirs = true;
                    SuperController.singleton.fileBrowserUI.SetTextEntry(true);
                    SuperController.singleton.fileBrowserUI.hideExtension = true;
                    SuperController.singleton.fileBrowserUI.shortCuts = new List<ShortCut>();
                    SuperController.singleton.fileBrowserUI.Show(SavePresetCallback);
                });

                _loadPresetButton.button.onClick.AddListener(() =>
                {   //update for 1.21 to use the existing preset dialog system
                    List<ShortCut> shortCuts = FileManagerSecure.GetShortCutsForDirectory(presetsPath, false, true, true, false);
                    SuperController.singleton.GetMediaPathDialog(PresetLoadCallBack, "json", presetsPath, false, true, false, null, true, shortCuts, true, true);
                });

                //legacy storables some addon may use
                JSONStorableAction resetAllAction = new JSONStorableAction("ClearAll", ResetAll);
                RegisterAction(resetAllAction);
                JSONStorableAction performLoadAction = new JSONStorableAction("PerformLoad", PerformLoad);
                RegisterAction(performLoadAction);
                JSONStorableAction resetTexturesAction = new JSONStorableAction("Reset To Original Textures", ResetOriginalGPUTexturesAction);
                RegisterAction(resetTexturesAction);

            }
            catch (Exception e)
            {
                SuperController.LogError("Decal Maker: Error during Init" + e);
            }
        }

        public Texture2D GetResource(string path, bool linear = false)
        {
            string cachepath = path + linear.ToString();
            Texture2D texture;
            if (resourceTextures.TryGetValue(cachepath, out texture))
            {
                return texture;
            }

            //not in dictionary
            byte[] tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(this) + path);

            texture = new Texture2D(1, 1, TextureFormat.RGBA32, linear);
            texture.LoadImage(tmppng);

            resourceTextures.Add(cachepath, texture);
            return texture;
        }

        private void LogError(string err)
        {
            if (_Debug)
            {
                Debug.LogError(err);
            }
        }

        public void DebugSave(Texture2D tempTex, string name)
        {
            //string path = "Custom/PluginData/Chokaphi/VAM_Decal_Maker/" + name;
            //byte[] bytes = tempTex.EncodeToPNG();

            //FileManagerSecure.WriteAllBytes(path, bytes);
        }
        private void LoadAssetBundle(MeshVR.AssetLoader.AssetBundleFromFileRequest request)
        {
            if (request.assetBundle == null)
            {
                SuperController.LogError("Decal Maker was unable to load chokpahi-decal.assetbundle");
                return;
            }

            _customSpecGlossShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/unlit-specular2.shader");
            _customNormShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/unlit-normalmap2.shader");
            _customPackedNormShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/unlit-packednormalmap.shader");
            _customBulkDecalShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/unlit-decal4.shader");
            _customUINormalMapShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/custom-ui-normalmapoverlay.shader");
            _customUIGenitalMakerShader = request.assetBundle.LoadAsset<Shader>("assets/_shaders/unlit-genitalmaker2.shader");

            if (_customSpecGlossShader && _customNormShader && _customPackedNormShader && _customBulkDecalShader && _customUINormalMapShader && _customUIGenitalMakerShader)
            {
                _ShadersLoaded = true;
            }
        }

        // Update is called with each rendered frame by Unity
        private void Update()
        {
            try
            {
                if (_SetupError)
                    return;

                //Fire UpdateEvents to subscribers
                if (_SetupFinished == true && OnUpDateAction != null)
                {
                    OnUpDateAction();
                }

                if (_dazCharacter != null && !_processingCharacterChange)
                {
                    if (_dazCharacter != _dazCharacterSelector.selectedCharacter)
                    {
                        _processingCharacterChange = true;
                        LogError("StartCoroutine(CharacterChanged()");
                        StartCoroutine(CharacterChanged());
                    }
                }

                if (_ShadersLoaded == false)
                {
                    LogError("On update call waiting on shaders to load");
                }

                if (_SetupFinished == false && _ShadersLoaded == true)
                {
                    LogError("On update call waiting on SetUp");
                    Setup();
                    LogError("setup should be finished " + _SetupFinished);
                }

            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e);
            }
        }

        public void OnCoreChange(object o, PanelEventArgs e)
        {
            //Fire the event - notifying all subscribers
            CoreEvent?.Invoke(o, e);
        }

        //called on reload or removal of plugin
        public void OnDestroy()
        {
            ResetOriginalGPUTextures(_dazCharacter);
            AssetLoader.DoneWithAssetBundleFromFile("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/chokpahi-decal.assetbundle");
            OnDestroyAction?.Invoke();
        }

        private void Setup()
        {
            try
            {
                //create Header Panel
                _headerPanel = new HeaderPanel(this);

                _resetButton.button.onClick.AddListener(() =>
                {
                    ResetOriginalGPUTextures(_dazCharacter);
                });

                //UIDynamicButton NewPanelButton = CreateButton(" LAST LEFT ELEMENT");

                //Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
                //int step = 0;
                //Dictionary<string, Material> stockMats = new Dictionary<string, Material>();
                //NewPanelButton.button.onClick.AddListener(() =>
                //{
                //    string[] matNames =  _dazSkin.dazMesh.materialNames;
                //    //stock shader Custom/Subsurface/GlossNMTessMappedFixedComputeBuff

                //    SuperController.LogError("------------------------------------------------------------");
                //    if (shaders == null)
                //        shaders = Resources.FindObjectsOfTypeAll<Shader>();

                //    Shader s = shaders[step];

                //    for (int i = 0; i < matNames.Length; i++)
                //    {
                //        string m = matNames[i];
                //        if (m == "Pupils" || m == "Cornea")
                //        {

                //            Material n = new Material(s);
                //            n.CopyPropertiesFromMaterial(stockMats[m]);
                //            _dazSkin.GPUmaterials[i] = n;
                //        }
                //    }

                //    SuperController.LogError(DateTime.Now +" Shader changed to " + step + " " + shaders[step].name + " " + _dazSkin.GPUmaterials[1].HasProperty("_DecalTex"));
                //    step++;

                //    //SuperController.LogError(count +  $"  {m} " +
                //    //    " Shader= "+ _dazSkin.GPUmaterials[count].shader.name + " " +
                //    //    _dazSkin.GPUmaterials[count].HasProperty("_DecalTex")); 
                //    //;  
                //    //}
                //    //@"Custom/Subsurface/GlossNMCullComputeBuff";





                //});

                //UIDynamicButton NewPanelButtonBack = CreateButton(" Back");


                //NewPanelButtonBack.button.onClick.AddListener(() =>
                //{
                //    string[] matNames = _dazSkin.dazMesh.materialNames;
                //    //stock shader Custom/Subsurface/GlossNMTessMappedFixedComputeBuff

                //    SuperController.LogError("------------------------------------------------------------");
                //    if (shaders == null)
                //        shaders = Resources.FindObjectsOfTypeAll<Shader>();

                //    Shader s = shaders[step];

                //    for (int i = 0; i < matNames.Length; i++)
                //    {
                //        string m = matNames[i];
                //        if (m == "Pupils" || m == "Cornea")
                //        {

                //            Material n = new Material(s);
                //            n.CopyPropertiesFromMaterial(stockMats[m]);
                //            _dazSkin.GPUmaterials[i] = n;
                //        }
                //    }

                //    SuperController.LogError(DateTime.Now + " Shader changed to " + step + " " + shaders[step].name + " " + _dazSkin.GPUmaterials[1].HasProperty("_DecalTex"));
                //    step--;


                //    //SuperController.LogError(count +  $"  {m} " +
                //    //    " Shader= "+ _dazSkin.GPUmaterials[count].shader.name + " " +
                //    //    _dazSkin.GPUmaterials[count].HasProperty("_DecalTex")); 
                //    //;  
                //    //}
                //    //@"Custom/Subsurface/GlossNMCullComputeBuff";





                //});
                ////CORNEA SHADER Custom/Subsurface/TransparentGlossNMSeparateAlphaComputeBuff
                ////Custom/Subsurface/GlossCullComputeBuff False
                ////GlossNMDetailCullComputeBuff False
                ////Custom/Subsurface/TransparentGlossNMNoCullSeparateAlphaComputeBuff False

                //UIDynamicButton UnusedButton2 = CreateButton("LAST RIGHT ELEMENT", true);
                //UnusedButton2.button.onClick.AddListener(() =>
                //{
                //    //_parentAtom.tempDisableRender = !_parentAtom.tempDisableRender;
                //    //SuperController.singleton.SyncHiddenAtoms();

                //    string[] matNames = _dazSkin.dazMesh.materialNames;
                //    //stock shader Custom/Subsurface/GlossNMTessMappedFixedComputeBuff

                //    for (int i = 0; i < matNames.Length; i++)
                //    {
                //        string m = matNames[i];
                //        if (m == "Pupils" || m == "Cornea")
                //        {
                //            stockMats.Add(m, _dazSkin.GPUmaterials[i]);
                //            SuperController.LogError("Mat name " + _dazSkin.GPUmaterials[i].name + " " + _dazSkin.GPUmaterials[i].mainTexture.name);
                //        }
                //    }

                //});

                //this is hacky but sets the UI to look nice on start.
                _prefabButton.button.onClick.Invoke();

                _SetupFinished = true;

                OnCoreChange(this, new PanelEventArgs(EventEnum.CoreSetupFinished));
            }
            catch (Exception e)
            {
                SuperController.LogError("Exception caught: " + e.Message);
                SuperController.LogError(e.StackTrace);
                _SetupError = true;
            }
        }

        private void StoreGPUMats()
        {
            gpuDecalArray = new Texture[_dazSkin.GPUmaterials.Length];
            gpuSpecArray = new Texture[_dazSkin.GPUmaterials.Length];
            gpuGlossArray = new Texture[_dazSkin.GPUmaterials.Length];
            gpuDiffuseArray = new Texture[_dazSkin.GPUmaterials.Length];
            gpuNormalArray = new Texture[_dazSkin.GPUmaterials.Length];


            for (int i = 0; i < _dazSkin.GPUmaterials.Length; i++)
            {
                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.MainTex))
                    gpuDiffuseArray[i] = _dazSkin.GPUmaterials[i].GetTexture(MatSlotEnum.MainTex);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.DecalTex))
                    gpuDecalArray[i] = _dazSkin.GPUmaterials[i].GetTexture(MatSlotEnum.DecalTex);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.SpecTex))
                    gpuSpecArray[i] = _dazSkin.GPUmaterials[i].GetTexture(MatSlotEnum.SpecTex);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.GlossTex))
                    gpuGlossArray[i] = _dazSkin.GPUmaterials[i].GetTexture(MatSlotEnum.GlossTex);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.BumpMap))
                    gpuNormalArray[i] = _dazSkin.GPUmaterials[i].GetTexture(MatSlotEnum.BumpMap);
            }

        }
        private void RestoreGPUMats()
        {   //why not use .GPUmaterials[i] = DazSkinv2.dazMesh.materials[i];

            for (int i = 0; i < _dazSkin.GPUmaterials.Length; i++)
            {
                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.MainTex))
                    _dazSkin.GPUmaterials[i].SetTexture(MatSlotEnum.MainTex, gpuDiffuseArray[i]);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.DecalTex))
                    _dazSkin.GPUmaterials[i].SetTexture(MatSlotEnum.DecalTex, gpuDecalArray[i]);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.SpecTex))
                    _dazSkin.GPUmaterials[i].SetTexture(MatSlotEnum.SpecTex, gpuSpecArray[i]);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.GlossTex))
                    _dazSkin.GPUmaterials[i].SetTexture(MatSlotEnum.GlossTex, gpuGlossArray[i]);

                if (_dazSkin.GPUmaterials[i].HasProperty(MatSlotEnum.BumpMap))
                    _dazSkin.GPUmaterials[i].SetTexture(MatSlotEnum.BumpMap, gpuNormalArray[i]);
            }

        }

        public Texture2D GetOriginalGPUTexture(int id, string MaterialSlot)
        {
            switch (MaterialSlot)
            {
                case MatSlotEnum.DecalTex:
                    return (Texture2D)gpuDiffuseArray[id];

                case MatSlotEnum.SpecTex:
                    return (Texture2D)gpuSpecArray[id];

                case MatSlotEnum.GlossTex:
                    return (Texture2D)gpuGlossArray[id];

                case MatSlotEnum.BumpMap:
                    return (Texture2D)gpuNormalArray[id];

            }
            return null;
        }

        public void RestoreGPUMatbyID(int id, string MaterialSlot)
        {
            switch (MaterialSlot)
            {
                case MatSlotEnum.DecalTex:
                    if (_dazSkin.GPUmaterials[id].HasProperty(MatSlotEnum.DecalTex))
                        _dazSkin.GPUmaterials[id].SetTexture(MaterialSlot, gpuDecalArray[id]);
                    break;
                case MatSlotEnum.SpecTex:
                    if (_dazSkin.GPUmaterials[id].HasProperty(MatSlotEnum.SpecTex))
                        _dazSkin.GPUmaterials[id].SetTexture(MaterialSlot, gpuSpecArray[id]);
                    break;
                case MatSlotEnum.GlossTex:
                    if (_dazSkin.GPUmaterials[id].HasProperty(MatSlotEnum.GlossTex))
                        _dazSkin.GPUmaterials[id].SetTexture(MaterialSlot, gpuGlossArray[id]);
                    break;
                case MatSlotEnum.BumpMap:
                    if (_dazSkin.GPUmaterials[id].HasProperty(MatSlotEnum.BumpMap))
                        _dazSkin.GPUmaterials[id].SetTexture(MaterialSlot, gpuNormalArray[id]);
                    break;
            }
        }
        public void QueueCharacterChanged()
        {
            if (_processingCharacterChange)
            {
                return;
            }
            _processingCharacterChange = true;
            StartCoroutine(CharacterUpdated(false));
        }

        private IEnumerator CharacterUpdated(bool newCharacter = false)
        {
            if (!newCharacter)
                yield return new WaitForSeconds(1);
            LogError("CharacterUpdated");

            //loadingIcon.gameObject stays true until all texture/cloathing load process is finished
            LogError("yielding till all textures load " + SuperController.singleton.loadingIcon.gameObject.activeSelf);
            yield return new WaitWhile(() => SuperController.singleton.loadingIcon.gameObject.activeSelf);
            LogError("textures load finished continue coroutine");

            //should store the current skin textures.
            LogError("STORE Current Skin Textures");
            StoreGPUMats();

            OnCoreChange(this, new PanelEventArgs(EventEnum.CoreNewCharacterSelected, newCharacter));
            _processingCharacterChange = false;
        }

        private IEnumerator CharacterChanged()
        {
            LogError("CharacterChanged");
            //used to know which character is being used or changed
            _dazCharacterSelector = _parentAtom.GetComponentInChildren<DAZCharacterSelector>();
            DAZCharacter previousDazCharacter = _dazCharacter;
            ResetOriginalGPUTextures(previousDazCharacter);
            _dazCharacter = _dazCharacterSelector.selectedCharacter;

            yield return new WaitUntil(() => _dazCharacter.ready);
            _dazSkin = (DAZMergedSkinV2)_dazCharacter.skin;
            _isMale = _dazCharacter.isMale;
            _uvSetName = _dazCharacter.UVname.Replace("UV: ", "");

            LogError("Waiting on UI");
            yield return new WaitUntil(() => _SetupFinished == true);
            LogError("UI should be ready");


            yield return StartCoroutine(CharacterUpdated(true));
            _processingCharacterChange = true;

            if (_savedData != null)
            {
                LogError("Restore save state");
                PerformLoad(_savedData);
                _savedData = null;
            }

            _processingCharacterChange = false;
        }

        //called on character change or UI
        private void ResetOriginalGPUTexturesAction()
        {
            ResetOriginalGPUTextures(_dazCharacter);
        }

        private void ResetOriginalGPUTextures(DAZCharacter dAZCharacter)
        {
            try
            {
                if (dAZCharacter == null || _dazSkin == null)
                    return;

                RestoreGPUMats();
            }
            catch (Exception e)
            {
                SuperController.LogError("Decal Maker: Error resetting custom textures " + e);
            }
        }

        private void ResetAll()
        {
            ResetOriginalGPUTextures(_dazCharacter);
            OnCoreChange(this, new PanelEventArgs(EventEnum.CoreResetAll));
        }

        private void PresetLoadCallBack(string path)
        {
            PresetLoad(path);
        }
        #region LOAD/SAVE
        public void PresetLoad(string path, bool temp = false, string package = null)
        {
            if (string.IsNullOrEmpty(path))
                return;

            JSONClass jc = this.LoadJSON(path) as JSONClass;
            PerformLoad(jc, temp, package, path);
        }

        private void SavePresetCallback(string path, bool autoScreenShot = false)
        {
            if (string.IsNullOrEmpty(path))
                return;

            JSONClass json = GetJSON();
            path = path.Replace(".dsgn.DecalMakerPreset", "");
            path = path.Replace(".DecalMakerPreset", "");
            path += ".DecalMakerPreset.json";
            this.SaveJSON(json, path);
            SuperController.singleton.DoSaveScreenshot(path);
            //if(autoScreenShot)
            //    SuperController.singleton.SetLeftSelect();
            //SuperController.singleton.fileBrowserUI.ClearCacheImage(path);
        }

        //SAVE

        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
        {
            JSONClass jc = base.GetJSON(includePhysical, includeAppearance, true);
            jc["SaveVersion"] = "2";

            foreach (string matSlot in MatSlotEnum.Values)
            {
                foreach (string region in BodyRegionEnum.Values)
                {
                    //each manager has subscribed to this delegate. Each will only respond when they match the passed arguments
                    //since only the last subscriber gets val return normally. Use a custom invoke to record each 
                    var subs = GetJSONDelegate.GetInvocationList();
                    foreach (var sub in subs)
                    {
                        JSONArray dc = (JSONArray)sub.DynamicInvoke(matSlot, region);
                        if (dc != null)
                        {
                            jc[matSlot][region] = dc;
                        }
                    }
                }
            }

            jc["Nipple Cutouts ON"] = _toggleNippleCutout.val.ToString();
            jc["Genital Cutouts ON"] = _toggleGenitalCutout.val.ToString();

            return jc;
        }

        //LOAD 
        public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
        {
            base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms);
            //store and call once we have everything loaded. RestorefromJSON is too early
            _savedData = jc;
        }
        //used for JSONStorableAction
        public void PerformLoad()
        {
            PerformLoad(_savedData);
        }

        private void PerformLoad(JSONClass jc, bool IsTemp = false, string package = null, string selectedFile = null)
        {
            if (jc == null)
                return;

            //convert to new file version if save is old
            try
            {
                if (jc["SaveVersion"] == null)
                    jc = ConvertSaveToV1(jc);

                if (jc["SaveVersion"].AsInt == 1)
                    jc = ConvertSaveToV2(jc);
            }
            catch
            {
                SuperController.LogError("Decal Maker: Unable to convert save");
                return;
            }
            // Load Save Data
            try
            {
                _toggleNippleCutout.RestoreFromJSON(jc);
                _toggleGenitalCutout.RestoreFromJSON(jc);

                foreach (string matSlot in MatSlotEnum.Values)
                {
                    foreach (string region in BodyRegionEnum.Values)
                    {
                        if (jc[matSlot] != null && jc[matSlot][region] != null)
                        {
                            JSONArray array = jc[matSlot][region] as JSONArray;
                            foreach (JSONClass saveJSON in array)
                            {
                                OnCoreChange(this, new PanelEventArgs(EventEnum.CoreRestoreFromJSON, saveJSON) { materialSlot = matSlot, bodyRegion = region, Bool = IsTemp });
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                SuperController.LogError("Decal Maker: Unable to Load save");
                SuperController.LogError(e.Message);
                SuperController.LogError(e.StackTrace);
                return;
            }
        }
        #endregion
        #region Upgrade Save Version
        //upgrades old saves to new format
        private JSONClass ConvertSaveToV1(JSONClass jc)
        {
            List<string> keys = new List<string>() { BodyRegionEnum.Torso, BodyRegionEnum.Genitals, BodyRegionEnum.Face, BodyRegionEnum.Limbs };
            JSONClass dc = new JSONClass();
            foreach (string key in keys)
            {
                if (jc[key] != null)
                {
                    dc.Add(key, jc[key]);
                }
                jc.Remove(key);
            }
            jc["Decal"] = dc;

            return jc;
        }
        private JSONClass ConvertSaveToV2(JSONClass jc)
        {
            try
            {
                //old keys
                Dictionary<string, string> conversion = new Dictionary<string, string>() { { "Decal", MatSlotEnum.DecalTex }, { "Specular", MatSlotEnum.SpecTex }, { "Gloss", MatSlotEnum.GlossTex }, { "Normal", MatSlotEnum.BumpMap } };

                //SuperController.singleton.SaveJSON(jc, "OUT 1.txt");
                foreach (var keyValue in conversion)
                {
                    JSONClass n = new JSONClass();
                    if (jc[keyValue.Key] != null)
                    {
                        n.Add(keyValue.Value, jc[keyValue.Key]);
                        jc.Remove(keyValue.Key);
                        jc.Add(keyValue.Value, n[keyValue.Value]);
                    }
                }
                jc["SaveVersion"].AsInt = 2;
                return jc;
            }
            catch (Exception e)
            {
                SuperController.LogError("Convert to save version 2 error " + e.Message);
                return null;
            }

        }

    }

    #endregion

}

