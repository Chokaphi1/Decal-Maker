using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class RenderPanelBase
    {
        protected Decal_Maker DM;
        protected bool Processing = false;
        protected float dirtyTime = Time.time;
        protected float dirtyTimeStart = Time.time;

        protected Material material;
        protected List<Material> materials = new List<Material>();

        private static readonly Dictionary<string, int[]> femaleTextureIndex = new Dictionary<string, int[]>()
        {
            {BodyRegionEnum.Torso, new int[]{15,18,19,20,21,27,29}},
            {BodyRegionEnum.Face, new int[]{2,5,11} },
            {BodyRegionEnum.Limbs, new int[]{0,12,14,16,17,22,23}},
            {BodyRegionEnum.Genitals, new int[]{28}}
        };
        private static readonly Dictionary<string, int[]> maleTextureIndex = new Dictionary<string, int[]>()
        {
            {BodyRegionEnum.Torso, new int[]{15,16,19,20,21,22,30,32}},
            {BodyRegionEnum.Face, new int[]{2,5,11}},
            {BodyRegionEnum.Limbs, new int[]{0,12,14,17,18,23,24}},
            {BodyRegionEnum.Genitals, new int[]{28,29}}
        };
        public Dictionary<string, int[]> _TextureIndex
        {
            get
            {
                if (IsMale)
                    return maleTextureIndex;

                return femaleTextureIndex;
            }
        }

        public bool IsMale { get { return DM._isMale; } }
        public string TextureSlot { get; protected set; }
        public string MaterialSlot { get; protected set; }
        public List<DecalPanel> DecalPanels { get; set; }
        private bool _isDirty = false;
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
                if (value == true)
                    dirtyTime = Time.time;
            }
        }
        //this is our target texture will alway update referance in all materials it is assigned to 
        protected virtual Texture2D tempTexture { get; set; }


        public RenderPanelBase(Decal_Maker DM, string MaterialSlot, string TextureSlot)
        {
            this.DM = DM;

            DM.OnUpDateAction += Update;
            DM.OnDestroyAction += OnDestroy;

            this.MaterialSlot = MaterialSlot;
            this.TextureSlot = TextureSlot;
        }

        public bool GpuCombine(Texture baseTex, Material material, bool linear = false)
        {
            int w = baseTex.width;
            int h = baseTex.height;

            RenderTextureReadWrite renderTexConversion = RenderTextureReadWrite.sRGB;
            if (linear)
            {
                renderTexConversion = RenderTextureReadWrite.Linear;
            }

            RenderTexture tmp = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, renderTexConversion);        //tmp.antiAliasing = 8;                                                                                                                           //Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            Graphics.Blit(baseTex, tmp, material);
            RenderTexture.active = tmp;

            //tempTexture = new Texture2D(w, h, TextureFormat.RGBA32, false, linear);
            //tempTexture.ReadPixels(new Rect(0, 0, w, h), 0, 0, false);
            //tempTexture.Apply();
            //Avoids GPU to CPU Copy
            Graphics.CopyTexture(tmp, tempTexture);

            // Reset the active RenderTexture
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            return true;
        }

        public void Update()
        {
            //SuperController.LogError("OnLateUpdate()");
            if (IsDirty)
            {
                //SuperController.LogError("IS DIRTY!" + MaterialSlot + " " + TextureSlot);
                if (!Processing)
                {
                    dirtyTimeStart = dirtyTime;
                    Processing = true;
                    DM.StartCoroutine(ApplyChanges());
                }
            }
        }

        public virtual IEnumerator ApplyChanges()
        {
            yield return null;
        }

        public virtual void OnDestroy()
        {
            DM.OnUpDateAction -= Update;
            DM.OnDestroyAction -= OnDestroy;

            //delete materials and texture references
            foreach (Material mat in materials)
            {
                GameObject.Destroy(mat);
                Resources.UnloadAsset(mat);
            }

            if (material != null)
            {
                GameObject.Destroy(material);
                Resources.UnloadAsset(material);
            }

            if (tempTexture != null)
            {
                GameObject.Destroy(tempTexture);
                Resources.UnloadAsset(tempTexture);
            }
        }

        public Texture2D GetGPUTexture(string MaterialSlot, string TextureSlot)
        {
            int num = _TextureIndex[TextureSlot].FirstOrDefault();
            return (Texture2D)DM._dazSkin.GPUmaterials[num].GetTexture(MaterialSlot);
        }

        protected void SetGPUTexture(Texture2D tempTex, string MaterialSlot, string TextureSlot)
        {
            //apply textures
            foreach (int num in _TextureIndex[TextureSlot])
            {
                DM._dazSkin.GPUmaterials[num].SetTexture(MaterialSlot, tempTex);
            }
        }

        protected void ResetGPUTexture(string MaterialSlot, string TextureSlot)
        {
            //reset to default textures
            foreach (int num in _TextureIndex[TextureSlot])
            {
                DM.RestoreGPUMatbyID(num, MaterialSlot);
            }
        }

        //From https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-
        //public Texture2D GetUnreadableTexture(Texture2D texture, bool linear = false)
        //{
        //    RenderTextureReadWrite renderTexConversion = RenderTextureReadWrite.sRGB;
        //    if (linear)
        //    {
        //        renderTexConversion = RenderTextureReadWrite.Linear;
        //    }
        //    //LogError("Get Unreadable texture");
        //    RenderTexture tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, renderTexConversion);

        //    // Blit the pixels on texture to the RenderTexture
        //    Graphics.Blit(texture, tmp);
        //    // Backup the currently set RenderTexture
        //    RenderTexture previous = RenderTexture.active;
        //    // Set the current RenderTexture to the temporary one we created
        //    RenderTexture.active = tmp;

        //    Texture2D newTexture = TempManager.TempTexture2D(texture.width, texture.height, TextureFormat.RGBA32, false, linear);

        //    newTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        //    newTexture.Apply();

        //    // Reset the active RenderTexture
        //    RenderTexture.active = previous;
        //    // Release the temporary RenderTexture
        //    RenderTexture.ReleaseTemporary(tmp);
        //    //LogError("Finish Getting Unreadable texture");
        //    return newTexture;
        //}

    }
}