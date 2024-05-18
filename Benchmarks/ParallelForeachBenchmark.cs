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


| Method               | ListLength | Mean         | Error       | StdDev     | Gen0   | Allocated |
|--------------------- |----------- |-------------:|------------:|-----------:|-------:|----------:|
| ParallelForEach      | 10         |     2.975 us |   0.0262 us |  0.0232 us | 1.2283 |   2.45 KB |
| AsParallel           | 10         |     7.409 us |   0.1142 us |  0.1068 us | 2.0523 |    4.1 KB |
| AsParallelAsync      | 10         |     8.318 us |   0.0985 us |  0.0922 us | 1.5564 |   3.13 KB |
| AsParallelAsync_Half | 10         |     6.167 us |   0.1126 us |  0.0998 us | 0.9384 |   1.88 KB |
| ParallelForEach      | 100        |     3.860 us |   0.0547 us |  0.0512 us | 1.2360 |   2.47 KB |
| AsParallel           | 100        |     8.076 us |   0.1394 us |  0.1549 us | 2.0523 |    4.1 KB |
| AsParallelAsync      | 100        |     8.874 us |   0.1742 us |  0.3519 us | 1.5564 |   3.13 KB |
| AsParallelAsync_Half | 100        |     6.454 us |   0.1243 us |  0.1659 us | 0.9308 |   1.88 KB |
| ParallelForEach      | 1000       |    37.351 us |   0.2001 us |  0.1774 us | 1.5869 |   3.18 KB |
| AsParallel           | 1000       |    16.157 us |   0.3215 us |  0.5879 us | 2.0447 |    4.1 KB |
| AsParallelAsync      | 1000       |    10.957 us |   0.1186 us |  0.1109 us | 1.5564 |   3.13 KB |
| AsParallelAsync_Half | 1000       |     9.493 us |   0.1860 us |  0.2840 us | 0.9308 |   1.88 KB |
| ParallelForEach      | 10000      |   444.658 us |   3.1277 us |  2.7726 us | 1.4648 |   3.19 KB |
| AsParallel           | 10000      |    75.836 us |   1.5164 us |  4.1255 us | 1.9531 |    4.1 KB |
| AsParallelAsync      | 10000      |    26.647 us |   0.1432 us |  0.1269 us | 1.5564 |   3.13 KB |
| AsParallelAsync_Half | 10000      |    32.930 us |   0.0971 us |  0.0908 us | 0.9155 |   1.88 KB |
| ParallelForEach      | 20000      |   893.693 us |   6.7343 us |  5.6235 us | 0.9766 |   3.19 KB |
| AsParallel           | 20000      |   144.428 us |   2.8721 us |  7.5159 us | 1.9531 |   4.13 KB |
| AsParallelAsync      | 20000      |    44.033 us |   0.2531 us |  0.2113 us | 1.5259 |   3.13 KB |
| AsParallelAsync_Half | 20000      |    58.677 us |   1.1536 us |  1.3733 us | 0.9155 |   1.89 KB |
| ParallelForEach      | 40000      | 1,810.709 us |  30.8795 us | 28.8847 us |      - |   3.19 KB |
| AsParallel           | 40000      |   249.528 us |   4.4997 us |  3.5131 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 40000      |    73.636 us |   1.4477 us |  1.7234 us | 1.4648 |   3.15 KB |
| AsParallelAsync_Half | 40000      |    89.694 us |   1.7799 us |  3.1174 us | 0.8545 |   1.91 KB |
| ParallelForEach      | 80000      | 3,585.120 us |  32.3490 us | 25.2560 us |      - |   3.19 KB |
| AsParallel           | 80000      |   449.590 us |   3.0698 us |  2.7213 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 80000      |   148.339 us |   4.2851 us | 12.5673 us | 1.4648 |   3.18 KB |
| AsParallelAsync_Half | 80000      |   174.657 us |   2.0587 us |  1.8250 us | 0.7324 |   1.93 KB |
| ParallelForEach      | 160000     | 7,326.779 us | 101.4336 us | 94.8810 us |      - |    3.2 KB |
| AsParallel           | 160000     |   858.643 us |   8.1720 us |  7.6441 us | 1.9531 |   4.15 KB |
| AsParallelAsync      | 160000     |   293.419 us |   3.9131 us |  3.6603 us | 1.4648 |    3.2 KB |
| AsParallelAsync_Half | 160000     |   427.159 us |   8.5345 us | 24.7602 us | 0.4883 |   1.94 KB |

*/
