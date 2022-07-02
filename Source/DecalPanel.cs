using MVR.FileManagementSecure;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static VAM_Decal_Maker.PathHelper;

namespace VAM_Decal_Maker
{
    public class DecalPanel : PanelBase
    {
        public ImagePanel ImagePanel { get; set; }
        public MyButton Up { get; private set; }
        public MyButton Down { get; private set; }
        public MyButton Open { get; private set; }
        public MyButton Close { get; private set; }
        public Color Color { get { return ImagePanel.color; } }
        public JSONStorableColor jcolor { get; private set; }
        public UIDynamicSlider sliderDynamic { get; private set; }
        public Action<string> LastDir;
        public event EventHandler<PanelEventArgs> DecalPanelUpdate;
        public string ImagePathText { get { return texturePathTextPanel.text; } set { texturePathTextPanel.text = value; } }

        private string _randomName;
        public string RandomName
        {
            get
            {
                if (_randomName == null)
                {
                    return string.Empty;
                }
                return _randomName;
            }
            set
            {
                if (randonNameTextPanel != null)
                    randonNameTextPanel.text = value;
                _randomName = value;
            }
        }

        private TextPanel texturePathTextPanel;
        private TextPanel randonNameTextPanel;
        private Image panelImage;
        private JSONStorableFloat sliderJSF;
        private JSONStorableAction videoPlayPauseJSA;
        private JSONStorableFloat videoFrameJSF;
        private JSONStorableFloat videoTimeJSF;



        private float width;
        private float height;

        //event Handler for this class
        private void OnPanelChange(PanelEventArgs e)
        {
            if (DecalPanelUpdate != null)
            {
                DecalPanelUpdate(this, e);
            }
           
        }

        //events from image panel
        public void ImagePanelEventProcessor(object o, PanelEventArgs e)
        {
            switch (e.EventName)
            {
                case EventEnum.ImagePanelVidePrepared:
                    VideoLoaded(e.videoPlayer);
                    break;
                case EventEnum.ImagePanelImageLoad:
                    //Deregister video controls
                    if (DM.GetAction(videoPlayPauseJSA.name) != null)
                    {
                        DM.DeregisterAction(videoPlayPauseJSA);
                        DM.DeregisterFloat(videoFrameJSF);
                        DM.DeregisterFloat(videoTimeJSF);
                    }
                    
                    break;
                case EventEnum.ImagePanelVideoLoad:
                    //register video controls
                    if (DM.GetAction(videoPlayPauseJSA.name) == null)
                    {
                        DM.RegisterAction(videoPlayPauseJSA);
                        DM.RegisterFloat(videoFrameJSF);
                        DM.RegisterFloat(videoTimeJSF);
                    }
                    break;
            }
            if (e.EventName == EventEnum.ImagePanelVidePrepared)
            {
                
            }
            
            //add reference to this panel and pass on. Manager Panel handles operations
            e.DecalPanel = this;
            OnPanelChange(e);
        }

        public override void OnDestroy()
        {
            //ImagePanel.OnDestroy();
            DM.DeregisterFloat(sliderJSF);
            DM.DeregisterColor(jcolor);
            if (videoPlayPauseJSA != null)
            {
                DM.DeregisterAction(videoPlayPauseJSA);
                DM.DeregisterFloat(videoFrameJSF);
                DM.DeregisterFloat(videoTimeJSF);
            }
            base.OnDestroy();
        }

