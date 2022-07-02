using MVR.FileManagementSecure;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    public class GenitalMaker
    {
        public GameObject gameObject = new GameObject("GenitalPanel");
        private BasePanel _basePanel;
        public Image image { get; private set; }
        private UIDynamic spacerLeft { get { return _basePanel.spacerLeft; } }
        private UIDynamic spacerRight { get { return _basePanel.spacerRight; } }
        private Decal_Maker _DM;
        public GenitalMaker(Decal_Maker DM)
        {
            _DM = DM;
            _basePanel = new BasePanel(DM);

            image = gameObject.AddComponent<Image>();
            image.material = new Material(Shader.Find("UI/Default-Overlay"));


            image.color = Color.white;
            RectTransform imageRect = image.rectTransform;
            imageRect.sizeDelta = new Vector2(500, 500);
            image.gameObject.transform.SetParent(spacerLeft.transform, false);


            GridLayoutGroup glg = spacerRight.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(500, 120);

            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 1;
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.spacing = new Vector2(10, 0);




            //sliders
            UIDynamicSlider Hue = CreateSlider(TEST, 0, 1, 1, "HUE");
            UIDynamicSlider Sat = CreateSlider(TEST, -1, 1, 0, "SAT");
            UIDynamicSlider Val = CreateSlider(TEST, 0, 1, 1, "VAL");

            Hue.gameObject.transform.SetParent(spacerRight.transform, false);
            Sat.gameObject.transform.SetParent(spacerRight.transform, false);
            Val.gameObject.transform.SetParent(spacerRight.transform, false);
        }
        private void TEST(float value)
        {

        }

        public void Start(Texture2D torso)
        {
            Material material = new Material(_DM._customUIGenitalMakerShader);

            byte[] Genital_Base = FileManagerSecure.ReadAllBytes(_DM.GetPackagePath() + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Genital_Base.png");
            Texture2D Genital_BaseTex = TempManager.TempTexture2D(1, 1, TextureFormat.RGBA32, false);
            Genital_BaseTex.LoadImage(Genital_Base);

            byte[] Genital_Base_Anus = FileManagerSecure.ReadAllBytes(_DM.GetPackagePath() + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Genital_Base_Anus.png");
            Texture2D Genital_Base_AnusTex = TempManager.TempTexture2D(1, 1, TextureFormat.RGBA32, false);
            Genital_Base_AnusTex.LoadImage(Genital_Base_Anus);

            byte[] Genital_Base_Hood = FileManagerSecure.ReadAllBytes(_DM.GetPackagePath() + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Genital_Base_Hood.png");
            Texture2D Genital_Base_HoodTex = TempManager.TempTexture2D(1, 1, TextureFormat.RGBA32, false);
            Genital_Base_HoodTex.LoadImage(Genital_Base_Hood);

            byte[] Genital_Base_Labia = FileManagerSecure.ReadAllBytes(_DM.GetPackagePath() + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Cutout/Genital_Base_Labia.png");
            Texture2D Genital_Base_LabiaTex = TempManager.TempTexture2D(1, 1, TextureFormat.RGBA32, false);
            Genital_Base_LabiaTex.LoadImage(Genital_Base_Labia);



            material.SetColor("_GenitalColor", new Color(1, 0, 1, 0));
            material.SetColor("_HoodColor", new Color(1, 1, 1, 0));
            material.SetColor("_LabiaColor", new Color(1, 1, 0, 0));
            material.SetColor("_AnusColor", new Color(1, 1, 1, 0));

            material.SetTexture("_GenitalTex", Genital_BaseTex);
            material.SetTexture("_HoodTex", Genital_Base_HoodTex);
            material.SetTexture("_LabiaTex", Genital_Base_LabiaTex);
            material.SetTexture("_AnusTex", Genital_Base_AnusTex);

            material.SetTexture("_TorsoTex", torso);


            //_DM.DebugSave(torso,"TorsoTEX.png");
            //_DM.DebugSave(Genital_BaseTex, "TGenital_BaseTex.png");
            //_DM.DebugSave(Genital_Base_AnusTex, "Genital_Base_AnusTex.png");
            //_DM.DebugSave(Genital_Base_HoodTex, "enital_Base_HoodTex.png");
            //_DM.DebugSave(Genital_Base_LabiaTex, "Genital_Base_LabiaTex.png");

            GameObject.Destroy(image.material);
            image.material = material;

        }

        private UIDynamicSlider CreateSlider(UnityAction<float> action, int min = 0, int max = 1, int value = 1, string title = "")
        {
            Transform sliderPrefab = GameObject.Instantiate<Transform>(_DM.manager.configurableSliderPrefab);
            sliderPrefab.SetParent(gameObject.transform, false);
            sliderPrefab.localPosition = Vector3.zero;

            UIDynamicSlider sliderDynamic = sliderPrefab.GetComponent<UIDynamicSlider>();
            sliderDynamic.transform.localPosition = new Vector2(0, -80);
            sliderDynamic.quickButtonsEnabled = false;
            sliderDynamic.rangeAdjustEnabled = false;
            sliderDynamic.defaultButtonEnabled = false;


            Image sliderBackground = sliderDynamic.GetComponentsInChildren<Image>().Where(x => x.name == "Panel").First();
            Color c = sliderBackground.color;
            c.a = 0.1f;
            sliderBackground.color = c;

            sliderDynamic.slider.onValueChanged.AddListener(action);
            sliderDynamic.slider.minValue = min;
            sliderDynamic.slider.maxValue = max;
            sliderDynamic.slider.value = value;
            sliderDynamic.label = title;
            //sliderDynamic.labelText.transform.localPosition += new Vector3(400, 0, 0);

            return sliderDynamic;
        }
    }


}

