// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Enumerable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Benchmarks;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByMethod)]
public class ParallelForeachBenchmark
{
    private List<int> _data = null!;

    [Params(
        10
        , 10000
        , 160000
        )] 
    public int ListLength;

    [Params(1, 2, 4, 8)]
    public int ThreadsCount;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, ListLength).ToList();
    }

    class Calculator
    {
        public bool Bool;

        public void Calculate(double value)
        {
            for (int i = 0; i < 100; i++)
            {
                var calculated = (((value / 3 + 1.1) * 4.5 + value + i) * 1.3 + 2 * value) / 1.1;
                if (Math.Abs(calculated) < 0.00001)
                {
                    Bool = !Bool;
                }
            }
        }
    }
    
    [Benchmark]
    public bool ParallelForEach()
    {
        var calculator = new Calculator();
        Parallel.ForEach(
            _data,
            new ParallelOptions { MaxDegreeOfParallelism = ThreadsCount },
            i =>
            {
                calculator.Calculate(i);
            });
        return calculator.Bool;
    }
    
    [Benchmark]
    public bool AsParallel()
    {
        var calculator = new Calculator();
        _data.AsParallel()
            .WithDegreeOfParallelism(ThreadsCount)
            .ForAll(
                i =>
                {
                    calculator.Calculate(i);
                });
        return calculator.Bool;
    }
    
    [Benchmark]
    public bool AsParallelAsync()
    {
        return DoAsync().AsTask().Result;
    
            async ParallelTask<bool> DoAsync()
            {
                var calculator = new Calculator();
                await foreach (var i in await _data.AsParallelAsync(ThreadsCount))
                {
                    calculator.Calculate(i);
                }
        
                return calculator.Bool;
            }
    }

    [Benchmark]
    public bool AsParallelLazyAsync()
    {
        return DoAsync().AsTask().Result;
    
        async ParallelTask<bool> DoAsync()
        {
            var calculator = new Calculator();
            await foreach (var i in _data.AsParallel(ThreadsCount))
            {
                calculator.Calculate(i);
            }
    
            return calculator.Bool;
        }
    }
}

