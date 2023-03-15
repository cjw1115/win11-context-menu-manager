@echo off
echo %~dp0

.\AppCertKit\makeappx.exe pack  /d  .\SparsePackage  /p ContextMenuySparseAppx.msix  /nv /o
.\AppCertKit\SignTool.exe sign /fd SHA256 /a /f .\TemporaryKey.pfx ContextMenuySparseAppx.msix

REM move ./ContextMenuySparseAppx.msix  .\x64\debug\ContextMenuySparseAppx.msix