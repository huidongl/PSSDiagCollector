using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

//PSSDiagCollector.exe  -eid 7195 -c 3 -p 30 -es "BizTalk Server" -lc "Application"

namespace PSSDiagCollector
{
    class Program
    {
        private static int delay;

        private static string SplitLine = "================================================\r\n";
        // Import necessary functions from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlTypes sig);

        static EventHandler _handler;

        private enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1
        }





        private static bool Handler(CtrlTypes sig)
        {
            Console.WriteLine("Waiting PSSDiag to write the trace files...");

            while (!PSSDiag.IsAlive)
            {
                Thread.Sleep(10);
            }

            Environment.Exit(-1);

            return true;
        }

        static void Main(string[] args)
        {

            if (!CheckIsAdmin())
            {
                Console.WriteLine("Please run the CMD as administrator");

                Environment.Exit(0);
            }

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            CommandLineApplication app = new CommandLineApplication();

            app.HelpOption("-?|-h|-help");
            app.Description = "PSSDiag collector";

            // Start a process to run your executable from a specified folder
            try
            {
                CommandOption DelayCO = app.Option("-p|--Postpone", "Postpone (in seconds) before stop the trace, default value is 60.", CommandOptionType.SingleValue);
                CommandOption EventIDCO = app.Option("-eid|--EventID", "Event ID in the Event Log", CommandOptionType.SingleValue);
                CommandOption SourceCO = app.Option("-es|--EventSource", "Event Source in the Event Log", CommandOptionType.SingleValue);
                CommandOption CountCO = app.Option("-c|--EventCount", "How many events triggered to stop the capture, default value is 1.", CommandOptionType.SingleValue);
                CommandOption LogCategoryCO = app.Option("-lc|--LogCategory", "The type of the Event (Application, Security, Setup, System), the default is Application.", CommandOptionType.SingleValue);

                app.OnExecute(() =>
                {
                    int Delay = Int32.Parse(DelayCO.Value() ?? "60");
                    long EventID = long.Parse(EventIDCO.Value() ?? "0");
                    string Source = SourceCO.Value();
                    int Count = Int32.Parse(CountCO.Value() ?? "1");
                    string LogCategory = LogCategoryCO.Value() ?? "Application";
                    string FileName = "output\\timelog.txt";

                    delay = Delay;
                    EventLogMonitor.Register(LogCategory, EventID, Source, Count, FileName);
                    EventLogMonitor.OnReached += EventLog_Reached;
                    PSSDiag.Start();

                    Console.WriteLine($"\r\n{SplitLine}Capture finished! You can close the window.");

                    return 0;
                });

                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        private static bool CheckIsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private static void EventLog_Reached(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"Hit the preset event count, process to stop the PSSDiag trace.");
                EventLogMonitor.Unregister();
                PSSDiag.Stop(delay);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping tracing: {ex.Message}");
            }
        }
    }
}
