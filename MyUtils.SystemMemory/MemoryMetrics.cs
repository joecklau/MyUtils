using System;

namespace MyUtils.SystemMemory
{
    public class MemoryMetrics
    {
        public double Total;
        public double Used;
        public double Free;
        public double Utilization => Total == 0 ? 0 : Used / Total;

        //public override string ToString() => $"Total: {this.Total} / Used: {this.Used} / Free: {this.Free}";
        public override string ToString() => $"Mem Usage: {this.Utilization:0%} ({this.Used} of {this.Total} used)";
    }
}
