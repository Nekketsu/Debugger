﻿using Debugger.Models;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Windows.Win32
{
    internal static partial class PInvoke
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT lpContext);

        // Get context of thread x64, in x64 application
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

        // Use Marshal.AllocHGlobal and Marshal.StructureToPtr to create IntPtr from either
        // CONTEXT or CONTEXT64
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetThreadContext(IntPtr hThread, IntPtr lpContext);



        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool Wow64GetThreadContext(SafeFileHandle SafeAccessTokenHandle, ref CONTEXT lpContext);
    }

    public enum CONTEXT_FLAGS : uint
    {
        CONTEXT_i386 = 0x10000,
        CONTEXT_i486 = 0x10000,   //  same as i386
        CONTEXT_CONTROL = CONTEXT_i386 | 0x01, // SS:SP, CS:IP, FLAGS, BP
        CONTEXT_INTEGER = CONTEXT_i386 | 0x02, // AX, BX, CX, DX, SI, DI
        CONTEXT_SEGMENTS = CONTEXT_i386 | 0x04, // DS, ES, FS, GS
        CONTEXT_FLOATING_POINT = CONTEXT_i386 | 0x08, // 387 state
        CONTEXT_DEBUG_REGISTERS = CONTEXT_i386 | 0x10, // DB 0-3,6,7
        CONTEXT_EXTENDED_REGISTERS = CONTEXT_i386 | 0x20, // cpu specific extensions
        CONTEXT_FULL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS,
        CONTEXT_ALL = CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS | CONTEXT_FLOATING_POINT | CONTEXT_DEBUG_REGISTERS | CONTEXT_EXTENDED_REGISTERS
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct FLOATING_SAVE_AREA
    {
        public uint ControlWord;
        public uint StatusWord;
        public uint TagWord;
        public uint ErrorOffset;
        public uint ErrorSelector;
        public uint DataOffset;
        public uint DataSelector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RegisterArea;
        public uint Cr0NpxState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONTEXT
    {
        public uint ContextFlags; //set this to an appropriate value
                                  // Retrieved by CONTEXT_DEBUG_REGISTERS
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        // Retrieved by CONTEXT_FLOATING_POINT
        public FLOATING_SAVE_AREA FloatSave;
        // Retrieved by CONTEXT_SEGMENTS
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        // Retrieved by CONTEXT_INTEGER
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        // Retrieved by CONTEXT_CONTROL
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        // Retrieved by CONTEXT_EXTENDED_REGISTERS
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ExtendedRegisters;
    }

    // Next x64

    [StructLayout(LayoutKind.Sequential)]
    public struct M128A
    {
        public ulong High;
        public long Low;

        public override string ToString()
        {
            return string.Format("High:{0}, Low:{1}", this.High, this.Low);
        }
    }

    /// <summary>
    /// x64
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct XSAVE_FORMAT64
    {
        public ushort ControlWord;
        public ushort StatusWord;
        public byte TagWord;
        public byte Reserved1;
        public ushort ErrorOpcode;
        public uint ErrorOffset;
        public ushort ErrorSelector;
        public ushort Reserved2;
        public uint DataOffset;
        public ushort DataSelector;
        public ushort Reserved3;
        public uint MxCsr;
        public uint MxCsr_Mask;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public M128A[] FloatRegisters;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public M128A[] XmmRegisters;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 96)]
        public byte[] Reserved4;
    }

    /// <summary>
    /// x64
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct CONTEXT64
    {
        public ulong P1Home;
        public ulong P2Home;
        public ulong P3Home;
        public ulong P4Home;
        public ulong P5Home;
        public ulong P6Home;

        public CONTEXT_FLAGS ContextFlags;
        public uint MxCsr;

        public ushort SegCs;
        public ushort SegDs;
        public ushort SegEs;
        public ushort SegFs;
        public ushort SegGs;
        public ushort SegSs;
        public uint EFlags;

        public ulong Dr0;
        public ulong Dr1;
        public ulong Dr2;
        public ulong Dr3;
        public ulong Dr6;
        public ulong Dr7;

        public ulong Rax;
        public ulong Rcx;
        public ulong Rdx;
        public ulong Rbx;
        public ulong Rsp;
        public ulong Rbp;
        public ulong Rsi;
        public ulong Rdi;
        public ulong R8;
        public ulong R9;
        public ulong R10;
        public ulong R11;
        public ulong R12;
        public ulong R13;
        public ulong R14;
        public ulong R15;
        public ulong Rip;

        public XSAVE_FORMAT64 DUMMYUNIONNAME;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public M128A[] VectorRegister;
        public ulong VectorControl;

        public ulong DebugControl;
        public ulong LastBranchToRip;
        public ulong LastBranchFromRip;
        public ulong LastExceptionToRip;
        public ulong LastExceptionFromRip;
    }
}