/*
BenchmarkDotNet v0.13.12, macOS Sonoma 14.4.1 (23E224) [Darwin 23.4.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD


| Method              | ListLength | ThreadsCount | Mean          | Error      | StdDev     | Median        | Gen0   | Allocated |
|-------------------- |----------- |------------- |--------------:|-----------:|-----------:|--------------:|-------:|----------:|
| ParallelForEach     | 10         | 1            |      1.405 us |  0.0091 us |  0.0076 us |      1.404 us | 0.8907 |   1.82 KB |
| ParallelForEach     | 10         | 2            |      2.550 us |  0.0505 us |  0.1321 us |      2.590 us | 0.9956 |   1.97 KB |
| ParallelForEach     | 10         | 4            |      4.228 us |  0.0326 us |  0.0305 us |      4.231 us | 1.1368 |   2.28 KB |
| ParallelForEach     | 10         | 8            |      4.795 us |  0.0433 us |  0.0405 us |      4.782 us | 1.2894 |   2.58 KB |
| ParallelForEach     | 10000      | 1            |    868.179 us |  0.5532 us |  0.4904 us |    868.121 us |      - |   1.83 KB |
| ParallelForEach     | 10000      | 2            |    446.932 us |  0.3022 us |  0.2523 us |    446.954 us | 0.4883 |   1.97 KB |
| ParallelForEach     | 10000      | 4            |    276.164 us |  2.7224 us |  2.2733 us |    275.305 us | 0.9766 |   2.38 KB |
| ParallelForEach     | 10000      | 8            |    208.904 us |  1.3268 us |  1.2411 us |    209.300 us | 1.4648 |   3.19 KB |
| ParallelForEach     | 160000     | 1            | 13,993.983 us |  5.9441 us |  4.9636 us | 13,992.963 us |      - |   2.03 KB |
| ParallelForEach     | 160000     | 2            |  7,089.421 us |  5.2472 us |  4.9083 us |  7,088.276 us |      - |   2.07 KB |
| ParallelForEach     | 160000     | 4            |  4,116.067 us | 43.1979 us | 40.4074 us |  4,128.510 us |      - |   2.38 KB |
| ParallelForEach     | 160000     | 8            |  3,210.419 us | 15.7713 us | 13.9809 us |  3,208.084 us |      - |   3.19 KB |
|                     |            |              |               |            |            |               |        |           |
| AsParallel          | 10         | 1            |      1.672 us |  0.0049 us |  0.0043 us |      1.671 us | 1.0738 |    2.2 KB |
| AsParallel          | 10         | 2            |      2.863 us |  0.0435 us |  0.0363 us |      2.856 us | 1.2360 |    2.5 KB |
| AsParallel          | 10         | 4            |      6.358 us |  0.1269 us |  0.3112 us |      6.416 us | 1.5488 |   3.11 KB |
| AsParallel          | 10         | 8            |      8.105 us |  0.0973 us |  0.0759 us |      8.086 us | 2.1515 |   4.33 KB |
| AsParallel          | 10000      | 1            |    863.604 us |  0.8316 us |  0.6493 us |    863.783 us | 0.9766 |   2.21 KB |
| AsParallel          | 10000      | 2            |    454.331 us |  3.4412 us |  3.2189 us |    453.345 us | 0.9766 |   2.51 KB |
| AsParallel          | 10000      | 4            |    304.155 us |  2.4793 us |  2.3192 us |    303.644 us | 1.4648 |   3.15 KB |
| AsParallel          | 10000      | 8            |    320.026 us |  4.2255 us |  3.9526 us |    320.249 us | 1.9531 |   4.39 KB |
| AsParallel          | 160000     | 1            | 13,871.729 us | 11.6029 us | 10.2857 us | 13,871.109 us |      - |   2.21 KB |
| AsParallel          | 160000     | 2            |  7,090.179 us |  7.4918 us |  7.0079 us |  7,090.687 us |      - |   2.53 KB |
| AsParallel          | 160000     | 4            |  4,247.555 us | 32.9770 us | 30.8467 us |  4,235.540 us |      - |   3.17 KB |
| AsParallel          | 160000     | 8            |  3,642.040 us | 25.1656 us | 22.3086 us |  3,644.570 us |      - |   4.39 KB |
|                     |            |              |               |            |            |               |        |           |
| AsParallelAsync     | 10         | 1            |      3.114 us |  0.0338 us |  0.0627 us |      3.102 us | 0.5569 |   1.13 KB |
| AsParallelAsync     | 10         | 2            |      6.519 us |  0.1265 us |  0.1600 us |      6.539 us | 0.7172 |   1.45 KB |
| AsParallelAsync     | 10         | 4            |      7.970 us |  0.0905 us |  0.0802 us |      7.967 us | 1.0986 |   2.22 KB |
| AsParallelAsync     | 10         | 8            |     12.037 us |  0.0859 us |  0.0762 us |     12.053 us | 1.8921 |    3.8 KB |
| AsParallelAsync     | 10000      | 1            |    867.155 us |  0.5176 us |  0.4841 us |    867.281 us |      - |    1.2 KB |
| AsParallelAsync     | 10000      | 2            |    448.173 us |  0.8921 us |  0.7449 us |    448.313 us | 0.4883 |   1.51 KB |
| AsParallelAsync     | 10000      | 4            |    577.670 us |  6.2012 us |  5.4972 us |    578.538 us | 0.9766 |   2.23 KB |
| AsParallelAsync     | 10000      | 8            |    218.148 us |  1.4988 us |  1.4020 us |    218.027 us | 1.7090 |   3.62 KB |
| AsParallelAsync     | 160000     | 1            | 13,610.943 us |  7.1190 us |  6.3108 us | 13,611.745 us |      - |   1.22 KB |
| AsParallelAsync     | 160000     | 2            |  6,910.883 us |  5.8370 us |  5.4600 us |  6,909.836 us |      - |   1.52 KB |
| AsParallelAsync     | 160000     | 4            |  4,432.964 us | 23.0373 us | 21.5491 us |  4,437.645 us |      - |   2.24 KB |
| AsParallelAsync     | 160000     | 8            |  3,189.780 us | 13.4831 us | 11.9524 us |  3,186.493 us |      - |   3.66 KB |
|                     |            |              |               |            |            |               |        |           |
| AsParallelLazyAsync | 10         | 1            |      3.338 us |  0.0331 us |  0.0276 us |      3.339 us | 0.6447 |   1.31 KB |
| AsParallelLazyAsync | 10         | 2            |      6.979 us |  0.2030 us |  0.5888 us |      7.136 us | 0.9232 |   1.86 KB |
| AsParallelLazyAsync | 10         | 4            |      8.965 us |  0.1776 us |  0.3421 us |      8.806 us | 1.5411 |   3.11 KB |
| AsParallelLazyAsync | 10         | 8            |     13.000 us |  0.1835 us |  0.1717 us |     12.992 us | 2.7924 |   5.61 KB |
| AsParallelLazyAsync | 10000      | 1            |  1,034.270 us |  0.8181 us |  0.7653 us |  1,034.220 us |      - |   1.38 KB |
| AsParallelLazyAsync | 10000      | 2            |    535.494 us |  2.6233 us |  2.1906 us |    534.982 us |      - |   1.92 KB |
| AsParallelLazyAsync | 10000      | 4            |    638.467 us |  7.1224 us |  6.6623 us |    639.517 us | 0.9766 |   3.11 KB |
| AsParallelLazyAsync | 10000      | 8            |    258.056 us |  1.7239 us |  1.5282 us |    257.667 us | 2.4414 |   5.45 KB |
| AsParallelLazyAsync | 160000     | 1            | 16,388.982 us | 11.1418 us |  9.8769 us | 16,387.444 us |      - |   1.41 KB |
| AsParallelLazyAsync | 160000     | 2            |  8,322.120 us |  7.9829 us |  7.4672 us |  8,320.984 us |      - |   1.94 KB |
| AsParallelLazyAsync | 160000     | 4            |  5,289.254 us | 19.5325 us | 17.3151 us |  5,293.264 us |      - |   3.12 KB |
| AsParallelLazyAsync | 160000     | 8            |  3,783.976 us | 23.1686 us | 21.6719 us |  3,784.483 us |      - |   5.48 KB |

*/
