﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    public class EngineWindow : EventEmitter
    {
        private IntPtr mHandle;
        private APILibrary.Win32.Internal.WndProc mWndProc;

        public string Name { get; private set; }
        public string Icon { get; }

        public Size<int> Size { get; private set; }
        public Position<int> Position { get; private set; }

        public bool IsExisted { get; set; }

        public event UpdateEventHandler OnUpdateEvent;
        public event RenderEventHandler OnRenderEvent;
        public event KeyBoardEventHandler OnKeyBoardEvent;
        public event MouseMoveEventHandler OnMouseMoveEvent;
        public event MouseClickEventHandler OnMouseClickEvent;
        public event MouseWheelEventHandler OnMouseWhellEvent;
        public event SizeChangeEventHandler OnSizeChangeEvent;

        private IntPtr WindowProc(IntPtr Hwnd, uint message,
           IntPtr wParam, IntPtr lParam)
        {
            //record old size
            Size<int> oldSize = Size;

            //get window size
            Size<int> getSize()
            {
                return new Size<int>(
                    APILibrary.Win32.Message.LowWord(lParam),
                    APILibrary.Win32.Message.HighWord(lParam));
            }

            switch ((APILibrary.Win32.WinMsg)message)
            {
                case APILibrary.Win32.WinMsg.WM_DESTROY: APILibrary.Win32.Internal.PostQuitMessage(0); break;
                //catch the size event, because the order, we record old size
                case APILibrary.Win32.WinMsg.WM_SIZE: SenderEvent(new SizeChangeEvent(DateTime.Now, oldSize, Size = getSize())); break;
                default: break;
            }

            return APILibrary.Win32.Internal.DefWindowProc(Hwnd, message, wParam, lParam);
        }

        private void PumpMessage()
        {
            var message = new APILibrary.Win32.Message();

            //peek message
            while (APILibrary.Win32.Internal.PeekMessage(out message, IntPtr.Zero, 0, 0, 
                (int)APILibrary.Win32.PeekMessageFlags.PM_REMOVE) == true)
            {
                //translate and dispatch message
                APILibrary.Win32.Internal.TranslateMessage(ref message);
                APILibrary.Win32.Internal.DispatchMessage(ref message);

                //catch and tranlate message
                CatchMessage(message);

                //quit, the window is not existed
                if (message.type == (uint)APILibrary.Win32.WinMsg.WM_QUIT) IsExisted = false;
            }
        }

        private void CatchMessage(APILibrary.Win32.Message message)
        {
            //get mouse position
            Position<int> mousePosition()
            {
                return new Position<int>(
                    APILibrary.Win32.Message.GetXFromLparam(message.lParam),
                    APILibrary.Win32.Message.GetYFromLparam(message.lParam));
            }

            //get mouse wheel scroll offset
            int mouseWheelScrollOffset()
            {
                return APILibrary.Win32.Message.HighWord(message.wParam);
            }

            switch ((APILibrary.Win32.WinMsg)message.type)
            {
                //sender event
                case APILibrary.Win32.WinMsg.WM_KEYUP: SenderEvent(new KeyBoardEvent(DateTime.Now, (KeyCode)message.wParam, false)); break;
                case APILibrary.Win32.WinMsg.WM_KEYDOWN: SenderEvent(new KeyBoardEvent(DateTime.Now, (KeyCode)message.wParam, true)); break;
                case APILibrary.Win32.WinMsg.WM_MOUSEMOVE: SenderEvent(new MouseMoveEvent(DateTime.Now, mousePosition())); break;
                case APILibrary.Win32.WinMsg.WM_LBUTTONUP: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Left, false)); break;
                case APILibrary.Win32.WinMsg.WM_MBUTTONUP: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Middle, false)); break;
                case APILibrary.Win32.WinMsg.WM_RBUTTONUP: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Right, false)); break;
                case APILibrary.Win32.WinMsg.WM_MOUSEWHEEL: SenderEvent(new MouseWheelEvent(DateTime.Now, mousePosition(), mouseWheelScrollOffset())); break;
                case APILibrary.Win32.WinMsg.WM_LBUTTONDOWN: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Left, true)); break;
                case APILibrary.Win32.WinMsg.WM_MBUTTONDOWN: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Middle, true)); break;
                case APILibrary.Win32.WinMsg.WM_RBUTTONDOWN: SenderEvent(new MouseClickEvent(DateTime.Now, mousePosition(), MouseButton.Right, true)); break;
                default: break;
            }
        }

        public EngineWindow(string name, string icon, Size<int> size)
        {
            Name = name; Icon = icon;
            Size = size;

            mWndProc = WindowProc;

            LogEmitter.Apply(LogLevel.Information, "[Start Create Window] [Name = {0}] from [EngineWindow]", Name);

            //get instance
            var hInstance = APILibrary.Win32.Internal.GetModuleHandle(null);

            //create app info(WNDCLASS)s, default setting
            var appInfo = new APILibrary.Win32.AppInfo()
            {
                style = (uint)APILibrary.Win32.AppInfoStyle.CS_DBLCLKS,
                lpfnWndProc = mWndProc,
                cbClsExtra = 0,
                cbWndExtra = 0,
                hInstance = hInstance,
                hIcon = APILibrary.Win32.Internal.LoadImage(IntPtr.Zero, Icon, 1, 0, 0, 0x00000010),
                hbrBackground = APILibrary.Win32.Internal.GetStockObject(0),
                hCursor = APILibrary.Win32.Internal.LoadCursor(IntPtr.Zero, (uint)APILibrary.Win32.CursorType.IDC_ARROW),
                lpszMenuName = null,
                lpszClassName = Name
            };

            //register app info(WNDCLASS)
            APILibrary.Win32.Internal.RegisterAppinfo(ref appInfo);

            APILibrary.Win32.Rect rect = new APILibrary.Win32.Rect()
            {
                top = 0, left = 0, right = Size.Width, bottom = Size.Height
            };

            //get real rect
            APILibrary.Win32.Internal.AdjustWindowRect(ref rect, (uint)APILibrary.Win32.WindowStyles.WS_OVERLAPPEDWINDOW, false);

            //create window
            mHandle = APILibrary.Win32.Internal.CreateWindowEx(0, Name, Name,
                (uint)APILibrary.Win32.WindowStyles.WS_OVERLAPPEDWINDOW, 0x80000000, 0x80000000,
                rect.right - rect.left, rect.bottom - rect.top, IntPtr.Zero, IntPtr.Zero,
                hInstance, IntPtr.Zero);

            LogEmitter.Apply(LogLevel.Information, "[Finish Create Window] [Width = {0}] [Height = {1}] from [EngineWindow]",
                Size.Width, Size.Height);

            //get window rect property
            APILibrary.Win32.Internal.GetWindowRect(mHandle, ref rect);

            //set position and size
            Position = new Position<int>(rect.left, rect.top);
            Size = new Size<int>(rect.right - rect.left, rect.bottom - rect.top);
            IsExisted = true;
        }

        public void Update(float deltaTime)
        {
            //pump message
            PumpMessage();

            SenderEvent(new UpdateEvent(DateTime.Now, deltaTime));
            SenderEvent(new RenderEvent(DateTime.Now, deltaTime));

            //process the event
            while (EventCount != 0)
            {
                switch (GetEvent(true))
                {
                    case UpdateEvent update: OnUpdateEvent?.Invoke(this, update); break;
                    case RenderEvent render: OnRenderEvent?.Invoke(this, render); break;
                    case KeyBoardEvent keyBoard: OnKeyBoardEvent?.Invoke(this, keyBoard); break;
                    case MouseClickEvent mouseClick: OnMouseClickEvent?.Invoke(this, mouseClick); break;
                    case MouseWheelEvent mouseWheel: OnMouseWhellEvent?.Invoke(this, mouseWheel); break;
                    case MouseMoveEvent mouseMove: OnMouseMoveEvent?.Invoke(this, mouseMove); break;
                    case SizeChangeEvent sizeChange: OnSizeChangeEvent?.Invoke(this, sizeChange); break;
                }
            }
        }

        public void Show()
        {
            APILibrary.Win32.Internal.ShowWindow(mHandle, (int)APILibrary.Win32.ShowWindowStyles.SW_SHOW);
        }

        public void Hide()
        {
            APILibrary.Win32.Internal.ShowWindow(mHandle, (int)APILibrary.Win32.ShowWindowStyles.SW_HIDE);
        }

        public void SetName(string name)
        {
            APILibrary.Win32.Internal.SetWindowText(mHandle, Name = name);
        }

        public void SetSize(Size<int> size)
        {
            Size = size;

            APILibrary.Win32.Internal.SetWindowPos(mHandle, IntPtr.Zero, 0, 0,
                Size.Width, Size.Height, (uint)
                (APILibrary.Win32.SetWindowPosFlags.SWP_NOMOVE | 
                APILibrary.Win32.SetWindowPosFlags.SWP_NOZORDER));
        }

        public void SetPosition(Position<int> position)
        {
            Position = position;

            APILibrary.Win32.Internal.SetWindowPos(mHandle, IntPtr.Zero, Position.X, Position.Y,
                0, 0, (uint)(APILibrary.Win32.SetWindowPosFlags.SWP_NOSIZE |
                APILibrary.Win32.SetWindowPosFlags.SWP_NOZORDER));
        }
    }
}
