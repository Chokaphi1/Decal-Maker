using System.Collections;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class RenderPanelDecal : RenderPanelBase
    {
        private Texture2D _clearTex;
        public RenderPanelDecal(Decal_Maker DM, string MaterialSlot, string TextureSlot) : base(DM, MaterialSlot, TextureSlot)
        {
            material = new Material(DM._customBulkDecalShader);
            tempTexture = new Texture2D(4096, 4096, TextureFormat.RGBA32, false, false);
            _clearTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Clear.png");
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
                ZeroMaterial();
                //Uses shader to clip part of one texture and applys it to another based on the alpha of a third texture.
                //Used to Apply "Clean" nipples and genital areas after decals have been applied.
                //This Alpha control will be Character UV specific. So Victoria has a diffrent nipple area than Olympia etc..
                if (TextureSlot == BodyRegionEnum.Torso && DM._toggleNippleCutout.val)
                {
                    DM.GetBoolJSONParam("Nipple Cutouts ON");
                    Texture2D alphaTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/" + DM._uvSetName + ".png");
                    material.SetTexture("_Alpha", alphaTex);
                }
                if (TextureSlot == BodyRegionEnum.Genitals && DM._toggleGenitalCutout.val && IsMale == false)
                {
                    Texture2D alphaTex = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/_FemaleGenitals.png");
                    material.SetTexture("_Alpha", alphaTex);
                }

                int count = 0;
                bool linear = false;
                bool multiRender = false;
                foreach (DecalPanel d in DecalPanels)
                {
                    material.SetVector("_DecalColor" + count, d.ImagePanel.color);
                    material.SetTexture("_DecalTex" + count, d.ImagePanel.mainTexture);

                    linear = d.ImagePanel.linear;
                    count++;
                    //we can bake 10 textures per material
                    if (count > 9)
                    {
                        count = 0;
                        yield return GpuCombine(_clearTex, material, linear);
                        ZeroMaterial();
                        multiRender = true;
                    }
                }
                if (multiRender)
                {
                    yield return GpuCombine(tempTexture, material, linear);
                }
                else
                {
                    yield return GpuCombine(_clearTex, material, linear);
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
        private void ZeroMaterial()
        {
            material.SetTexture("_Alpha", null);
            for (int i = 0; i < 10; i++)
            {
                material.SetVector("_DecalColor" + i, Color.clear);
                material.SetTexture("_DecalTex" + i, null);
            }
        }


    }
}