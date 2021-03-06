﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using consoleXstream.Input;

namespace consoleXstream.Output
{
    public class ControllerMax
    {
        public ControllerMax(BaseClass baseClass) { _class = baseClass; }
        private readonly BaseClass _class;

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        //private Form1 frmMain;
        //private Config.Configuration system;
        //private Input.KeyboardInterface keyboardInterface;

        public int intCMHomeCount { get; set; }

        public bool boolPs4Touchpad { get; set; }

        private Input.GamePadState _controls;

        private string _strCMDevice;

        private bool _boolGCAPILoaded = false;
        private bool _boolNoticeCMDisconnected = false;
        private bool _boolHoldBack = false;
        private bool _boolLoadShortcuts = false;

        private int _intXboxCount;
        private int _intMenuWait;
        private int _intMenuShow;

        private int[,] _intShortcut;
        private int _intShortcutCount;


        #region ControllerMax References
        public struct GCAPI_CONSTANTS
        {
            public const int GCAPI_INPUT_TOTAL = 30;
            public const int GCAPI_OUTPUT_TOTAL = 36;
        }
        public struct GCAPI_STATUS
        {
            public byte value; // Current value - Range: [-100 ~ 100] %
            public byte prev_value; // Previous value - Range: [-100 ~ 100] %
            public int press_tv; // Time marker for the button press event
        }
        public struct GCAPI_REPORT_CONTROLLERMAX
        {
            public byte console; // Receives values established by the #defines CONSOLE_*
            public byte controller; // Values from #defines CONTROLLER_* and EXTENSION_*

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] led; // Four LED - #defines LED_*

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] rumble; // Two rumbles - Range: [0 ~ 100] %
            public byte battery_level; // Battery level - Range: [0 ~ 10] 0 = empty, 10 = full

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GCAPI_CONSTANTS.GCAPI_INPUT_TOTAL, ArraySubType = UnmanagedType.Struct)]
            public GCAPI_STATUS[] input;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte GCAPI_LOAD();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate byte GCAPI_ISCONNECTED();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GCAPI_GETTIMEVAL();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint GCAPI_GETFWVER();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate byte GCAPI_WRITE(byte[] output);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate byte GCAPI_WRITE_EX(byte[] output);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate byte GCAPI_WRITEREF(byte[] output);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int GCAPI_CALCPRESSTIME(byte time);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void GCAPI_UNLOAD();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr GCAPI_READ_CM([In, Out] ref GCAPI_REPORT_CONTROLLERMAX gcapi_report);

        public GCAPI_LOAD _gcapi_Load = null;
        private GCAPI_ISCONNECTED _gcapi_IsConnected = null;
        private GCAPI_GETTIMEVAL _gcapi_GetTimeVal = null;
        private GCAPI_GETFWVER _gcapi_GetFwVer = null;
        private GCAPI_WRITE _gcapi_Write = null;
        private GCAPI_WRITE_EX _gcapi_WriteEx = null;
        private GCAPI_WRITEREF _gcapi_WriteRef = null;
        private GCAPI_READ_CM _gcapi_Read_CM = null;
        private GCAPI_CALCPRESSTIME _gcapi_CalcPressTime = null;
        public GCAPI_UNLOAD _gcapi_Unload = null;
        #endregion

        public void initControllerMax()
        {
            //TODO: Read from setup menu
            _intMenuShow = 35;

            string strDevice = "ControllerMax";
            string strRef = "CMDI";
            string strAPI = "controllerMax_gcdapi.dll";

            _class.System.Debug("ControllerMax.log", "[0] Opening " + strDevice + " api");
            string strDir = Directory.GetCurrentDirectory() + @"\";
            _strCMDevice = strDevice;

            if (File.Exists(strDir + strAPI) == false)
            {
                _class.System.Debug("ControllerMax.log", "[0] [FAIL] Unable to find " + strDevice + " API");
                return;
            }

            _class.System.Debug("ControllerMax.log", "[TRY] Attempting to open " + strDevice + " Device Interface (" + strRef + ")");

            IntPtr ptrDll = LoadLibrary(strDir + strAPI);
            if (ptrDll == IntPtr.Zero)
            {
                _class.System.Debug("ControllerMax.log", "[0] [FAIL] Unable to allocate Device API");
                return;
            }

            IntPtr ptrLoad = loadExternalFunction(ptrDll, "gcdapi_Load");
            if (ptrLoad == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_Load"); return; }

            IntPtr ptrIsConnected = loadExternalFunction(ptrDll, "gcapi_IsConnected");
            if (ptrIsConnected == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_IsConnected"); return; }

            IntPtr ptrUnload = loadExternalFunction(ptrDll, "gcdapi_Unload");
            if (ptrUnload == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_Unload"); return; }

            IntPtr ptrGetTimeVal = loadExternalFunction(ptrDll, "gcapi_GetTimeVal");
            if (ptrGetTimeVal == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_GetTimeVal"); return; }
            
            IntPtr ptrGetFwVer = loadExternalFunction(ptrDll, "gcapi_GetFWVer");
            if (ptrGetFwVer == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_GetFWVer"); return; }

            IntPtr ptrWrite = loadExternalFunction(ptrDll, "gcapi_Write");
            if (ptrWrite == IntPtr.Zero) return;
            
            IntPtr ptrRead = loadExternalFunction(ptrDll, "gcapi_Read");
            if (ptrRead == IntPtr.Zero) return;
            
            IntPtr ptrWriteEx = IntPtr.Zero;                        
            IntPtr ptrReadEx = IntPtr.Zero;
            
            IntPtr ptrCalcPressTime = loadExternalFunction(ptrDll, "gcapi_CalcPressTime");
            if (ptrCalcPressTime == IntPtr.Zero) { _class.System.Debug("ControllerMax.log", "[0] [FAIL] gcapi_CalcPressTime"); return; }

            try
            {
                _gcapi_Load = (GCAPI_LOAD)Marshal.GetDelegateForFunctionPointer(ptrLoad, typeof(GCAPI_LOAD));
                _gcapi_IsConnected = (GCAPI_ISCONNECTED)Marshal.GetDelegateForFunctionPointer(ptrIsConnected, typeof(GCAPI_ISCONNECTED));
                _gcapi_Unload = (GCAPI_UNLOAD)Marshal.GetDelegateForFunctionPointer(ptrUnload, typeof(GCAPI_UNLOAD));
                _gcapi_GetTimeVal = (GCAPI_GETTIMEVAL)Marshal.GetDelegateForFunctionPointer(ptrGetTimeVal, typeof(GCAPI_GETTIMEVAL));
                _gcapi_GetFwVer = (GCAPI_GETFWVER)Marshal.GetDelegateForFunctionPointer(ptrGetFwVer, typeof(GCAPI_GETFWVER));
                _gcapi_Write = (GCAPI_WRITE)Marshal.GetDelegateForFunctionPointer(ptrWrite, typeof(GCAPI_WRITE));
                _gcapi_Read_CM = (GCAPI_READ_CM)Marshal.GetDelegateForFunctionPointer(ptrRead, typeof(GCAPI_READ_CM));
                _gcapi_CalcPressTime = (GCAPI_CALCPRESSTIME)Marshal.GetDelegateForFunctionPointer(ptrCalcPressTime, typeof(GCAPI_CALCPRESSTIME));
            }
            catch (Exception ex)
            {
                _class.System.Debug("ControllerMax.log", "[0] Fail -> " + ex.ToString());
            }

            _gcapi_Load();
            _class.System.Debug("ControllerMax.log", "[0] Initialize ControllerMax GCAPI ok");
            
            loadShortcutKeys();
            _boolGCAPILoaded = true;
        }

        //Finds the pointer for the dll function
        private IntPtr loadExternalFunction(IntPtr ptrDll, string strFunction)
        {
            IntPtr ptrFunction = IntPtr.Zero;
            ptrFunction = GetProcAddress(ptrDll, strFunction);
            if (ptrFunction == IntPtr.Zero)
            {
                _class.System.Debug("ControllerMax.log", "[0] [NG] " + strFunction + " alloc fail");
            }
            else
            {
                _class.System.Debug("ControllerMax.log", "[5] [OK] " + strFunction);
            }
            return ptrFunction;
        }

        public void closeControllerMaxInterface()
        {
            string strDevice = "ControllerMax"; 
            string strRef = "CMDI"; 

            if (_gcapi_Unload != null)
                _gcapi_Unload();

            _gcapi_Load = null;
            _gcapi_IsConnected = null;
            _gcapi_GetTimeVal = null;
            _gcapi_GetFwVer = null;
            _gcapi_Write = null;
            _gcapi_WriteEx = null;
            _gcapi_WriteRef = null;
            _gcapi_Read_CM = null;
            _gcapi_CalcPressTime = null;
            _gcapi_Unload = null;

            _boolGCAPILoaded = false;

            _class.System.Debug("ControllerMax.log", "[OK] Closed " + strDevice + " (" + strRef + ")");
        }

        public void CheckControllerInput()
        {
            if (!_boolGCAPILoaded)
                return;

            var boolOverride = _class.Home.boolIDE;

            if ((_gcapi_IsConnected() == 1) || boolOverride)
            {
                //Update gamepad status
                _controls = GamePad.GetState(PlayerIndex.One);
                
                if (_intXboxCount == 0) { _intXboxCount = Enum.GetNames(typeof(Xbox)).Length; }
                byte[] output = new byte[_intXboxCount];

                if (_controls.DPad.Left) { output[(int)Xbox.Left] = Convert.ToByte(100); }
                if (_controls.DPad.Right) { output[(int)Xbox.Right] = Convert.ToByte(100); }
                if (_controls.DPad.Up) { output[(int)Xbox.Up] = Convert.ToByte(100); }
                if (_controls.DPad.Down) { output[(int)Xbox.Down] = Convert.ToByte(100); }

                if (_controls.Buttons.A) { output[(int)Xbox.A] = Convert.ToByte(100); }
                if (_controls.Buttons.B) { output[(int)Xbox.B] = Convert.ToByte(100); }
                if (_controls.Buttons.X) { output[(int)Xbox.X] = Convert.ToByte(100); }
                if (_controls.Buttons.Y) { output[(int)Xbox.Y] = Convert.ToByte(100); }

                if (_controls.Buttons.Start) { output[(int)Xbox.Start] = Convert.ToByte(100); }
                if (_controls.Buttons.Guide) { output[(int)Xbox.Home] = Convert.ToByte(100); }
                if (_controls.Buttons.Back)
                {
                    if (_class.System.boolBlockMenuButton == false)
                    {
                        _intMenuWait++;
                        if (_class.System.boolMenu == false)
                            if (_intMenuWait >= _intMenuShow + 20)
                                openMenu();
                    }

                    //Remap back buton to touchpad
                    if (_class.System.IsPs4ControllerMode)
                        output[(int)Xbox.Touch] = Convert.ToByte(100);
                    else
                        output[(int)Xbox.Back] = Convert.ToByte(100);
                }

                if (_controls.Buttons.LeftShoulder) { output[(int)Xbox.LeftShoulder] = Convert.ToByte(100); }
                if (_controls.Buttons.RightShoulder) { output[(int)Xbox.RightShoulder] = Convert.ToByte(100); }
                if (_controls.Buttons.LeftStick) { output[(int)Xbox.LeftStick] = Convert.ToByte(100); }
                if (_controls.Buttons.RightStick) { output[(int)Xbox.RightStick] = Convert.ToByte(100); }

                if (_controls.Triggers.Left > 0) { output[(int)Xbox.LeftTrigger] = Convert.ToByte(_controls.Triggers.Left * 100); }
                if (_controls.Triggers.Right > 0) { output[(int)Xbox.RightTrigger] = Convert.ToByte(_controls.Triggers.Right * 100); }

                double dblLX = _controls.ThumbSticks.Left.X * 100;
                double dblLY = _controls.ThumbSticks.Left.Y * 100;
                double dblRX = _controls.ThumbSticks.Right.X * 100;
                double dblRY = _controls.ThumbSticks.Right.Y * 100;

                if (_class.System.IsNormalizeControls == true)
                {
                    normalGamepad(ref dblLX, ref dblLY);
                    normalGamepad(ref dblRX, ref dblRY);
                }
                else
                {
                    dblLY = -dblLY;
                    dblRY = -dblRY;
                }

                if (dblLX != 0) { output[(int)Xbox.LeftX] = (byte)Convert.ToSByte((int)(dblLX)); }
                if (dblLY != 0) { output[(int)Xbox.LeftY] = (byte)Convert.ToSByte((int)(dblLY)); }
                if (dblRX != 0) { output[(int)Xbox.RightX] = (byte)Convert.ToSByte((int)(dblRX)); }
                if (dblRY != 0) { output[(int)Xbox.RightY] = (byte)Convert.ToSByte((int)(dblRY)); }

                if (intCMHomeCount > 0)
                {
                    output[(int)Xbox.Home] = Convert.ToByte(100);
                    intCMHomeCount--;
                }

                if (boolPs4Touchpad == true)
                    output[(int)Xbox.Touch] = Convert.ToByte(100);


                if (_boolLoadShortcuts)
                    output = checkKeys(output);

                int intTarget = -1;
                if (_class.System.IsPs4ControllerMode == false) { intTarget = (int)Xbox.Back; } else { intTarget = (int)Xbox.Touch; }

                //Back button. Wait until released as its also the menu button
                if (intTarget > -1)
                {
                    if (_class.System.boolBlockMenuButton)
                    {
                        if (output[intTarget] == 100)
                        {
                            _boolHoldBack = true;
                            output[intTarget] = Convert.ToByte(0);
                            _intMenuWait++;
                            if (!_class.System.boolMenu)
                            {
                                if (_intMenuWait >= _intMenuShow)
                                {
                                    _boolHoldBack = false;
                                    openMenu();
                                }
                            }
                        }
                        else
                        {
                            if (_boolHoldBack)
                            {
                                _boolHoldBack = false;
                                output[intTarget] = Convert.ToByte(100);
                                _intMenuWait = 0;
                            }
                            else
                                _intMenuWait = 0;
                        }
                    }
                }
                
                if (_class.KeyboardInterface != null)
                {
                    for (var intCount = 0; intCount < _intXboxCount; intCount++)
                    {
                        if (_class.KeyboardInterface.output[intCount] != 0)
                            output[intCount] = _class.KeyboardInterface.output[intCount];
                    }
                }

                _gcapi_Write(output);

                if (_class.System.UseRumble != true) return;
                var report = new GCAPI_REPORT_CONTROLLERMAX();
                if (_gcapi_Read_CM(ref report) != IntPtr.Zero)
                    GamePad.SetState(PlayerIndex.One, report.rumble[0], report.rumble[1]);
            }
            else
            {
                //If device just disconnected open up notice to tell user
                if (_boolNoticeCMDisconnected == false)
                {
                    _class.System.Debug("ControllerMax.log", "[NOTE] " + _strCMDevice + " is disconnected");
                    _boolNoticeCMDisconnected = true;
                }

                //Keep alive for opening the menu
                _controls = GamePad.GetState(PlayerIndex.One);
                if (_controls.Buttons.Back)
                {
                    if (_class.System.boolBlockMenuButton== false)
                    {
                        _intMenuWait++;
                        if (!_class.System.boolMenu)
                        {
                            if (_intMenuWait >= _intMenuShow + 20)
                                _class.Home.OpenMenu();
                        }
                    }
                }
            }
        }

        private void normalGamepad(ref double dblLX, ref double dblLY)
        {
            double dblNewX = dblLX;
            double dblNewY = dblLY;

            double dblLength = Math.Sqrt(Math.Pow(dblLX, 2) + Math.Pow(dblLY, 2));
            if (dblLength > 99.9)
            {
                double dblTheta = Math.Atan2(dblLY, dblLX);
                double dblAngle = (90 - ((dblTheta * 180) / Math.PI)) % 360;

                if ((dblAngle < 0) && (dblAngle >= -45)) { dblNewX = (int)(100 / Math.Tan(dblTheta)); dblNewY = -100; }
                if ((dblAngle >= 0) && (dblAngle <= 45)) { dblNewX = (int)(100 / Math.Tan(dblTheta)); dblNewY = -100; }
                if ((dblAngle > 45) && (dblAngle <= 135)) { dblNewY = -(int)(Math.Tan(dblTheta) * 100); dblNewX = 100; }
                if ((dblAngle > 135) && (dblAngle <= 225)) { dblNewX = -(int)(100 / Math.Tan(dblTheta)); dblNewY = 100; }
                if (dblAngle > 225) { dblNewY = (int)(Math.Tan(dblTheta) * 100); dblNewX = -100; }
                if (dblAngle < -45) { dblNewY = (int)(Math.Tan(dblTheta) * 100); dblNewX = -100; }
            }
            else
            {
                dblNewY = -dblNewY;
            }

            //Return values
            dblLX = dblNewX;
            dblLY = dblNewY;
        }

        private byte[] checkKeys(byte[] output)
        {
            int intData1;
            int intData2;
            int intTarget;

            for (int intCount = 0; intCount < _intShortcutCount; intCount++)
            {
                intData1 = _intShortcut[0, intCount];
                intData2 = _intShortcut[1, intCount];
                intTarget = _intShortcut[2, intCount];

                if ((output[intData1].ToString() == "100") && (output[intData2].ToString() == "100"))
                {
                    output[intData1] = Convert.ToByte(0);
                    output[intData2] = Convert.ToByte(0);

                    if (intTarget < 32)
                        output[intTarget] = Convert.ToByte(100);
                    else
                        runScript();
                }
            }

            return output;
        }

        private void loadShortcutKeys()
        {
            _intShortcutCount = 0;

            string strInput = "";
            string[] strTemp;
            int intTemp1, intTemp2, intTemp3;
            _intShortcut = new int[3, 25];

            //Load these in incase there is no shortcut file
            //Home - normal mode
            _intShortcut[0, 0] = (int)Xbox.Back;
            _intShortcut[1, 0] = (int)Xbox.B;
            _intShortcut[2, 0] = (int)Xbox.Home;
            _intShortcutCount++;

            //Home - PS4 mode
            _intShortcut[0, 1] = (int)Xbox.Touch;
            _intShortcut[1, 1] = (int)Xbox.B;
            _intShortcut[2, 1] = (int)Xbox.Home;
            _intShortcutCount++;

            if (File.Exists(@"Data\shortcutGamepad.txt") == true)
            {
                int intID = 0;
                StreamReader txtIn = new StreamReader(@"Data\shortcutGamepad.txt");
                while ((strInput = txtIn.ReadLine()) != null)
                {
                    try
                    {
                        strTemp = strInput.Split(',');
                        if (strTemp.Length == 3)
                        {
                            intTemp1 = Convert.ToInt32(strTemp[0]);
                            intTemp2 = Convert.ToInt32(strTemp[1]);
                            intTemp3 = Convert.ToInt32(strTemp[2]);

                            _intShortcut[0, intID] = intTemp1;
                            _intShortcut[1, intID] = intTemp2;
                            _intShortcut[2, intID] = intTemp3;

                            intID++;
                            _intShortcutCount++;
                        }
                    }
                    catch { }
                }
                txtIn.Close();
            }
            _boolLoadShortcuts = true;
        }

        private void runScript()
        {

        }

        private void openMenu()
        {
            _boolHoldBack = false;
            _intMenuWait = 0;

            _class.Home.OpenMenu();
        }

    }
}
