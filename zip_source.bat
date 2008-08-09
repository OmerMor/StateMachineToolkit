SET WINRAR="%ProgramFiles%\WinRar\winrar.exe"
DEL ..\StateMachineToolkit.Sources.v6.zip
%WINRAR% a -afzip -m5 -x*\.svn -x*\.svn\* -r ..\StateMachineToolkit.Sources.v6.zip *.*
PAUSE