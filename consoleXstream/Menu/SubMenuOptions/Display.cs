﻿namespace consoleXstream.Menu.SubMenuOptions
{
    public class Display
    {
        public Display(Classes classes) { _class = classes; }
        private readonly Classes _class;

        public void ChangeResolution(string resolution)
        {
            _class.Base.System.strCurrentResolution = resolution;
            resolution = resolution.ToLower();
            if (resolution == "resolution")
                return;

            var listRes = _class.Base.VideoCapture.GetVideoResolution();
            for (var count = 0; count < listRes.Count; count++)
            {
                if (resolution != listRes[count].ToLower())
                    continue;

                _class.Base.VideoCapture.SetVideoResolution(count);
                _class.Base.VideoCapture.RunGraph();

                _class.Base.System.AddData("CaptureResolution", resolution);

                break;
            }
        }

        private void ListDisplayRefresh()
        {
            _class.Data.ClearButtons();

            _class.Shutter.Scroll = 0;

            _class.Data.SubItems.Clear();
            _class.Data.Checked.Clear();
            _class.Shutter.Error = "";
            _class.Shutter.Explain = "";
            _class.User.Menu = "videorefresh";

            var listDisplayRef = _class.Base.System.GetDisplayRefresh();
            var currentRef = _class.Base.System.GetRefreshRate().ToLower();

            foreach (var title in listDisplayRef)
            {
                if (title.ToLower() == currentRef)
                    _class.SubAction.AddSubItem(title, title, true);
                else
                    _class.SubAction.AddSubItem(title, title);
            }

            SelectSubItem();
        }

        private void ListDisplayResolution()
        {
            _class.Data.ClearButtons();

            _class.Shutter.Scroll = 0;

            _class.Data.SubItems.Clear();
            _class.Data.Checked.Clear();
            _class.Shutter.Error = "";
            _class.Shutter.Explain = "";
            _class.User.Menu = "videoresolution";

            var listDisplayRes = _class.Base.System.GetDisplayResolutionList();
            var currentRes = _class.Base.System.GetResolution().ToLower();

            foreach (var title in listDisplayRes)
            {
                if (title.ToLower() == currentRes)
                    _class.SubAction.AddSubItem(title, title, true);
                else
                    _class.SubAction.AddSubItem(title, title);
            }

            SelectSubItem();
        }

        public void ChangeVideoResolution(string command)
        {
            if (command.ToLower() == "resolution")
                return;

            _class.Base.System.SetDisplayResolution(command);
            _class.Data.Checked.Clear();
            _class.Data.Checked.Add(command);
            
            _class.DisplayMenu.PositionMenu();
        }
    
        public void ChangeVideoRefresh(string command)
        {
            if (command.ToLower() == "refresh")
                return;

            _class.Base.System.SetDisplayRefresh(command);
            _class.Data.Checked.Clear();
            _class.Data.Checked.Add(command);

            _class.DisplayMenu.PositionMenu();
        }

        private void ChangeAutoRes()
        {
            if (_class.Data.Checked.IndexOf("Auto Set") > -1)
                _class.Data.Checked.RemoveAt(_class.Data.Checked.IndexOf("Auto Set"));
            else
                _class.Data.Checked.Add("Auto Set");

            _class.Base.System.Class.Display.SetAutoChangeDisplay();
        }

        private void ChangeStayOnTop()
        {
            if (_class.Data.Checked.IndexOf("Stay On Top") > -1)
                _class.Data.Checked.RemoveAt(_class.Data.Checked.IndexOf("Stay On Top"));
            else
                _class.Data.Checked.Add("Stay On Top");

            _class.Base.System.Class.Display.SetStayOnTop();
        }

        public void ChangeVideoDisplay(string command)
        {
            command = command.ToLower();
            if (command == "autoset") ChangeAutoRes();
            if (command == "resolution") ListDisplayResolution();
            if (command == "refresh") ListDisplayRefresh();
            if (command == "stayontop") ChangeStayOnTop();
        }

        private void SelectSubItem()
        {
            if (_class.Data.SubItems.Count > 0)
            {
                _class.User.SubSelected = _class.Data.SubItems[0].Command;
            }
        }


    }
}
