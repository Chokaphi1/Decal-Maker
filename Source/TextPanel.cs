using UnityEngine;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    //create text gameobjects

    public class TextPanel
    {
        public string text { get { return TextObject.text; } set { TextObject.text = value; } }
        public TextAnchor alignment { get { return TextObject.alignment; } set { TextObject.alignment = value; } }
        public Color color { get { return TextObject.color; } set { TextObject.color = value; } }
        public Font font { get { return TextObject.font; } set { TextObject.font = value; } }
        public FontStyle fontStyle { get { return TextObject.fontStyle; } set { TextObject.fontStyle = value; } }
        public int fontSize { get { return TextObject.fontSize; } set { TextObject.fontSize = value; } }
        public RectTransform rectTransform { get; }
        public Vector2 anchorMin { get { return rectTransform.anchorMin; } set { rectTransform.anchorMin = value; } }
        public Vector2 anchorMax { get { return rectTransform.anchorMax; } set { rectTransform.anchorMax = value; } }
        public Vector2 sizeDelta { get { return rectTransform.sizeDelta; } set { rectTransform.sizeDelta = value; } }
        //used to prevent text object from blocking clicks
        public bool blocksRaycasts { get; set; }

        private GameObject gameObject;
        private CanvasGroup canvasGroup;
        private Text TextObject;
        public TextPanel(GameObject parentObject, Vector3 localPosition, string description = null)
        {
            gameObject = new GameObject(description);
            rectTransform = gameObject.AddComponent<RectTransform>();
            gameObject.transform.SetParent(parentObject.transform, false);
            if (localPosition != null)
                gameObject.transform.localPosition = localPosition;

            TextObject = gameObject.AddComponent<Text>();

            font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
        }

    }
}
