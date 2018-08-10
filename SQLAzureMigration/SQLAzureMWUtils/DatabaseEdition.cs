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

namespace SQLAzureMWUtils
{
    public class DatabaseEdition
    {
        private KeyValuePair<string, string> _Edition;
        private List<KeyValuePair<string, string>> _PerformanceLevel;
        private List<KeyValuePair<string, string>> _Size;

        public KeyValuePair<string, string> Edition
        {
            get
            {
                return _Edition;
            }

            set
            {
                _Edition = value;
            }
        }

        public List<KeyValuePair<string, string>> PerformanceLevel
        {
            get
            {
                if (_PerformanceLevel == null)
                {
                    _PerformanceLevel = new List<KeyValuePair<string, string>>();
                }
                return _PerformanceLevel;
            }

            set
            {
                _PerformanceLevel = value;
            }
        }

        public List<KeyValuePair<string, string>> Size
        {
            get
            {
                if (_Size == null)
                {
                    _Size = new List<KeyValuePair<string, string>>();
                }
                return _Size;
            }

            set
            {
                _Size = value;
            }
        }

        public string Display
        {
            get
            {
                return _Edition.Key;
            }
        }

        public DatabaseEdition() { }
        public DatabaseEdition(KeyValuePair<string, string> edition)
        {
            _Edition = edition;
        }
    }
}
