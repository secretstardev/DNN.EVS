using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageVerification.Tests
{
    public class PackagesTestBase
    {
        private string Root = @"C:\Projects\DNNAppVerification\DNNAppVerification\"; 

        public string Package(string Name)
        {
            return System.IO.Path.Combine(Root, string.Format(@"Tests\{0}.zip", Name));
        }

        public string[] AllPackages()
        {
            return System.IO.Directory.GetFiles(System.IO.Path.Combine(Root, @"Tests\"));
        }
    }
}