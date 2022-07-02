using System.Collections;
using System.Linq;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class RenderPanelSpecGloss : RenderPanelBase
    {
        private Texture2D _clearTex;
        public RenderPanelSpecGloss(Decal_Maker DM, string MaterialSlot, string TextureSlot) : base(DM, MaterialSlot, TextureSlot)
        {
            material = new Material(DM._customSpecGlossShader);
            _clearTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Clear.png", true);
            tempTexture = new Texture2D(4096, 4096, TextureFormat.RGBA32, false, true);
        }

        public override void OnDestroy()
        {
            if (_clearTex != null)
            {
                GameObject.Destroy(_clearTex);
                Resources.UnloadAsset(_clearTex);
            }

            base.OnDestroy();
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
                bool multiRender = false;
                ZeroMaterial();
                //if we have a specular material on Base model use it as spec0
                Texture2D mainTex = DM.GetOriginalGPUTexture(_TextureIndex[TextureSlot].FirstOrDefault(), MaterialSlot);
                if (mainTex != null)
                {
                    material.SetVector("_SpecColor" + count, new Vector4(1, 1, 1, 0.5f));
                    material.SetTexture("_SpecTex" + count, mainTex);
                    count = 1;
                }

                foreach (DecalPanel d in DecalPanels)
                {
                    material.SetVector("_SpecColor" + count, new Vector4(1, 1, 1, 0.5f));
                    material.SetTexture("_SpecTex" + count, d.ImagePanel.mainTexture);
                    //material.SetFloat("_Smoothness" +count, d.ImagePanel.sliderValue);

                    count++;
                    //we can bake 10 textures per material
                    if (count > 9)
                    {
                        count = 0;
                        yield return GpuCombine(_clearTex, material, true);
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
                    yield return GpuCombine(_clearTex, material, true);
                }

                //apply textures
                SetGPUTexture(tempTexture, MaterialSlot, TextureSlot);
            }
            //Only un-flag IsDirty if no updates happened while processing.
            if (dirtyTimeStart == dirtyTime)
            {
                IsDirty = false;
            }
            Processing = false;
        }

        protected void ZeroMaterial()
        {
            for (int i = 0; i < 10; i++)
            {
                material.SetVector("_SpecColor" + i, new Vector4(1, 1, 1, 0));
                material.SetTexture("_SpecTex" + i, null);
            }
        }
    }
}