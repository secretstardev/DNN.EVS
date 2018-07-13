using System;
using NUnit.Framework;
using PackageVerification.Models;

namespace PackageVerification.Tests
{
    [TestFixture]
    public class VerificationControllerTests : PackagesTestBase
    {
        [Test]
        public void ProperTest()
        {
            //arrange
            var path = Package("Proper");

            Console.WriteLine(path);

            //act
            var package = VerificationController.TryParse(path);

            //assert
            Assert.IsNotNull(package, "Proper package result is null:" + path);

        }

        [Test]
        public void ProperNoErrorsTest()
        {
            //arrang
            var path = Package("Proper");

            Console.WriteLine(path);

            //act
            var package = VerificationController.TryParse(path);

            //assert
            Assert.IsNotNull(package, "Proper package result is null:" + path);
            Assert.IsNotNull(package.Messages, "Proper package messages is null:" + path);
            foreach (var m in package.Messages)
            {
                Assert.AreEqual(m.MessageType, MessageTypes.Error, "Proper package message has an error:" + m.Message + " " + m.Rule+ " " + path);
            }
        }

        [Test]
        public void AllTestZipsFailExceptProper()
        {
            //arrange
            var files = AllPackages();

            foreach (var file in files)
            {
                if (!file.Contains("Proper.zip"))
                {
                    var package = VerificationController.TryParse(file);

                    //assert
                    Assert.IsNotNull(package, "Proper package result is null:" + file);
                    Assert.IsNotNull(package.Messages, "Proper package messages is null:" + file);
                    var hasFail = false;

                    foreach (var m in package.Messages)
                    {
                        if (m.MessageType == MessageTypes.Error)
                        {
                            hasFail = true;
                            break;
                        }
                    }
                    Assert.IsTrue(hasFail, "All non-proper test failed:" + file);
                }
            }
        }

        [Test]
        public void ProperMinDotNetNuke()
        {
            //arrange
            var path = Package("Proper");
            //act
            var package = VerificationController.TryParse(path);
            //assert
            Assert.AreEqual(package.MinDotNetNukeVersion, "06.01.00");
        }

        [Test]
        public void ProperMinDotNet()
        {
            //arrange
            var path = Package("Proper");
            //act
            var package = VerificationController.TryParse(path);
            //assert
            Assert.AreEqual(package.MinDotNetVersion, "3.5");
        }
    }
}
