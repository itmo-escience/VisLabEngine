using System;
using System.Collections.Generic;
using System.Linq;
using FusionUI.UI.Factories;
using Fusion.Core.Mathematics;
using Fusion.Engine.Frames;

namespace FusionUI.UI.Elements
{
    public class TabViewer : ScalableFrame
    {
        protected FrameProcessor ui;

        public float WidthTapLabel = UIConfig.UnitDefaultTapWidth;
        public float HeightTapLabel = UIConfig.UnitDefaultTapHeight;

        protected Color ActiveColor = Color.Zero;
        protected Color PassiveColor = Color.Zero;

        protected Color ActiveColorText = Color.White;
        protected Color PassiveColorText = new Color(1, 1, 1, 0.5f);

        protected Color BorderColorActive = UIConfig.ActiveColor;


        public Frame ActiveLabel;
        protected Frame ActiveFrame;

        public Button addTabButton;

        public Action<string> OnChangeTab;
        public Action<string> OnRemoveTab;
        public Action<string, string> OnRenameTab;

        public string RemoveWindowCaption = "Remove tab?";
        public string RenameWindowCaption = "Rename tab?";
        public string RemoveWindowText = "Are you sure you want to remove this tab?";
        public string RenameWindowText = "Do you want to rename this tab?";

        protected Dictionary<string, TabElement> listTab;


        public  TabViewer(FrameProcessor ui, float x, float y, float w, float h, string text, Color backColor) : base(ui, x, y, w, h, text, backColor)
        {
            this.ui = ui;
            listTab = new Dictionary<string, TabElement>();
        }

        public void addAdderTabButton(float size)
        {
            addTabButton = new Button(ui, 0, 0, size, size, "+", Color.Zero, Color.Zero)
            {
//                Border = 1,
                TextAlignment = Alignment.MiddleCenter
            };
            this.Add(addTabButton);
        }

        public void addTab(string name, ScalableFrame frame)
        {
            if (listTab.ContainsKey(name))
            {
                name += listTab.Count;
            }
            float WidthTapLabel = Math.Max(this.WidthTapLabel,
                UIConfig.FontSubtitle[ApplicationInterface.uiScale].MeasureStringF(name).Width / ScaleMultiplier + 2 * UIConfig.UnitTabTextOffsetX);
            var xPosition =  listTab.Count * WidthTapLabel;
            if (addTabButton != null)
            {
                xPosition = addTabButton.UnitX;
                addTabButton.UnitX += WidthTapLabel;
            }
            
            var newTabLabel = new ScalableFrame(ui, xPosition, 0, WidthTapLabel, HeightTapLabel, name, Color.Zero)
            {
                BackColor = PassiveColor,
                ForeColor = PassiveColorText,
                TextAlignment  = Alignment.MiddleCenter,
                FontHolder = UIConfig.FontSubtitle,
                //UnitTextOffsetX = 10
                Name = name,
            };
            newTabLabel.ActionClick += (ControlActionArgs args, ref bool flag) =>
            {
//                if (!args.IsClick) return;
                if (args.IsAltClick)
                {
                    ScalableFrame popup = null;
                    if (name == "Sensors") return;
                    ScalableFrame menu = null;
                    List<Button> buttons = null;
                    ui.RootFrame.Add(menu = ContextMenuFactory.ContextMenu(ui,
                        Game.Mouse.Position.X / ApplicationInterface.ScaleMod,
                        Game.Mouse.Position.Y / ApplicationInterface.ScaleMod,
                        40, UIConfig.UnitMenuButtonHeight, new List<Tuple<string, Action>>()
                        {
                            new Tuple<string, Action>("Rename", () =>
                            {
                                ui.RootFrame.Add(popup = PopupFactory.RenamePopupWindow(ui, RenameWindowCaption,
                                    RenameWindowText, "Yes", "No", (s) =>
                                    {
                                        renameTab(newTabLabel.Text, s);
                                        popup.Clear(popup);
                                        popup.Clean();
                                        ui.RootFrame.Remove(popup);
                                    }, () =>
                                    {
                                        popup.Clear(popup);
                                        popup.Clean();
                                        ui.RootFrame.Remove(popup);
                                    }));
                            }),
                            new Tuple<string, Action>("Delete", () =>
                            {
                                ui.RootFrame.Add(popup = PopupFactory.ConfirmationPopupWindow(ui, RemoveWindowCaption,
                                    RemoveWindowText, "Yes", "No", () =>
                                    {
                                        removeTab(newTabLabel.Text);
                                        popup.Clear(popup);
                                        popup.Clean();
                                        ui.RootFrame.Remove(popup);
                                    }, () =>
                                    {
                                        popup.Clear(popup);
                                        popup.Clean();
                                        ui.RootFrame.Remove(popup);
                                    }));
                            })
                        }, out buttons));
                }
                else if (args.IsClick)
                {
                    unlockTap();
                    setActiveTap(newTabLabel, frame);
                }
            };
            frame.Name = name;
            frame.UnitX = 0;
            frame.UnitY = HeightTapLabel;
            if (listTab.Count == 0)
            {
                setActiveTap(newTabLabel, frame);
            }
            else
            {
                frame.Visible = false;
            }

            this.Add(newTabLabel);
            this.Add(frame);
            listTab.Add(name, new TabElement(newTabLabel, frame));
        }

        public void removeTab(string name)
        {
            // TODO: need shift another tabs            
            var tabInfo = listTab[name];
            if (ActiveLabel == tabInfo.Label)
            {
                setActiveTap(listTab.Values.First().Label, listTab.Values.First().Frame);
            }
            this.Remove(tabInfo.Label);
            this.Remove(tabInfo.Frame);
            listTab.Remove(name);

            foreach (var label in listTab.Values.Select(l => l.Label))
            {
                if (label.UnitX > tabInfo.Label.UnitX)
                {
                    label.UnitX -= tabInfo.Label.UnitWidth;
                }
            }
            addTabButton.UnitX -= tabInfo.Label.UnitWidth;
            OnRemoveTab.Invoke(name);
        }

        public void renameTab(string oldName, string newName)
        {
            var tabInfo = listTab[oldName];
            while (listTab.ContainsKey(newName)) newName += (1);
            tabInfo.Label.Text = newName;
            listTab.Remove(oldName);
            listTab[newName] = tabInfo;
            OnRenameTab?.Invoke(oldName, newName);
        }

        protected void setActiveTap(Frame label, Frame frame)
        {
            this.Add(frame);
            frame.Visible = true;
            label.BackColor = ActiveColor;
            label.ForeColor = ActiveColorText;
            label.BorderBottom = 5;
            label.BorderColor = BorderColorActive;
            ActiveLabel = label;
            ActiveFrame = frame;
            // action
            OnChangeTab?.Invoke(label.Text);
            
        }


	    public void SetActiveTab(string tabName)
	    {
		    TabElement tab;
		    if (listTab.TryGetValue(tabName, out tab)) {
			    unlockTap();
				setActiveTap(tab.Label, tab.Frame);
		    }
	    }


        protected void unlockTap()
        {
            this.Remove(ActiveFrame);
            ActiveFrame.Visible = false;
            ActiveLabel.BackColor = PassiveColor;
            ActiveLabel.ForeColor = PassiveColorText;
            ActiveLabel.BorderBottom = 0;
            ActiveLabel.BorderColor = Color.Zero;
        }

    }

    public class TabElement {
        public ScalableFrame Label;
        public ScalableFrame Frame;

        public TabElement(ScalableFrame newTabLabel, ScalableFrame scalableFrame)
        {
            Label = newTabLabel;
            Frame = scalableFrame;
        }
    }
}
