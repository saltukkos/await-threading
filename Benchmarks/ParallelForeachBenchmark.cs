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
            Sum += Math.Sin(Math.Sqrt(value) * 2.1) / 3;
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
    public double AsParallelLazyAsync()
    {
        return DoAsync().AsTask().Result;
    
        async ParallelTask<double> DoAsync()
        {
            var calculator = new Calculator();
            await foreach (var i in _data.AsParallel(Environment.ProcessorCount))
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


| Method              | ListLength | Mean         | Error       | StdDev      | Median       | Gen0   | Allocated |
|-------------------- |----------- |-------------:|------------:|------------:|-------------:|-------:|----------:|
| ParallelForEach     | 10         |     3.430 us |   0.0170 us |   0.0159 us |     3.431 us | 1.2436 |   2.49 KB |
| AsParallel          | 10         |     7.440 us |   0.1018 us |   0.0952 us |     7.435 us | 2.0447 |    4.1 KB |
| AsParallelAsync     | 10         |     8.512 us |   0.1629 us |   0.1673 us |     8.487 us | 1.5564 |   3.13 KB |
| AsParallelLazyAsync | 10         |     9.424 us |   0.1839 us |   0.3172 us |     9.388 us | 2.2888 |   4.61 KB |
| ParallelForEach     | 100        |     5.324 us |   0.0509 us |   0.0452 us |     5.320 us | 1.2970 |    2.6 KB |
| AsParallel          | 100        |     9.101 us |   0.1762 us |   0.2228 us |     8.994 us | 2.0447 |    4.1 KB |
| AsParallelAsync     | 100        |     9.072 us |   0.1403 us |   0.1313 us |     9.044 us | 1.5564 |   3.13 KB |
| AsParallelLazyAsync | 100        |    12.518 us |   0.2422 us |   0.3474 us |    12.511 us | 2.2888 |   4.61 KB |
| ParallelForEach     | 1000       |    49.247 us |   0.9421 us |   1.0849 us |    49.605 us | 1.5869 |   3.18 KB |
| AsParallel          | 1000       |    20.991 us |   0.1833 us |   0.1625 us |    20.933 us | 2.0447 |    4.1 KB |
| AsParallelAsync     | 1000       |    15.151 us |   0.2364 us |   0.2096 us |    15.122 us | 1.5564 |   3.13 KB |
| AsParallelLazyAsync | 1000       |    35.636 us |   0.4905 us |   0.4588 us |    35.682 us | 2.2583 |   4.61 KB |
| ParallelForEach     | 10000      |   508.068 us |   5.5524 us |   5.1938 us |   507.718 us | 0.9766 |   3.18 KB |
| AsParallel          | 10000      |   124.042 us |   2.9401 us |   8.6689 us |   126.222 us | 1.9531 |   4.13 KB |
| AsParallelAsync     | 10000      |    73.515 us |   1.3758 us |   1.4128 us |    73.778 us | 1.4648 |   3.14 KB |
| AsParallelLazyAsync | 10000      |   313.950 us |   6.2699 us |  12.2289 us |   311.996 us | 1.9531 |   4.67 KB |
| ParallelForEach     | 20000      | 1,028.099 us |  11.8956 us |  11.1271 us | 1,029.392 us |      - |   3.19 KB |
| AsParallel          | 20000      |   224.534 us |   4.4800 us |   8.7380 us |   221.019 us | 1.9531 |   4.15 KB |
| AsParallelAsync     | 20000      |   121.262 us |   5.3789 us |  15.6052 us |   119.743 us | 1.4648 |   3.15 KB |
| AsParallelLazyAsync | 20000      |   638.560 us |  13.6819 us |  40.3413 us |   643.917 us | 1.9531 |   4.67 KB |
| ParallelForEach     | 40000      | 2,034.660 us |  19.5872 us |  18.3219 us | 2,029.911 us |      - |   3.18 KB |
| AsParallel          | 40000      |   368.684 us |   6.4755 us |   5.4073 us |   367.683 us | 1.9531 |   4.16 KB |
| AsParallelAsync     | 40000      |   251.969 us |  11.7242 us |  34.5691 us |   249.227 us | 1.4648 |   3.18 KB |
| AsParallelLazyAsync | 40000      |   966.866 us |   9.8519 us |   8.7335 us |   968.162 us | 1.9531 |   4.68 KB |
| ParallelForEach     | 80000      | 4,002.759 us |  65.2485 us |  61.0335 us | 4,006.167 us |      - |   3.19 KB |
| AsParallel          | 80000      |   689.597 us |   6.3506 us |   5.9403 us |   689.873 us | 1.9531 |   4.15 KB |
| AsParallelAsync     | 80000      |   500.738 us |  11.5755 us |  34.1306 us |   506.545 us | 0.9766 |    3.2 KB |
| AsParallelLazyAsync | 80000      | 1,434.128 us |   9.6188 us |   8.9975 us | 1,434.408 us | 1.9531 |   4.67 KB |
| ParallelForEach     | 160000     | 8,102.670 us | 152.2597 us | 142.4238 us | 8,036.470 us |      - |   3.21 KB |
| AsParallel          | 160000     | 1,345.013 us |  11.9939 us |  11.2191 us | 1,344.636 us | 1.9531 |   4.16 KB |
| AsParallelAsync     | 160000     |   773.463 us |  14.9658 us |  24.5892 us |   777.672 us | 0.9766 |    3.2 KB |
| AsParallelLazyAsync | 160000     | 2,812.498 us |  21.4792 us |  20.0916 us | 2,810.348 us |      - |   4.68 KB |
   
*/
