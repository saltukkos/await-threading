// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Enumerable;
using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class ParallelForeachBenchmark
{
    private List<int> _data = null!;

    [Params(
        10
        , 100
        , 1000
        , 10000
        , 20000
        , 40000
        , 80000
        , 160000
        )] 
    public int ListLength;
    
    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, ListLength).ToList();
    }

    class Calculator
    {
        public double Sum;

        public void Calculate(double value)
        {
            Sum += Math.Sqrt(value) * 2;
        }
    }

    [Benchmark]
    public double ParallelForEach()
    {
        var calculator = new Calculator();
        Parallel.ForEach(
            _data,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            i =>
            {
                calculator.Calculate(i);
            });
        return calculator.Sum;
    }

    [Benchmark]
    public double AsParallel()
    {
        var calculator = new Calculator();
        _data.AsParallel().ForAll(
            i =>
            {
                calculator.Calculate(i);
            });
        return calculator.Sum;
    }
    
    [Benchmark]
    public double AsParallelAsync()
    {
        return DoAsync().AsTask().Result;
    
        async ParallelTask<double> DoAsync()
        {
            var calculator = new Calculator();
            await foreach (var i in await _data.AsParallelAsync(Environment.ProcessorCount))
            {
                calculator.Calculate(i);
            }
    
            return calculator.Sum;
        }
    }
    
    [Benchmark]
    public double AsParallelAsync_Half()
    {
        return DoAsync().AsTask().Result;
    
        async ParallelTask<double> DoAsync()
        {
            var calculator = new Calculator();
            await foreach (var i in await _data.AsParallelAsync(Math.Max(2, Environment.ProcessorCount / 2)))
            {
                calculator.Calculate(i);
            }
    
            return calculator.Sum;
        }
    }
}

/*
BenchmarkDotNet v0.13.12, macOS Sonoma 14.4.1 (23E224) [Darwin 23.4.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD


| Method               | ListLength | Mean         | Error       | StdDev      | Gen0   | Allocated |
|--------------------- |----------- |-------------:|------------:|------------:|-------:|----------:|
| ParallelForEach      | 10         |     2.950 us |   0.0403 us |   0.0377 us | 1.2245 |   2.44 KB |
| AsParallel           | 10         |     7.630 us |   0.1086 us |   0.1292 us | 2.0447 |    4.1 KB |
| AsParallelAsync      | 10         |     9.078 us |   0.1789 us |   0.4111 us | 2.4567 |   4.92 KB |
| AsParallelAsync_Half | 10         |     6.390 us |   0.1092 us |   0.1022 us | 1.4267 |   2.86 KB |
| ParallelForEach      | 100        |     3.773 us |   0.0384 us |   0.0340 us | 1.2283 |   2.47 KB |
| AsParallel           | 100        |     8.142 us |   0.1563 us |   0.2087 us | 2.0447 |    4.1 KB |
| AsParallelAsync      | 100        |     9.121 us |   0.1815 us |   0.2360 us | 2.4414 |   4.92 KB |
| AsParallelAsync_Half | 100        |     6.996 us |   0.1387 us |   0.2570 us | 1.4267 |   2.86 KB |
| ParallelForEach      | 1000       |    37.947 us |   0.3876 us |   0.3626 us | 1.5869 |   3.19 KB |
| AsParallel           | 1000       |    13.437 us |   0.0598 us |   0.0560 us | 2.0447 |    4.1 KB |
| AsParallelAsync      | 1000       |    11.846 us |   0.1941 us |   0.1816 us | 2.4414 |   4.92 KB |
| AsParallelAsync_Half | 1000       |    10.661 us |   0.2129 us |   0.6210 us | 1.4191 |   2.86 KB |
| ParallelForEach      | 10000      |   444.607 us |   4.2010 us |   3.7241 us | 1.4648 |   3.19 KB |
| AsParallel           | 10000      |    74.922 us |   1.4898 us |   3.6546 us | 1.9531 |    4.1 KB |
| AsParallelAsync      | 10000      |    27.398 us |   0.1729 us |   0.1533 us | 2.4414 |   4.92 KB |
| AsParallelAsync_Half | 10000      |    36.252 us |   0.7237 us |   1.5578 us | 1.3428 |   2.86 KB |
| ParallelForEach      | 20000      |   902.151 us |  11.4283 us |  10.6900 us | 0.9766 |   3.19 KB |
| AsParallel           | 20000      |   140.733 us |   2.8119 us |   6.8975 us | 1.9531 |   4.13 KB |
| AsParallelAsync      | 20000      |    44.644 us |   0.2000 us |   0.1871 us | 2.4414 |   4.92 KB |
| AsParallelAsync_Half | 20000      |    58.170 us |   1.1419 us |   1.5631 us | 1.4038 |   2.87 KB |
| ParallelForEach      | 40000      | 1,811.224 us |  24.7867 us |  23.1855 us |      - |   3.19 KB |
| AsParallel           | 40000      |   253.070 us |   5.0297 us |  10.6094 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 40000      |    78.508 us |   1.2894 us |   1.2664 us | 2.4414 |   4.92 KB |
| AsParallelAsync_Half | 40000      |    99.632 us |   1.8452 us |   4.9251 us | 1.3428 |   2.89 KB |
| ParallelForEach      | 80000      | 3,620.395 us |  71.5776 us |  66.9537 us |      - |   3.19 KB |
| AsParallel           | 80000      |   444.914 us |   3.2883 us |   2.9150 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 80000      |   139.536 us |   2.7340 us |   4.3364 us | 2.4414 |   4.95 KB |
| AsParallelAsync_Half | 80000      |   184.999 us |   3.5679 us |   3.3374 us | 1.2207 |   2.91 KB |
| ParallelForEach      | 160000     | 7,254.711 us | 139.7574 us | 149.5388 us |      - |    3.2 KB |
| AsParallel           | 160000     |   858.118 us |   8.2812 us |   7.7463 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 160000     |   295.875 us |   3.2879 us |   3.0755 us | 2.4414 |   4.98 KB |
| AsParallelAsync_Half | 160000     |   449.229 us |   8.8799 us |  22.2779 us | 0.9766 |   2.92 KB |
*/