using UnityEngine;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    public class MyButton
    {
        public GameObject gameObject = new GameObject("MyButton");
        public Transform transform { get { return gameObject.transform; } }
        public Image image { get; set; }
        public Button button { get; set; }
        public Text buttonText { get; set; }


        private Color normalColor;
        private Color unselectedColor;

        public void Selected(bool value)
        {
            if (value)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = normalColor;
                button.colors = colors;
            }
            else
            {
                ColorBlock colors = button.colors;
                colors.normalColor = unselectedColor;
                button.colors = colors;
            }
        }

        public MyButton(string label = "Button", Color? color = null, Transform parent = null)
        {
            if(parent != null)
                transform.SetParent(parent,false);

            //transform = gameObject.transform;
            RectTransform rt = gameObject.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160, 30);

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(gameObject.transform, false);

            RectTransform tRT = textGO.AddComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero;
            tRT.anchorMax = Vector2.one;
            tRT.sizeDelta = Vector2.zero;

            buttonText = textGO.AddComponent<Text>();
            buttonText.text = label;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;

            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 30;

            image = gameObject.AddComponent<Image>();
            image.material = new Material(Shader.Find("UI/Default-Overlay"));
            //image.color = color ?? Color.green;

            normalColor = color ?? new Color(0.6f, 0, 0, 1);
            button = gameObject.AddComponent<Button>();

            //convert color to HSV to allow easier lighten/darken operation
            float H, S, V;
            Color.RGBToHSV(normalColor, out H, out S, out V);
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.HSVToRGB(H, S, V * 1.4f);
            colors.normalColor = normalColor;
            colors.pressedColor = normalColor;// Color.HSVToRGB(H, S, V * 1.2f);
            unselectedColor = Color.HSVToRGB(H, S, V * 0.5f);

            button.colors = colors;

            Selected(false);
        }

        public void SetIcon(Texture2D icon)
        {
            image.material.mainTexture = icon;
        }
    }


}

