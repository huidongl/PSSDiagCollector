# PSSDiagCollector

The PSSDiagCollector integrates the BizTalk PSSDiag tool and Windows Event Log to collect BizTalk traces based on user-specified event IDs, event sources, event counts, and event categories.

## Instructions

### 1. Download the Tool
- Visit the "Releases" section on the right panel of the repository and download the **PSSDiagCollector.exe**.

### 2. Prepare the BizTalk PSSDiag Tool
- The PSSDiagCollector requires the BizTalk PSSDiag tool, which must first be downloaded and copied to your BizTalk server.
- Download **PSSDiagForBizTalkV10.0** from [this link](https://github.com/huidongl/Biztalk/blob/main/PSSDiagForBizTalkV10.0.zip).
- Extract the zip file to a directory on your disk.

### 3. Configure the Tool
- Run **Initialize.exe** to open the configuration window.
- Set the options as shown in the screenshot below:
  - **Trace Type**: `-rollover`
  - **Size**: `100MB`
  - **Keep the last**: `1` trace file
    
    This configuration ensures that the PSSDiag tool generates a new `BTStrace.bin` file each time it reaches 100MB, retaining only the current file and one previous file. Older files will be deleted. This setup is crucial for maintaining a manageable trace file size       and preventing the disk from being filled up.   
   ![image](https://github.com/user-attachments/assets/1805309a-0038-4076-b923-6ea2aeef1c6f)


- Click **Save and Close** to save the settings.

### 4. Start the Collection
- Open a command prompt and navigate to the **PSSDiagForBizTalkV10.0** directory.
- Run the following command to begin the collection:

  ```
  PSSDiagCollector.exe -eid <EventID> -c <EventCount> -p <PauseDuration> -es "<EventSource>" -lc "<LogCategory>"
  ```

  For example, to continuously collect the BizTalk trace until event ID `7195` occurs three times, then pause for 30 seconds before stopping the PSSDiag tracing, use the command:

  ```
  PSSDiagCollector.exe -eid 7195 -c 3 -p 30 -es "BizTalk Server" -lc "Application"
  ```
   Alternatively, for a single occurrence of event ID `7195` and a pause of 60 seconds, you can omit the `-p` parameter since the default pause duration is 60 seconds:

  ```
  PSSDiagCollector.exe -eid 7195 -c 1
  ```
### 5. Access the Collected Logs
- Once the collection is complete, the tool will save all logs to the **output** folder within the **PSSDiagForBizTalk** directory.
- This folder will include:
  - All trace files generated by the BizTalk PSSDiag tool.
  - An additional file, **timelog.txt**, which records timestamps for the occurrences of the specified events.

## Command options
You can run `PSSDiagCollector.exe -?` to view detailed information about the available command options.

```
Options:
  -?|-h|-help        Show help information
  -p|--Postpone      Postpone (in seconds) before stop the trace, default value is 60.
  -eid|--EventID     Event ID in the Event Log
  -es|--EventSource  Event Source in the Event Log
  -c|--EventCount    How many events triggered to stop the capture, default value is 1.
  -lc|--LogCategory  The type of the Event (Application, Security, Setup, System), the default is Application.
```
