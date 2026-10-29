@echo off
rem Builds bin\KeepSpeakerOn.exe with the C# compiler bundled with Windows (.NET Framework 4.x).
setlocal
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
  echo C# compiler not found. Install or enable .NET Framework 4.x.
  exit /b 1
)
cd /d "%~dp0"
if not exist bin mkdir bin
"%CSC%" /nologo /target:winexe /platform:anycpu32bitpreferred /optimize+ /warnaserror+ ^
  /win32icon:assets\KeepSpeakerOn.ico /out:bin\KeepSpeakerOn.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll src\KeepSpeakerOn.cs
if errorlevel 1 exit /b 1
echo Built bin\KeepSpeakerOn.exe
