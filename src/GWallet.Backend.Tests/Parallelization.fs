﻿namespace GWallet.Backend.Tests

open System
open System.Threading
open System.Diagnostics

open NUnit.Framework

open GWallet.Backend

module Parallelization =

    exception SomeException
    [<Test>]
    let ``calls both funcs (because it launches them in parallel)``() =
        let NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED = 2
        // because this test doesn't deal with inconsistencies
        let NUMBER_OF_CONSISTENT_RESULTS = NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED

        let someStringArg = "foo"
        let mutable func1Called = false
        let func1 (arg: string) =
            func1Called <- true
            0
        let mutable func2Called = false
        let func2 (arg: string) =
            func2Called <- true
            0

        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        client.Query<string,int> someStringArg [ func1; func2 ]
            |> Async.RunSynchronously |> ignore

        Assert.That(func1Called, Is.True)
        Assert.That(func2Called, Is.True)

        func1Called <- false
        func2Called <- false

        //same as before, but with different order now
        client.Query<string,int> someStringArg [ func2; func1 ]
            |> Async.RunSynchronously |> ignore

        Assert.That(func1Called, Is.True)
        Assert.That(func2Called, Is.True)

    [<Test>]
    let ``a long func doesn't block the others``() =
        let someLongTime = TimeSpan.FromSeconds 10.0

        let NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED = 2
        // because this test doesn't deal with inconsistencies
        let NUMBER_OF_CONSISTENT_RESULTS = 1

        let func1 (arg: string) =
            raise SomeException
            0
        let func2 (arg: string) =
            Thread.Sleep someLongTime
            0
        let func3Result = 1
        let func3 (arg: string) =
            func3Result

        let someStringArg = "foo"

        let stopWatch = Stopwatch.StartNew()
        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        let result = client.Query<string,int> someStringArg [ func1; func2; func3 ]
                         |> Async.RunSynchronously

        stopWatch.Stop()
        Assert.That(result, Is.EqualTo(func3Result))
        Assert.That(stopWatch.Elapsed, Is.LessThan(someLongTime))

    [<Test>]
    let ``a long func doesn't block gathering more succesful results for consistency``() =
        let someLongTime = TimeSpan.FromSeconds 10.0

        let NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED = 2
        let NUMBER_OF_CONSISTENT_RESULTS = 2

        let func1 (arg: string) =
            0
        let func2 (arg: string) =
            Thread.Sleep someLongTime
            0
        let func3 (arg: string) =
            0

        let someStringArg = "foo"

        let stopWatch = Stopwatch.StartNew()
        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        let result = client.Query<string,int> someStringArg [ func1; func2; func3 ]
                         |> Async.RunSynchronously

        stopWatch.Stop()
        Assert.That(result, Is.EqualTo(0))
        Assert.That(stopWatch.Elapsed, Is.LessThan(someLongTime))

        // different order of funcs
        let stopWatch = Stopwatch.StartNew()
        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        let result = client.Query<string,int> someStringArg [ func1; func3; func2; ]
                         |> Async.RunSynchronously

        stopWatch.Stop()
        Assert.That(result, Is.EqualTo(0))
        Assert.That(stopWatch.Elapsed, Is.LessThan(someLongTime))

        // different order of funcs
        let stopWatch = Stopwatch.StartNew()
        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        let result = client.Query<string,int> someStringArg [ func3; func2; func1; ]
                         |> Async.RunSynchronously

        stopWatch.Stop()
        Assert.That(result, Is.EqualTo(0))
        Assert.That(stopWatch.Elapsed, Is.LessThan(someLongTime))

        // different order of funcs
        let stopWatch = Stopwatch.StartNew()
        let client = FaultTolerantParallelClient<SomeException>(NUMBER_OF_CONSISTENT_RESULTS,
                                                                NUMBER_OF_PARALLEL_JOBS_TO_BE_TESTED)
        let result = client.Query<string,int> someStringArg [ func3; func1; func2; ]
                         |> Async.RunSynchronously

        stopWatch.Stop()
        Assert.That(result, Is.EqualTo(0))
        Assert.That(stopWatch.Elapsed, Is.LessThan(someLongTime))
