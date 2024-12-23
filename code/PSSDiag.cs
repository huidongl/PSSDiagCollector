using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PSSDiagCollector
{
    public static class PSSDiag
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        private static Process PSSDiagProcess;
        private static string SplitLine = "================================================\r\n";

        public static bool IsAlive
        {
            get
            {
                if (PSSDiagProcess != null)
                {
                    return PSSDiagProcess.HasExited;
                }

                return true;
            }
        }

        public static void Start()
        {
            CancelExisingPSSDiag();
            StartProcess();
        }

        private static void StartProcess()
        {

            Console.WriteLine("Starting PSSDiag");
            ProcessStartInfo PSSDiagStartInfo = new ProcessStartInfo();
            PSSDiagStartInfo.FileName = "PSSDiag.exe";
            PSSDiagStartInfo.RedirectStandardError = false;
            PSSDiagStartInfo.RedirectStandardOutput = false;
            PSSDiagStartInfo.RedirectStandardInput = false;
            PSSDiagStartInfo.UseShellExecute = false;
            PSSDiagStartInfo.CreateNoWindow = false;

            //PSSDiag Process
            PSSDiagProcess = new Process();
            PSSDiagProcess.StartInfo = PSSDiagStartInfo;
            PSSDiagProcess.Start();

            PSSDiagProcess.WaitForExit();
        }

        private static void CancelExisingPSSDiag()
        {
            Process[] Ps = Process.GetProcessesByName("PSSDiag.exe");

            Console.WriteLine($"{SplitLine}Checking whether any PSSDiag.exe is running...");

            if (Ps.Length == 0)
            {
                Console.WriteLine("No running PSSDiag.exe detected");
            }
            else
            {
                Console.WriteLine($"{Ps.Length} existing PSSDiag.exe detected, start cancelling");

                foreach (Process P in Ps)
                {
                    Console.WriteLine($"PID: {P.Id} killed.");
                    P.Kill();
                }
            }

            Console.WriteLine(SplitLine);
        }

        public static void Stop(int Delay)
        {
            if (Delay != 0)
            {
                Console.WriteLine($"{SplitLine}\r\nHit target event count, postpone the stop according to the configured setting ({Delay}) seconds.\r\n\r\n{SplitLine}");
                Thread.Sleep(Delay * 1000);
            }

            Console.WriteLine($"\r\n{SplitLine}\r\nStopping the PSSDiag trace capture.\r\n\r\n{SplitLine}");

            UInt32 CommandCtrlC = 0;
            GenerateConsoleCtrlEvent(CommandCtrlC, Convert.ToUInt32(PSSDiagProcess.Id));
        }
    }
}
