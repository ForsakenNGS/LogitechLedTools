using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LogitechLedTools
{
    using System.IO;
    using System.Runtime.CompilerServices;
    using HWND = IntPtr;

    class WebinterfaceNative
    {
        private static bool fullscreenNotice = false;

        private static SlimDX.Direct3D9.Direct3D                    _direct3D = null;
        private static Dictionary<IntPtr, SlimDX.Direct3D9.Device>  _direct3DDeviceCache = null;

        private delegate bool EnumWindowsProc(HWND hWnd, int lParam);
        
        [DllImport("user32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(HWND hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

        [DllImport("Kernel32.DLL")]
        private static extern long GetLastError();
        
        public static void Init()
        {
            if (_direct3D == null)
            {
                _direct3D = new SlimDX.Direct3D9.Direct3D();
            }
            if (_direct3DDeviceCache == null)
            {
                _direct3DDeviceCache = new Dictionary<IntPtr, SlimDX.Direct3D9.Device>();
            }
        }

        public static int GetForegroundWindowInt32()
        {
            return GetForegroundWindow().ToInt32();
        }

        public static IDictionary<UInt32, string> GetOpenWindows()
        {
            HWND shellWindow = GetShellWindow();
            Dictionary<UInt32, string> windows = new Dictionary<UInt32, string>();
            EnumWindows(delegate (HWND hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);
                string title = builder.ToString();

                windows[(UInt32)hWnd] = title;
                return true;

            }, 0);

            return windows;
        }

        public static RECT GetWindowRect(int handle)
        {
            RECT applicationRect;
            GetWindowRect((HWND)handle, out applicationRect);
            applicationRect.Right -= applicationRect.Left;
            applicationRect.Bottom -= applicationRect.Top;
            applicationRect.Left = 0;
            applicationRect.Top = 0;
            return applicationRect;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static Bitmap CaptureScreen(int handle, int Left, int Top, int Right, int Bottom)
        {
            RECT    region = new RECT(Left, Top, Right, Bottom);
            HWND    hWnd = (HWND)handle;
            Bitmap  bitmap = null;
            
            try
            {
                // We are only supporting the primary display adapter for Direct3D mode
                SlimDX.Direct3D9.AdapterInformation adapterInfo = _direct3D.Adapters.DefaultAdapter;
                SlimDX.Direct3D9.Device device;

                #region Get Direct3D Device
                // Retrieve the existing Direct3D device if we already created one for the given handle
                if (_direct3DDeviceCache.ContainsKey(hWnd))
                {
                    device = _direct3DDeviceCache[hWnd];
                }
                // We need to create a new device
                else
                {
                    // Setup the device creation parameters
                    SlimDX.Direct3D9.PresentParameters parameters = new SlimDX.Direct3D9.PresentParameters();
                    parameters.BackBufferFormat = adapterInfo.CurrentDisplayMode.Format;
                    RECT applicationRect;
                    GetWindowRect(hWnd, out applicationRect);
                    parameters.BackBufferHeight = applicationRect.Height;
                    parameters.BackBufferWidth = applicationRect.Width;
                    parameters.Multisample = SlimDX.Direct3D9.MultisampleType.None;
                    parameters.SwapEffect = SlimDX.Direct3D9.SwapEffect.Discard;
                    parameters.DeviceWindowHandle = hWnd;
                    parameters.PresentationInterval = SlimDX.Direct3D9.PresentInterval.Default;
                    parameters.FullScreenRefreshRateInHertz = 0;

                    // Create the Direct3D device
                    device = new SlimDX.Direct3D9.Device(_direct3D, adapterInfo.Adapter, SlimDX.Direct3D9.DeviceType.Hardware, hWnd, SlimDX.Direct3D9.CreateFlags.SoftwareVertexProcessing, parameters);
                    _direct3DDeviceCache.Add(hWnd, device);
                }
                #endregion

                // Capture the screen and copy the region into a Bitmap
                using (SlimDX.Direct3D9.Surface surface = SlimDX.Direct3D9.Surface.CreateOffscreenPlain(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, SlimDX.Direct3D9.Format.A8R8G8B8, SlimDX.Direct3D9.Pool.SystemMemory))
                {
                    device.GetFrontBufferData(0, surface);


                    // Update: thanks digitalutopia1 for pointing out that SlimDX have fixed a bug
                    // where they previously expected a RECT type structure for their Rectangle
                    bitmap = new Bitmap(SlimDX.Direct3D9.Surface.ToStream(surface, SlimDX.Direct3D9.ImageFileFormat.Bmp, new Rectangle(region.Left, region.Top, region.Width, region.Height)));
                    // Previous SlimDX bug workaround: new Rectangle(region.Left, region.Top, region.Right, region.Bottom)));
                    surface.Dispose();
                }

            }
            catch (SlimDX.Direct3D9.Direct3D9Exception exception)
            {
                if (exception.ResultCode.Name.Equals("D3DERR_DEVICELOST") && _direct3DDeviceCache.ContainsKey(hWnd))
                {
                    _direct3DDeviceCache[hWnd].Reset();
                    //_direct3DDeviceCache.Remove(hWnd);
                } else
                {
                    if (!fullscreenNotice)
                    {
                        fullscreenNotice = true;
                        System.Windows.Forms.MessageBox.Show("Fullscreen mode not supported yet! Please switch to 'Windowed (Fullscreen)' instead of 'Fullscreen' to avoid this issue.");
                    }
                    Console.WriteLine(exception.ToString());
                }
            }
            return bitmap;
        }

        public static long GetFileUpdateDate(string filename)
        {
            if (File.Exists(filename))
            {
                return File.GetLastWriteTime(filename).ToFileTime();
            } else
            {
                return 0;
            }
        }

    }
}
