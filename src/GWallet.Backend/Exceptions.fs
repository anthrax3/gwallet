﻿namespace GWallet.Backend

exception InsufficientFunds
exception InvalidPassword
exception DestinationEqualToOrigin
exception AddressMissingProperPrefix of seq<string>
exception AddressWithInvalidLength of int
exception AddressWithInvalidChecksum of Option<string>
exception AccountAlreadyAdded
