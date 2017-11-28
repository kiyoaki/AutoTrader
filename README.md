# AutoTrader
The sample of BTC FX trading software for bitFlyer.

## Getting Started

### Download and install .NET SDK

To start building .NET Core apps, you just need to download and install .NET SDK.
https://www.microsoft.com/net/learn/get-started/windows

### Create release build

In AutoTrader.csproj directory, execute this command.

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
