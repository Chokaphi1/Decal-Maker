using System.Collections.Generic;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class HeaderPanel : UIPanelBase
    {
        public SelectionPanel ActivePanel { get; set; }
        public SelectionPanel SelectionPanelDecal { get; private set; }
        public SelectionPanel SelectionPanelSpec { get; private set; }
        public SelectionPanel SelectionPanelGloss { get; private set; }
        public SelectionPanel SelectionPanelNorm { get; private set; }
        public SelectionPanel SelectionPanelDecalEye { get; private set; }
        public SelectionPanel SelectionPanelSpecEye { get; private set; }
        public SelectionPanel SelectionPanelGlossEye { get; private set; }
        public SelectionPanel SelectionPanelNormEye { get; private set; }

        public MyButton decalButton { get; private set; }
        public MyButton specButton { get; private set; }
        public MyButton glossButton { get; private set; }
        public MyButton normButton { get; private set; }

        public MyButton eyedecalButton { get; private set; }
        public MyButton eyespecButton { get; private set; }
        public MyButton eyeglossButton { get; private set; }
        public MyButton eyenormButton { get; private set; }
        public MyButton eyesButton { get; private set; }
        public Dictionary<string, SelectionPanel> selectionPanels { get; private set; }

        private List<MyButton> buttons;

        //events from Core
        private void CorePanelEvent(object o, PanelEventArgs e)
        {
            switch (e.EventName)
            {
                case EventEnum.HeaderPanelSelection:
                    UpdateSelection(o);
                    break;
                //once setup is finished set Decal as active panel
                case EventEnum.CoreSetupFinished:
                    RaiseCoreEvent(decalButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelDecal));
                    break;
            }
        }

        private void UpdateSelection(object o)
        {
            if (o.GetType() != typeof(MyButton))
                return;

            foreach (MyButton button in buttons)
            {
                if (button == (MyButton)o)
                {
                    button.Selected(true);
                }
                else
                {
                    button.Selected(false);
                }
            }
        }

        public HeaderPanel(Decal_Maker DM) : base(DM, 50)
        {
            //Image i = spacerLeft.gameObject.AddComponent<Image>();
            //i.material = new Material(Shader.Find("UI/Default-Overlay"));
            //i.material.color = Color.green;

            gameObject.transform.SetParent(spacerLeft.transform, false);
            //center panel in UI
            gameObject.transform.localPosition += new Vector3(270, 10, 0);
            //basePanel
            CreatePanelBackground(1050, 70, new Color(.5f, .5f, .5f, 1));
            SetLayout(200, 50);

            SelectionPanelDecal = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.DecalTex,
                IsNormalMap = false,
                linear = false,

                PanelName = MaterialSlot
            };

            SelectionPanelSpec = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.SpecTex,
                IsNormalMap = false,
                linear = true,

                PanelName = MaterialSlot
            };

            SelectionPanelGloss = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.GlossTex,
                IsNormalMap = false,
                linear = true,

                PanelName = MaterialSlot
            };

            SelectionPanelNorm = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.BumpMap,
                IsNormalMap = true,
                linear = true,

                PanelName = MaterialSlot
            };

            //EYES

            SelectionPanelDecalEye = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.DecalTex,
                IsNormalMap = false,
                linear = false,

                PanelName = MaterialSlot
            };

            SelectionPanelSpecEye = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.SpecTex,
                IsNormalMap = false,
                linear = true,

                PanelName = MaterialSlot
            };


            SelectionPanelGlossEye = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.GlossTex,
                IsNormalMap = false,
                linear = true,

                PanelName = MaterialSlot
            };

            SelectionPanelNormEye = new SelectionPanel(1050, 300, new Color(1, 1, 1, 1f), DM)
            {
                MaterialSlot = MatSlotEnum.BumpMap,
                IsNormalMap = true,
                linear = true,

                PanelName = MaterialSlot
            };


            decalButton = new MyButton("Decal", new Color(1, 0.3f, 0.4f), transform);
            decalButton.button.onClick.AddListener(() => RaiseCoreEvent(decalButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelDecal)));

            specButton = new MyButton("Specular", new Color(0.95f, 0.25f, 0.91f), transform);
            specButton.button.onClick.AddListener(() => RaiseCoreEvent(specButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelSpec)));

            glossButton = new MyButton("Gloss", new Color(0.2f, .98f, 0.2f), transform);
            glossButton.button.onClick.AddListener(() => RaiseCoreEvent(glossButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelGloss)));

            normButton = new MyButton("Normal", new Color(0.2f, 0.9f, .9f), transform);
            normButton.button.onClick.AddListener(() => RaiseCoreEvent(normButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelNorm)));

            eyedecalButton = new MyButton("EyeDecal", new Color(1, 0.3f, 0.4f), transform);
            eyedecalButton.button.onClick.AddListener(() => RaiseCoreEvent(decalButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelDecalEye)));
            eyedecalButton.gameObject.SetActive(false);

            eyespecButton = new MyButton("EyeSpecular", new Color(0.95f, 0.25f, 0.91f), transform);
            eyespecButton.button.onClick.AddListener(() => RaiseCoreEvent(specButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelSpecEye)));
            eyespecButton.gameObject.SetActive(false);

            eyeglossButton = new MyButton("EyeGloss", new Color(0.2f, .98f, 0.2f), transform);
            eyeglossButton.button.onClick.AddListener(() => RaiseCoreEvent(glossButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelGlossEye)));
            eyeglossButton.gameObject.SetActive(false);

            eyenormButton = new MyButton("EyeNormal", new Color(0.2f, 0.9f, .9f), transform);
            eyenormButton.button.onClick.AddListener(() => RaiseCoreEvent(normButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelNormEye)));
            eyenormButton.gameObject.SetActive(false);

            buttons = new List<MyButton>() { decalButton, specButton, glossButton, normButton, eyedecalButton, eyeglossButton, eyenormButton, eyespecButton };

            eyesButton = new MyButton("Eyes Toggle", new Color(0.2f, 0.8f, .6f), transform);

            bool eyesActive = false;
            eyesButton.button.onClick.AddListener(() =>
            {
                foreach (MyButton button in buttons)
                {
                    button.gameObject.SetActive(!button.gameObject.activeSelf);
                }
                eyesActive = !eyesActive;
                if (eyesActive)
                {
                    RaiseCoreEvent(eyedecalButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelDecalEye));
                }
                else
                {
                    RaiseCoreEvent(decalButton, new PanelEventArgs(EventEnum.HeaderPanelSelection, SelectionPanelDecal));
                }
            });



            //now finalize panels
            SelectionPanelDecal.CreateManagerPanels();
            SelectionPanelSpec.CreateManagerPanels();
            SelectionPanelGloss.CreateManagerPanels();
            SelectionPanelNorm.CreateManagerPanels();

            SelectionPanelDecal.PanelName = "Decal";
            SelectionPanelSpec.PanelName = "Specular";
            SelectionPanelGloss.PanelName = "Gloss";
            SelectionPanelNorm.PanelName = "Normal";

            SelectionPanelDecalEye.CreateManagerEyePanels();
            SelectionPanelSpecEye.CreateManagerEyePanels();
            SelectionPanelGlossEye.CreateManagerEyePanels();
            SelectionPanelNormEye.CreateManagerEyePanels();


            ActivePanel = SelectionPanelDecal;
            selectionPanels = new Dictionary<string, SelectionPanel> { { "Decal", SelectionPanelDecal }, { "Specular", SelectionPanelSpec }, { "Gloss", SelectionPanelGloss }, { "Normal", SelectionPanelNorm } };

            DM.CoreEvent += CorePanelEvent;
        }

    }


}

