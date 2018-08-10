#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace SQLAzureMWUtils
{
    public class AsyncNotificationEventArgs : EventArgs
    {
        private NotificationEventFunctionCode _functionCode; // 0 = BCPUploadData, 1 = ExecuteSQLonAzure, 2 = ParseFile
                                                             // 3 = GenerateScriptFromSQLServer, 4 ScriptDatabase Results, 5 ScriptDatabase TSQL
        private string _StatusMsg;
        private string _DisplayText;
        private int _percentComplete;
        private int _numeratorComplete;
        private int _totalDenominator;
        private Color _DisplayColor;
        private bool _disableNext = false;
        private int _filesProcessed = 0;
        private int _filesToProcess = 0;

        public string StatusMsg
        {
            get { return _StatusMsg; }
            set { _StatusMsg = value; }
        }

        public Color DisplayColor
        {
            get { return _DisplayColor; }
            set { _DisplayColor = value; }
        }

        public string DisplayText
        {
            get { return _DisplayText; }
            set { _DisplayText = value; }
        }

        public int PercentComplete
        {
            get { return _percentComplete; }
            set { _percentComplete = value; }
        }

        //optional 
        public int NumeratorComplete
        {
            get { return _numeratorComplete; }
            set { _numeratorComplete = value; }
        }

        //optional
        public int TotalDenominator
        {
            get { return _totalDenominator; }
            set { _totalDenominator = value; }
        }

        public NotificationEventFunctionCode FunctionCode
        {
            get { return _functionCode; }
            set { _functionCode = value; }
        }

        public bool DisableNext
        {
            get { return _disableNext; }
            set { _disableNext = value; }
        }

        public int FilesProcessed
        {
            get { return _filesProcessed; }
            set { _filesProcessed = value; }
        }

        public int FilesToProcess
        {
            get { return _filesToProcess; }
            set { _filesToProcess = value; }
        }
        public AsyncNotificationEventArgs(NotificationEventFunctionCode functionCode, int percentComplete, string statusMessage, string displayText, Color displayColor)
        {
            _functionCode = functionCode;
            _StatusMsg = statusMessage;
            _percentComplete = percentComplete;
            _DisplayText = displayText;
            _DisplayColor = displayColor;
            _disableNext = false;
        }
    }
}
