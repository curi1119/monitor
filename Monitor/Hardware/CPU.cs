using System;
using System.Diagnostics;
using System.Text;

namespace Monitor.Hardware
{

    class CPU
    {
        private PerformanceCounter _cpu;
        private PerformanceCounter[] _cores;
        public int coreCnt;
        public float loadTotal;
        public float[] loadCores;
        private string _name;

        public string Name { get { return _name; } }

        public CPU()
        {
            coreCnt = Environment.ProcessorCount;
            _cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _cores = new PerformanceCounter[coreCnt];
            loadTotal = 0;
            loadCores = new float[coreCnt];

            for (var i=0; i < coreCnt; i++)
            {
                _cores[i] = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
            }

            Microsoft.Win32.RegistryKey registrykeyHKLM = Microsoft.Win32.Registry.LocalMachine;
            string keyPath = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            Microsoft.Win32.RegistryKey registrykeyCPU = registrykeyHKLM.OpenSubKey(keyPath, false);
            string MHz = registrykeyCPU.GetValue("~MHz").ToString();
            string processorName = (string)registrykeyCPU.GetValue("ProcessorNameString");
            _name = buildCpuName(processorName);
            registrykeyCPU.Close();
            registrykeyHKLM.Close();
        }

        private string buildCpuName(string processorName)
        {
            StringBuilder txBuild = new StringBuilder(processorName);
            txBuild.Replace("Intel", "");
            txBuild.Replace("(R)", "");
            txBuild.Replace("(TM)", "");
            return txBuild.ToString();
        }


        public void update()
        {
            loadTotal = _cpu.NextValue();
            for(int i=0; i < _cores.Length; i++)
            {
                loadCores[i] = _cores[i].NextValue();
            }
        }
    }
}
