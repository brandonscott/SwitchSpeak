﻿/* RazerAPI.cs
 *
 * Copyright © 2013 by Adam Hellberg and Brandon Scott.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
 * of the Software, and to permit persons to whom the Software is furnished to do
 * so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Disclaimer: SharpBlade is in no way affiliated
 * with Razer and/or any of its employees and/or licensors.
 * Adam Hellberg does not take responsibility for any harm caused, direct
 * or indirect, to any Razer peripherals via the use of SharpBlade.
 * 
 * "Razer" is a trademark of Razer USA Ltd.
 */

// Credits to itsbth for helping with P/Invoke

/* ╔══════════════════════════════╗
 * ║ Help table for mapping types ║
 * ╠════════╦═════════╦═══════════╩──────────┐
 * │ C Type │ C# Type │ MarshalAs            │
 * ├────────┼─────────┼──────────────────────┤
 * │ BYTE   │ Byte    │                      │
 * │ UINT   │ UInt32  │                      │
 * │ HWND   │ IntPtr  │                      │
 * │ WORD   │ UInt16  │                      │
 * │ DWORD  │ UInt32  │                      │
 * │ LPARAM │ Int32   │                      │
 * │ WPARAM │ UInt32  │                      │
 * │ LPWSTR │ String  │ UnmanagedType.LPWStr │
 * └────────┴─────────┴──────────────────────┘
 */

// 2013-04-05: Major update to reflect changes in the new SDK.

using System;
using System.Runtime.InteropServices;

namespace Sharparam.SharpBlade.Native
{
    /// <summary>
    /// Static class containing all functions
    /// provided by the Razer SwitchBlade UI SDK.
    /// </summary>
    public static class RazerAPI
    {
        // Native functions from SwitchBladeSDK32.dll, all functions are __cdecl calls

        #region File Constants

        /// <summary>
        /// The DLL file containing the SDK functions.
        /// </summary>
        /// <remarks>Must be located in the system PATH.</remarks>
        public const string DllName = "RzSwitchbladeSDK2.dll";

        #endregion File Constants

        #region Functions

        /// <summary>
        /// Grants access to the Switchblade device, establishing application connections.
        /// </summary>
        /// <remarks>
        /// RzSBStart sets up the connections that allow an application to access the Switchblade hardware device.
        /// This routine returns RZSB_OK on success, granting the calling application control of the device.
        /// Subsequent calls to this routine prior to a matching RzSBStop call are ignored.
        /// RzSBStart must be called before other Switchblade SDK routines will succeed.
        /// RzSBStart must always be accompanied by an RzSBStop.
        /// COM initialization should be called prior to calling RzSBStart.
        /// If the application developer intends to use Single-Threaded Apartment model (STA) and call the SDK
        /// functions within the same thread where the COM was initialized, then CoInitialize() should be called
        /// before RzSBStart. Note that some MFC controls automatically initializes to STA.
        /// If the application developer intends to call the SDK functions on different threads,
        /// then the CoInitializeEx() should be called before RzSBStart.
        /// Note: When the RzSBStart() is called without the COM being initialized (e.g. thru calling CoInitializeEx)
        /// the SDK initializes the COM to Multi-Threaded Apartment (MTA) model.
        /// As such, callers must invoke SDK functions from an MTA thread.
        /// Future SDK versions will move these calls into an isolated STA, giving application developers additional
        /// freedom to use COM in any fashion.
        /// However, application developers may already implement their own processing to isolate the SDK
        /// initialization and calls to avoid the issues for COM in different threading models.
        /// </remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBStart();

        /// <summary>
        /// Cleans up the Switchblade device connections and releases it for other applications.
        /// </summary>
        /// <remarks>
        /// RzSBStop cleans up the connections made by RzSBStart.
        /// This routine releases an application’s control of the Switchblade hardware device,
        /// allowing other applications to take control.
        /// Subsequent calls to this routine prior to a matching RzSBStart are ignored.
        /// If an application terminates after calling RzSBStart without a matching call to RzSBStop,
        /// other applications may fail to acquire control of the Switchblade device.
        /// In this case, manually kill the framework processes.
        /// </remarks>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern void RzSBStop();
        
