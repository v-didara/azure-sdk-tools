@echo off

if defined AzurePSRoot exit /b 0

echo Initializing environment...

if exist "%USERPROFILE%\SetNugetFeed.cmd" call "%USERPROFILE%\SetNugetFeed.cmd"

if not defined PRIVATE_FEED_URL (
    echo Error, please set following environment variables so that build script can download Spec Nuget Packages:
    echo     PRIVATE_FEED_URL, PRIVATE_FEED_USER_NAME and PRIVATE_FEED_PASSWORD
	exit /b 1
)

set "AzurePSRoot=%~dp0"
::get rid of the \tools\
set "AzurePSRoot=%AzurePSRoot:~0,-7%" 

if defined ProgramFiles(x86) (
    set "ADXSDKProgramFiles=%ProgramFiles(x86)%"
    set "ADXSDKPlatform=x64"
) else (
    set "ADXSDKProgramFiles=%ProgramFiles%"
    set "ADXSDKPlatform=x86"
)

if exist "%ADXSDKProgramFiles%\Microsoft Visual Studio 12.0" (
    set ADXSDKVSVersion=12.0
) else (
    set ADXSDKVSVersion=11.0
)

call "%ADXSDKProgramFiles%\Microsoft Visual Studio %ADXSDKVSVersion%\VC\vcvarsall.bat" x86