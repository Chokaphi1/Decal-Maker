using System.Collections;
using System.Linq;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class RenderPanelNormal : RenderPanelBase
    {
        private Texture2D _normTex;
        public Texture2D BlankNormalTex { get { return _normTex; } }
        public RenderPanelNormal(Decal_Maker DM, string MaterialSlot, string TextureSlot) : base(DM, MaterialSlot, TextureSlot)
        {
            material = new Material(DM._customNormShader);
            _normTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Normal.png", true);
            tempTexture = new Texture2D(4096, 4096, TextureFormat.RGBA32, false, true);
        }

        public override void OnDestroy()
        {
            if (_normTex != null)
            {
                GameObject.Destroy(_normTex);
                Resources.UnloadAsset(_normTex);
            }

            base.OnDestroy();
        }

        //convert normal from packed version for UI button display
        public Texture2D ConvertNormal(Texture2D mainTex)
        {
            material.SetTexture("_BumpMap0", mainTex);
            material.SetFloat("_BumpMapScale0", 1);
            GpuCombine(_normTex, material, true);
            return tempTexture;
        }

        public override IEnumerator ApplyChanges()
        {
            if (DecalPanels.Count == 0)
            {
                ResetGPUTexture(MaterialSlot, TextureSlot);
            }
            else
            {
                int count = 0;
                ZeroMaterial();
                //if we have a normal material on Base model use it as norm0
                Texture2D mainTex = DM.GetOriginalGPUTexture(_TextureIndex[TextureSlot].FirstOrDefault(), MaterialSlot);
                if (mainTex != null)
                {
                    material.SetTexture("_BumpMap" + count, mainTex);
                    count = 1;
                }

                bool multiRender = false;
                foreach (DecalPanel d in DecalPanels)
                {
                    material.SetFloat("_BumpMapScale" + count, d.sliderJSF.val);
                    material.SetTexture("_BumpMap" + count, d.ImagePanel.mainTexture);

                    count++;
                    //we can bake 10 textures per material
                    if (count > 9)
                    {
                        count = 0;
                        yield return GpuCombine(_normTex, material, true);
                        ZeroMaterial();
                        multiRender = true;
                    }
                }
                if (multiRender)
                {
                    yield return GpuCombine(tempTexture, material, true);
                }
                else
                {
                    yield return GpuCombine(_normTex, material, true);
                }

                SetGPUTexture(tempTexture, MaterialSlot, TextureSlot);
            }
            //Only un-flag IsDirty if no updates happened while processing.
            if (dirtyTimeStart == dirtyTime)
            {
                IsDirty = false;
            }
            Processing = false;
        }

        //recycle material by setting alphas back to 0
        protected void ZeroMaterial()
        {
            for (int i = 0; i < 10; i++)
            {
                material.SetFloat("_BumpMapScale" + i, 1);
                material.SetTexture("_BumpMap" + i, null);
            }
        }
    }
}