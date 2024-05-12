// MIT License
// Copyright (c) 2024 Saltuk Konstantin
// See the LICENSE file in the project root for more information.

using AwaitThreading.Core;
using AwaitThreading.Enumerable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace Benchmarks;

[MemoryDiagnoser]
[Config(typeof(Config))]
public class ParallelForeachBenchmark
{
    private class Config : ManualConfig
    {
        // private const string MsBuildSemicolon = ";"; // for some reason, `;` does not work on macOS, even if escaped correctly. 
        
        public Config()
        {
            AddJob(Job.MediumRun);
            // AddJob(Job.MediumRun.WithId("Stack"));
            // AddJob(Job.MediumRun.WithId("Stack+SyncResult").WithArguments(new[] { new MsBuildArgument("/p:DefineConstants=\"FEATTURE_DEDICATED_SYNCRESULT\"") }));
            // AddJob(Job.MediumRun.WithId("Queue").WithArguments(new[] { new MsBuildArgument("/p:DefineConstants=\"FEATURE_TASKIMPL_QUEUE\"") }));
            // AddJob(Job.MediumRun.WithId("Queue+SyncResult").WithArguments(new[] { new MsBuildArgument($"/p:DefineConstants=\"FEATTURE_DEDICATED_SYNCRESULT{MsBuildSemicolon}FEATURE_TASKIMPL_QUEUE\"") }));
            // AddJob(Job.MediumRun.WithId("AsyncLocal+SyncResult(bug)").WithArguments(new[] { new MsBuildArgument($"/p:DefineConstants=\"FEATURE_TASKIMPL_ASYNCLOCAL{MsBuildSemicolon}FEATTURE_DEDICATED_SYNCRESULT\"") }));
            // AddJob(Job.MediumRun.WithId("NoResult").WithArguments(new[] { new MsBuildArgument("/p:DefineConstants=\"FEATURE_TASKIMPL_NORESULT\"") }));
        }
    }

    private List<int> _data = null!;

    [Params(
        10
        , 100
        , 1000
        , 10000
        // , 20000
        // , 40000
        // , 80000
        // , 160000
        )] 
    public int ListLength;
    
    [GlobalSetup]
    public void Setup()
    {
        _data = Enumerable.Range(0, ListLength).ToList();
    }

    class S
    {
        public double Sum;
    }
    
    // [IterationSetup]
    // public void SetUpEach()
    // {
    //     // var tasks = new List<Task>();
    //     // for (int i = 0; i < Environment.ProcessorCount; ++i)
    //     // {
    //     //     tasks.Add(Task.Run(() => { }));
    //     // }
    //     //
    //     // Task.WaitAll(tasks.ToArray());
    //
    // }

    // [Benchmark(Baseline = true)]
    // public double SingleThreadedForEach()
    // {
    //     return DoAsync().AsTask().Result;
    //
    //     async ParallelTask<double> DoAsync()
    //     {
    //
    //         S sum = new S();
    //         foreach (var i in _data)
    //         {
    //             sum.Sum += Math.Sqrt(i);
    //         }
    //
    //         return sum.Sum;
    //     }
    // }
    
    
    // [Benchmark]
    // public double ParallelForEach()
    // {
    //     return DoAsync().AsTask().Result;
    //
    //     async ParallelTask<double> DoAsync()
    //     {
    //
    //         S sum = new S();
    //         Parallel.ForEach(
    //             _data,
    //             new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
    //             i =>
    //             {
    //
    //                 var sqrt = Math.Sqrt(i);
    //                 sum.Sum += sqrt; //yep, it's not valid, but we are only benchmarking
    //             });
    //         return sum.Sum;
    //     }
    // }
    
    // [Benchmark]
    // public double AsParallel()
    // {
    //     return DoAsync().AsTask().Result;
    //
    //     async ParallelTask<double> DoAsync()
    //     {
    //
    //         S sum = new S();
    //         _data.AsParallel().ForAll(
    //             i =>
    //             {
    //                 var sqrt = Math.Sqrt(i);
    //                 sum.Sum += sqrt;
    //             });
    //         return sum.Sum;
    //     }
    // }

