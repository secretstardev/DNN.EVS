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
