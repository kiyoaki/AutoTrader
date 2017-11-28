# AutoTrader
BTC FX Trading Software Sample for bitFlyer

## Getting Started

### Create release build

```
dotnet publish -f netcoreapp2.0 -c Release
```

### Execute application

```
dotnet bin\Release\netcoreapp2.0\AutoTrader.dll -b {order btc} -k {key} -s {secret}

  -b, --betting    (Default: 1) Betting BTC amount for orders.

  -k, --key        Required. bitFlyer API Key.

  -s, --secret     Required. bitFlyer API Secret.

  --help           Display this help screen.
```
