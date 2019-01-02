using System;
using System.Diagnostics;
using System.Text;

namespace Monitor.Hardware
{
    class CPU
    {
        private PerformanceCounter _cpu;
        private PerformanceCounter[] _cores;
        public int CoreCnt { get; private set; }
        public float LoadTotal { get; private set; }
        public float[] LoadCores { get; private set; }
        public string Name { get; private set; }

        public CPU()
        {
            CoreCnt = Environment.ProcessorCount;
            _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cores = new PerformanceCounter[CoreCnt];
            LoadTotal = 0;
            LoadCores = new float[CoreCnt];

            for (var i=0; i < CoreCnt; i++)
            {
                _cores[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }

            Microsoft.Win32.RegistryKey registrykeyHKLM = Microsoft.Win32.Registry.LocalMachine;
            string keyPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            Microsoft.Win32.RegistryKey registrykeyCPU = registrykeyHKLM.OpenSubKey(keyPath, false);
            string MHz = registrykeyCPU.GetValue("~MHz").ToString();
            string processorName = (string)registrykeyCPU.GetValue("ProcessorNameString");
            this.Name = BuildCpuName(processorName);
            registrykeyCPU.Close();
            registrykeyHKLM.Close();
        }

        private string BuildCpuName(string processorName)
        {
            StringBuilder txBuild = new StringBuilder(processorName);
            txBuild.Replace("Intel", "");
            txBuild.Replace("(R)", "");
            txBuild.Replace("(TM)", "");
            return txBuild.ToString();
        }


        public void Update()
        {
            LoadTotal = _cpu.NextValue();
            for(int i=0; i < _cores.Length; i++)
            {
                LoadCores[i] = _cores[i].NextValue();
            }
        }
    }
}
