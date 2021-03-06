﻿using System;
using DirectShowLib;

namespace consoleXstream.VideoCapture.GraphBuilder
{
    public class Display
    {
        public Display(Classes classes) { _class = classes; }
        private readonly Classes _class;

        public void Setup()
        {
            if (_class.System.IsVr)
            {
                SetVrDisplay();
                return;
            }

            var intVideoWidth = _class.FrmMain.imgDisplay.Width;
            var intVideoHeight = _class.FrmMain.imgDisplay.Height;

            try
            {
                var videoHandle = _class.FrmMain.imgDisplay.Handle;
                _class.Graph.VideoWindow.put_Owner(videoHandle);
                    _class.Graph.VideoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);

                if (_class.Graph.VideoWindow != null)
                    _class.Graph.VideoWindow.SetWindowPosition(0, 0, intVideoWidth, intVideoHeight);

                _class.Graph.VideoWindow.SetWindowForeground(OABool.True);

                if (_class.Graph.VideoWindow != null) _class.Graph.VideoWindow.SetWindowPosition(0, 0, intVideoWidth, intVideoHeight);

                _class.Graph.VideoWindow.put_Visible(OABool.True);
                _class.FrmMain.FocusWindow();
                /*
                if (!_class.System.IsVr) return;

                var videoHandle2 = _class.FrmMain.imgDisplayVr.Handle;
                _class.Graph.VideoWindowVr.put_Owner(videoHandle2);
                _class.Graph.VideoWindowVr.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);

                if (_class.Graph.VideoWindow != null)
                    _class.Graph.VideoWindow.SetWindowPosition(0, _class.System.Class.Vr.HeightOffset,
                        _class.FrmMain.imgDisplay.Width,
                        _class.FrmMain.imgDisplay.Height - (_class.System.Class.Vr.HeightOffset * 2));

                if (_class.Graph.VideoWindowVr != null)
                    _class.Graph.VideoWindowVr.SetWindowPosition(0, _class.System.Class.Vr.HeightOffset,
                        _class.FrmMain.imgDisplayVr.Width,
                        _class.FrmMain.imgDisplayVr.Height - (_class.System.Class.Vr.HeightOffset * 2));

                _class.Graph.VideoWindowVr.put_Visible(OABool.True);
                 */
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void SetVrDisplay()
        {
            var VideoWidth = _class.FrmMain.imgDisplay.Width;
            var VideoHeight = _class.FrmMain.imgDisplay.Height;

            try
            {
                var videoHandle = _class.FrmMain.imgDisplay.Handle;
                var videoHandle2 = _class.FrmMain.imgDisplayVr.Handle;

                _class.Graph.VideoWindow.put_Owner(videoHandle);
                _class.Graph.VideoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);

                _class.Graph.VideoWindowVr.put_Owner(videoHandle2);
                _class.Graph.VideoWindowVr.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);

                if (_class.Graph.VideoWindow != null)
                    _class.Graph.VideoWindow.SetWindowPosition(_class.System.Class.Vr.WidthOffset, _class.System.Class.Vr.HeightOffset,
                        _class.FrmMain.imgDisplay.Width - (_class.System.Class.Vr.WidthOffset * 2),
                        _class.FrmMain.imgDisplay.Height - (_class.System.Class.Vr.HeightOffset * 2));

                if (_class.Graph.VideoWindowVr != null)
                    _class.Graph.VideoWindowVr.SetWindowPosition(_class.System.Class.Vr.WidthOffset, _class.System.Class.Vr.HeightOffset,
                        _class.FrmMain.imgDisplayVr.Width - (_class.System.Class.Vr.WidthOffset * 2),
                        _class.FrmMain.imgDisplayVr.Height - (_class.System.Class.Vr.HeightOffset * 2));

                _class.Graph.VideoWindow.SetWindowForeground(OABool.True);
                _class.Graph.VideoWindowVr.put_Visible(OABool.True);

                _class.FrmMain.FocusWindow();
            }
            catch (Exception)
            {
                //ignored
            }
        }
    }
}
