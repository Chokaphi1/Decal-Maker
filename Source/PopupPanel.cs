using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    public class PopupPanel
    {
        private List<MyButton> buttons = new List<MyButton>();
        public GameObject gameObject = new GameObject("Panel");
        public Transform transform { get { return gameObject.transform; } }
        protected Decal_Maker DM;

        private MyButton UpButton;
        private MyButton DownButton;
        private MyButton PrimaryButton;
        private GameObject selectionPanel;
        private JSONStorableStringChooser jSONStorableStringChooser;
        public PopupPanel(Decal_Maker DM, JSONStorableStringChooser jSONStorableStringChooser)
        {
            this.DM = DM;
            this.jSONStorableStringChooser = jSONStorableStringChooser;
            jSONStorableStringChooser.setCallbackFunction += SelectionMadeJSSC;

            Image baseImage = gameObject.AddComponent<Image>();
            baseImage.material = new Material(Shader.Find("UI/Default-Overlay"));
            baseImage.color = Color.cyan;
            baseImage.rectTransform.sizeDelta = new Vector2(60, 60);

            Vector2 buttonSize = new Vector2(60, 60);
            PrimaryButton = new MyButton("", new Color(0.8f, 0.8f, 0.8f), gameObject.transform, new Vector3(0, 0, 0), buttonSize);
            PrimaryButton.buttonText.fontSize = 40;

            PrimaryButton.buttonText.text = jSONStorableStringChooser.val;
            selectionPanel = new GameObject();
            selectionPanel.transform.SetParent(gameObject.transform, false);


            Image selectionImage = selectionPanel.AddComponent<Image>();
            selectionImage.material = new Material(Shader.Find("UI/Default-Overlay"));
            selectionImage.color = Color.red;
            selectionImage.rectTransform.sizeDelta = new Vector2(60, 200);


            GridLayoutGroup glg = selectionPanel.AddComponent<GridLayoutGroup>();
            glg.cellSize = buttonSize;
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 1;
            glg.spacing = Vector2.zero;


            UpButton = new MyButton("", new Color(0.9f, 0.6f, 0.1f), selectionPanel.transform, new Vector3(0, 0, 0), buttonSize);
            Texture2D UpIcon = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Up-Icon.jpg");
            UpButton.SetIcon(UpIcon);
            UpButton.button.onClick.AddListener(() =>
            {
                int count = 0;
                foreach (MyButton b in buttons)
                {
                    b.buttonText.text = jSONStorableStringChooser.choices[count];
                    count++;
                }
            });


            for (int i = 0; i <= 5; i++)
            {
                MyButton button = new MyButton("", new Color(0.9f, 0.9f, 0.9f), selectionPanel.transform, new Vector3(0, 0, 0), buttonSize);

                button.buttonText.text = jSONStorableStringChooser.choices[i];
                buttons.Add(button);

                button.button.onClick.AddListener(() => { SelectionMadeButton(button.buttonText.text); });

            }

            DownButton = new MyButton("", new Color(0.1f, 0.6f, 0.8f), selectionPanel.transform, new Vector3(0, 0, 0), buttonSize);
            Texture2D DownIcon = DM.GetResource("Custom/Scripts/Chokaphi/VAM_Decal_Maker/Icons/Down-Icon.jpg");
            DownButton.SetIcon(DownIcon);
            DownButton.button.onClick.AddListener(() => { int count = 6; foreach (MyButton b in buttons) { b.buttonText.text = jSONStorableStringChooser.choices[count]; count++; } });

            selectionPanel.gameObject.SetActive(false);
            PrimaryButton.button.onClick.AddListener(() => { selectionPanel.gameObject.SetActive(!selectionPanel.gameObject.activeSelf); });


        }

        private void SelectionMadeButton(string selection)
        {
            PrimaryButton.buttonText.text = selection;
            selectionPanel.gameObject.SetActive(false);
            jSONStorableStringChooser.valNoCallback = selection;
        }

        private void SelectionMadeJSSC(string selection)
        {
            PrimaryButton.buttonText.text = selection;
            selectionPanel.gameObject.SetActive(false);
        }

    }
}