    [Benchmark]
    public double AsParallelAsync()
    {
        return DoAsync().AsTask().Result;

        async ParallelTask<double> DoAsync()
        {
            S sum = new S();
            await foreach (var i in await _data.AsParallelAsync(Environment.ProcessorCount))
            {

                var sqrt = Math.Sqrt(i);
                sum.Sum += sqrt;
            }

            return sum.Sum;
        }
    }

    [Benchmark]
    public double AsParallelAsync_2()
    {
        return DoAsync().AsTask().Result;

        async ParallelTask<double> DoAsync()
        {
            S sum = new S();
            await foreach (var i in await _data.AsParallelAsync(2))
            {

                var sqrt = Math.Sqrt(i);
                sum.Sum += sqrt;
            }

            return sum.Sum;
        }
    }
    //
    // [Benchmark]
    // public double AsParallelAsync_1()
    // {
    //     return DoAsync().AsTask().Result;
    //
    //     async ParallelTask<double> DoAsync()
    //     {
    //         S sum = new S();
    //         await foreach (var i in await _data.AsParallelAsync(1))
    //         {
    //
    //             var sqrt = Math.Sqrt(i);
    //             sum.Sum += sqrt;
    //         }
    //
    //         return sum.Sum;
    //     }
    // }

//     [Benchmark]
//     public double AsParallelAsyncTwice()
//     {
// //        _ = DoAsync().AsTask().Result;
//         return DoAsync().AsTask().Result;
//
//         async ParallelTask<double> DoAsync()
//         {
//             double sum = 0;
//             await foreach (var i in await _data.AsParallelAsync(Environment.ProcessorCount))
//             {
//
//                 var sqrt = Math.Sqrt(i);
//                 sum += sqrt;
//             }
//
//             Thread.Sleep(1);
//             return sum;
//         }
//     }
}

