﻿using System;
using DirectShowLib;

namespace consoleXstream.VideoCapture.Sampling
{
    public class SampleGrabberCallback : ISampleGrabberCB
    {
        private Form1 _form1;
        private string _strFpsCheck;
        private int _intSampleFrame;

        public void GetForm1Handle(Form1 inForm) { _form1 = inForm; }

        public int BufferCB(double sampleTime, IntPtr pBuffer, int bufferLen)
        {
            if (_strFpsCheck != DateTime.Now.ToString("ss"))
            {
                _form1.intSampleFPS = _intSampleFrame;
                _intSampleFrame = 0;
                _strFpsCheck = DateTime.Now.ToString("ss");
            }
            else
            {
                _intSampleFrame++;
            }

            return 0;
        }

        public int SampleCB(double sampleTime, IMediaSample pSample)
        {
            return 0;
        }
    }
}
