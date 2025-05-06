rem del C:\QUIK\lua\NetQuikConnector.dll
dotnet publish -r win-x64 -c Release
rem xcopy /s /y bin\Release\net8.0\win-x64\native\NetQuikConnector.dll C:\QUIK\lua
pause