using Debugger.Commands;
using Debugger.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Diagnostics.Debug;
using Windows.Win32.System.Threading;


const int DBG_CONTINUE = 10002;
const int DBG_EXCEPTION_NOT_HANDLED = unchecked((int)0x80010001L);
const int EXCEPTION_SINGLE_STEP = unchecked((int)0x80000004);

void MainDebuggerLoop(HANDLE process)
{
    var context = new Context();
    context.ExpectStepException = false;
    while (true)
    {
        PInvoke.WaitForDebugEventEx(out var debugEvent, uint.MaxValue);
        context.ContinueStatus = new NTSTATUS(DBG_CONTINUE);

        switch (debugEvent.dwDebugEventCode)
        {
            case DEBUG_EVENT_CODE.EXCEPTION_DEBUG_EVENT:
                var code = debugEvent.u.Exception.ExceptionRecord.ExceptionCode;
                var firstChance = debugEvent.u.Exception.dwFirstChance;
                var chanceString = firstChance == 0
                    ? "second chance"
                    : "first chance";

                if (context.ExpectStepException && code.Value == EXCEPTION_SINGLE_STEP)
                {
                    context.ExpectStepException = false;
                    context.ContinueStatus = new NTSTATUS(DBG_CONTINUE);
                }
                else
                {
                    Console.WriteLine($"Exception code {code:x} ({chanceString})");
                    context.ContinueStatus = new NTSTATUS(DBG_EXCEPTION_NOT_HANDLED);
                }

                break;
            case DEBUG_EVENT_CODE.CREATE_THREAD_DEBUG_EVENT:
                Console.WriteLine("CreateThread");
                break;
            case DEBUG_EVENT_CODE.CREATE_PROCESS_DEBUG_EVENT:
                Console.WriteLine("CreateProcess");
                break;
            case DEBUG_EVENT_CODE.EXIT_THREAD_DEBUG_EVENT:
                Console.WriteLine("ExitThread");
                break;
            case DEBUG_EVENT_CODE.EXIT_PROCESS_DEBUG_EVENT:
                Console.WriteLine("ExitProcess");
                break;
            case DEBUG_EVENT_CODE.LOAD_DLL_DEBUG_EVENT:
                Console.Write("LoadDll");
                break;
            case DEBUG_EVENT_CODE.UNLOAD_DLL_DEBUG_EVENT:
                Console.WriteLine("UnloadDll");
                break;
            case DEBUG_EVENT_CODE.OUTPUT_DEBUG_STRING_EVENT:
                Console.WriteLine("OutputDebugString");
                break;
            case DEBUG_EVENT_CODE.RIP_EVENT:
                Console.WriteLine("RipEvent");
                break;
            default:
                throw new Exception("Unexpected debug event");
        };

        var thread = PInvoke.OpenThread_SafeHandle(THREAD_ACCESS_RIGHTS.THREAD_GET_CONTEXT | THREAD_ACCESS_RIGHTS.THREAD_SET_CONTEXT,
            false,
            debugEvent.dwThreadId);

        var threadContext = new CONTEXT64();
        threadContext.ContextFlags = CONTEXT_FLAGS.CONTEXT_ALL;
        var result = PInvoke.GetThreadContext(thread.DangerousGetHandle(), ref threadContext);

        if (!result)
        {
            var error = Marshal.GetLastWin32Error();
            throw new Exception($"GetThreadContext failed ({error})");
        }

        context.ContinueExecution = false;

        while (!context.ContinueExecution)
        {
            Console.WriteLine($"[{debugEvent.dwThreadId:X}] 0x{context.ThreadContext.Rip:x16}");

            var command = Command.ReadCommand(context);
            if (command is not null)
            {
                command.Handle();
                if (context.Quit)
                {
                    // The process will be terminated since we didn't detach.
                    return;
                }
            }
        }

        if (debugEvent.dwDebugEventCode == DEBUG_EVENT_CODE.EXIT_PROCESS_DEBUG_EVENT)
        {
            break;
        }

        PInvoke.ContinueDebugEvent(
            debugEvent.dwProcessId,
            debugEvent.dwThreadId,
            new NTSTATUS(context.ContinueStatus)
        );
    }
}

var startupInfo = new STARTUPINFOW();
startupInfo.cb = (uint)Marshal.SizeOf<STARTUPINFOW>();
args = new[] { "cmd" }; 
if (!args.Any())
{
    Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} <Debuggee>");
    return;
}

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

    MainDebuggerLoop(processInformation.hProcess);

    PInvoke.CloseHandle(processInformation.hProcess);
}