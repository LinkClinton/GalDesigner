﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public static partial class Application
    {
        private static ValueManager<string> name = new ValueManager<string>(ChangeNameEvent);
        private static ValueManager<string> icon = new ValueManager<string>();
        private static ValueManager<Size> size = new ValueManager<Size>(ChangeSizeEvent);

        private static Position mousePosition = new Position();

        public static string Name { set => name.Value = value; get => name.Value; }
        public static Size Size { set => size.Value = value; get => size.Value; }

        public static event MouseMoveHandler MouseMove;
        public static event MouseClickHandler MouseClick;
        public static event MouseWheelHandler MouseWheel;
        public static event BoardClickHandler BoardClick;
        public static event UpdateHandler Update;

        private static void ChangeNameEvent(object owner, string oldName, string newName)
        {
            Systems.Windows.SetWindowText(newName);
        }

        private static void ChangeSizeEvent(object owner, Size oldSize, Size newSize)
        {
            Systems.Windows.SetWindowSize(newSize);
        }

        internal static void OnMouseMove(object sender, MouseMoveEvent eventArg)
        {
            mousePosition = eventArg.MousePosition;

            MouseMove?.Invoke(sender, eventArg);
        }

        internal static void OnMouseClick(object sender, MouseClickEvent eventArg)
        {
            eventArg.MousePosition = mousePosition;

            MouseClick?.Invoke(sender, eventArg);
        }

        internal static void OnMouseWheel(object sender, MouseWheelEvent eventArg)
        {
            eventArg.MousePosition = mousePosition;

            MouseWheel?.Invoke(sender, eventArg);
        }

        internal static void OnBoardClick(object sender, BoardClickEvent eventArg)
        {
            BoardClick?.Invoke(sender, eventArg);
        }

        internal static void OnUpdate(object sender)
        {
            name.Update(null);
            icon.Update(null);
            size.Update(null);

            Time.Update();

            Update?.Invoke(sender);

            GameScene.Main?.Update(Time.DeltaSeconds);

            Systems.Windows.PresentBitmap();
        }

        public static void Create(string Name, Size Size, string Icon)
        {
            name.Value = Name;
            size.Value = Size;
            icon.Value = Icon;

            Systems.Windows.CreateWindow(name.Value, size.Value, icon.Value);
        }

        public static void RunLoop()
        {
            while (Systems.Windows.UpdateWindow() is false)
            {
                OnUpdate(null);
            }
        }
    }
}