using System.Collections.Generic;
using UnityEngine;

namespace VAM_Decal_Maker
{
    public class SelectionPanel : UIPanelBase
    {
        private UIDynamicButton AddButton;
        public UIDynamicButton RemoveButton { get; private set; }
        public Dictionary<string, ManagerPanel> ManagerPanels { get; private set; }
        public ManagerPanel ActivePanel { get; private set; }
        public ManagerPanel TorsoPanel { get; private set; }
        public ManagerPanel GenitalPanel { get; private set; }
        public ManagerPanel FacelPanel { get; private set; }
        public ManagerPanel LimbsPanel { get; private set; }

        public ManagerPanel IrisPanel { get; private set; }
        public ManagerPanel ScleraPanel { get; private set; }
        public ManagerPanel PupilPanel { get; private set; }
        public ManagerPanel CorneaPanel { get; private set; }
        public ManagerPanel ReflectionPanel { get; private set; }


        //events from Core
        private void CorePanelEvent(object o, PanelEventArgs e)
        {
            switch (e.EventName)
            {
                //we have 4 Selection panels each with 4 Manager panels. we only want to update on the 4 UI buttons of this material slot.
                case EventEnum.ManagerPanelSelection:
                    if (e.ManagerPanel.MaterialSlot == MaterialSlot)
                    {
                        ActivePanel = e.ManagerPanel;
                    }
                    break;

                case EventEnum.HeaderPanelSelection:
                    if (e.SelectionPanel == this)
                        ActivateWindow(true);
                    else
                        ActivateWindow(false);
                    break;

                case EventEnum.DecalPanelLinkChanged:
                    if (e.DecalPanel.linkedPanelID == "*")
                        return;
                    if (e.DecalPanel.MaterialSlot != MaterialSlot)
                        return;

                    //if one panel already has this link id then copy data from it
                    List<ManagerPanel> managers = ManagerPanels.Values.ToList();
                    foreach (ManagerPanel m in managers)
                    {
                        foreach (DecalPanel existing in m.DecalPanels)
                        {
                            if (e.DecalPanel != existing && existing.linkedPanelID == e.DecalPanel.linkedPanelID)
                            {
                                existing.CopyDataToTargetPanel(e.DecalPanel, false);
                                return;
                            }
                        }
                    }
                    break;

            }
        }

        public SelectionPanel(float width, float height, Color? color, Decal_Maker DM) : base(DM, 300)
        {
            //Image i = spacerLeft.gameObject.AddComponent<Image>();
            //i.material = new Material(Shader.Find("UI/Default-Overlay"));
            //i.material.color = Color.blue;

            DM.CoreEvent += CorePanelEvent;

            //create buttons to add/remove panels
            AddButton = DM.CreateButton("Add Image");
            RemoveButton = DM.CreateButton("Remove Image", true);


            gameObject.transform.SetParent(spacerLeft.transform, false);
            gameObject.transform.localPosition += new Vector3(270, 0, 0);
            //basePanel
            CreatePanelBackground(width, height, color);
            SetLayout(250, 250);

            AddButton.button.onClick.AddListener(() =>
            {
                RaiseCoreEvent(this, new PanelEventArgs(EventEnum.ManagerPanelButtonADD, ActivePanel));
            });
            RemoveButton.button.onClick.AddListener(() =>
            {
                RaiseCoreEvent(this, new PanelEventArgs(EventEnum.ManagerPanelButtonCLOSE, ActivePanel));
            });

            //CreateManagerPanels();

        }

        public void CreateManagerPanels()
        {
            //Torso panel
            TorsoPanel = new ManagerPanel(DM, null, BodyRegionEnum.Torso, MaterialSlot, IsNormalMap, linear);
            TorsoPanel.gameObject.transform.SetParent(gameObject.transform, false);


            //Genital panel
            GenitalPanel = new ManagerPanel(DM, null, BodyRegionEnum.Genitals, MaterialSlot, IsNormalMap, linear);
            GenitalPanel.gameObject.transform.SetParent(gameObject.transform, false);


            //Face panel
            FacelPanel = new ManagerPanel(DM, null, BodyRegionEnum.Face, MaterialSlot, IsNormalMap, linear);
            FacelPanel.gameObject.transform.SetParent(gameObject.transform, false);


            //Limbs panel
            LimbsPanel = new ManagerPanel(DM, null, BodyRegionEnum.Limbs, MaterialSlot, IsNormalMap, linear);
            LimbsPanel.gameObject.transform.SetParent(gameObject.transform, false);


            ActivePanel = TorsoPanel;
            ManagerPanels = new Dictionary<string, ManagerPanel>() {
                { BodyRegionEnum.Torso, TorsoPanel },
                { BodyRegionEnum.Genitals, GenitalPanel },
                { BodyRegionEnum.Face, FacelPanel },
                { BodyRegionEnum.Limbs, LimbsPanel }
            };
        }

        public void CreateManagerEyePanels()
        {
            SetLayout(200, 200);
            //Torso panel
            IrisPanel = new ManagerPanel(DM, null, BodyRegionEnum.EyeIris, MaterialSlot, IsNormalMap, linear);
            IrisPanel.gameObject.transform.SetParent(gameObject.transform, false);

            //Genital panel
            ScleraPanel = new ManagerPanel(DM, null, BodyRegionEnum.EyeSclera, MaterialSlot, IsNormalMap, linear);
            ScleraPanel.gameObject.transform.SetParent(gameObject.transform, false);

            //Face panel
            PupilPanel = new ManagerPanel(DM, null, BodyRegionEnum.EyePupil, MaterialSlot, IsNormalMap, linear);
            PupilPanel.gameObject.transform.SetParent(gameObject.transform, false);

            //Limbs panel
            ReflectionPanel = new ManagerPanel(DM, null, BodyRegionEnum.EyeReflection, MaterialSlot, IsNormalMap, linear);
            ReflectionPanel.gameObject.transform.SetParent(gameObject.transform, false);

            //cornea
            CorneaPanel = new ManagerPanel(DM, null, BodyRegionEnum.EyeCornea, MaterialSlot, IsNormalMap, linear);
            CorneaPanel.gameObject.transform.SetParent(gameObject.transform, false);

            ManagerPanels = new Dictionary<string, ManagerPanel>()
            {
                { BodyRegionEnum.EyeIris, IrisPanel },
                { BodyRegionEnum.EyeSclera, ScleraPanel },
                { BodyRegionEnum.EyePupil, PupilPanel },
                { BodyRegionEnum.EyeReflection, ReflectionPanel },
                { BodyRegionEnum.EyeCornea, CorneaPanel }
            };
        }

        public void ActivateWindow(bool value)
        {
            spacerLeft.gameObject.SetActive(value);
            spacerRight.gameObject.SetActive(value);
            AddButton.gameObject.SetActive(value);
            RemoveButton.gameObject.SetActive(value);
            ActivePanel.ActivateWindow(value);
        }

    }

}

