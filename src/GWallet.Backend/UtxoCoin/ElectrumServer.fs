﻿namespace GWallet.Backend.UtxoCoin

open System
open System.IO
open System.Reflection

open FSharp.Data
open FSharp.Data.JsonExtensions

open GWallet.Backend

type IncompatibleServerException(message) =
    inherit JsonRpcSharp.ConnectionUnsuccessfulException(message)

type IncompatibleProtocolException(message) =
    inherit IncompatibleServerException(message)

type ServerTooNewException(message) =
    inherit IncompatibleProtocolException(message)

type ServerTooOldException(message) =
    inherit IncompatibleProtocolException(message)

type TlsNotSupportedYetInGWalletException(message) =
   inherit IncompatibleServerException(message)

type TorNotSupportedYetInGWalletException(message) =
   inherit IncompatibleServerException(message)

type ElectrumServer =
    {
        Fqdn: string;
        Pruning: string;
        PrivatePort: Option<int>;
        UnencryptedPort: Option<int>;
        Version: string;
    }
    member this.CheckCompatibility (): unit =
        if this.UnencryptedPort.IsNone then
            raise(TlsNotSupportedYetInGWalletException("TLS not yet supported"))
        if this.Fqdn.EndsWith ".onion" then
            raise(TorNotSupportedYetInGWalletException("Tor(onion) not yet supported"))

module ElectrumServerSeedList =

    let private ExtractServerListFromEmbeddedResource resourceName =
        let assembly = Assembly.GetExecutingAssembly()
        use stream = assembly.GetManifestResourceStream resourceName
        if (stream = null) then
            failwithf "Embedded resource %s not found" resourceName
        use reader = new StreamReader(stream)
        let list = reader.ReadToEnd()
        let serversParsed = JsonValue.Parse list
        let servers =
            seq {
                for (key,value) in serversParsed.Properties do
                    let maybeUnencryptedPort = value.TryGetProperty "t"
                    let unencryptedPort =
                        match maybeUnencryptedPort with
                        | None -> None
                        | Some portAsString -> Some (Int32.Parse (portAsString.AsString()))
                    let maybeEncryptedPort = value.TryGetProperty "s"
                    let encryptedPort =
                        match maybeEncryptedPort with
                        | None -> None
                        | Some portAsString -> Some (Int32.Parse (portAsString.AsString()))
                    yield { Fqdn = key;
                            Pruning = value?pruning.AsString();
                            PrivatePort = encryptedPort;
                            UnencryptedPort = unencryptedPort;
                            Version = value?version.AsString(); }
            }
        servers |> List.ofSeq

    let private FilterCompatibleServer (electrumServer: ElectrumServer) =
        try
            electrumServer.CheckCompatibility()
            true
        with
        | :? IncompatibleServerException -> false

    let DefaultBtcList =
        ExtractServerListFromEmbeddedResource "btc-servers.json"
            |> Seq.filter FilterCompatibleServer
            |> List.ofSeq

    let DefaultLtcList =
        ExtractServerListFromEmbeddedResource "ltc-servers.json"
            |> Seq.filter FilterCompatibleServer
            |> List.ofSeq

    let Randomize currency =
        let serverList =
            match currency with
            | BTC -> DefaultBtcList
            | LTC -> DefaultLtcList
            | _ -> failwithf "Currency %s is not UTXO" (currency.ToString())
        Shuffler.Unsort serverList
