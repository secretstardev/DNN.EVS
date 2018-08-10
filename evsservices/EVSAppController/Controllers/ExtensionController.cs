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

using EVSAppController.Models;

namespace EVSAppController.Controllers
{
    public class ExtensionController : PetaPocoBaseClass
    {
        //create

        public static void AddExtension(Extension extension)
        {
            EVSDatabase.Insert("Extension", "ExtensionID", true, extension);
        }

        //read

        public static Extension GetExtension(int extensionId)
        {
            return EVSDatabase.First<Extension>("SELECT * FROM [dbo].[Extension] WHERE ExtensionID = @0", extensionId);
        }

        public static Extension GetExtension(string fileId)
        {
            return EVSDatabase.First<Extension>("SELECT * FROM [dbo].[Extension] WHERE FileID LIKE @0", fileId);
        }

        //update

        //delete

        //business logic
    }
}