ILMerge.exe /out:publish\AutoTrader.exe /ndebug /targetplatform:v4 /wildcards AutoTrader\bin\Release\AutoTrader.exe AutoTrader\bin\Release\*.dll
copy AutoTrader\bin\Release\AutoTrader.exe.config publish\AutoTrader.exe.config
copy AutoTrader\bin\Release\NLog.config publish\NLog.config