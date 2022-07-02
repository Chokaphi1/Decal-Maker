using SimpleJSON;
using System;
using UnityEngine;
using UnityEngine.Video;

namespace VAM_Decal_Maker
{
    //class that creates a custom eventArgs to be sent with events
    public class PanelEventArgs : EventArgs
    {
        public PanelEventArgs(string EventName)
        {
            this.EventName = EventName;
        }
        public PanelEventArgs(string EventName, bool Value)
        {
            this.EventName = EventName;
            this.Bool = Value;
        }
        public PanelEventArgs(string EventName, string Path)
        {
            this.EventName = EventName;
            this.Path = Path;
        }
        public PanelEventArgs(string EventName, Material Material)
        {
            this.EventName = EventName;
            this.Material = Material;
        }
        public PanelEventArgs(string EventName, Color Color)
        {
            this.EventName = EventName;
            this.Color = Color;
        }
        public PanelEventArgs(string EventName, DecalPanel DecalPanel)
        {
            this.EventName = EventName;
            this.DecalPanel = DecalPanel;
        }
        public PanelEventArgs(string EventName, ManagerPanel ManagerPanel)
        {
            this.EventName = EventName;
            this.ManagerPanel = ManagerPanel;
        }
        public PanelEventArgs(string EventName, SelectionPanel selectionPanel)
        {
            this.EventName = EventName;
            this.SelectionPanel = selectionPanel;
        }

        public PanelEventArgs(string EventName, JSONClass saveJSON)
        {
            this.EventName = EventName;
            this.saveJSON = saveJSON;
        }

        public PanelEventArgs(string EventName, VideoPlayer videoPlayer)
        {
            this.EventName = EventName;
            this.videoPlayer = videoPlayer;
        }

        public string materialSlot { get; set; }
        public string bodyRegion { get; set; }

        public JSONClass saveJSON { get; set; }
        public string EventName { get; }
        public Material Material { get; }
        public Color Color { get; }
        public DecalPanel DecalPanel { get; set; }
        public ImagePanel ImagePanel { get; set; }
        public ManagerPanel ManagerPanel { get; set; }
        public HeaderPanel HeaderPanel { get; set; }
        public SelectionPanel SelectionPanel { get; set; }
        public int NewPosition { get; set; }
        public bool Bool { get; set; }
        public string Path { get; set; }

        public VideoPlayer videoPlayer { get; set; }

    }


}