        /// <summary>
        /// Collects information about the SDK and the hardware supported.
        /// </summary>
        /// <param name="pSBSDKCap">
        /// A pointer to a previously allocated structure of type <see cref="Capabilities"/>.
        /// On successful execution, this routine fills the parameters in pSBSDKCap with the
        /// proper information about the SDK and supported hardware.
        /// </param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBQueryCapabilities(out Capabilities pSBSDKCap);

        /// <summary>
        /// Controls output to the Switchblade display.
        /// The application can send bitmap data buffer directly to the Switchblade trackpad
        /// display thru this function providing are more direct and faster way of updating the display.
        /// </summary>
        /// <param name="dwTarget">
        /// Specifies the target location on the Switchblade display – the main display or one of the dynamic key areas.
        /// Please refer to the definition for <see cref="TargetDisplay" /> for accepted values.
        /// </param>
        /// <param name="bufferParams">
        /// A pointer to a buffer parameter structure of type <see cref="BufferParams" /> that
        /// must be filled with the appropriate information for the image being sent to the render buffer.
        /// This input parameter is an RGB565 bitmap image buffer with a bottom-up orientation.
        /// Please refer to the definition for <see cref="BufferParams" /> for further detail.
        /// </param>
        /// <remarks>
        /// Since the function accepts the buffer for bottom-up bitmap,
        /// the application should invert the original image along its vertical axis prior to calling the function.
        /// This can be done easily with BitBlit and StretchBlt APIs.
        /// </remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBRenderBuffer([In] TargetDisplay dwTarget, [In] /*ref BufferParams*/ IntPtr bufferParams);

        /// <summary>
        /// Set images on the Switchblade UI’s Dynamic Keys.
        /// </summary>
        /// <param name="dk"><see cref="DynamicKeyType" /> indicating which key to set the image on.</param>
        /// <param name="state">
        /// The desired dynamic key state (up, down) for the specified image. See <see cref="DynamicKeyState" /> for accepted values.
        /// </param>
        /// <param name="filename">
        /// The image filepath for the given state. This image should be 115 x 115 pixels in dimension.
        /// Accepted file formats are BMP, GIF, JPG, and PNG.
        /// </param>
        /// <remarks>Animation in GIF files are not supported.</remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBSetImageDynamicKey(
            [In] DynamicKeyType dk,
            [In] DynamicKeyState state,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string filename);

        /// <summary>
        /// Places an image on the main Switchblade display.
        /// </summary>
        /// <param name="filename">
        /// Filepath to the image to be placed on the main Switchblade display.
        /// This image should be 800 x 480 pixels in dimension. Accepted file formats are BMP, GIF, JPG, and PNG.
        /// </param>
        /// <remarks>Animation in GIF files are not supported.</remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBSetImageTouchpad([In] [MarshalAs(UnmanagedType.LPWStr)] string filename);

        /// <summary>
        /// Sets the callback function for application event callbacks.
        /// </summary>
        /// <param name="callback">
        /// Pointer to a callback function. If this argument is set to NULL, the routine clears the previously set callback function.
        /// </param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBAppEventSetCallback([In] AppEventCallbackDelegate callback);

        /// <summary>
        /// Sets the callback function for dynamic key events.
        /// </summary>
        /// <param name="callback">
        /// Pointer to a callback function. If this argument is set to NULL, the routine clears the previously set callback function.
        /// </param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBDynamicKeySetCallback([In] DynamicKeyCallbackFunctionDelegate callback);

        /// <summary>
        /// Enables or disables the keyboard capture functionality.
        /// </summary>
        /// <param name="bEnable">The enable state. true enables the capture while false disables it.</param>
        /// <remarks>
        /// When the capture is enabled, the SDK application can receive keyboard input events through the callback assigned using RzSBKeyboardCaptureSetCallback.
        /// The OS will not receive any keyboard input from the Switchblade device as long as the capture is active.
        /// Hence, applications must release the capture when no longer in use (call RzSBEnableGesture with false as parameter).
        /// The function only affects the keyboard device where the application is running. Other keyboard devices will work normally.
        /// </remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBCaptureKeyboard(bool bEnable);

