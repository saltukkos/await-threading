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
        // , 100
        // , 1000
        , 10000
        // , 20000
        // , 40000
        // , 80000
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


| Method              | ListLength | ThreadsCount | Mean          | Error      | StdDev     | Gen0   | Allocated |
|-------------------- |----------- |------------- |--------------:|-----------:|-----------:|-------:|----------:|
| ParallelForEach     | 10         | 1            |      1.428 us |  0.0267 us |  0.0223 us | 0.8907 |   1.82 KB |
| ParallelForEach     | 10         | 2            |      2.474 us |  0.0474 us |  0.0751 us | 0.9880 |   1.97 KB |
| ParallelForEach     | 10         | 4            |      3.985 us |  0.0443 us |  0.0414 us | 1.1292 |   2.27 KB |
| ParallelForEach     | 10         | 8            |      4.441 us |  0.0571 us |  0.0534 us | 1.2817 |   2.58 KB |
| ParallelForEach     | 10000      | 1            |    874.534 us |  0.2852 us |  0.2667 us |      - |   1.83 KB |
| ParallelForEach     | 10000      | 2            |    448.758 us |  0.2845 us |  0.2662 us | 0.4883 |   1.97 KB |
| ParallelForEach     | 10000      | 4            |    268.018 us |  2.0178 us |  1.7887 us | 0.9766 |   2.38 KB |
| ParallelForEach     | 10000      | 8            |    213.479 us |  3.7448 us |  3.5029 us | 1.4648 |   3.16 KB |
| ParallelForEach     | 160000     | 1            | 14,010.347 us |  4.7996 us |  4.2547 us |      - |   1.84 KB |
| ParallelForEach     | 160000     | 2            |  7,091.299 us |  3.7071 us |  3.4676 us |      - |   1.98 KB |
| ParallelForEach     | 160000     | 4            |  4,190.386 us | 35.1721 us | 32.9000 us |      - |   2.38 KB |
| ParallelForEach     | 160000     | 8            |  3,395.585 us | 39.0934 us | 36.5680 us |      - |   3.19 KB |
|                     |            |              |               |            |            |        |           |
| AsParallel          | 10         | 1            |      1.657 us |  0.0044 us |  0.0037 us | 1.0738 |    2.2 KB |
| AsParallel          | 10         | 2            |      3.066 us |  0.0604 us |  0.1105 us | 1.2360 |    2.5 KB |
| AsParallel          | 10         | 4            |      5.902 us |  0.0624 us |  0.0584 us | 1.5411 |   3.11 KB |
| AsParallel          | 10         | 8            |      7.994 us |  0.1596 us |  0.2878 us | 2.1515 |   4.33 KB |
| AsParallel          | 10000      | 1            |    867.189 us |  1.2422 us |  1.1619 us | 0.9766 |    2.2 KB |
| AsParallel          | 10000      | 2            |    448.982 us |  1.1591 us |  0.9679 us | 0.9766 |   2.51 KB |
| AsParallel          | 10000      | 4            |    298.986 us |  1.7944 us |  1.5907 us | 1.4648 |   3.15 KB |
| AsParallel          | 10000      | 8            |    325.605 us |  4.6200 us |  4.0955 us | 1.9531 |   4.38 KB |
| AsParallel          | 160000     | 1            | 13,847.851 us |  2.9534 us |  2.6181 us |      - |   2.21 KB |
| AsParallel          | 160000     | 2            |  7,085.316 us |  9.1999 us |  8.6056 us |      - |   2.53 KB |
| AsParallel          | 160000     | 4            |  4,282.067 us | 27.6395 us | 25.8540 us |      - |   3.15 KB |
| AsParallel          | 160000     | 8            |  3,722.779 us | 29.9159 us | 24.9811 us |      - |   4.39 KB |
|                     |            |              |               |            |            |        |           |
| AsParallelAsync     | 10         | 1            |      3.026 us |  0.0342 us |  0.0303 us | 0.5836 |   1.19 KB |
| AsParallelAsync     | 10         | 2            |      6.515 us |  0.1168 us |  0.1818 us | 0.7477 |    1.5 KB |
| AsParallelAsync     | 10         | 4            |      8.142 us |  0.1610 us |  0.2258 us | 1.1444 |    2.3 KB |
| AsParallelAsync     | 10         | 8            |     11.795 us |  0.2048 us |  0.1816 us | 1.9379 |    3.9 KB |
| AsParallelAsync     | 10000      | 1            |    866.766 us |  0.3749 us |  0.2927 us |      - |   1.25 KB |
| AsParallelAsync     | 10000      | 2            |    449.085 us |  1.1534 us |  0.9005 us | 0.4883 |   1.56 KB |
| AsParallelAsync     | 10000      | 4            |    544.903 us | 10.8602 us | 11.6203 us | 0.9766 |   2.29 KB |
| AsParallelAsync     | 10000      | 8            |    220.621 us |  2.8922 us |  2.4151 us | 1.7090 |   3.71 KB |
| AsParallelAsync     | 160000     | 1            | 13,701.459 us |  5.8360 us |  5.4590 us |      - |   1.27 KB |
| AsParallelAsync     | 160000     | 2            |  6,958.837 us |  4.3015 us |  3.8132 us |      - |   1.57 KB |
| AsParallelAsync     | 160000     | 4            |  4,474.482 us | 43.3604 us | 38.4379 us |      - |   2.31 KB |
| AsParallelAsync     | 160000     | 8            |  3,325.081 us | 19.0779 us | 16.9120 us |      - |   3.76 KB |
|                     |            |              |               |            |            |        |           |
| AsParallelLazyAsync | 10         | 1            |      3.356 us |  0.0483 us |  0.0452 us | 0.6676 |   1.35 KB |
| AsParallelLazyAsync | 10         | 2            |      7.010 us |  0.1393 us |  0.3597 us | 0.9537 |   1.91 KB |
| AsParallelLazyAsync | 10         | 4            |      8.501 us |  0.1364 us |  0.1276 us | 1.6022 |   3.21 KB |
| AsParallelLazyAsync | 10         | 8            |     12.848 us |  0.2559 us |  0.3671 us | 2.8992 |   5.81 KB |
| AsParallelLazyAsync | 10000      | 1            |  1,036.081 us |  0.9934 us |  0.9292 us |      - |   1.42 KB |
| AsParallelLazyAsync | 10000      | 2            |    537.028 us |  1.1429 us |  1.0690 us | 0.9766 |   1.98 KB |
| AsParallelLazyAsync | 10000      | 4            |    605.514 us | 11.5874 us | 13.3441 us | 0.9766 |   3.21 KB |
| AsParallelLazyAsync | 10000      | 8            |    255.192 us |  4.9633 us |  7.7272 us | 2.4414 |   5.63 KB |
| AsParallelLazyAsync | 160000     | 1            | 16,391.756 us | 22.8568 us | 21.3803 us |      - |   1.45 KB |
| AsParallelLazyAsync | 160000     | 2            |  8,374.102 us | 11.9937 us | 11.2189 us |      - |      2 KB |
| AsParallelLazyAsync | 160000     | 4            |  5,374.254 us | 34.9108 us | 32.6556 us |      - |   3.22 KB |
| AsParallelLazyAsync | 160000     | 8            |  3,932.218 us | 29.3498 us | 27.4538 us |      - |   5.72 KB |

*/
