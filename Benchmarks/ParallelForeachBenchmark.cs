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
// [DisassemblyDiagnoser(printSource: true)]
public class ParallelForeachBenchmark
{
    private int[] _data = null!;

    [Params(
        // 10
        // , 100
        // , 1000
        // , 10000
        // , 20000
        // , 40000
        // , 80000
        //,
        160000
        )] 
    public int ListLength;

//    [Params(1, 2, 4, 8)]
    public int ThreadsCount = 2;

    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, ListLength).ToArray();
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
            await foreach (var i in _data.AsAsyncParallel(ThreadsCount))
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
| AsParallelAsync     | 10         | 1            |      3.058 us |  0.0328 us |  0.0307 us | 0.5836 |   1.19 KB |
| AsParallelAsync     | 10         | 2            |      6.804 us |  0.1261 us |  0.2460 us | 0.7477 |    1.5 KB |
| AsParallelAsync     | 10         | 4            |      8.003 us |  0.1272 us |  0.1514 us | 1.1444 |    2.3 KB |
| AsParallelAsync     | 10         | 8            |     11.815 us |  0.1530 us |  0.1431 us | 1.9531 |   3.91 KB |
| AsParallelAsync     | 10000      | 1            |    870.795 us |  3.5375 us |  2.9539 us |      - |   1.25 KB |
| AsParallelAsync     | 10000      | 2            |    453.632 us |  6.2659 us |  8.5769 us |      - |   1.56 KB |
| AsParallelAsync     | 10000      | 4            |    558.011 us | 21.1598 us | 62.3901 us | 0.9766 |    2.3 KB |
| AsParallelAsync     | 10000      | 8            |    188.933 us |  2.7895 us |  2.6093 us | 1.7090 |   3.72 KB |
| AsParallelAsync     | 160000     | 1            | 13,805.764 us | 96.9952 us | 85.9837 us |      - |   1.27 KB |
| AsParallelAsync     | 160000     | 2            |  6,960.659 us | 21.6161 us | 19.1621 us |      - |   1.57 KB |
| AsParallelAsync     | 160000     | 4            |  4,010.717 us | 20.6152 us | 19.2835 us |      - |   2.31 KB |
| AsParallelAsync     | 160000     | 8            |  2,970.931 us | 43.2653 us | 40.4704 us |      - |   3.76 KB |
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



Threads=2
    Original:
   | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
   |-------------------- |----------- |---------:|----------:|----------:|----------:|
   | AsParallelLazyAsync | 160000     | 8.333 ms | 0.0086 ms | 0.0076 ms |      2 KB |
   
   | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
   |-------------------- |----------- |---------:|----------:|----------:|----------:|
   | AsParallelAsync     | 160000     | 7.026 ms | 0.0279 ms | 0.0261 ms |   1.57 KB |
   |                     |            |          |           |           |           |
   | AsParallelLazyAsync | 160000     | 8.372 ms | 0.0065 ms | 0.0055 ms |      2 KB |
   
   With IReadOnlyList:
    | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
    |-------------------- |----------- |---------:|----------:|----------:|----------:|
    | AsParallelAsync     | 160000     | 6.860 ms | 0.0044 ms | 0.0041 ms |   1.57 KB |
    |                     |            |          |           |           |           |
    | AsParallelLazyAsync | 160000     | 8.340 ms | 0.0086 ms | 0.0080 ms |      2 KB |
   | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
   |-------------------- |----------- |---------:|----------:|----------:|----------:|
   | AsParallelAsync     | 160000     | 6.863 ms | 0.0035 ms | 0.0031 ms |   1.57 KB |
   |                     |            |          |           |           |           |
   | AsParallelLazyAsync | 160000     | 8.361 ms | 0.0065 ms | 0.0060 ms |      2 KB |
   
   array as IROList:
   | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
   |-------------------- |----------- |---------:|----------:|----------:|----------:|
   | AsParallelAsync     | 160000     | 6.827 ms | 0.0058 ms | 0.0051 ms |   1.57 KB |
   |                     |            |          |           |           |           |
   | AsParallelLazyAsync | 160000     | 8.383 ms | 0.0076 ms | 0.0067 ms |      2 KB |
   
   array as array:
   | Method              | ListLength | Mean     | Error     | StdDev    | Allocated |
   |-------------------- |----------- |---------:|----------:|----------:|----------:|
   | AsParallelAsync     | 160000     | 6.869 ms | 0.0054 ms | 0.0050 ms |   1.57 KB |
   |                     |            |          |           |           |           |
   | AsParallelLazyAsync | 160000     | 8.397 ms | 0.0078 ms | 0.0073 ms |      2 KB |
   
*/
