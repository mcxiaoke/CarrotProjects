call "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" -arch=x64 -host_arch=x64
msbuild --version
:: msbuild GenshinNotifier.sln -t:Restore
msbuild GenshinNotifier.sln -detailedSummary -restore
msbuild GenshinNotifier.sln -detailedSummary -t:rebuild -p:Configuration=Debug
msbuild GenshinNotifier.sln -detailedSummary -t:rebuild -p:Configuration=Release
cd bin
7z a -tzip ../publish/Release.zip Release/ -xr!backups -xr!temp
cd ../publish
call powershell -c "Get-FileHash" Release.exe
echo `powershell -c "Get-FileHash" Release.exe` > version.json