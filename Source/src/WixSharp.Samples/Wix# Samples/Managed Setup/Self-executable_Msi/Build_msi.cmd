rem delete *.cs.dll as otherwise it can get locked
del *.cs.dll
..\..\..\cscs.exe /d ManagedSetup.msi.cs
pause