        /// <summary>
        /// Sets the callback function for dynamic key events. [sic]
        /// </summary>
        /// <param name="callback">
        /// Pointer to a callback function. If this argument is set to NULL, the routine clears the previously set callback function.
        /// </param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBKeyboardCaptureSetCallback([In] KeyboardCallbackFunctionDelegate callback);

        /// <summary>
        /// Sets the callback function for gesture events.
        /// </summary>
        /// <param name="callback">
        /// Pointer to a callback function. If this argument is set to NULL, the routine clears the previously set callback function.
        /// </param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBGestureSetCallback([In] TouchpadGestureCallbackFunctionDelegate callback);

        /// <summary>
        /// Enables or disables gesture events.
        /// </summary>
        /// <param name="gestureType"><see cref="GestureType" /> to be enabled or disabled.</param>
        /// <param name="bEnable">The enable state. true enables the gesture while false disables it.</param>
        /// <remarks>
        /// In nearly all cases, gestural events are preceded by a <see cref="GestureType.Press" /> event.
        /// With multiple finger gestures, the first finger contact registers as a press,
        /// and the touchpad reports subsequent contacts as the appropriate compound gesture (tap, flick, zoom or rotate).
        /// </remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBEnableGesture([In] GestureType gestureType, [In] bool bEnable);

        /// <summary>
        /// Enables or disables gesture event forwarding to the OS.
        /// </summary>
        /// <param name="gestureType"><see cref="GestureType" /> to be enabled or disabled.</param>
        /// <param name="bEnable">The enable state. true enables the gesture while false disables it.</param>
        /// <remarks>
        /// Setting the <see cref="GestureType.Press"/> for OS gesture is equivalent to
        /// <see cref="GestureType.Press"/>, <see cref="GestureType.Move" /> and <see cref="GestureType.Release" />.
        /// </remarks>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern HRESULT RzSBEnableOSGesture([In] GestureType gestureType, [In] bool bEnable);

        #endregion Functions

        #region Delegates

        /// <summary>
        /// Function delegate for dynamic key callbacks.
        /// </summary>
        /// <param name="dynamicKeyType">The key type that was changed.</param>
        /// <param name="dynamicKeyState">The new state of the key.</param>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate HRESULT DynamicKeyCallbackFunctionDelegate(DynamicKeyType dynamicKeyType, DynamicKeyState dynamicKeyState);

        /// <summary>
        /// Function delegate for app events.
        /// </summary>
        /// <param name="appEventType">The type of app event.</param>
        /// <param name="dwAppMode">The app mode-</param>
        /// <param name="dwProcessID">The process ID.</param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure.</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate HRESULT AppEventCallbackDelegate(AppEventType appEventType, uint dwAppMode, uint dwProcessID);

        /// <summary>
        /// Function delegate for touchpad gesture events.
        /// </summary>
        /// <param name="gestureType">The type of gesture.</param>
        /// <param name="dwParameters">Parameters specific to gesture type.</param>
        /// <param name="wXPos">X pos where gesture happened.</param>
        /// <param name="wYPos">Y pos where gesture happened.</param>
        /// <param name="wZPos">Z pos where gesture happened.</param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate HRESULT TouchpadGestureCallbackFunctionDelegate(GestureType gestureType, uint dwParameters, ushort wXPos, ushort wYPos, ushort wZPos);

        /// <summary>
        /// Function delegate for keyboard callback.
        /// </summary>
        /// <param name="uMsg">Indicates the keyboard event (WM_KEYDOWN, WM_KEYUP, WM_CHAR).</param>
        /// <param name="wParam">Indicates the key that was pressed (Virtual Key value).</param>
        /// <param name="lParam">Indicates key modifiers (CTRL, ALT, SHIFT).</param>
        /// <returns><see cref="HRESULT" /> object indicating success or failure</returns>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate HRESULT KeyboardCallbackFunctionDelegate(uint uMsg, UIntPtr wParam, IntPtr lParam);

