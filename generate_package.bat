@echo off

call VsDevCmd.bat


echo %~dp0

makeappx pack  /d  .\SparsePackage  /p ContextMenuySparseAppx.msix  /nv /o

SignTool sign /fd SHA256 /a /f .\\AppxSample\AppxSample_TemporaryKey.pfx ContextMenuySparseAppx.msix

move ./ContextMenuySparseAppx.msix  .\x64\debug\ContextMenuySparseAppx.msix