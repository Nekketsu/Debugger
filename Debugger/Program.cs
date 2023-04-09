using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.System.Threading;


void MainDebuggerLoop()
{
    while (true)
    {
        PInvoke.WaitForDebugEventEx(out var debugEvent, uint.MaxValue);

        var text = debugEvent.dwDebugEventCode switch
        {
            DEBUG_EVENT_CODE.EXCEPTION_DEBUG_EVENT => "Exception",
            DEBUG_EVENT_CODE.CREATE_THREAD_DEBUG_EVENT => "CreateThread",
            DEBUG_EVENT_CODE.CREATE_PROCESS_DEBUG_EVENT => "CreateProcess",
            DEBUG_EVENT_CODE.EXIT_THREAD_DEBUG_EVENT => "ExitThread",
            DEBUG_EVENT_CODE.EXIT_PROCESS_DEBUG_EVENT => "ExitProcess",
            DEBUG_EVENT_CODE.LOAD_DLL_DEBUG_EVENT => "LoadDll",
            DEBUG_EVENT_CODE.UNLOAD_DLL_DEBUG_EVENT => "UnloadDll",
            DEBUG_EVENT_CODE.OUTPUT_DEBUG_STRING_EVENT => "OutputDebugString",
            DEBUG_EVENT_CODE.RIP_EVENT => "RipEvent",
            _ => throw new Exception("Unexpected debug event")
        };

        Console.WriteLine(text);

        if (debugEvent.dwDebugEventCode == DEBUG_EVENT_CODE.EXIT_PROCESS_DEBUG_EVENT)
        {
            break;
        }

        const int DBG_EXCEPTION_NOT_HANDLED = unchecked((int)0X80010001);
        PInvoke.ContinueDebugEvent(
            debugEvent.dwProcessId,
            debugEvent.dwThreadId,
            new NTSTATUS(DBG_EXCEPTION_NOT_HANDLED)
        );
    }
}

var startupInfo = new STARTUPINFOW();
startupInfo.cb = (uint)Marshal.SizeOf<STARTUPINFOW>();
if (!args.Any())
{
    Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <Debuggee>");
    return;
}
Console.WriteLine(args.First());
var commandLine = args.First().Append((char)0).ToArray().AsSpan();

unsafe
{
    var ret = PInvoke.CreateProcess(
        null,
        ref commandLine,
        null,
        null,
        false,
        PROCESS_CREATION_FLAGS.DEBUG_ONLY_THIS_PROCESS | PROCESS_CREATION_FLAGS.CREATE_NEW_CONSOLE,
        null,
        null,
        startupInfo,
        out var processInformation
        );

    if (ret.Value == 0)
    {
        Debug.Write("CreateProcess failed");
    }

    PInvoke.CloseHandle(processInformation.hThread);

    MainDebuggerLoop();

    PInvoke.CloseHandle(processInformation.hProcess);
}