        public DecalPanel(Transform anchor, float width, float height, Color? color, Decal_Maker DM, string RandomName, string MaterialSlot, string TextureSlot, bool IsNormalMap = false, bool linear = false) : base(DM)
        {
            gameObject.transform.SetParent(anchor, false);

            this.MaterialSlot = MaterialSlot;
            this.TextureSlot = TextureSlot;
            this.IsNormalMap = IsNormalMap;
            this.linear = linear;
            this.width = width;
            this.height = height;
            this.RandomName = RandomName;
            //basePanel
            panelImage = gameObject.AddComponent<Image>();
            panelImage.material = new Material(Shader.Find("UI/Default-Overlay"));
            panelImage.color = color ?? new Color(0, 0, 0, 0f);
            panelImage.rectTransform.sizeDelta = new Vector2(width, height);

            //Image panel
            GameObject imageObject = new GameObject("ImagePanel");
            imageObject.transform.SetParent(gameObject.transform, false);

            ImagePanel = new ImagePanel(DM, null, TextureSlot, MaterialSlot, IsNormalMap, linear);
            ImagePanel.gameObject.transform.SetParent(gameObject.transform, false);
            ImagePanel.gameObject.transform.localPosition = new Vector3(panelImage.rectTransform.rect.xMax - ImagePanel.rectTransform.rect.xMax - 10, 0, 0);
            //register for event
            ImagePanel.ImagePanelUpdate += ImagePanelEventProcessor;

            //create name and slider
            //must be before text and color selector so it layers properly
            AddSlider();

            GameObject colorObject = new GameObject("ColorPanel");
            colorObject.transform.SetParent(gameObject.transform, false);
            colorObject.transform.localScale = new Vector3(.55f, .55f, .55f);
            colorObject.transform.localPosition = new Vector2(-350, 50);

            Image colorImage = colorObject.AddComponent<Image>();
            colorImage.material = new Material(Shader.Find("UI/Default-Overlay"));
            colorImage.color = new Color(0, 1, 1, 0f);
            colorImage.rectTransform.sizeDelta = new Vector2(600, 300);
            colorImage.transform.localPosition = new Vector2(-350, 50);

            Transform colorPickerPrefab = GameObject.Instantiate<Transform>(DM.manager.configurableColorPickerPrefab);
            colorPickerPrefab.SetParent(colorImage.transform, false);
            colorPickerPrefab.localPosition = Vector3.zero;

            UIDynamicColorPicker colorPickerDynamic = colorPickerPrefab.GetComponent<UIDynamicColorPicker>();
            colorPickerDynamic.showLabel = false;
            HSVColor hsvc = HSVColorPicker.RGBToHSV(1f, 1f, 1f);
            jcolor = new JSONStorableColor(CreateJSN("Color"), hsvc, ImagePanel.SetColor);
            jcolor.isStorable = false;
            jcolor.colorPicker = colorPickerDynamic.colorPicker;
            DM.RegisterColor(jcolor);

            videoPlayPauseJSA = new JSONStorableAction(CreateJSN("Play-Pause"), VideoPlayPause);
            videoFrameJSF = new JSONStorableFloat(CreateJSN("Step to Frame"), 0, VideoPlaySeekFrame, 0, 0);
            videoTimeJSF = new JSONStorableFloat(CreateJSN("Step to Time"), 0, VideoPlaySeekTime, 0, 0);


            Up = new MyButton("", new Color(0.9f, 0.6f, 0.1f));
            Up.gameObject.transform.SetParent(gameObject.transform, false);
            Up.gameObject.transform.localPosition = new Vector2(-100, 50);

            RectTransform upRT = Up.gameObject.GetComponent<RectTransform>();
            upRT.sizeDelta = new Vector2(60, 60);

            byte[] tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(DM) + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Up-Icon.jpg");
            Texture2D UpIcon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            UpIcon.LoadImage(tmppng);
            Up.SetIcon(UpIcon);

            Down = new MyButton("", new Color(0.1f, 0.6f, 0.8f));
            Down.gameObject.transform.SetParent(gameObject.transform, false);
            Down.gameObject.transform.localPosition = Up.gameObject.transform.localPosition + new Vector3(65, 0, 0);

            RectTransform dwnRT = Down.gameObject.GetComponent<RectTransform>();
            dwnRT.sizeDelta = new Vector2(60, 60);

            tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(DM) + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Down-Icon.jpg");
            Texture2D DownIcon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            DownIcon.LoadImage(tmppng);
            Down.SetIcon(DownIcon);

            Close = new MyButton("", null, transform);
            //Close.transform.SetParent(transform, false);
            Close.transform.localPosition = Down.transform.localPosition + new Vector3(160, 0, 0);

            RectTransform clsRT = Close.gameObject.GetComponent<RectTransform>();
            clsRT.sizeDelta = new Vector2(60, 60);

            tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(DM) + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Close-Icon.jpg");
            Texture2D CloseIcon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            CloseIcon.LoadImage(tmppng);
            Close.SetIcon(CloseIcon);

            Open = new MyButton("", new Color(.7f, .7f, .7f));
            Open.gameObject.transform.SetParent(gameObject.transform, false);
            Open.gameObject.transform.localPosition = Up.gameObject.transform.localPosition + new Vector3(-65, 0, 0);

            RectTransform opnRT = Open.gameObject.GetComponent<RectTransform>();
            opnRT.sizeDelta = new Vector2(60, 60);

            tmppng = FileManagerSecure.ReadAllBytes(GetPackagePath(DM) + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Open-Icon.jpg");
            Texture2D OpenIcon = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            OpenIcon.LoadImage(tmppng);
            Open.SetIcon(OpenIcon);

            //Panel Name Text
            randonNameTextPanel = new TextPanel(gameObject, Vector3.zero, "")
            {
                text = RandomName,
                color = new Color(1 - panelImage.color.r, 1 - panelImage.color.g, 1 - panelImage.color.b),
                fontSize = 60,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                sizeDelta = new Vector2(width, height),
            };

            //Texture Path Text
            texturePathTextPanel = new TextPanel(gameObject, new Vector2(40, 40), "Path Panel")
            {
                alignment = TextAnchor.LowerLeft,
                color = Color.black,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                text = "Click White Square to select image.-->",
                blocksRaycasts = false,
                sizeDelta = new Vector2(width, height),
            };


            Up.button.onClick.AddListener(() =>
            {
                OnPanelChange(new PanelEventArgs(EventEnum.DecalPanelButtonUP, this));
            });

            Down.button.onClick.AddListener(() =>
            {
                OnPanelChange(new PanelEventArgs(EventEnum.DecalPanelButtonDOWN, this));
            });

            Close.button.onClick.AddListener(() =>
            {
                OnPanelChange(new PanelEventArgs(EventEnum.DecalPanelButtonCLOSE, this));
            });

            Open.button.onClick.AddListener(() =>
            {
                OnPanelChange(new PanelEventArgs(EventEnum.DecalPanelButtonAdd, this));
            });

        }

        public void VideoLoaded(VideoPlayer videoPlayer)
        {
            try
            {
                ulong frames = videoPlayer.frameCount;
                double time = frames / videoPlayer.frameRate;
                float seconds = (float)time;

                videoFrameJSF.max = frames;
                videoTimeJSF.max = seconds;

            }
            catch(Exception e)
            {
                SuperController.LogError( e.Message);
            }

        }

        private void VideoPlaySeekTime(float seconds)
        {
            ImagePanel.videoPlayer.time = seconds;
        }

        private void VideoPlaySeekFrame(float frame)
        {
            int f = Mathf.RoundToInt(frame);
            ImagePanel.videoPlayer.frame = f;
        }

        private void VideoPlayPause()
        { 
            if (ImagePanel.videoPlayer.isPlaying)
            {
                ImagePanel.videoPlayer.Pause();
            }
            else
            {
                ImagePanel.videoPlayer.Play();
            }
        }

        private string CreateJSN(string prefix = null)
        {
            if (string.IsNullOrEmpty(prefix))
                return string.Format("{0}{1}{2}", TextureSlot, MaterialSlot, RandomName);

            return string.Format("{0}_{1}{2}{3}", prefix, TextureSlot, MaterialSlot, RandomName);
        }

        private void AddSlider()
        {
            if (IsNormalMap)
            {
                sliderJSF = new JSONStorableFloat(CreateJSN(), 1, ImagePanel.SetNormalScale, -10, 10);
                CreateSlider(sliderJSF, -10, 10, 1, "Normal Strength");
            }
            else if (linear)
            {
                sliderJSF = new JSONStorableFloat(CreateJSN(), 1, ImagePanel.SetSpecularScale, -10, 10);
                CreateSlider(sliderJSF, -10, 10, 1, "Spec/Gloss Strength").gameObject.SetActive(false);
            }
            else
            {
                sliderJSF = new JSONStorableFloat(CreateJSN(), 1, ImagePanel.SetAlpha, 0, 1);
                CreateSlider(sliderJSF, 0, 1, 1, "Alpha value");
            }
            sliderJSF.isStorable = false;
            DM.RegisterFloat(sliderJSF);
        }

        private UIDynamicSlider CreateSlider(JSONStorableFloat action, int min = 0, int max = 1, int value = 1, string title = "")
        {
            Transform sliderPrefab = GameObject.Instantiate<Transform>(DM.manager.configurableSliderPrefab);
            sliderPrefab.SetParent(gameObject.transform, false);
            sliderPrefab.localPosition = Vector3.zero;

            sliderDynamic = sliderPrefab.GetComponent<UIDynamicSlider>();
            sliderDynamic.transform.localPosition = new Vector2(0, -80);
            sliderDynamic.quickButtonsEnabled = false;
            sliderDynamic.rangeAdjustEnabled = false;
            sliderDynamic.defaultButtonEnabled = false;


            Image sliderBackground = sliderDynamic.GetComponentsInChildren<Image>().Where(x => x.name == "Panel").First();
            Color c = sliderBackground.color;
            c.a = 0.1f;
            sliderBackground.color = c;

            action.slider = sliderDynamic.slider;
            //sliderDynamic.slider.onValueChanged.AddListener(action);

            sliderDynamic.slider.minValue = min;
            sliderDynamic.slider.maxValue = max;
            sliderDynamic.slider.value = value;
            sliderDynamic.label = title;
            sliderDynamic.labelText.transform.localPosition += new Vector3(400, 0, 0);
            RectTransform fill = sliderDynamic.slider.fillRect;
            fill.sizeDelta += new Vector2(0, 20);

            return sliderDynamic;
        }

        public void SetPathText(string text)
        {
            //count char back to first slash
            int lastIndex = text.LastIndexOf("/");
            if (text.Length > 55 && lastIndex >= 0)
            {
                string endText = text.Substring(lastIndex);
                int leftover = 55 - endText.Length + 3;
                string startText = text.Substring(0, leftover);

                text = startText + "..." + endText;
            }
            ImagePathText = text;
        }

        //Used to correct paths in JSON to available VAR files until VAM expands FileManagerSecure
        //this is a nasty fucking mess
        //path = image path package
        public void RestoreImageFromJSON(string path, string package = null, string selectedFile = null)
        {
            try
            {
                path = FileManagerSecure.NormalizePath(path);
                Match m;
                //VAR Path for textures with direct var ref
                if ((m = Regex.Match(path, "^(.+\\..+)\\.(.+):/(.+/)(.+)")).Success)
                {
                    //this is a packagedVAR reference use latest version
                    string varName = m.Groups[1].Value;
                    string filePath = m.Groups[3].Value;
                    string fileName = m.Groups[4].Value;

                    ShortCut sc = FileManagerSecure.GetShortCutsForDirectory(filePath).Where(x => x.package.Contains(varName) && x.isLatest).FirstOrDefault();
                    //if nothing found with this path
                    if (sc == null)
                    {
                        SuperController.LogError("Decal Maker: Texture not found: " + path);
                        return;
                    }


                    path = Regex.Replace(sc.path, "^.+/(.+\\..+\\..+)\\..+(:.+)", "$1$2/" + fileName); //removes AddonPackages/ path prefix and the .var from shortcut path

                }
                //if texture and presets are in same VAR  VAM changes all paths to SELF:
                //prefabs UI passes in a package ref
                else if ((m = Regex.Match(path, "^SELF:.+")).Success && package != null)
                {   //SELF:/Custom/Atom/Person/Textures/DecalMaker - mofmes power goo/Face/Cheeks/face04/D.png
                    path = Regex.Replace(path, "^(SELF)(:.+)", package + "$2"); //replace SELF:/ with source package
                }
                else if ((m = Regex.Match(path, "^SELF:.+")).Success && selectedFile != null)
                {
                    if ((m = Regex.Match(selectedFile, "^(.+\\..+\\..+):.+")).Success)
                    {
                        string varName = m.Groups[1].Value;
                        path = Regex.Replace(path, "^(SELF)(:.+)", varName + "$2"); //replace SELF:/ with source package
                    }
                }
                //no fing clue. We have to find it
                else if ((m = Regex.Match(path, "^SELF:.+")).Success)
                {
                    //Find the first package with this path
                    path = Regex.Replace(path, "^SELF:/(.+/)", "$1");
                    string pathWithoutFileName = Regex.Replace(path, "^(.+/).+", "$1");

                    List<ShortCut> shortCuts = FileManagerSecure.GetShortCutsForDirectory(pathWithoutFileName);
                    foreach (ShortCut s in shortCuts)
                    {
                        if (s.isLatest && s.package != "" && s.package != "AddonPackages Flattened" && s.package != "AddonPackages Filtered")
                        {
                            List<string> files = GetFilesAtPathRecursive(s.path, null);
                            foreach (string f in files)
                            {
                                string t = FileManagerSecure.NormalizePath(f);
                                if (t.Contains(path))
                                {
                                    path = t;
                                    break;
                                }
                            }
                        }
                    }
                }

                BrowserCallBack(path);
            }
            catch (Exception e)
            {
                SuperController.LogError("Decal Maker: Unable to Resove Texture Path");
                SuperController.LogError(e.InnerException.StackTrace);
            }
        }

        public void BrowserCallBack(string path)
        {  
            if (string.IsNullOrEmpty(path))
            {
                SetPathText("");
            }
            else
            {
                SetPathText(path);
                ImagePanel.LoadResourceFile(path);
            }
        }

        public JSONClass Save()
        {
            JSONClass decalSaveJSON = new JSONClass();
            decalSaveJSON["H"].AsFloat = jcolor.val.H;
            decalSaveJSON["S"].AsFloat = jcolor.val.S;
            decalSaveJSON["V"].AsFloat = jcolor.val.V;
            decalSaveJSON["Alpha"].AsFloat = ImagePanel.sliderValue;
            decalSaveJSON["RandomName"] = RandomName;



            //SuperController.LogError("ALPHA IS " + ImagePanel.sliderValue);
            //jcolor.StoreJSON(decalSaveJSON, true, true, true);

            if (!string.IsNullOrEmpty(ImagePanel.Path))
            {
                string relativePath = FileManagerSecure.NormalizePath(ImagePanel.Path);
                decalSaveJSON["Path"] = relativePath;
            }
            return decalSaveJSON;
        }

    }


}

