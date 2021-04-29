using System;
using BenchmarkDotNet.Running;

namespace SmartFormat.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SourcePerformanceTests>();
        }
    }
}