        #endregion

        #region Structs

        /// <summary>
        /// Specifies a specific point on the touchpad.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            /// <summary>
            /// X position on the touchpad.
            /// </summary>
            public int X;

            /// <summary>
            /// Y position on the touchpad.
            /// </summary>
            public int Y;
        }

        /// <summary>
        /// Specifies the capabilities of this SwitchBlade device.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Capabilities
        {
            /// <summary>
            /// Version.
            /// </summary>
            public ulong Version;

            /// <summary>
            /// BEVersion
            /// </summary>
            public ulong BEVersion;

            /// <summary>
            /// Type of device.
            /// </summary>
            public HardwareType HardwareType;

            /// <summary>
            /// Number of surfaces available.
            /// </summary>
            public ulong NumSurfaces;

            /// <summary>
            /// Surface geometry for each surface.
            /// </summary>
            /// <remarks>Contains <see cref="NumSurfaces" /> entries.</remarks>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSupportedSurfaces)]
            public Point[] Surfacegeometry;

            /// <summary>
            /// Pixel format of each surface.
            /// </summary>
            /// <remarks>Contains <see cref="NumSurfaces" /> entries.</remarks>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSupportedSurfaces)]
            public uint[] Pixelformat;

            /// <summary>
            /// Number of dynamic keys available on device.
            /// </summary>
            public byte NumDynamicKeys;

            /// <summary>
            /// Arrangement of the dynamic keys.
            /// </summary>
            public Point DynamicKeyArrangement;

            /// <summary>
            /// Size of each dynamic key.
            /// </summary>
            public Point DynamicKeySize;
        }
        
        /// <summary>
        /// Buffer data sent to display when rendering image data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BufferParams
        {
            /// <summary>
            /// Pixel format of the image data.
            /// </summary>
            public PixelType PixelType;

            /// <summary>
            /// Total size of the data.
            /// </summary>
            public uint DataSize;

            /// <summary>
            /// Pointer to image data.
            /// </summary>
            public IntPtr PtrData;
        }

        #endregion Structs

        #region Enumerations

        /// <summary>
        /// Possible states of a dynamic key.
        /// </summary>
        public enum DynamicKeyState
        {
            /// <summary>
            /// No active state.
            /// </summary>
            None = 0,

            /// <summary>
            /// Depressed.
            /// </summary>
            Up,

            /// <summary>
            /// Pressed.
            /// </summary>
            Down,

            /// <summary>
            /// Being held.
            /// </summary>
            Hold,

            /// <summary>
            /// Invalid key state.
            /// </summary>
            Invalid
        }

        /// <summary>
        /// Direction of motion/gesture.
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// Nowhere.
            /// </summary>
            None = 0,

            /// <summary>
            /// To the left.
            /// </summary>
            Left,

            /// <summary>
            /// To the right.
            /// </summary>
            Right,

            /// <summary>
            /// upwards.
            /// </summary>
            Up,

            /// <summary>
            /// Downwards.
            /// </summary>
            Down,

            /// <summary>
            /// Invalid direction.
            /// </summary>
            Invalid
        }

        /// <summary>
        /// Dynamic keys available on the SwitchBlade device.
        /// </summary>
        public enum DynamicKeyType
        {
            /// <summary>
            /// None of the keys.
            /// </summary>
            None = 0,

            /// <summary>
            /// Key #1.
            /// </summary>
            DK1,

            /// <summary>
            /// Key #2.
            /// </summary>
            DK2,

            /// <summary>
            /// Key #3.
            /// </summary>
            DK3,

            /// <summary>
            /// Key #4.
            /// </summary>
            DK4,

            /// <summary>
            /// Key #5.
            /// </summary>
            DK5,

            /// <summary>
            /// Key #6.
            /// </summary>
            DK6,

            /// <summary>
            /// Key #7.
            /// </summary>
            DK7,

            /// <summary>
            /// Key #8.
            /// </summary>
            DK8,

            /// <summary>
            /// Key #9.
            /// </summary>
            DK9,

            /// <summary>
            /// Key #10.
            /// </summary>
            DK10,

            /// <summary>
            /// Invalid dynamic key.
            /// </summary>
            Invalid,

            /// <summary>
            /// Number of keys available.
            /// </summary>
            Count = 10
        }

        /// <summary>
        /// Target displays available on SwitchBlade device.
        /// </summary>
        public enum TargetDisplay
        {
            /// <summary>
            /// The touchpad screen.
            /// </summary>
            Widget = 0x10000,

            /// <summary>
            /// Dynamic key #1.
            /// </summary>
            DK1    = 0x10001,

            /// <summary>
            /// Dynamic key #2.
            /// </summary>
            DK2    = 0x10002,

            /// <summary>
            /// Dynamic key #3.
            /// </summary>
            DK3    = 0x10003,

            /// <summary>
            /// Dynamic key #4.
            /// </summary>
            DK4    = 0x10004,

            /// <summary>
            /// Dynamic key #5.
            /// </summary>
            DK5    = 0x10005,

            /// <summary>
            /// Dynamic key #6.
            /// </summary>
            DK6    = 0x10006,

            /// <summary>
            /// Dynamic key #7.
            /// </summary>
            DK7    = 0x10007,

            /// <summary>
            /// Dynamic key #8.
            /// </summary>
            DK8    = 0x10008,

            /// <summary>
            /// Dynamic key #9.
            /// </summary>
            DK9    = 0x10009,

            /// <summary>
            /// Dynamic key #10.
            /// </summary>
            DK10   = 0x1000A
        }

        /// <summary>
        /// Supported pixel formats.
        /// </summary>
        public enum PixelType
        {
            /// <summary>
            /// RGB565 pixel format.
            /// </summary>
            RGB565 = 0
        }

        /// <summary>
        /// App event types used by Razer's AppEvent callback system.
        /// </summary>
        public enum AppEventType
        {
            /// <summary>
            /// No/empty app event.
            /// </summary>
            None = 0,

            /// <summary>
            /// The Switchblade framework has activated the SDK application.
            /// The application can resume its operations and update the Switchblade UI display.
            /// </summary>
            Activated,

            /// <summary>
            /// The application has been deactivated to make way for another application.
            /// In this state, the SDK application will not receive any Dynamic Key or Gesture events,
            /// nor will it be able to update the Switchblade displays.
            /// </summary>
            Deactivated,

            /// <summary>
            /// The Switchblade framework has initiated a request to close the application.
            /// The application should perform cleanup and can terminate on its own when done.
            /// </summary>
            Close,

            /// <summary>
            /// The Switchblade framework will forcibly close the application.
            /// This event is always preceeded by the <see cref="Close" /> event.
            /// Cleanup should be done there.
            /// </summary>
            Exit,

            /// <summary>
            /// Invalid app event.
            /// </summary>
            Invalid
        }

        /// <summary>
        /// Mode that app is running in.
        /// </summary>
        public enum AppEventMode
        {
            /// <summary>
            /// Running in applet mode.
            /// </summary>
            Applet = 0x02,

            /// <summary>
            /// Running normally.
            /// </summary>
            Normal = 0x04
        }

        /// <summary>
        /// Gesture types supported by the device.
        /// </summary>
        [Flags]
        public enum GestureType : uint
        {
            /// <summary>
            /// Invalid or no gesture.
            /// </summary>
            None    = 0x00000000,

            /// <summary>
            /// A press on the touchpad.
            /// </summary>
            Press   = 0x00000001,

            /// <summary>
            /// A tap on the touchpad.
            /// </summary>
            Tap     = 0x00000002,

            /// <summary>
            /// Flick with finger(s?) on the touchpad.
            /// </summary>
            Flick   = 0x00000004,

            /// <summary>
            /// To fingers pinching out on touchpad.
            /// </summary>
            Zoom    = 0x00000008,

            /// <summary>
            /// Two fingers rotating on touchpad.
            /// </summary>
            Rotate  = 0x00000010,

            /// <summary>
            /// Finger is moving around on touchpad.
            /// </summary>
            Move    = 0x00000020,

            /// <summary>
            /// Finger is being held on touchpad.
            /// </summary>
            Hold    = 0x00000040,

            /// <summary>
            /// Finger was released from touchpad.
            /// </summary>
            Release = 0x00000080,

            /// <summary>
            /// Scroll gesture.
            /// </summary>
            Scroll  = 0x00000100,

            /// <summary>
            /// Every gesture.
            /// </summary>
            All     = 0x0000FFFF
        }

        /// <summary>
        /// Different hardware types returned by <see cref="RzSBQueryCapabilities" />.
        /// </summary>
        public enum HardwareType
        {
            /// <summary>
            /// Invalid hardware.
            /// </summary>
            Invalid = 0,

            /// <summary>
            /// SwitchBlade device.
            /// </summary>
            Switchblade,

            /// <summary>
            /// Unknown device type.
            /// </summary>
            Undefined
        }

        #endregion Enumerations

        #region Constants

        /*
         * definitions for the Dynamic Key display region of the Switchblade
         */

        /// <summary>
        /// Number of dynamic keys per row on the device.
        /// </summary>
        public const int DynamicKeysPerRow = 5;

        /// <summary>
        /// Number of rows on the dynamic keys.
        /// </summary>
        public const int DynamicKeysRows = 2;

        /// <summary>
        /// Total number of dynamic keys that exist on the device.
        /// </summary>
        public const int DynamicKeysCount = DynamicKeysPerRow * DynamicKeysRows;
        
        /// <summary>
        /// The width of one dynamic key, in pixels.
        /// </summary>
        /// <remarks>Note that this refers to the width of the display area on a dynamic key,
        /// not physical size.</remarks>
        public const int DynamicKeyWidth = 115;

        /// <summary>
        /// The height of one dynamic key, in pixels.
        /// </summary>
        /// <remarks>Note that this refers to the height of the display area on a dynamic key,
        /// not physical size.</remarks>
        public const int DynamicKeyHeight = 115;

        /// <summary>
        /// Size of image data for one dynamic key.
        /// </summary>
        public const int DynamicKeyImageDataSize = DynamicKeyWidth * DynamicKeyHeight * sizeof(ushort);

        /*
         * definitions for the Touchpad display region of the Switchblade
         */

        /// <summary>
        /// Width of the touchpad on standard devices.
        /// </summary>
        public const int TouchpadWidth = 800;

        /// <summary>
        /// Height of the touchpad on standard devices.
        /// </summary>
        public const int TouchpadHeight = 480;

        /// <summary>
        /// Size of image data to cover the touchpad.
        /// </summary>
        public const int TouchpadImageDataSize = TouchpadWidth * TouchpadHeight * sizeof(ushort);

        /// <summary>
        /// Color depth of the device's display areas.
        /// </summary>
        public const int DisplayColorDepth = 16;

        /// <summary>
        /// Max string length.
        /// </summary>
        public const int MaxStringLength = 260;

        /// <summary>
        /// Maximum supported surfaces.
        /// </summary>
        public const int MaxSupportedSurfaces = 2;

        /// <summary>
        /// Invalid pixel format.
        /// </summary>
        public const int PixelFormatInvalid = 0;

        /// <summary>
        /// RGB565 pixel format, used by standard SwitchBlade devices.
        /// </summary>
        public const int PixelFormatRgb565 = 1;

        #endregion Constants

        #region Macros

        /// <summary>
        /// Checks that the specified gesture type value is valid.
        /// </summary>
        /// <param name="a">Gesture type value to check.</param>
        /// <returns>True if valid gesture, false otherwise.</returns>
        public static bool ValidGesture(uint a) { return (a & (uint) GestureType.All) != 0; }
        
        /// <summary>
        /// Checks if the gesture type value denotes a single gesture.
        /// </summary>
        /// <param name="a">Gesture type value to check.</param>
        /// <returns>True if this is a single gesture, false if multiple.</returns>
        public static bool SingleGesture(uint a) { return 0 == ((a - a) & a); }

        #endregion Macros
    }
}
