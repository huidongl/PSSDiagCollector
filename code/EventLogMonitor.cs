using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSSDiagCollector
{
    public static class EventLogMonitor
    {
        private static EventLog eventLog;
        public static EventHandler OnReached;

        private static long ID;
        private static long LongEntryID;
        private static string Source;
        private static string FilePath;
        private static int TargetCount;
        private static int CurrentCount = 0;
        private static string SplitLine = "================================================\r\n";

        public static void Register(string LogCategory, long EventID, string EventSource, int Count, string FileName)
        {
            ID = EventID;
            Source = EventSource;
            TargetCount = Count;
            FilePath = FileName;

            Console.WriteLine($"{SplitLine}Attaching {ID} to Event Log.");
            eventLog = new EventLog();
            eventLog.Log = LogCategory;

            eventLog.EntryWritten += EventLog_EntryWritten;
            eventLog.EnableRaisingEvents = true;

            Console.WriteLine("Successfully attached");
        }
        private static void EventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            EventLogEntry entry = e.Entry;
            LongEntryID = e.Entry.InstanceId & 0xFFFF;

            bool isWatchingInstance = (LongEntryID == ID) || (ID == 0);
            bool isWatchingSource = (e.Entry.Source == Source) || (String.IsNullOrEmpty(Source));

            if (isWatchingInstance && isWatchingSource)
            {
                CurrentCount++;
                string textToWrite = $"\r\n{CurrentCount}/{TargetCount} event(s) hit. Event ID: {LongEntryID}. Event Soure: {e.Entry.Source}. Current Timestamp: " + DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz");
                Console.WriteLine(textToWrite);
                File.AppendAllText(FilePath, textToWrite + Environment.NewLine);
                if (CurrentCount >= TargetCount)
                {
                    OnReached(null, null);
                }
            }
        }

        public static void Unregister()
        {
            Console.WriteLine("Unattaching the Event Log.");
            if (eventLog != null)
            {
                eventLog.EntryWritten -= EventLog_EntryWritten;
                eventLog.EnableRaisingEvents = false;
            }
        }
    }
}
