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

namespace PackageVerification.Data
{
    public class DotNetNukeVersions
    {
        public static List<DotNetNukeVersionInfo> DotNetNukeVersionList()
        {
            var output = new List<DotNetNukeVersionInfo>();
            output.Add(new DotNetNukeVersionInfo("01.00.05", "12/24/03", ReleaseType.Stable, "1.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("01.00.07", "04/09/03", ReleaseType.Stable, "1.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("01.00.08", "05/14/03", ReleaseType.Stable, "1.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("01.00.09", "06/02/03", ReleaseType.Stable, "1.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("01.00.10", "01/27/04", ReleaseType.Stable, "1.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("02.00.03", "03/26/04", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("02.00.04", "04/24/04", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("02.01.01", "06/04/04", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("02.01.02", "06/14/04", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.04", "11/19/04", ReleaseType.Alpha, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.05", "11/26/04", ReleaseType.Alpha, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.06", "12/07/04", ReleaseType.Alpha, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.07", "12/10/04", ReleaseType.Alpha, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.08", "12/31/04", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.09", "01/25/05", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.10", "02/07/05", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.11", "02/23/05", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.12", "03/12/05", ReleaseType.Beta, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.00.13", "04/04/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.01.00", "06/09/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.01.01", "08/22/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.02.00", "11/07/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.00.00", "11/07/05", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.02.01", "12/07/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.00.01", "12/07/05", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.02.02", "12/23/05", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.00.02", "12/23/05", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.00.03", "03/16/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.00", "06/13/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.00", "06/13/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.01", "06/20/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.01", "06/20/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.02", "07/03/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.02", "07/03/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.03", "07/19/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.03", "07/19/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.04", "08/03/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.04", "08/03/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.05", "09/16/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.05", "09/16/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.06", "11/16/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.06", "11/16/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("03.03.07", "11/30/06", ReleaseType.Stable, "1.1", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.03.07", "11/30/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.04.00", "12/25/06", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.04.01", "01/15/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.00", "04/07/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.01", "04/16/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.02", "05/30/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.03", "05/31/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.04", "07/21/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.05.05", "07/25/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.06.00", "09/16/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.06.02", "10/01/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.07.00", "11/06/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.08.00", "12/26/07", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.08.01", "02/27/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.08.02", "03/19/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.08.03", "05/27/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.08.04", "06/11/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.00", "09/10/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.00.00", "11/11/08", ReleaseType.RC2, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.01", "12/24/08", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.00.00", "12/24/08", ReleaseType.Beta, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.02", "02/17/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.00.01", "02/25/09", ReleaseType.Beta, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.03", "04/07/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.00", "05/12/09", ReleaseType.Beta, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.04", "05/21/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.00", "06/16/09", ReleaseType.RC1, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.00", "06/23/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.01", "06/28/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("04.09.05", "09/02/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.02", "09/02/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.03", "09/30/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.01.04", "10/01/09", ReleaseType.Stable, "2.0", "2000", false));
            output.Add(new DotNetNukeVersionInfo("05.02.00", "10/13/09", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.02.00", "11/25/09", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.02.01", "12/22/09", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.02.02", "01/20/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.02.03", "02/17/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.03.00", "03/01/10", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.03.01", "03/24/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.04.00", "04/19/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.04.01", "04/28/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.04.02", "05/19/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.04.04", "06/28/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.05.00", "06/28/10", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.05.00", "08/18/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.05.01", "09/21/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.00", "10/26/10", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.00", "11/17/10", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.01", "12/15/10", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.01", "01/13/11", ReleaseType.RC1, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.01", "01/19/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.02", "03/03/11", ReleaseType.Beta, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.00.00", "03/15/11", ReleaseType.CTP1, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.02", "03/23/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("05.06.03", "07/05/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.00.00", "07/07/11", ReleaseType.Beta, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.00.00", "07/20/11", ReleaseType.RC1, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.00.00", "07/20/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.00.01", "08/24/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.00.02", "10/05/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.01.00", "10/07/11", ReleaseType.Beta, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.01.00", "11/01/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.04", "11/01/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.01.01", "11/15/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.05", "11/15/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.01.02", "12/14/11", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.06", "12/14/11", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.01.03", "02/01/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.07", "02/01/12", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.01.04", "03/07/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.02.00", "03/16/12", ReleaseType.RC1, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.01.05", "04/04/12", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.02.00", "05/29/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.02.01", "07/11/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("05.06.08", "07/11/12", ReleaseType.Stable, "3.5", "2005", false));
            output.Add(new DotNetNukeVersionInfo("06.02.02", "08/01/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.02.03", "09/05/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.00.00", "09/07/12", ReleaseType.RC1, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("06.02.04", "10/10/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("06.02.05", "11/15/12", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.00.00", "11/28/12", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.00.01", "01/09/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("06.02.06", "01/09/13", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.00.02", "01/11/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.00.03", "02/06/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.00.04", "03/06/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.00.05", "04/03/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("06.02.07", "04/03/13", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.00.06", "05/01/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.01.00", "07/09/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("06.02.08", "07/09/13", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.01.01", "08/13/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("06.02.09", "08/15/13", ReleaseType.Stable, "3.5", "2005", true));
            output.Add(new DotNetNukeVersionInfo("07.02.00", "10/16/13", ReleaseType.Beta, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.02.00", "11/27/13", ReleaseType.RC1, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.02.00", "12/04/13", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.02.01", "01/21/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.02.02", "03/19/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.00", "06/11/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.01", "06/26/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.02", "08/14/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.03", "10/01/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.03.04", "11/12/14", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.00", "01/15/15", ReleaseType.Beta, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.00", "01/28/15", ReleaseType.RC1, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.00", "02/04/15", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "03/19/15", ReleaseType.CTP1, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "05/22/15", ReleaseType.CTP2, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.01", "05/26/15", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "07/20/15", ReleaseType.CTP3, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.02", "08/06/15", ReleaseType.Beta, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "08/31/15", ReleaseType.CTP4, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "09/24/15", ReleaseType.CTP5, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("07.04.02", "10/05/15", ReleaseType.Stable, "4.0", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "10/07/15", ReleaseType.CTP6, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "11/10/15", ReleaseType.CTP7, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "12/16/15", ReleaseType.Beta, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "01/12/16", ReleaseType.RC1, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.00", "01/14/16", ReleaseType.Stable, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.01", "03/16/16", ReleaseType.Stable, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.02", "04/18/16", ReleaseType.Stable, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.03", "05/26/16", ReleaseType.Stable, "4.5", "2008", true));
            output.Add(new DotNetNukeVersionInfo("08.00.04", "08/17/16", ReleaseType.Stable, "4.5", "2008", true));
            return output;
        }

        public static int ConvertNameToRank(string input)
        {
            var output = 0;
            var nameArray = input.Split('.');

            var place = 10000;

            foreach (var s in nameArray)
            {
                int val;
                if (int.TryParse(s, out val) && place >= 1)
                {
                    output += val*place;
                    place = place/100;
                }
            }

            return output;
        }
    }

    [Serializable]
    public class DotNetNukeVersionInfo
    {
        public string Name { get; set; }
        public string AssemblyVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
        public ReleaseType ReleaseType { get; set; }
        public FrameworkInfo MinDotNetVersion { get; set; }
        public SQLServerVersionInfo MinSQLServerVersion { get; set; }
        public IISVersionInfo MinIISVersion { get; set; }
        public Boolean SupportsAzure { get; set; }
        
        public int SortOrder()
        {
            var nameParts = Name.Split('.');

            int major = 0;
            int minor = 0;
            int sub = 0;

            int.TryParse(nameParts[0], out major);
            int.TryParse(nameParts[1], out minor);
            int.TryParse(nameParts[2], out sub);

            return ((major*100000) + (minor*1000) + (sub*10));
        }

        public DotNetNukeVersionInfo()
        {
            Name = "00.00.00";
            AssemblyVersion = "0.0.0.0";
            ReleaseDate = DateTime.MinValue;
            ReleaseType = ReleaseType.Stable;
            MinDotNetVersion = new FrameworkInfo();
            MinSQLServerVersion = new SQLServerVersionInfo();
            MinIISVersion = new IISVersionInfo();
            SupportsAzure = false;
        }

        public DotNetNukeVersionInfo(String name, String strReleaseDate, ReleaseType releaseType, string dotNetVersion, string sqlServerVersion, Boolean supportsAzure)
        {
            //first we set the name
            Name = name;
            
            //next we need to parse and set the release date
            DateTime releaseDate;
            DateTime.TryParse(strReleaseDate, out releaseDate);
            ReleaseDate = releaseDate;

            //now we can set the release type
            ReleaseType = releaseType;

            //let's figure out and set the minimum required framework for this release.
            var dotNetVersionList = DotNetVersions.FrameworkList();

            foreach (var frameworkInfo in dotNetVersionList)
            {
                if(frameworkInfo.VersionName == dotNetVersion)
                {
                    MinDotNetVersion = frameworkInfo;
                }
            }

            //let's figure out and se the minimum required version of SQL server.
            var sqlServerVersionList = SQLServerVersions.SQLServerVersionList();

            foreach (var sqlServerVersionInfo in sqlServerVersionList)
            {
                if(sqlServerVersionInfo.ShortName == sqlServerVersion)
                {
                    MinSQLServerVersion = sqlServerVersionInfo;
                }
            }

            //next set the supports azure flag.
            SupportsAzure = supportsAzure;
        }

        public string PackageFolderPath()
        {
            return "../../../DNNPackages/" + Name + "_" + ReleaseType.ToString();
        }

        public string PackageFolderPathLive()
        {
            return "/approot/" + Name + "_" + ReleaseType.ToString();
        }
    }
}
