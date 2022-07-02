using SimpleJSON;
using System;
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

            AddButton.button.onClick.AddListener(() => { AddDecalPanel(); });
            RemoveButton.button.onClick.AddListener(RemoveDecalPanel);

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
            ManagerPanels = new Dictionary<string, ManagerPanel>() { { BodyRegionEnum.Torso, TorsoPanel }, { BodyRegionEnum.Genitals, GenitalPanel }, { BodyRegionEnum.Face, FacelPanel }, { BodyRegionEnum.Limbs, LimbsPanel } };
        }

        public void ActivateWindow(bool value)
        {
            spacerLeft.gameObject.SetActive(value);
            spacerRight.gameObject.SetActive(value);
            AddButton.gameObject.SetActive(value);
            RemoveButton.gameObject.SetActive(value);
            ActivePanel.ActivateWindow(value);
        }

        public DecalPanel AddDecalPanel()
        {
            DecalPanel d = ActivePanel.AddDecalPanels();
            return d;
        }

        public void RemoveDecalPanel()
        {
            ActivePanel.RemoveDecalPanel();
        }

    }

}

