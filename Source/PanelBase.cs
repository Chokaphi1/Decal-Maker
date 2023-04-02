using System;
using UnityEngine;
using UnityEngine.UI;

namespace VAM_Decal_Maker
{
    public class PanelBase
    {
        public GameObject gameObject = new GameObject("Panel");
        public Transform transform { get { return gameObject.transform; } }
        protected Decal_Maker DM;

        public string PanelName { get; set; }
        public string TextureSlot { get; set; }
        public string MaterialSlot { get; set; }
        public bool IsNormalMap { get; set; }
        public bool linear { get; set; }


        protected void RegisterForCoreEvents(EventHandler<PanelEventArgs> eventHandler)
        {
            DM.CoreEvent += eventHandler;
        }
        protected void UnRegisterForCoreEvents(EventHandler<PanelEventArgs> eventHandler)
        {
            DM.CoreEvent -= eventHandler;
        }

        protected virtual void RaiseCoreEvent(object o, PanelEventArgs e)
        {
            DM.OnCoreChange(o, e);
        }

        public PanelBase(Decal_Maker DM)
        {
            this.DM = DM;
            DM.OnUpDateAction += Update;
            DM.OnDestroyAction += OnDestroy;
            //DM.CoreEvent += CoreEvent;
        }

        protected virtual void Update()
        {
            //if (Input.GetKeyDown("space"))
            //{
            //    SuperController.LogError("PRESSSSSSSS  ");
            //}
        }

        public virtual void OnDestroy()
        {
            DM.OnUpDateAction -= Update;
            DM.OnUpDateAction -= OnDestroy;
        }

        protected void CreatePanelBackground(float width, float height, Color? color)
        {
            //basePanel
            Image panelImage = gameObject.AddComponent<Image>();
            panelImage.material = new Material(Shader.Find("UI/Default-Overlay"));
            panelImage.color = color ?? new Color(.9f, .9f, .9f, 1);
            panelImage.rectTransform.sizeDelta = new Vector2(width, height);
        }

        protected virtual void SetLayout(float width, float height)
        {
            GridLayoutGroup glg = gameObject.GetComponent<GridLayoutGroup>();
            if (glg == null)
            {
                glg = gameObject.AddComponent<GridLayoutGroup>();
            }
            glg.cellSize = new Vector2(width, height);
            glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            glg.constraintCount = 1;
            glg.padding = new RectOffset(10, 10, 10, 10);
            glg.spacing = new Vector2(10, 0);
        }
    }
}