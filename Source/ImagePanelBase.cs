using UnityEngine;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    public class ImagePanelBase : PanelBase
    {
        protected Image image;
        public Button Button { get; private set; }
        public Material material { get; private set; }
                
        public ImagePanelBase(Decal_Maker DM, string TextureSlot, string MaterialSlot, bool IsNormalMap, bool linear) : base(DM)
        {
            this.MaterialSlot = MaterialSlot;
            this.TextureSlot = TextureSlot;
            this.IsNormalMap = IsNormalMap;
            this.linear = linear;

            if (IsNormalMap)
            {
                material = new Material(DM._customUINormalMapShader);
            }
            else if (linear)
            {
                material = new Material(DM._customSpecGlossShader);
            }
            else
            {   //new Material(Graphic.defaultGraphicMaterial.shader);
                material = new Material(Shader.Find("UI/Default-Overlay"));
            }


            image = gameObject.AddComponent<Image>();
            image.material = material;

            Button = gameObject.AddComponent<Button>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameObject.Destroy(image);
            Resources.UnloadAsset(image);
            GameObject.Destroy(material);
            Resources.UnloadAsset(material);

        }
        public virtual void ApplyTexture(Texture2D texture)
        {
            image.material.mainTexture = texture;
        }

    }
}
