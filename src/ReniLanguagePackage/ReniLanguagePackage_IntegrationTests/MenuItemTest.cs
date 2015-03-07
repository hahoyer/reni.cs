using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using HoyerWare.ReniLanguagePackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VsSDK.IntegrationTestLibrary;
using Microsoft.VSSDK.Tools.VsIdeTesting;

namespace ReniLanguagePackage_IntegrationTests
{
    [TestClass]
    public sealed class MenuItemTest
    {
        delegate void ThreadInvoker();

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     A test for lauching the command and closing the associated dialogbox
        /// </summary>
        [TestMethod]
        [HostType("VS IDE")]
        public void LaunchCommand() => UIThreadInvoker.Invoke((ThreadInvoker)ExecuteCommand);

        static void ExecuteCommand()
        {
            var menuItemCmd = new CommandID
                (
                GuidList.guidReniLanguagePackageCmdSet,
                (int) PkgCmdIDList.cmdidMyCommand
                );

            // Create the DialogBoxListener Thread.
            var expectedDialogBoxText = string.Format
                (
                    CultureInfo.CurrentCulture,
                    "{0}\n\nInside {1}.MenuItemCallback()",
                    "ReniLanguagePackage",
                    "HoyerWare.ReniLanguagePackage.ReniLanguagePackagePackage"
                );
            var purger = new DialogBoxPurger(NativeMethods.IDOK, expectedDialogBoxText);

            try
            {
                purger.Start();

                var testUtils = new TestUtils();
                testUtils.ExecuteCommand(menuItemCmd);
            }
            finally
            {
                Assert.IsTrue
                    (
                        purger.WaitForDialogThreadToTerminate(),
                        "The dialog box has not shown."
                    );
            }
        }
    }
}