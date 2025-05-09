// Copyright (c) 2022 namazso <admin@namazso.eu>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace ThemeManagerDemo
{
    [Flags]
    public enum InitializationFlags
    {
        ThemeInitNoFlags = 0,
        ThemeInitCurrentThemeOnly = 1 << 0,
        ThemeInitFlagUnk1 = 1 << 1,
        ThemeInitFlagUnk2 = 1 << 2,
    };

    [Flags]
    public enum ThemeApplyFlags
    {
        ThemeApplyFlagIgnoreBackground = 1 << 0,
        ThemeApplyFlagIgnoreCursor = 1 << 1,
        ThemeApplyFlagIgnoreDesktopIcons = 1 << 2,
        ThemeApplyFlagIgnoreColor = 1 << 3,
        ThemeApplyFlagIgnoreSound = 1 << 4,
        ThemeApplyFlagIgnoreScreensaver = 1 << 5,
        ThemeApplyFlagUnknown = 1 << 6, // something about window metrics
        ThemeApplyFlagUnknown2 = 1 << 7,
        ThemeApplyFlagNoHourglass = 1 << 8
    };

    [Flags]
    public enum ThemePackFlags
    {
        ThemepackFlagUnknown1 = 1 << 0, // setting this seems to supress hourglass
        ThemepackFlagUnknown2 = 1 << 1, // setting this seems to supress hourglass
        ThemepackFlagSilent = 1 << 2, // hides all dialogs and prevents sound
        ThemepackFlagRoamed = 1 << 3, // something about roaming
    };

    public enum DesktopWallpaperPosition
    {
    }

    public enum ThemeCategory
    {
    }

    [Guid("26e4185f-0528-475f-acaf-abe89ba6017d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITheme
    {
        public string DisplayName { get; set; }
        public string VisualStyle1 { get; set; }
        public string VisualStyle2 { get; set; }
    }

    [Guid("c1e8c83e-845d-4d95-81db-e283fdffc000")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IThemeManager2
    {
        void Init(InitializationFlags initFlags);
        void InitAsync(IntPtr hwnd, int unk1);
        void Refresh();
        void RefreshAsync(IntPtr hwnd, int unk1);
        void RefreshComplete();
        void GetThemeCount(out int count);
        void GetTheme(int index, out ITheme theme);
        void IsThemeDisabled(int index, out int disabled);
        void GetCurrentTheme(out int index);

        void SetCurrentTheme(IntPtr parent, int themeIndex, int applyNow, ThemeApplyFlags applyFlags,
            ThemePackFlags packFlags);

        void GetCustomTheme(out int index);
        void GetDefaultTheme(out int index);
        void CreateThemePack(IntPtr hwnd, string unk1, ThemePackFlags packFlags);
        void CloneAndSetCurrentTheme(IntPtr hwnd, string unk1, out string unk2);

        void InstallThemePack(IntPtr hwnd, string unk1, int unk2, ThemePackFlags packFlags, out string unk3,
            out ITheme unk4);

        void DeleteTheme(string unk1);
        void OpenTheme(IntPtr hwnd, string path, ThemePackFlags packFlags);
        void AddAndSelectTheme(IntPtr hwnd, string path, ThemeApplyFlags applyFlags, ThemePackFlags packFlags);
        void SQMCurrentTheme();
        void ExportRoamingThemeToStream(IStream stream, int unk1);
        void ImportRoamingThemeFromStream(IStream stream, int unk1);
        void UpdateColorSettingsForLogonUI();
        void GetDefaultThemeId(out Guid guid);
        void UpdateCustomTheme();
    }

    class Program
    {
        [DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall)]
        internal static extern int CoCreateInstance(
            [In, MarshalAs(UnmanagedType.LPStruct)]
            Guid rclsid,
            IntPtr pUnkOuter,
            uint dwClsContext,
            [In, MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object? ppv
        );

        static void Main()
        {
            Thread t = new Thread(
                () =>
                {
                    var hr = CoCreateInstance(
                        Guid.Parse("9324da94-50ec-4a14-a770-e90ca03e7c8f"),
                        IntPtr.Zero,
                        0x17,
                        typeof(IThemeManager2).GUID,
                        out var obj);
                    if (obj == null)
                    {
                        Console.WriteLine($"Cannot create IThemeManager2 instance: {hr:x8}!");
                        return;
                    }

                    var manager = (IThemeManager2)obj;
                    manager.Init(InitializationFlags.ThemeInitNoFlags);
                    manager.GetThemeCount(out var count);
                    Console.WriteLine($"Count: {count}");
                    for (var i = 0; i < count; i++)
                    {
                        manager.GetTheme(i, out var theme);
                        string? visualStyle = null;
                        try
                        {
                            var maybeVisualStyle = theme.VisualStyle1;
                            if (maybeVisualStyle.Contains("msstyle"))
                                visualStyle = maybeVisualStyle;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            var maybeVisualStyle = theme.VisualStyle1;
                            if (maybeVisualStyle.Contains("msstyle"))
                                visualStyle = maybeVisualStyle;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        Console.WriteLine(
                            $"Name: {theme.DisplayName} VisualStyle: {visualStyle}");
                    }
                });

            // shell crap is always STA
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();
        }
    }
}