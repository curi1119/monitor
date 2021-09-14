using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware.Nvidia;

namespace Monitor.Hardware
{
    class Nvidia
    {
        public string Name { get; private set; }
        private readonly NvPhysicalGpuHandle _handle;
        private readonly NvDisplayHandle? _displayHandle;

        public uint TotalMem { get; private set; }
        public uint FreeMem { get; private set; }
        public uint UsedMem { get; private set; }
        public float LoadMemPct { get; private set; }

        public uint TotalMemMb { get; private set; }
        public uint FreeMemMb { get; private set; }
        public uint UsedMemMb { get; private set; }

        public int LoadPct { get; private set; }
        public float Temperature { get; private set; }

        public Nvidia()
        {
            bool available = NVAPI.IsAvailable;

            NvPhysicalGpuHandle[] handles = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            int count;
            if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
            {
                //Console.WriteLine("Error: NvAPI_EnumPhysicalGPUs not available");
                return;
            }
            else {
                NvStatus status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out count);
                if (status != NvStatus.OK)
                {
                    //Console.WriteLine("Error: NvAPI_EnumPhysicalGPUs not available");
                    //Console.WriteLine("Status: " + status);
                    return;
                }
            }
            IDictionary<NvPhysicalGpuHandle, NvDisplayHandle> displayHandles = new Dictionary<NvPhysicalGpuHandle, NvDisplayHandle>();
            NvDisplayHandle displayHandle;
            if (NVAPI.NvAPI_EnumNvidiaDisplayHandle != null && NVAPI.NvAPI_GetPhysicalGPUsFromDisplay != null)
            {
                NvStatus status = NvStatus.OK;
                int i = 0;
                while (status == NvStatus.OK)
                {
                    displayHandle = new NvDisplayHandle();
                    status = NVAPI.NvAPI_EnumNvidiaDisplayHandle(i, ref displayHandle);
                    i++;

                    if (status == NvStatus.OK)
                    {
                        NvPhysicalGpuHandle[] handlesFromDisplay = new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
                        uint countFromDisplay;
                        if (NVAPI.NvAPI_GetPhysicalGPUsFromDisplay(displayHandle, handlesFromDisplay, out countFromDisplay) == NvStatus.OK)
                        {
                            for (int j = 0; j < countFromDisplay; j++)
                            {
                                if (!displayHandles.ContainsKey(handlesFromDisplay[j]))
                                    displayHandles.Add(handlesFromDisplay[j], displayHandle);
                            }
                        }
                    }
                }
            }
            if (count > 1) Console.WriteLine("only suppoert 1 GUP");

            displayHandles.TryGetValue(handles[0], out displayHandle);
            this._handle = handles[0];
            this._displayHandle = displayHandle;
            this.Name = GetName(this._handle);
        }

        float _coreClock = 0;
        float _memoryClock = 0;
        float _shaderClock = 0;

        public void Update()
        {
            uint[] values = GetClocks();
            if (values != null)
            {
                _memoryClock = 0.001f * values[8];
                if (values[30] != 0)
                {
                    _coreClock = 0.0005f * values[30];
                    _shaderClock = 0.001f * values[30];
                }
                else {
                    _coreClock = 0.001f * values[0];
                    _shaderClock = 0.001f * values[14];
                }
            }
            /*
            Console.Write("core:" + _coreClock.ToString());
            Console.Write("mem:" + _memoryClock.ToString());
            Console.Write("shader:" + _shaderClock.ToString());
            Console.WriteLine();
            */


            NvMemoryInfo memoryInfo = new NvMemoryInfo();
            memoryInfo.Version = NVAPI.GPU_MEMORY_INFO_VER;
            memoryInfo.Values = new uint[NVAPI.MAX_MEMORY_VALUES_PER_GPU];
            if (NVAPI.NvAPI_GPU_GetMemoryInfo != null && _displayHandle.HasValue &&
              NVAPI.NvAPI_GPU_GetMemoryInfo(_displayHandle.Value, ref memoryInfo) ==
              NvStatus.OK)
            {
                TotalMem = memoryInfo.Values[0];
                TotalMemMb = TotalMem / 1024;
                FreeMem  = memoryInfo.Values[4];
                FreeMemMb = FreeMem / 1024;
                UsedMem  = Math.Max(TotalMem - FreeMem, 0);
                UsedMemMb = UsedMem / 1024;
                LoadMemPct = 100.0f * UsedMem / TotalMem;
            }


            NvPStates states = new NvPStates();
            states.Version = NVAPI.GPU_PSTATES_VER;
            states.PStates = new NvPState[NVAPI.MAX_PSTATES_PER_GPU];
            if (NVAPI.NvAPI_GPU_GetPStates != null && NVAPI.NvAPI_GPU_GetPStates(_handle, ref states) == NvStatus.OK)
            {
                /*
                0: Core
                1: MemoryController
                2: VideoEngine
                */
                for (int i = 0; i < 3; i++)
                    if (states.PStates[i].Present)
                    {
                        LoadPct = states.PStates[i].Percentage;
                        break;
                        //Console.WriteLine(states.PStates[i].Percentage);
                    }
            }

            NvGPUThermalSettings thermalSettings = GetThermalSettings();
            for (int i = 0; i < thermalSettings.Count; i++)
            {
                NvSensor sensor = thermalSettings.Sensor[i];
                if(sensor.Target == NvThermalTarget.GPU)
                {
                    Temperature = sensor.CurrentTemp;
                    //Console.WriteLine(temp);
                    break;

                }
            }
        }

        private static string GetName(NvPhysicalGpuHandle handle)
        {
            string gpuName;
            if (NVAPI.NvAPI_GPU_GetFullName(handle, out gpuName) == NvStatus.OK)
            {
                return gpuName.Trim();
            }
            else {
                return "NVIDIA";
            }
        }

        private uint[] GetClocks()
        {
            NvClocks allClocks = new NvClocks();
            allClocks.Version = NVAPI.GPU_CLOCKS_VER;
            allClocks.Clock = new uint[NVAPI.MAX_CLOCKS_PER_GPU];
            if (NVAPI.NvAPI_GPU_GetAllClocks != null &&
              NVAPI.NvAPI_GPU_GetAllClocks(_handle, ref allClocks) == NvStatus.OK)
            {
                return allClocks.Clock;
            }
            return null;
        }

        private NvGPUThermalSettings GetThermalSettings()
        {
            NvGPUThermalSettings settings = new NvGPUThermalSettings();
            settings.Version = NVAPI.GPU_THERMAL_SETTINGS_VER;
            settings.Count = NVAPI.MAX_THERMAL_SENSORS_PER_GPU;
            settings.Sensor = new NvSensor[NVAPI.MAX_THERMAL_SENSORS_PER_GPU];
            if (!(NVAPI.NvAPI_GPU_GetThermalSettings != null &&
              NVAPI.NvAPI_GPU_GetThermalSettings(_handle, (int)NvThermalTarget.ALL,
                ref settings) == NvStatus.OK))
            {
                settings.Count = 0;
            }
            return settings;
        }
    }
}
