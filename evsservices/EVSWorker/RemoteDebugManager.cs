using System;

namespace EVSWorker
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    /// <summary> 
    /// This class is a wrapper around Visual Studio 2012 Remote Debugger 
    ///     Installs, Configures, Starts, Stops the remote debugger 
    /// </summary> 
    public class RemoteDebugManager
    {

        public string HostName { get; set; }

        public string LocalResource { get; set; }

        public string StorageAccountConfigKey { get; set; }

        public string ToolsContainer { get; set; }

        //We could change this to a get only property that checks to see if the processes are up 
        public bool IsRunning { get; set; }

        private bool IsInstalled
        {
            get
            {
                return File.Exists(RemoteDebuggerExecutablePath);
            }
        }

        //NOTE: We could resolve this from registry 
        public string RemoteDebuggerExecutablePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Microsoft Visual Studio 11.0\Common7\IDE\Remote Debugger\x64\msvsmon.exe");
            }
        }

        /// <summary> 
        /// Retrieve remote debugger from azure storage, install, and run msvsmon with /prepcomputer 
        /// </summary> 
        public void InstallAndConfigure()
        {
            //We may want to consider repair or just donig a reinstall from some point 
            if (IsInstalled) return;

            var account =
                CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(StorageAccountConfigKey));

            var blobClient = account.CreateCloudBlobClient();
            var toolsContainer = blobClient.GetContainerReference(ToolsContainer);
            var cloudBlob = toolsContainer.GetBlobReference("rtools_setup_x64.exe");

            var toolsResource = RoleEnvironment.GetLocalResource(LocalResource);
            if (null == toolsResource)
                throw new ApplicationException(string.Format("Could Not Find Local Resource: {0}", LocalResource));

            var remoteToolsInstall = Path.Combine(toolsResource.RootPath, "rtools_setup_x64.exe");
            cloudBlob.DownloadToFile(remoteToolsInstall);

            //Install Remote Tools - Wait For Install to Complete 
            CreateProcess(remoteToolsInstall, "/install /norestart /quiet");

            if (!IsInstalled)
                throw new ApplicationException("Visual Studio 2012 Remote Debugger Tools Failed Install");

            //Run Remote Debugger Configuration (Checks for WWSAPI and  Configures Firewall) 
            CreateProcess(RemoteDebuggerExecutablePath, "/prepcomputer /domain /private /public /quiet");
        }

        /// <summary> 
        /// Creates a Remote Debugger Process and currently kills / starts all msvsmon instances when called 
        /// </summary> 
        public void Start()
        {
            Trace.TraceInformation("Start Diagnostics Called");
            //Kill Any Process if they are already running 
            //We could check to see if the process is already running and leave it but then any configuration changes would not work here 
            KillAllRemoteDebugProcesses();

            try
            {
                //Timeout is Max Int Value -1 
                CreateProcess(RemoteDebuggerExecutablePath, string.Format("/hostname {0} /noauth /anyuser /nosecuritywarn /silent /timeout 2147483646", HostName), 0);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                throw;
            }

            //We blindly set this to true without checking to see if the process is indeed up 
            IsRunning = true;
        }

        /// <summary> 
        /// Kills all msvsmon processes on the local system 
        /// </summary> 
        public void Stop()
        {
            KillAllRemoteDebugProcesses();

            //We blindly set this to false here without verifying 
            IsRunning = false;
        }

        /// <summary> 
        /// Kill all msvsmon processes 
        /// </summary> 
        private void KillAllRemoteDebugProcesses()
        {
            foreach (var process in Process.GetProcessesByName("msvsmon"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    // process was terminating or can't be terminated 
                    Trace.TraceError("Exception attempting to terminate process (msvsmon) - Exception Message: ", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    // process has already exited? 
                    Trace.TraceError("Exception attempting to terminate process (msvsmon) - Exception Message: ", ex.Message);
                }
            }

        }

        /// <summary> 
        /// Simple method to create a process with parameters for hidden/silent 
        /// </summary> 
        /// <param name="processPath">Path to the process (.exe) to run</param> 
        /// <param name="args">Arguments to be passed in to the process</param> 
        /// <param name="wait">Time to wait for the process to exit, the default is -1 (infinite).  0 indicates not to wait for the process to exit</param> 
        private static void CreateProcess(string processPath, string args, int wait = -1)
        {
            var p = new Process
            {
                StartInfo =
                    new ProcessStartInfo(processPath)
                    {
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    }
            };

            p.Start();

            if (0 != wait)
                p.WaitForExit(wait);
        }
    }
}
