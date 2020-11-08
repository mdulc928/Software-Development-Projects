setlocal

    set "_folder=C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\"

    set "_file=msbuild.exe"

    set "_solution="

    "%_folder%"\%_file% %_solution% /p:configuration=debug

endlocal

pause 