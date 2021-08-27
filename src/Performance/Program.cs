using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SmartFormat.Performance
{
    public class Program
    {
        public static void Main()
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            //BenchmarkRunner.Run<SourcePerformanceTests>();
            BenchmarkRunner.Run<FormatTests>();
            //BenchmarkRunner.Run<ParserTests>();

            //BenchmarkRunner.Run<SimpleSpanParserTests>();
            //BenchmarkRunner.Run<NullFormatterChooseFormatterTests>();
            //BenchmarkRunner.Run<ReflectionVsStringSourceTests>();
        }
    }
}
