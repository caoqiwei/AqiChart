using System;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows;
using Point = System.Windows.Point;

namespace AqiChart.Client.ScreenshotTool
{
    public static class ScreenHelper
    {
        #region 屏幕信息获取

        public static List<Rect> GetAllScreens()
        {
            var screens = new List<Rect>();

            NativeMethods.MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor,
                ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
            {
                screens.Add(new Rect(
                    lprcMonitor.Left,
                    lprcMonitor.Top,
                    lprcMonitor.Width,
                    lprcMonitor.Height));
                return true;
            };

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);
            return screens;
        }

        public static Rect GetScreenFromPoint(Point point)
        {
            NativeMethods.POINT pt = new NativeMethods.POINT((int)point.X, (int)point.Y);
            IntPtr monitor = NativeMethods.MonitorFromPoint(pt, NativeMethods.MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero)
            {
                var monitorInfo = new NativeMethods.MONITORINFO();
                monitorInfo.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(monitorInfo);

                if (NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
                {
                    return new Rect(
                        monitorInfo.rcMonitor.Left,
                        monitorInfo.rcMonitor.Top,
                        monitorInfo.rcMonitor.Width,
                        monitorInfo.rcMonitor.Height);
                }
            }

            // 默认返回主屏幕
            return new Rect(0, 0,
                SystemParameters.PrimaryScreenWidth,
                SystemParameters.PrimaryScreenHeight);
        }

        public static Rect GetCurrentScreen()
        {
            NativeMethods.POINT cursorPos;
            NativeMethods.GetCursorPos(out cursorPos);
            return GetScreenFromPoint(new Point(cursorPos.X, cursorPos.Y));
        }

        public static Rect GetVirtualScreenBounds()
        {
            return new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);
        }

        #endregion

        #region 截图功能

        public static BitmapSource CaptureScreen(Rect screenRect)
        {
            return CaptureRegion(screenRect);
        }

        public static BitmapSource CaptureRegion(Rect region)
        {
            int width = (int)region.Width;
            int height = (int)region.Height;

            if (width <= 0 || height <= 0)
                return null;

            IntPtr screenDC = IntPtr.Zero;
            IntPtr memoryDC = IntPtr.Zero;
            IntPtr bitmap = IntPtr.Zero;
            IntPtr oldBitmap = IntPtr.Zero;

            try
            {
                // 获取屏幕DC
                screenDC = NativeMethods.GetDC(IntPtr.Zero);
                if (screenDC == IntPtr.Zero)
                    throw new Exception("无法获取屏幕设备上下文");

                // 创建内存DC
                memoryDC = NativeMethods.CreateCompatibleDC(screenDC);
                if (memoryDC == IntPtr.Zero)
                    throw new Exception("无法创建兼容的设备上下文");

                // 创建兼容位图
                bitmap = NativeMethods.CreateCompatibleBitmap(screenDC, width, height);
                if (bitmap == IntPtr.Zero)
                    throw new Exception("无法创建兼容位图");

                // 选择位图到内存DC
                oldBitmap = NativeMethods.SelectObject(memoryDC, bitmap);

                // 复制屏幕区域到位图
                bool success = NativeMethods.BitBlt(
                    memoryDC, 0, 0, width, height,
                    screenDC, (int)region.X, (int)region.Y,
                    CopyPixelOperation.SourceCopy);

                if (!success)
                    throw new Exception("屏幕复制失败");

                // 创建BitmapSource
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                return bitmapSource;
            }
            catch (Exception ex)
            {
                throw new Exception($"截图失败: {ex.Message}", ex);
            }
            finally
            {
                // 清理资源
                if (oldBitmap != IntPtr.Zero)
                    NativeMethods.SelectObject(memoryDC, oldBitmap);

                if (bitmap != IntPtr.Zero)
                    NativeMethods.DeleteObject(bitmap);

                if (memoryDC != IntPtr.Zero)
                    NativeMethods.DeleteDC(memoryDC);

                if (screenDC != IntPtr.Zero)
                    NativeMethods.ReleaseDC(IntPtr.Zero, screenDC);
            }
        }

        #endregion

        #region 工具方法

        public static Point GetCursorPosition()
        {
            NativeMethods.POINT point;
            NativeMethods.GetCursorPos(out point);
            return new Point(point.X, point.Y);
        }

        public static bool IsPointInScreen(Point point, Rect screen)
        {
            return point.X >= screen.Left && point.X <= screen.Right &&
                   point.Y >= screen.Top && point.Y <= screen.Bottom;
        }

        public static double GetScreenDpi()
        {
            using (var source = HwndSource.FromHwnd(IntPtr.Zero))
            {
                if (source?.CompositionTarget != null)
                {
                    var matrix = source.CompositionTarget.TransformToDevice;
                    return matrix.M11 * 96; // DPI缩放因子 * 96
                }
            }
            return 96; // 默认DPI
        }

        #endregion
    }

}
