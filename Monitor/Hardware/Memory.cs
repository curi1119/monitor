﻿using System.Runtime.InteropServices;

namespace Monitor.Hardware
{
    class Memory
    {
        private NativeMethods.MemoryStatusEx _status;
        private ulong _usedMemory;
        private ulong _totalPhysicalMemory;
        private ulong _availablePhysicalMemory;

        public uint TotalMb { get; private set; }
        public uint UsedMb { get; private set; }
        public uint FreeMb { get; private set; }
        public uint LoadPct { get; private set; }

        public Memory()
        {
            _status = new NativeMethods.MemoryStatusEx();
        }

        public void Update()
        {
            _status.Length = checked((uint)Marshal.SizeOf(typeof(NativeMethods.MemoryStatusEx)));
            if (!NativeMethods.GlobalMemoryStatusEx(ref _status)) return;
        
            _totalPhysicalMemory = _status.TotalPhysicalMemory;
            _availablePhysicalMemory = _status.AvailablePhysicalMemory;
            _usedMemory = _totalPhysicalMemory - _availablePhysicalMemory;
            LoadPct = _status.MemoryLoad;
            TotalMb = (uint)(_totalPhysicalMemory / 1024.0f / 1024.0f);
            UsedMb  = (uint)(_usedMemory / 1024.0f / 1024.0f);
            FreeMb  = (uint)(_availablePhysicalMemory / 1024.0f / 1024.0f);
        }

        private class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MemoryStatusEx
            {
                public uint Length;
                public uint MemoryLoad;
                public ulong TotalPhysicalMemory;
                public ulong AvailablePhysicalMemory;
                public ulong TotalPageFile;
                public ulong AvailPageFile;
                public ulong TotalVirtual;
                public ulong AvailVirtual;
                public ulong AvailExtendedVirtual;
            }
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GlobalMemoryStatusEx(ref NativeMethods.MemoryStatusEx buffer);
        }
    }
}
