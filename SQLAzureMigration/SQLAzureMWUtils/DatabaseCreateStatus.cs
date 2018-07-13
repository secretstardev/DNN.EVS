using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLAzureMWUtils
{
    [Serializable]
    public enum DatabaseCreateStatus
    {
        Failed = 0,
        Waiting = 1,
        Success = 2
    }
}