/*

BenchmarkDotNet v0.13.12, macOS Sonoma 14.4.1 (23E224) [Darwin 23.4.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 6.0.100
  [Host]    : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD
  MediumRun : .NET 6.0.0 (6.0.21.52210), Arm64 RyuJIT AdvSIMD

No waits at all:
| Method                | ListLength | Mean        | Error      | StdDev       | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |------------:|-----------:|-------------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |    63.70 ns |   0.339 ns |     0.265 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         | 3,160.72 ns |  51.722 ns |    48.381 ns |  49.58 |    0.83 | 1.2894 |    2616 B |       20.44 |
| AsParallel            | 10         | 8,394.55 ns | 163.594 ns |   229.336 ns | 130.63 |    3.84 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 8,258.10 ns | 400.643 ns | 1,181.306 ns | 128.78 |   18.83 | 2.5177 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 4,925.34 ns |  97.932 ns |   191.009 ns |  78.99 |    3.16 | 0.9079 |    1864 B |       14.56 |

Spinwait only in SetResult:
| Method                | ListLength | Mean        | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |    63.96 ns |   0.632 ns |   0.528 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         | 3,214.88 ns |  49.880 ns |  46.658 ns |  50.22 |    0.96 | 1.2856 |    2614 B |       20.42 |
| AsParallel            | 10         | 8,242.71 ns | 163.883 ns | 414.153 ns | 127.35 |   10.96 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 9,949.84 ns | 194.833 ns | 341.234 ns | 154.84 |    5.64 | 2.5177 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 4,878.77 ns |  96.590 ns | 216.038 ns |  78.93 |    4.33 | 0.9079 |    1864 B |       14.56 |

Spinwait in SetResult + GetResult
| Method                | ListLength | Mean        | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |    64.87 ns |   0.200 ns |   0.187 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         | 3,183.38 ns |  46.013 ns |  43.040 ns |  49.07 |    0.67 | 1.2817 |    2609 B |       20.38 |
| AsParallel            | 10         | 7,989.22 ns |  49.376 ns |  43.771 ns | 123.17 |    0.81 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 9,846.65 ns | 186.484 ns | 199.536 ns | 151.90 |    3.13 | 2.5177 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 4,773.25 ns |  95.372 ns | 176.778 ns |  74.42 |    2.86 | 0.9079 |    1864 B |       14.56 |

With spinwaits:
| Method                | ListLength | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |     63.97 ns |   0.263 ns |   0.246 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         |  3,170.01 ns |  48.493 ns |  45.360 ns |  49.56 |    0.76 | 1.2779 |    2612 B |       20.41 |
| AsParallel            | 10         |  8,256.70 ns | 164.349 ns | 283.495 ns | 130.97 |    5.14 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 11,764.27 ns | 233.005 ns | 408.090 ns | 183.61 |    5.62 | 2.5024 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         |  5,551.51 ns | 109.036 ns | 267.466 ns |  87.96 |    4.52 | 0.9079 |    1864 B |       14.56 |

Spinwaits + lock in SingleWaiterBarrier + single pulse
| Method                | ListLength | Mean         | Error      | StdDev       | Median       | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |-------------:|-----------:|-------------:|-------------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |     66.73 ns |   1.087 ns |     0.963 ns |     66.82 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         |  3,147.08 ns |  26.659 ns |    24.937 ns |  3,153.33 ns |  47.20 |    0.89 | 1.2817 |    2616 B |       20.44 |
| AsParallel            | 10         |  8,106.82 ns | 161.549 ns |   269.912 ns |  8,025.80 ns | 123.88 |    4.53 | 2.1057 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 22,469.87 ns | 880.393 ns | 2,511.810 ns | 23,477.13 ns | 326.50 |   35.69 | 2.5024 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 20,395.34 ns | 399.918 ns |   427.908 ns | 20,491.31 ns | 306.58 |    8.65 | 0.8850 |    1864 B |       14.56 |

Spinwaits + single lock in SingleWaiterBarrier + single pulse
| Method                | ListLength | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |-------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |     64.18 ns |   0.655 ns |   0.512 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         |  3,137.06 ns |  36.904 ns |  34.520 ns |  48.81 |    0.51 | 1.2817 |    2619 B |       20.46 |
| AsParallel            | 10         |  8,001.50 ns | 137.713 ns | 128.817 ns | 124.84 |    2.28 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 24,728.38 ns | 482.068 ns | 450.927 ns | 387.08 |    7.65 | 2.5024 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 20,130.89 ns | 183.928 ns | 172.046 ns | 313.34 |    4.39 | 0.8850 |    1864 B |       14.56 |

Spinwaits + lock in SingleWaiterBarrier + pulseall
| Method                | ListLength | Mean         | Error        | StdDev       | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |-------------:|-------------:|-------------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |     64.03 ns |     0.192 ns |     0.160 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         |  3,097.14 ns |    59.508 ns |    63.673 ns |  48.19 |    1.03 | 1.2779 |    2612 B |       20.41 |
| AsParallel            | 10         |  8,060.71 ns |   160.360 ns |   171.583 ns | 126.32 |    2.81 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 34,206.31 ns | 1,396.055 ns | 4,116.299 ns | 547.09 |   57.53 | 2.5024 |    5180 B |       40.47 |
| AsParallelAsync_2     | 10         | 20,348.56 ns |   387.320 ns |   430.505 ns | 315.83 |    5.99 | 0.8850 |    1864 B |       14.56 |

Spinwait + no wait barrier:
| Method                | ListLength | Mean        | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------- |----------- |------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| SingleThreadedForEach | 10         |    62.64 ns |   0.131 ns |   0.122 ns |   1.00 |    0.00 | 0.0612 |     128 B |        1.00 |
| ParallelForEach       | 10         | 2,976.10 ns |  33.981 ns |  31.786 ns |  47.52 |    0.55 | 1.2856 |    2647 B |       20.68 |
| AsParallel            | 10         | 7,869.67 ns | 156.714 ns | 253.064 ns | 126.63 |    5.22 | 2.0905 |    4304 B |       33.62 |
| AsParallelAsync       | 10         | 8,922.07 ns | 172.024 ns | 204.783 ns | 143.19 |    3.29 | 2.5177 |    5176 B |       40.44 |
| AsParallelAsync_2     | 10         | 4,204.05 ns |  80.872 ns | 107.961 ns |  67.41 |    2.00 | 0.9079 |    1864 B |       14.56 |


| SingleThreadedForEach | 10         |     187.3 ns |     0.25 ns |      0.21 ns |     187.2 ns | 1.38 KB |
| ParallelForEach       | 10         |   3,223.1 ns |    28.15 ns |     26.33 ns |   3,217.5 ns |  3.8 KB |
| AsParallel            | 10         |   7,868.0 ns |   153.84 ns |    225.50 ns |   7,838.6 ns | 5.46 KB |
|                       |            |              |             |              |              |         |
| SingleThreadedForEach | 100        |     505.2 ns |     0.26 ns |      0.24 ns |     505.2 ns | 1.38 KB |
| ParallelForEach       | 100        |   4,049.4 ns |    27.48 ns |     22.95 ns |   4,046.7 ns | 3.82 KB |
| AsParallel            | 100        |   8,239.0 ns |   128.93 ns |    120.60 ns |   8,219.3 ns | 5.46 KB |
|                       |            |              |             |              |              |         |
| SingleThreadedForEach | 1000       |   3,673.7 ns |     1.09 ns |      1.02 ns |   3,674.1 ns | 1.38 KB |
| ParallelForEach       | 1000       |  36,997.8 ns |   498.30 ns |    466.11 ns |  37,072.9 ns | 4.54 KB |
| AsParallel            | 1000       |  13,902.1 ns |   232.64 ns |    217.61 ns |  13,949.7 ns | 5.46 KB |
|                       |            |              |             |              |              |         |
| SingleThreadedForEach | 10000      |  35,299.4 ns |    19.02 ns |     17.79 ns |  35,300.8 ns | 1.38 KB |
| ParallelForEach       | 10000      | 446,776.2 ns | 8,722.12 ns | 12,227.19 ns | 449,180.8 ns | 4.55 KB |
| AsParallel            | 10000      |  74,008.7 ns | 1,479.81 ns |  4,149.56 ns |  73,257.8 ns | 5.46 KB |


NO RESULT
| AsParallelAsync   | NA         | 9.068 us | 0.3019 us | 0.4426 us |   4.88 KB |
| AsParallelAsync_2 | NA         | 4.288 us | 0.0469 us | 0.0688 us |   1.78 KB |

STACK:
| AsParallelAsync   | 10         | 11.276 us | 0.2971 us | 0.4261 us |   5.35 KB |
| AsParallelAsync_2 | 10         |  5.113 us | 0.1429 us | 0.2095 us |   1.98 KB |
| AsParallelAsync   | 100        | 11.257 us | 0.1264 us | 0.1812 us |   5.35 KB |
| AsParallelAsync_2 | 100        |  6.297 us | 0.3018 us | 0.4328 us |   1.98 KB |
| AsParallelAsync   | 1000       | 13.136 us | 0.1961 us | 0.2935 us |   5.35 KB |
| AsParallelAsync_2 | 1000       |  8.323 us | 0.3070 us | 0.4500 us |   1.98 KB |
| AsParallelAsync   | 10000      | 32.754 us | 0.1782 us | 0.2668 us |   5.35 KB |
| AsParallelAsync_2 | 10000      | 31.620 us | 0.1511 us | 0.2215 us |   2.03 KB |

STACK + SYNC RESULT
| AsParallelAsync   | 10         | 11.025 us | 0.1666 us | 0.2443 us |   5.41 KB |
| AsParallelAsync_2 | 10         |  5.098 us | 0.1843 us | 0.2584 us |   2.03 KB |
| AsParallelAsync   | 100        | 11.563 us | 0.3312 us | 0.4533 us |   5.41 KB |
| AsParallelAsync_2 | 100        |  6.137 us | 0.2047 us | 0.2936 us |   2.03 KB |
| AsParallelAsync   | 1000       | 13.216 us | 0.2301 us | 0.3372 us |   5.41 KB |
| AsParallelAsync_2 | 1000       |  8.740 us | 0.4286 us | 0.6415 us |   2.03 KB |
| AsParallelAsync   | 10000      | 33.186 us | 0.6275 us | 0.9392 us |   5.41 KB |
| AsParallelAsync_2 | 10000      | 31.664 us | 0.1897 us | 0.2840 us |   2.08 KB |

| AsParallelAsync   | 10         | 11.197 us | 0.2755 us | 0.4038 us |   5.41 KB |
| AsParallelAsync_2 | 10         |  5.153 us | 0.2000 us | 0.2738 us |   2.03 KB |
| AsParallelAsync   | 100        | 11.300 us | 0.1448 us | 0.2168 us |   5.41 KB |
| AsParallelAsync_2 | 100        |  5.918 us | 0.1239 us | 0.1777 us |   2.03 KB |
| AsParallelAsync   | 1000       | 13.039 us | 0.1608 us | 0.2406 us |   5.41 KB |
| AsParallelAsync_2 | 1000       |  8.549 us | 0.3959 us | 0.5677 us |   2.03 KB |
| AsParallelAsync   | 10000      | 32.483 us | 0.1609 us | 0.2308 us |   5.41 KB |
| AsParallelAsync_2 | 10000      | 31.590 us | 0.1675 us | 0.2507 us |   2.08 KB |

QUEUE:
| AsParallelAsync   | 10         | 10.610 us | 0.1496 us | 0.2192 us |   7.64 KB |
| AsParallelAsync_2 | 10         |  7.167 us | 0.3534 us | 0.5290 us |   4.55 KB |
| AsParallelAsync   | 100        | 11.187 us | 0.2407 us | 0.3452 us |   7.64 KB |
| AsParallelAsync_2 | 100        |  7.309 us | 0.5265 us | 0.7881 us |   4.55 KB |
| AsParallelAsync   | 1000       | 13.293 us | 0.1144 us | 0.1712 us |   7.64 KB |
| AsParallelAsync_2 | 1000       |  9.872 us | 0.4847 us | 0.7255 us |   4.55 KB |
| AsParallelAsync   | 10000      | 33.527 us | 0.1836 us | 0.2747 us |   7.64 KB |
| AsParallelAsync_2 | 10000      | 32.269 us | 0.2190 us | 0.3277 us |    4.6 KB |

QUEUE + SYNC RESULT
| AsParallelAsync   | 10         | 10.883 us | 0.2394 us | 0.3584 us |    7.7 KB |
| AsParallelAsync_2 | 10         |  6.929 us | 0.2330 us | 0.3488 us |    4.6 KB |
| AsParallelAsync   | 100        | 11.519 us | 0.3028 us | 0.4532 us |    7.7 KB |
| AsParallelAsync_2 | 100        |  7.132 us | 0.1539 us | 0.2207 us |    4.6 KB |
| AsParallelAsync   | 1000       | 13.848 us | 0.1153 us | 0.1725 us |    7.7 KB |
| AsParallelAsync_2 | 1000       |  9.588 us | 0.3680 us | 0.5394 us |    4.6 KB |
| AsParallelAsync   | 10000      | 33.709 us | 0.1879 us | 0.2695 us |    7.7 KB |
| AsParallelAsync_2 | 10000      | 32.642 us | 0.1739 us | 0.2603 us |   4.65 KB |

ASYNC LOCAL + SYNC RESULT
| AsParallelAsync   | 10         |  9.761 us | 0.2957 us | 0.4426 us |   6.25 KB |
| AsParallelAsync_2 | 10         |  5.368 us | 0.1639 us | 0.2402 us |   2.31 KB |
| AsParallelAsync   | 100        |  9.871 us | 0.2296 us | 0.3365 us |   6.25 KB |
| AsParallelAsync_2 | 100        |  6.363 us | 0.2588 us | 0.3874 us |   2.31 KB |
| AsParallelAsync   | 1000       | 12.660 us | 0.2800 us | 0.4104 us |   6.25 KB |
| AsParallelAsync_2 | 1000       |  8.683 us | 0.5161 us | 0.7725 us |   2.31 KB |
| AsParallelAsync   | 10000      | 28.153 us | 0.1931 us | 0.2769 us |   6.25 KB |
| AsParallelAsync_2 | 10000      | 31.721 us | 0.1730 us | 0.2590 us |   2.36 KB |

*/