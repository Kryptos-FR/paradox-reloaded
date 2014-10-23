// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using Matrix = SiliconStudio.Core.Mathematics.Matrix;
#if SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Input;
using Point = Windows.Foundation.Point;
using WinRTPointerDeviceType = Windows.Devices.Input.PointerDeviceType;
using WinRTPointerPoint = Windows.UI.Input.PointerPoint;
using WindowsAccelerometer = Windows.Devices.Sensors.Accelerometer;
using WindowsGyroscope = Windows.Devices.Sensors.Gyrometer;
using WindowsOrientation = Windows.Devices.Sensors.OrientationSensor;
using WindowsCompass = Windows.Devices.Sensors.Compass;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private const uint DesiredSensorUpdateIntervalMs = (uint)(1f/DesiredSensorUpdateRate*1000f);

        // mapping between WinRT keys and toolkit keys
        private static readonly Dictionary<VirtualKey, Keys> _keysDictionary;

        private bool isLeftButtonPressed;
        private WindowsAccelerometer windowsAccelerometer;
        private WindowsCompass windowsCompass;
        private WindowsGyroscope windowsGyroscope;
        private WindowsOrientation windowsOrientation;

        // TODO: Support for MultiTouchEnabled on Windows Runtime
        public override bool MultiTouchEnabled { get { return true; } set { } }

        static InputManager()
        {
            _keysDictionary = new Dictionary<VirtualKey, Keys>();
            // this dictionary was built from Desktop version.
            // some keys were removed (like OEM and Media buttons) as they don't have mapping in WinRT
            _keysDictionary[VirtualKey.None] = Keys.None;
            _keysDictionary[VirtualKey.Back] = Keys.Back;
            _keysDictionary[VirtualKey.Tab] = Keys.Tab;
            _keysDictionary[VirtualKey.Enter] = Keys.Enter;
            _keysDictionary[VirtualKey.Pause] = Keys.Pause;
            _keysDictionary[VirtualKey.CapitalLock] = Keys.CapsLock;
            _keysDictionary[VirtualKey.Kana] = Keys.KanaMode;
            _keysDictionary[VirtualKey.Kanji] = Keys.KanjiMode;
            _keysDictionary[VirtualKey.Escape] = Keys.Escape;
            _keysDictionary[VirtualKey.Convert] = Keys.ImeConvert;
            _keysDictionary[VirtualKey.NonConvert] = Keys.ImeNonConvert;
            _keysDictionary[VirtualKey.Space] = Keys.Space;
            _keysDictionary[VirtualKey.PageUp] = Keys.PageUp;
            _keysDictionary[VirtualKey.PageDown] = Keys.PageDown;
            _keysDictionary[VirtualKey.End] = Keys.End;
            _keysDictionary[VirtualKey.Home] = Keys.Home;
            _keysDictionary[VirtualKey.Left] = Keys.Left;
            _keysDictionary[VirtualKey.Up] = Keys.Up;
            _keysDictionary[VirtualKey.Right] = Keys.Right;
            _keysDictionary[VirtualKey.Down] = Keys.Down;
            _keysDictionary[VirtualKey.Select] = Keys.Select;
            _keysDictionary[VirtualKey.Print] = Keys.Print;
            _keysDictionary[VirtualKey.Execute] = Keys.Execute;
            _keysDictionary[VirtualKey.Print] = Keys.PrintScreen;
            _keysDictionary[VirtualKey.Insert] = Keys.Insert;
            _keysDictionary[VirtualKey.Delete] = Keys.Delete;
            _keysDictionary[VirtualKey.Help] = Keys.Help;
            _keysDictionary[VirtualKey.Number0] = Keys.D0;
            _keysDictionary[VirtualKey.Number1] = Keys.D1;
            _keysDictionary[VirtualKey.Number2] = Keys.D2;
            _keysDictionary[VirtualKey.Number3] = Keys.D3;
            _keysDictionary[VirtualKey.Number4] = Keys.D4;
            _keysDictionary[VirtualKey.Number5] = Keys.D5;
            _keysDictionary[VirtualKey.Number6] = Keys.D6;
            _keysDictionary[VirtualKey.Number7] = Keys.D7;
            _keysDictionary[VirtualKey.Number8] = Keys.D8;
            _keysDictionary[VirtualKey.Number9] = Keys.D9;
            _keysDictionary[VirtualKey.A] = Keys.A;
            _keysDictionary[VirtualKey.B] = Keys.B;
            _keysDictionary[VirtualKey.C] = Keys.C;
            _keysDictionary[VirtualKey.D] = Keys.D;
            _keysDictionary[VirtualKey.E] = Keys.E;
            _keysDictionary[VirtualKey.F] = Keys.F;
            _keysDictionary[VirtualKey.G] = Keys.G;
            _keysDictionary[VirtualKey.H] = Keys.H;
            _keysDictionary[VirtualKey.I] = Keys.I;
            _keysDictionary[VirtualKey.J] = Keys.J;
            _keysDictionary[VirtualKey.K] = Keys.K;
            _keysDictionary[VirtualKey.L] = Keys.L;
            _keysDictionary[VirtualKey.M] = Keys.M;
            _keysDictionary[VirtualKey.N] = Keys.N;
            _keysDictionary[VirtualKey.O] = Keys.O;
            _keysDictionary[VirtualKey.P] = Keys.P;
            _keysDictionary[VirtualKey.Q] = Keys.Q;
            _keysDictionary[VirtualKey.R] = Keys.R;
            _keysDictionary[VirtualKey.S] = Keys.S;
            _keysDictionary[VirtualKey.T] = Keys.T;
            _keysDictionary[VirtualKey.U] = Keys.U;
            _keysDictionary[VirtualKey.V] = Keys.V;
            _keysDictionary[VirtualKey.W] = Keys.W;
            _keysDictionary[VirtualKey.X] = Keys.X;
            _keysDictionary[VirtualKey.Y] = Keys.Y;
            _keysDictionary[VirtualKey.Z] = Keys.Z;
            _keysDictionary[VirtualKey.LeftWindows] = Keys.LeftWin;
            _keysDictionary[VirtualKey.RightWindows] = Keys.RightWin;
            _keysDictionary[VirtualKey.Application] = Keys.Apps;
            _keysDictionary[VirtualKey.Sleep] = Keys.Sleep;
            _keysDictionary[VirtualKey.NumberPad0] = Keys.NumPad0;
            _keysDictionary[VirtualKey.NumberPad1] = Keys.NumPad1;
            _keysDictionary[VirtualKey.NumberPad2] = Keys.NumPad2;
            _keysDictionary[VirtualKey.NumberPad3] = Keys.NumPad3;
            _keysDictionary[VirtualKey.NumberPad4] = Keys.NumPad4;
            _keysDictionary[VirtualKey.NumberPad5] = Keys.NumPad5;
            _keysDictionary[VirtualKey.NumberPad6] = Keys.NumPad6;
            _keysDictionary[VirtualKey.NumberPad7] = Keys.NumPad7;
            _keysDictionary[VirtualKey.NumberPad8] = Keys.NumPad8;
            _keysDictionary[VirtualKey.NumberPad9] = Keys.NumPad9;
            _keysDictionary[VirtualKey.Multiply] = Keys.Multiply;
            _keysDictionary[VirtualKey.Add] = Keys.Add;
            _keysDictionary[VirtualKey.Separator] = Keys.Separator;
            _keysDictionary[VirtualKey.Subtract] = Keys.Subtract;
            _keysDictionary[VirtualKey.Decimal] = Keys.Decimal;
            _keysDictionary[VirtualKey.Divide] = Keys.Divide;
            _keysDictionary[VirtualKey.F1] = Keys.F1;
            _keysDictionary[VirtualKey.F2] = Keys.F2;
            _keysDictionary[VirtualKey.F3] = Keys.F3;
            _keysDictionary[VirtualKey.F4] = Keys.F4;
            _keysDictionary[VirtualKey.F5] = Keys.F5;
            _keysDictionary[VirtualKey.F6] = Keys.F6;
            _keysDictionary[VirtualKey.F7] = Keys.F7;
            _keysDictionary[VirtualKey.F8] = Keys.F8;
            _keysDictionary[VirtualKey.F9] = Keys.F9;
            _keysDictionary[VirtualKey.F10] = Keys.F10;
            _keysDictionary[VirtualKey.F11] = Keys.F11;
            _keysDictionary[VirtualKey.F12] = Keys.F12;
            _keysDictionary[VirtualKey.F13] = Keys.F13;
            _keysDictionary[VirtualKey.F14] = Keys.F14;
            _keysDictionary[VirtualKey.F15] = Keys.F15;
            _keysDictionary[VirtualKey.F16] = Keys.F16;
            _keysDictionary[VirtualKey.F17] = Keys.F17;
            _keysDictionary[VirtualKey.F18] = Keys.F18;
            _keysDictionary[VirtualKey.F19] = Keys.F19;
            _keysDictionary[VirtualKey.F20] = Keys.F20;
            _keysDictionary[VirtualKey.F21] = Keys.F21;
            _keysDictionary[VirtualKey.F22] = Keys.F22;
            _keysDictionary[VirtualKey.F23] = Keys.F23;
            _keysDictionary[VirtualKey.F24] = Keys.F24;
            _keysDictionary[VirtualKey.NumberKeyLock] = Keys.NumLock;
            _keysDictionary[VirtualKey.Scroll] = Keys.Scroll;
            _keysDictionary[VirtualKey.Shift] = Keys.LeftShift;
            _keysDictionary[VirtualKey.LeftShift] = Keys.LeftShift;
            _keysDictionary[VirtualKey.RightShift] = Keys.RightShift;
            _keysDictionary[VirtualKey.Control] = Keys.LeftCtrl;
            _keysDictionary[VirtualKey.LeftControl] = Keys.LeftCtrl;
            _keysDictionary[VirtualKey.RightControl] = Keys.RightCtrl;
            _keysDictionary[VirtualKey.Menu] = Keys.LeftAlt;
            _keysDictionary[VirtualKey.LeftMenu] = Keys.LeftAlt;
            _keysDictionary[VirtualKey.RightMenu] = Keys.RightAlt;
        }

        public InputManager(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasPointer = true;

#if SILICONSTUDIO_PLATFORM_WINDOWS_STORE
            GamePadFactories.Add(new XInputGamePadFactory());
            HasMouse = true;
#endif
        }

        public override void Initialize()
        {
            base.Initialize();

            var windowHandle = Game.Window.NativeWindow;
            switch (windowHandle.Context)
            {
                case AppContextType.WindowsRuntime:
                    InitializeFromFrameworkElement((FrameworkElement)windowHandle.NativeHandle);
                    break;
                default:
                    throw new ArgumentException(string.Format("WindowContext [{0}] not supported", Game.Context.ContextType));
            }

            // Scan all registered inputs
            Scan();
            
            // get sensor default instances
            windowsAccelerometer = WindowsAccelerometer.GetDefault();
            windowsCompass = WindowsCompass.GetDefault();
            windowsGyroscope = WindowsGyroscope.GetDefault();
            windowsOrientation = WindowsOrientation.GetDefault();

            // determine which sensors are available
            Accelerometer.IsSupported = windowsAccelerometer != null;
            Compass.IsSupported = windowsCompass != null;
            Gyroscope.IsSupported = windowsGyroscope != null;
            Orientation.IsSupported = windowsOrientation != null;
            Gravity.IsSupported = Orientation.IsSupported && Accelerometer.IsSupported;
            UserAcceleration.IsSupported = Gravity.IsSupported;
        }

        internal override void CheckAndEnableSensors()
        {
            base.CheckAndEnableSensors();

            if (Accelerometer.ShouldBeEnabled || Gravity.ShouldBeEnabled || UserAcceleration.ShouldBeEnabled)
                windowsAccelerometer.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsAccelerometer.MinimumReportInterval);

            if (Compass.ShouldBeEnabled)
                windowsCompass.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsCompass.MinimumReportInterval);

            if (Gyroscope.ShouldBeEnabled)
                windowsGyroscope.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsGyroscope.MinimumReportInterval);

            if (Orientation.ShouldBeEnabled || Gravity.ShouldBeEnabled || UserAcceleration.ShouldBeEnabled)
                windowsOrientation.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsOrientation.MinimumReportInterval);
        }

        internal override void UpdateEnabledSensorsData()
        {
            base.UpdateEnabledSensorsData();

            if (Accelerometer.IsEnabled)
            {
                var currentReading = windowsAccelerometer.GetCurrentReading();
                Accelerometer.Acceleration = new Vector3((float)currentReading.AccelerationX, (float)currentReading.AccelerationY, (float)currentReading.AccelerationZ);
            }

            if (Compass.IsEnabled)
            {
                var currentReading = windowsCompass.GetCurrentReading();
                Compass.Heading = (float)(currentReading.HeadingMagneticNorth * Math.PI / 180);
            }

            if (Gyroscope.IsEnabled)
            {
                var currentReading = windowsGyroscope.GetCurrentReading();
                Gyroscope.RotationRate = new Vector3((float)currentReading.AngularVelocityX, (float)currentReading.AngularVelocityY, (float)currentReading.AngularVelocityZ);
            }

            if (Orientation.IsEnabled || UserAcceleration.IsEnabled || Gravity.IsEnabled)
            {
                var currentReading = windowsOrientation.GetCurrentReading();
                var matrix = currentReading.RotationMatrix;

                if (Orientation.IsEnabled)
                {
                    var q = currentReading.Quaternion;
                    Orientation.Quaternion = new Quaternion(q.X, q.Y, q.Z, q.W);

                    var rotationMatrix = Matrix.Identity;
                    rotationMatrix.M11 = matrix.M11;
                    rotationMatrix.M12 = matrix.M21;
                    rotationMatrix.M13 = matrix.M31;
                    rotationMatrix.M21 = matrix.M12;
                    rotationMatrix.M22 = matrix.M22;
                    rotationMatrix.M23 = matrix.M32;
                    rotationMatrix.M31 = matrix.M13;
                    rotationMatrix.M32 = matrix.M23;
                    rotationMatrix.M33 = matrix.M33;

                    Orientation.RotationMatrix = rotationMatrix;

                    Orientation.Yaw = (float) Math.Atan2(2*(q.W*q.X + q.Y*q.Z), 1 - 2*(q.X * q.X + q.Y * q.Y));
                    Orientation.Pitch = (float) Math.Asin(2*(q.W*q.Y - q.Z*q.X));
                    Orientation.Roll = (float)Math.Atan2(2*(q.W*q.Z + q.X*q.Y), 1 - 2*(q.Y*q.Y + q.Z*q.Z));
                }
                if (UserAcceleration.IsEnabled || Gravity.IsEnabled)
                {
                    // calculate the gravity direction
                    var currentAcceleration = windowsAccelerometer.GetCurrentReading();
                    var acceleration = new Vector3((float)currentAcceleration.AccelerationX, (float)currentAcceleration.AccelerationY, (float)currentAcceleration.AccelerationZ);
                    var gravityDirection = new Vector3(-matrix.M13, -matrix.M23, -matrix.M33);
                    var gravity = Vector3.Dot(acceleration, gravityDirection) * gravityDirection;
                    
                    if (Gravity.IsEnabled)
                        Gravity.Vector = gravity;

                    if (UserAcceleration.IsEnabled)
                        UserAcceleration.Acceleration = acceleration - gravity;
                }
            }
        }

        internal override void CheckAndDisableSensors()
        {
            base.CheckAndDisableSensors();

            if (!(Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled) && (Accelerometer.ShouldBeDisabled || Gravity.ShouldBeDisabled || UserAcceleration.ShouldBeDisabled))
                windowsAccelerometer.ReportInterval = 0;

            if (Compass.ShouldBeDisabled)
                windowsCompass.ReportInterval = 0;

            if (Gyroscope.ShouldBeDisabled)
                windowsGyroscope.ReportInterval = 0;

            if (!(Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled) && (Orientation.ShouldBeDisabled || Gravity.ShouldBeDisabled || UserAcceleration.ShouldBeDisabled))
                windowsOrientation.ReportInterval = 0;
        }

        public override void OnApplicationPaused(object sender, EventArgs e)
        {
            base.OnApplicationPaused(sender, e);

            // revert sensor sampling rate to reduce battery consumption

            if (Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsAccelerometer.ReportInterval = 0;

            if (Compass.IsEnabled)
                windowsCompass.ReportInterval = 0;

            if (Gyroscope.IsEnabled)
                windowsGyroscope.ReportInterval = 0;

            if (Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsOrientation.ReportInterval = 0;
        }

        public override void OnApplicationResumed(object sender, EventArgs e)
        {
            base.OnApplicationResumed(sender, e);

            // reset the paradox sampling rate to activated sensors

            if (Accelerometer.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsAccelerometer.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsAccelerometer.MinimumReportInterval);

            if (Compass.IsEnabled)
                windowsCompass.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsCompass.MinimumReportInterval);

            if (Gyroscope.IsEnabled)
                windowsGyroscope.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsGyroscope.MinimumReportInterval);

            if (Orientation.IsEnabled || Gravity.IsEnabled || UserAcceleration.IsEnabled)
                windowsOrientation.ReportInterval = Math.Max(DesiredSensorUpdateIntervalMs, windowsOrientation.MinimumReportInterval);
        }

        private void InitializeFromFrameworkElement(FrameworkElement uiElement)
        {
            if (!(uiElement is Control))
            {
                uiElement.Loaded += uiElement_Loaded;
                uiElement.Unloaded += uiElement_Unloaded;
            }
            else
            {
                uiElement.KeyDown += (_, e) => HandleKeyFrameworkElement(e, InputEventType.Down);
                uiElement.KeyUp += (_, e) => HandleKeyFrameworkElement(e, InputEventType.Up);
            }

            uiElement.SizeChanged += (_, e) => HandleSizeChangedEvent(e.NewSize);
            uiElement.PointerPressed += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Down);
            uiElement.PointerReleased += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Up);
            uiElement.PointerWheelChanged += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Move);
            uiElement.PointerMoved += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Move);
            uiElement.PointerExited += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Out);
            uiElement.PointerCanceled += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Cancel);
            uiElement.PointerCaptureLost += (_, e) => HandlePointerEventFrameworkElement(uiElement, e, PointerState.Cancel);
        }

        void uiElement_Loaded(object sender, RoutedEventArgs e)
        {
            var uiElement = (DependencyObject)sender;

            while (uiElement != null)
            {
                var control = uiElement as Control;
                if (control != null && control.Focus(FocusState.Programmatic))
                {
                    // Get keyboard focus, and bind to this event
                    control.KeyDown += (_, e2) => HandleKeyFrameworkElement(e2, InputEventType.Down);
                    control.KeyUp += (_, e2) => HandleKeyFrameworkElement(e2, InputEventType.Up);
                    break;
                }

                uiElement = VisualTreeHelper.GetParent(uiElement);
            }
        }

        void uiElement_Unloaded(object sender, RoutedEventArgs e)
        {
            // TODO: Unregister event
        }
        
        private void InitializeFromCoreWindow(CoreWindow coreWindow)
        {
            coreWindow.SizeChanged += (_, args) => { HandleSizeChangedEvent(args.Size); args.Handled = true; };
            coreWindow.PointerPressed += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Down);
            coreWindow.PointerReleased += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Up);
            coreWindow.PointerWheelChanged += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Move);
            coreWindow.PointerMoved += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Move);
            coreWindow.PointerExited += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Out);
            coreWindow.PointerCaptureLost += (_, args) => HandlePointerEventCoreWindow(args, PointerState.Cancel);

            coreWindow.KeyDown += (_, args) => HandleKeyCoreWindow(args, InputEventType.Down);
            coreWindow.KeyUp += (_, args) => HandleKeyCoreWindow(args, InputEventType.Up);
        }

        private void HandleSizeChangedEvent(Size size)
        {
            ControlHeight = (float)size.Height;
            ControlWidth = (float)size.Width;
        }

        private void HandleKeyFrameworkElement(KeyRoutedEventArgs keyRoutedEventArgs, InputEventType inputEventType)
        {
            HandleKey(keyRoutedEventArgs.Key, inputEventType);

            keyRoutedEventArgs.Handled = true;
        }

        private void HandleKeyCoreWindow(KeyEventArgs args, InputEventType inputEventType)
        {
            HandleKey(args.VirtualKey, inputEventType);

            args.Handled = true;
        }

        private void HandleKey(VirtualKey virtualKey, InputEventType type)
        {
            Keys key;
            if (!_keysDictionary.TryGetValue(virtualKey, out key))
                key = Keys.None;

            lock (KeyboardInputEvents)
            {
                KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = type });
            }
        }

        private void HandlePointerEventFrameworkElement(FrameworkElement uiElement, PointerRoutedEventArgs pointerRoutedEventArgs, PointerState pointerState)
        {
            HandlePointerEvent(pointerRoutedEventArgs.GetCurrentPoint(uiElement), pointerState);

            pointerRoutedEventArgs.Handled = true;
        }

        private void HandlePointerEventCoreWindow(PointerEventArgs args, PointerState pointerState)
        {
            HandlePointerEvent(args.CurrentPoint, pointerState);

            args.Handled = true;
        }

        void HandlePointerEvent(WinRTPointerPoint p, PointerState ptrState)
        {
            var pointerType = p.PointerDevice.PointerDeviceType;
            var isMouse = pointerType == WinRTPointerDeviceType.Mouse;
            var position = NormalizeScreenPosition(PointToVector2(p.Position));

            if (isMouse && p.Properties.IsLeftButtonPressed)
                isLeftButtonPressed = true;

            if (isMouse)
            {
                CurrentMousePosition = position;

                if (ptrState != PointerState.Move)
                    UpdateButtons(p.Properties);
            }

            if (!isMouse || isLeftButtonPressed)
                HandlePointerEvents((int)p.PointerId, position, ptrState, ConvertPointerDeviceType(pointerType));

            if (isMouse && !p.Properties.IsLeftButtonPressed)
                isLeftButtonPressed = false;
        }

        private PointerType ConvertPointerDeviceType(WinRTPointerDeviceType deviceType)
        {
            switch (deviceType)
            {
                case WinRTPointerDeviceType.Mouse:
                    return PointerType.Mouse;
                case WinRTPointerDeviceType.Pen:
                    throw new NotSupportedException("Pen device input is not supported.");
                case WinRTPointerDeviceType.Touch:
                    return PointerType.Touch;
            }
            return PointerType.Unknown;
        }

        private void UpdateButtons(PointerPointProperties mouseProperties)
        {
            lock (MouseInputEvents)
            {
                var mouseInputEvent = new MouseInputEvent { Type = mouseProperties.IsLeftButtonPressed ? InputEventType.Down : InputEventType.Up, MouseButton = MouseButton.Left };
                MouseInputEvents.Add(mouseInputEvent);

                mouseInputEvent = new MouseInputEvent { Type = mouseProperties.IsRightButtonPressed ? InputEventType.Down : InputEventType.Up, MouseButton = MouseButton.Right };
                MouseInputEvents.Add(mouseInputEvent);

                mouseInputEvent = new MouseInputEvent { Type = mouseProperties.IsMiddleButtonPressed ? InputEventType.Down : InputEventType.Up, MouseButton = MouseButton.Middle };
                MouseInputEvents.Add(mouseInputEvent);

                mouseInputEvent = new MouseInputEvent { Type = mouseProperties.IsXButton1Pressed ? InputEventType.Down : InputEventType.Up, MouseButton = MouseButton.Extended1 };
                MouseInputEvents.Add(mouseInputEvent);

                mouseInputEvent = new MouseInputEvent { Type = mouseProperties.IsXButton2Pressed ? InputEventType.Down : InputEventType.Up, MouseButton = MouseButton.Extended2 };
                MouseInputEvents.Add(mouseInputEvent);
            }
        }

        private Vector2 PointToVector2(Point point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
}
#endif