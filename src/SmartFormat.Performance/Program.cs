﻿using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace SmartFormat.Performance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, new DebugInProcessConfig());
            //BenchmarkRunner.Run<SourcePerformanceTests>();
            BenchmarkRunner.Run<SimpleSpanParserTests>();
        }
    }
}
