setlocal
 
    set "_folder=C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin"
 
    set "_file=MSBuild.exe"
 
    set "_solution=".\armsim.sln""
 
    "%_folder%"\%_file% %_solution% /p:configuration=debug
 
endlocal
 
pause