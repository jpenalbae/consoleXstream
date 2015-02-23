﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using consoleXstream.Menu.Data;

namespace consoleXstream.Menu.SubMenuOptions
{
    class Profiles
    {
        private Interaction _data;
        private SubMenu.Action _subAction;
        private SubMenu.Shutter _shutter;
        private User _user;
        
        public void GetDataHandle(Interaction data) { _data = data; }
        public void GetSubActionHandle(SubMenu.Action subAction) { _subAction = subAction; }
        public void GetShutterHandle(SubMenu.Shutter shutter) { _shutter = shutter; }
        public void GetUserHandle(User user) { _user = user; }

        public List<string> List()
        {
            var listData = new List<string>();

            if (Directory.Exists("Profiles") != true) return listData;

            var listDir = Directory.GetFiles(@"Profiles", "*.connectProfile");
            if (!listDir.Any()) return listData;
            listData.AddRange(listDir.Select(Path.GetFileNameWithoutExtension));
            
            return listData;
        }

        private void Save(string strCommand)
        {
            _data.Checked.Clear();
            _data.Checked.Add(strCommand);

            _user.ConnectProfile = strCommand;
            _system.addUserData("CurrentProfile", strCommand);

            var strTitle = strCommand;
            strCommand = strCommand.Replace(" ", String.Empty);

            if (Directory.Exists("Profiles") == false) { Directory.CreateDirectory("Profiles"); }
            if (File.Exists(@"Profiles\" + strCommand + ".connectProfile")) { File.Delete(@"Profiles\" + strCommand + ".connectProfile"); }

            var strDev = _videoCapture.strVideoCaptureDevice;
            var strAud = _videoCapture.strAudioPlaybackDevice;
            var strCrossVideo = "";
            var strCrossAudio = "";

            if (_videoCapture._xBar != null)
            {
                int intPinVideo;
                int intPinAudio;
                _videoCapture._xBar.get_IsRoutedTo(0, out intPinVideo);
                _videoCapture._xBar.get_IsRoutedTo(1, out intPinAudio);
                strCrossVideo = _videoCapture.showCrossbarOutput(intPinVideo, "Video");
                strCrossAudio = _videoCapture.showCrossbarOutput(intPinAudio, "Audio");
            }

            //Control method

            var strSave = "<Profile>";
            strSave += "<Title>" + strTitle + "</Title>";
            strSave += "<videoCaptureSettings>";
            strSave += "<device>" + strDev + "</device>";
            strSave += "<audio>" + strAud + "</audio>";
            strSave += "</videoCaptureSettings>";
            if ((strCrossAudio.Length > 0) || (strCrossVideo.Length > 0))
            {
                strSave += "<videoInput>";
                if (strCrossVideo.Length > 0) { strSave += "<videoPin>" + strCrossVideo + "</videoPin>"; }
                if (strCrossAudio.Length > 0) { strSave += "<audioPin>" + strCrossAudio + "</audioPin>"; }
                strSave += "</videoInput>";
            }
            strSave += "</Profile>";

            var doc = new XmlDocument();
            doc.LoadXml(strSave);
            var settings = new XmlWriterSettings { Indent = true };
            var writer = XmlWriter.Create(@"Profiles\" + strCommand + ".connectProfile", settings);
            doc.Save(writer);
            writer.Close();
        }

        private void LoadProfile(string strFile)
        {
            var strDevice = "";
            var strAudio = "";
            var strVideoPin = "";
            var strAudioPin = "";

            var strSetting = "";
            if (Directory.Exists("Profiles") != true) return;
            if (File.Exists(@"Profiles\" + strFile + ".connectProfile") != true) return;
            var reader = new XmlTextReader(@"Profiles\" + strFile + ".connectProfile");
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        break;
                    case XmlNodeType.Text: //Display the text in each element.
                        strSetting = reader.Value;
                        break;
                    case XmlNodeType.EndElement:
                        if (strSetting.Length > 0)
                        {
                            if (reader.Name.ToLower() == "device") { strDevice = strSetting; }
                            if (reader.Name.ToLower() == "audio") { strAudio = strSetting; }
                            if (reader.Name.ToLower() == "videopin") { strVideoPin = strSetting; }
                            if (reader.Name.ToLower() == "audiopin") { strAudioPin = strSetting; }
                        }
                        strSetting = "";
                        break;
                }
            }
            reader.Close();

            _user.ConnectProfile = strFile;
            _system.addUserData("CurrentProfile", strFile);
            _system.addUserData("VideoCaptureDevice", strDevice);
            _system.addUserData("AudioPlaybackDevice", strAudio);
            if (strVideoPin.Length > 0) _system.addUserData("crossbarVideoPin", strVideoPin);
            if (strAudio.Length > 0) _system.addUserData("crossbarAudioPin", strAudioPin);

            _data.Checked.Clear();
            _data.Checked.Add(strFile);

            _videoCapture.setVideoCaptureDevice(strDevice);
            //TODO: set Audio device
            _videoCapture.setCrossbar(strVideoPin);
            _videoCapture.setCrossbar(strAudioPin);
            _videoCapture.runGraph();
        }

    }
}