using MVR.FileManagementSecure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static VAM_Decal_Maker.PathHelper;

namespace VAM_Decal_Maker
{
    public class PrefabPanel : UIPanelBase
    {
        private ImagePanel ImagePanel;
        private Dictionary<string, List<PrefabCollection>> prefabFiles = new Dictionary<string, List<PrefabCollection>>();
        private string selectedType;
        private string selectedItem;
        private List<string> author = new List<string>();
        private List<string> prefabTypes = new List<string>();
        private List<string> prefabNames = new List<string>();
        private JSONStorableStringChooser jsc;
        private JSONStorableStringChooser jsc2;
        public PrefabPanel(Decal_Maker DM) : base(DM)
        {

            ImagePanel = new ImagePanel(DM);
            ImagePanel.gameObject.transform.SetParent(spacerRight.transform, false);
            RectTransform imageRect = ImagePanel.rectTransform;
            imageRect.sizeDelta = new Vector2(500, 500);

            GridLayoutGroup glg = spacerLeft.gameObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(500, 100);

            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 1;
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.spacing = new Vector2(10, 0);


            Transform popupPrefabType = GameObject.Instantiate<Transform>(base.DM.manager.configurableScrollablePopupPrefab);
            popupPrefabType.SetParent(spacerLeft.gameObject.transform, false);
            popupPrefabType.localPosition = Vector3.zero;

            UIDynamicPopup dynamicPopupType = popupPrefabType.GetComponent<UIDynamicPopup>();
            dynamicPopupType.name = "CLASS";
            //popupToJSONStorableStringChooser.Add(PrefabType, jsc);
            jsc = new JSONStorableStringChooser("name", prefabTypes, "", "Choose Type");
            jsc.popup = dynamicPopupType.popup;


            Transform popupPrefabItem = GameObject.Instantiate<Transform>(base.DM.manager.configurableScrollablePopupPrefab);
            popupPrefabItem.SetParent(spacerLeft.gameObject.transform, false);
            popupPrefabItem.localPosition = Vector3.zero;

            UIDynamicPopup dynamicPopupItem = popupPrefabItem.GetComponent<UIDynamicPopup>();
            dynamicPopupItem.name = "ITEM";
            dynamicPopupItem.popup.showSlider = false;

            RefreshPrefabCollection();
            jsc2 = new JSONStorableStringChooser("name", prefabNames, "", "Choose Prefab");
            jsc2.popup = dynamicPopupItem.popup;

            dynamicPopupType.popup.onValueChangeUnityEvent.AddListener(TypeValueChanges);
            dynamicPopupItem.popup.onValueChangeUnityEvent.AddListener(ItemValueChanged);

            ImagePanel.Button.onClick.AddListener(() =>
            {
                DM.OnCoreChange(this, new PanelEventArgs(EventEnum.CoreTempDecalToPerm));
            });

            GameObject imageObject = new GameObject("text background");
            imageObject.transform.SetParent(ImagePanel.gameObject.transform, false);
            Image ima = imageObject.AddComponent<Image>();
            ima.color = new Color(.8f, .8f, .8f);
            RectTransform imRT = imageObject.GetComponent<RectTransform>();
            imageObject.transform.localPosition += new Vector3(0, -5f, 0);
            imRT.anchorMin = Vector2.zero;
            imRT.anchorMax = new Vector2(1, 0.1f);
            imRT.sizeDelta = Vector2.zero;


            //Button Text
            GameObject textObject = new GameObject("button text");
            textObject.transform.SetParent(imageObject.gameObject.transform, false);
            //used to prevent text object from blocking clicks
            CanvasGroup cg = textObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;

            Text panelText = textObject.AddComponent<Text>();
            panelText.alignment = TextAnchor.MiddleCenter;
            panelText.color = Color.black;
            panelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            panelText.fontSize = 30;
            panelText.fontStyle = FontStyle.Bold;
            panelText.text = "Make current Prefab Permanent";

            RectTransform ptRT = panelText.GetComponent<RectTransform>();
            ptRT.anchorMin = new Vector2(0, .02f);
            ptRT.anchorMax = Vector2.one;
            ptRT.sizeDelta = Vector2.zero;

        }

        private void TypeValueChanges(string value)
        {
            selectedType = value;
            RefreshPrefabCollection();
            List<PrefabCollection> a = prefabFiles[value];
            prefabNames = a.Select(x => x.ItemName).ToList();
            prefabNames.Insert(0, "");
            jsc2.SetVal("");
            ResetItem();
            jsc2.choices = prefabNames;
        }

        private void ItemValueChanged(string value)
        {
            selectedItem = value;
            DM.OnCoreChange(this, new PanelEventArgs(EventEnum.CoreRemoveTempPanels));
            if (value == "")
            {
                ResetItem();
            }
            else
            {
                PrefabCollection c = prefabFiles[selectedType].Where(x => x.ItemName == value).FirstOrDefault();
                //reset decals added from other prefabs
                if (c.ImagePath != null)
                    ImagePanel.LoadResourceFile(c.ImagePath);

                DM.PresetLoad(c.Path, true, c.Package);
            }
        }

        private void ResetItem()
        {
            DM.OnCoreChange(this, new PanelEventArgs(EventEnum.CoreRemoveTempPanels));
            ImagePanel.HideTexture();
            DM.PresetLoad(GetPackagePath(DM) + "Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/EMPTY.dsgn.DecalMakerPreset.json", true);
        }

        public void RefreshPrefabCollection()
        {
            string path = FileManagerSecure.NormalizePath("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Presets/PreFabs");
            if (!FileManagerSecure.DirectoryExists(path))
                return;

            prefabFiles = new Dictionary<string, List<PrefabCollection>>();


            List<string> files = GetFilesAtPathRecursive(path, null);

            List<ShortCut> shortCuts = FileManagerSecure.GetShortCutsForDirectory(path);

            foreach (ShortCut s in shortCuts)
            {
                if (s.isLatest && s.package != "" && s.package != "AddonPackages Flattened" && s.package != "AddonPackages Filtered")
                {
                    List<string> temp = GetFilesAtPathRecursive(s.path, null);
                    foreach (string SCFile in temp)
                    {
                        PrefabCollection pc = new PrefabCollection(SCFile, s.package);
                        if (pc.ItemType != null)
                        {
                            if (prefabFiles.ContainsKey(pc.ItemType))
                            {
                                prefabFiles[pc.ItemType].Add(pc);
                            }
                            else
                            {
                                prefabFiles.Add(pc.ItemType, new List<PrefabCollection>() { pc });
                            }
                        }
                    }
                }
            }


            foreach (string file in files)
            {
                PrefabCollection pc = new PrefabCollection(file);
                if (pc.ItemType != null)
                {
                    if (prefabFiles.ContainsKey(pc.ItemType))
                    {
                        prefabFiles[pc.ItemType].Add(pc);
                    }
                    else
                    {
                        prefabFiles.Add(pc.ItemType, new List<PrefabCollection>() { pc });
                    }
                }
            }
            prefabTypes = prefabFiles.Keys.ToList();
            jsc.choices = prefabTypes;
        }

        public void OnClick()
        {
            bool state = !spacerLeft.gameObject.activeSelf;
            spacerLeft.gameObject.SetActive(state);
            spacerRight.gameObject.SetActive(state);
            RefreshPrefabCollection();
        }
    }


}

