﻿namespace GWallet.Backend.Tests

open System
open System.Numerics
open System.Reflection

open NUnit.Framework
open Newtonsoft.Json

open GWallet.Backend

module Serialization =
    let version = Assembly.GetExecutingAssembly().GetName().Version.ToString()

    [<Test>]
    let ``basic caching export does not fail``() =
        let json = Caching.ExportToJson MarshallingData.EmptyCachingDataExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)

    [<Test>]
    let ``basic caching export is accurate``() =
        let json = Caching.ExportToJson MarshallingData.EmptyCachingDataExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json, Is.EqualTo (MarshallingData.EmptyCachingDataExampleInJson))

    [<Test>]
    let ``complex caching export works``() =

        let json = Caching.ExportToJson MarshallingData.SofisticatedCachingDataExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)

        Assert.That(json,
                    Is.EqualTo (MarshallingData.SofisticatedCachingDataExampleInJson))

    [<Test>]
    let ``unsigned BTC transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson
                               MarshallingData.UnsignedBtcTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json,
                    Is.EqualTo(MarshallingData.UnsignedBtcTransactionExampleInJson))

    [<Test>]
    let ``unsigned ether transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson
                               MarshallingData.UnsignedEtherTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json,
                    Is.EqualTo(MarshallingData.UnsignedEtherTransactionExampleInJson))

    [<Test>]
    let ``signed btc transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson MarshallingData.SignedBtcTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json, Is.EqualTo (MarshallingData.SignedBtcTransactionExampleInJson))

    [<Test>]
    let ``signed ether transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson MarshallingData.SignedEtherTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json, Is.EqualTo (MarshallingData.SignedEtherTransactionExampleInJson))

    [<Test>]
    let ``unsigned DAI transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson
                               MarshallingData.UnsignedDaiTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json |> MarshallingData.RemoveJsonFormatting,
                    Is.EqualTo(MarshallingData.UnsignedDaiTransactionExampleInJson))

    [<Test>]
    let ``signed DAI transaction export``() =
        let json = Account.ExportUnsignedTransactionToJson MarshallingData.SignedDaiTransactionExample
        Assert.That(json, Is.Not.Null)
        Assert.That(json, Is.Not.Empty)
        Assert.That(json|> MarshallingData.RemoveJsonFormatting,
                    Is.EqualTo (MarshallingData.SignedDaiTransactionExampleInJson))
