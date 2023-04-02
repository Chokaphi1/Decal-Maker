using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace VAM_Decal_Maker
{
    public class ImagePanel : ImagePanelBase
    {

        public bool ImageLoading { get; private set; }
        public string Path { get; set; }

        //isolate image from direct access since imagepanel sometimes modifies the image shader or image so use imagePanels properties not the image's
        public Color color { get; private set; } = Color.white;
        public Texture mainTexture { get { return base.material.mainTexture; } set { base.material.mainTexture = value; } }
        public RectTransform rectTransform { get { return image.GetComponent<RectTransform>(); } }

        private GameObject video;
        public VideoPlayer videoPlayer { get; private set; }
        //lazy load/create since it wont be used a lot of times
        private RenderTexture _videoTexture;
        private RenderTexture videoTexture
        {
            get
            {
                if (_videoTexture == null)
                {
                    if (linear)
                        _videoTexture = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                    else
                        _videoTexture = new RenderTexture(4096, 4096, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                    _videoTexture.Create();
                }

                return _videoTexture;
            }
        }

        public event EventHandler<PanelEventArgs> ImagePanelUpdate;
        private void OnImagePanelChange(PanelEventArgs e)
        {
            if (ImagePanelUpdate != null)
            {
                //store a ref to this imagepanel
                e.ImagePanel = this;
                ImagePanelUpdate(this, e);
            }
        }

        public ImagePanel(Decal_Maker DM, Color? color = null, string TextureSlot = null, string MaterialSlot = null, bool IsNormalMap = false, bool linear = false) : base(DM, TextureSlot, MaterialSlot, IsNormalMap, linear)
        {
            base.TextureSlot = TextureSlot;
            base.MaterialSlot = MaterialSlot;
            base.IsNormalMap = IsNormalMap;
            base.linear = linear;


            //don't parent video player to object that activates/inactivates or it stops
            video = new GameObject();
            videoPlayer = video.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.frameReady += FrameUpdateEvent;
            videoPlayer.prepareCompleted += VideoPreparedEvent;
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.isLooping = true;

            //image = gameObject.AddComponent<Image>();
            // image.material = new Material(Shader.Find("UI/Default-Overlay"));

            UpdateMaterialColor(color ?? new Color(1, 1, 1, 1f));

            RectTransform imageRect = image.rectTransform;
            imageRect.sizeDelta = new Vector2(300, 300);

            //Button = gameObject.AddComponent<Button>();
            Button.onClick.AddListener(() =>
            {
                OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelPathChanged));
            });

        }

        public void SetNormalScale(float value)
        {
            material.SetFloat("_NormalScale", value);
            //stupid but makes the UI texture update
            image.enabled = false;
            image.enabled = true;

            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelNormalSliderChange, material));
        }

        public void SetSpecularScale(float value)
        {
            material.SetFloat("_Smoothness", value);
            image.enabled = false;
            image.enabled = true;

            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelSpecSliderChange, material));
        }

        //public void SetAlpha(float value)
        //{
        //    sliderValue = value;
        //    Color newAlpha = this.color;// image.material.GetColor("_Color");
        //    newAlpha.a = value;

        //    UpdateMaterialColor(newAlpha);
        //}

        //public void SetColor(JSONStorableColor jcolor)
        //{
        //    Color newColor = jcolor.colorPicker.currentColor;
        //    //use alpha from slider
        //    newColor.a = this.color.a;

        //    UpdateMaterialColor(newColor);
        //}
        //cleanup
        public override void OnDestroy()
        {
            videoPlayer.frameReady -= FrameUpdateEvent;
            videoPlayer.prepareCompleted -= VideoPreparedEvent;
            //the ones on MainPanel are references to VAM skin.
            GameObject.Destroy(material);
            GameObject.Destroy(mainTexture);
            GameObject.Destroy(video);
            GameObject.Destroy(videoPlayer);
            GameObject.Destroy(videoTexture);

            Resources.UnloadAsset(material);
            Resources.UnloadAsset(mainTexture);
            Resources.UnloadAsset(videoTexture);

            base.OnDestroy();
        }

        public void UpdateMaterialColor(Color color, bool sendEvent = true)
        {
            this.color = color;
            UpdateMaterialColor(sendEvent);
        }

        private void UpdateMaterialColor(bool sendEvent = true)
        {
            //The Mask object causes a modified material to be returned so it is no longer rendering the texture that you are updating with the "material" property.
            image.materialForRendering.SetColor("_Color", color);
            //update unmasked material as well
            image.material.SetColor("_Color", color);
            if (sendEvent)
                OnImagePanelChange(new PanelEventArgs(EventEnum.DecalPanelColor, color));
        }

        private void UpdateMaterialTexture(Texture2D tex)
        {
            //The Mask object causes a modified material to be returned so it is no longer rendering the texture that you are updating with the "material" property.
            image.materialForRendering.SetTexture("_MainTex", tex);
            //update unmasked material as well
            image.material.SetTexture("_MainTex", tex);
        }
        //used by prefab panel when nothing selected
        public void HideTexture()
        {
            material.SetColor("_Color", Color.white);

            //UnityEngine.Object.Destroy(image.material);
            //image.material = new Material(Shader.Find("UI/Default-Overlay"));

            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelTextureCleared, material));
        }

        //Use existing texture
        public override void ApplyTexture(Texture2D texture)
        {
            // Material material = new Material(Shader.Find("UI/Default-Overlay"));
            material.SetTexture("_MainTex", texture);
            //UnityEngine.Object.Destroy(image.material);
            //Resources.UnloadAsset(image.material);
            image.SetMaterialDirty();

            //image.material = material;
            //image.material.mainTexture = texture;

            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelMaterialChanged, this.material));
        }

        //load texture from disk
        private void OnImageLoaded(ImageLoaderThreaded.QueuedImage qi)
        {
            if (qi != null)
            {
                mainTexture = qi.tex;

                image.SetMaterialDirty();
                UpdateMaterialColor();

                Path = qi.imgPath;

                ImageLoading = false;

                OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelTextureLoaded, material));
            }
        }


        //Old VAM version does not give access to load texture so use VAM image loading routine instead
        //Newer can use FileManagementSecure.FileManagerSecure or loadTexture
        //Wil call OnImageLoad once finished
        //
        //public Action<VideoPlayer> videoCallback;
        //public videoCallback videoCallback1 { get; set; }
        public void LoadResourceFile(string filePath)
        {
            if (videoPlayer.isPrepared)
                videoPlayer.Stop();

            string ext = PathHelper.GetExtension(filePath);

            if (PathHelper.videoExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelVideoLoad));
                LoadVideo(filePath);

            }
            else if (PathHelper.imageExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelImageLoad));
                LoadPNG(filePath);
            }
            else
            {
                SuperController.LogError("Decal Maker LoadResourceFile: Extension not recognized");
            }
        }
        private void LoadPNG(string filePath)
        {
            ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
            queuedImage.imgPath = filePath;
            queuedImage.forceReload = false;
            queuedImage.createMipMaps = false;
            queuedImage.isNormalMap = IsNormalMap;
            queuedImage.isThumbnail = false;
            queuedImage.linear = linear;
            queuedImage.createAlphaFromGrayscale = false;
            queuedImage.compress = false;//!queuedImage.isNormalMap;

            queuedImage.callback = new ImageLoaderThreaded.ImageLoaderCallback(OnImageLoaded); // = new ImageLoaderThreaded.ImageLoaderCallback(OnImageLoaded);

            //start image load
            ImageLoaderThreaded.singleton.QueueImage(queuedImage);
            ImageLoading = true;
        }


        private void LoadVideo(string filePath)
        {
            videoPlayer.targetTexture = videoTexture;

            videoPlayer.url = filePath;
            videoPlayer.Prepare();

            //create a maintexture as are target if it doesn't exist OR is wrong size for graphics copy 4096,4096
            if (mainTexture == null || mainTexture.width != 4096 || mainTexture.height != 4096)
            {
                mainTexture = new Texture2D(videoTexture.width, videoTexture.height, TextureFormat.ARGB32, false, linear);
                image.material = material;
                image.enabled = false;
                image.enabled = true;
            }

            videoPlayer.Play();

        }
        //caled when video is ready to play
        private void VideoPreparedEvent(VideoPlayer source)
        {
            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelVidePrepared, source));
        }
        //called on each frame so trigger a dirty event
        private void FrameUpdateEvent(VideoPlayer source, long frameIdx)
        {
            Graphics.CopyTexture(videoTexture, mainTexture);
            OnImagePanelChange(new PanelEventArgs(EventEnum.ImagePanelVideoFrameUpdate, source));
        }

    }

}

