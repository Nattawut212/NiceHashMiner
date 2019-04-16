// SHARED
using NiceHashMiner.Benchmarking;
using NiceHashMiner.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using static NiceHashMiner.Translations;

namespace NiceHashMiner
{
    static partial class ApplicationStateManager
    {
        #region BuildTag
        private const string BetaAlphaPostfixString = " - Alpha";
#if TESTNET
        private static readonly string BuildTag = " (TESTNET)";
#elif TESTNETDEV
        private static readonly string BuildTag = " (TESTNETDEV)";
#else
        private static readonly string BuildTag = "";
#endif
        public static string Title
        {
            get
            {
                return " v" + Application.ProductVersion + BetaAlphaPostfixString + BuildTag;
            }
        }
        #endregion BuildTag

        public static CancellationTokenSource ExitApplication { get; } = new CancellationTokenSource();

        public static void BeforeExit()
        {
            // TESTNET
#if TESTNET || TESTNETDEV
            StopRefreshDeviceListViewTimer();
            // close websocket
            NiceHashStats.EndConnection();
            // stop all mining and benchmarking devices
            StopAllDevice();
#endif
            // PRODUCTION
#if !(TESTNET || TESTNETDEV)
            try
            {
                ExitApplication.Cancel();
            }
            catch { }
            NiceHashMiner.Miners.MinersManager.StopAllMiners();
            MessageBoxManager.Unregister();
#endif
        }

        public static void RestartProgram()
        {
            var pHandle = new Process
            {
                StartInfo =
                {
                    FileName = Application.ExecutablePath
                }
            };
            pHandle.Start();
            Application.Exit();
        }

        public static bool BurnCalled { get; private set; } = false;
        public static void Burn(string message)
        {
            if (BurnCalled) return;
            BurnCalled = true;
            BeforeExit();
            MessageBox.Show(message, Tr("Error!"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        // EnsureSystemRequirements will check if all system requirements are met if not it will show an error/warning message box and exit the application
        // TODO this one holds
        public static bool SystemRequirementsEnsured()
        {
            // check WMI
            if (!WindowsManagementObjectSearcher.IsWmiEnabled())
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy cannot run needed components. It seems that your system has Windows Management Instrumentation service Disabled. In order for NiceHash Miner Legacy to work properly Windows Management Instrumentation service needs to be Enabled. This service is needed to detect RAM usage and Avaliable Video controler information. Enable Windows Management Instrumentation service manually and start NiceHash Miner Legacy."),
                        Tr("Windows Management Instrumentation Error"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }

            if (!Helpers.Is45NetOrHigher())
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy requires .NET Framework 4.5 or higher to work properly. Please install Microsoft .NET Framework 4.5"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            if (!Helpers.Is64BitOperatingSystem)
            {
                MessageBox.Show(Tr("NiceHash Miner Legacy supports only x64 platforms. You will not be able to use NiceHash Miner Legacy with x86"),
                    Tr("Warning!"),
                    MessageBoxButtons.OK);

                return false;
            }

            return true;
        }
    }
}
