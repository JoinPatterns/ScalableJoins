set PLATFORMNAME=x86
msbuild Benchmark.sln /t:Rebuild /p:Configuration=Release /p:Platform=%PLATFORMNAME%
set DIR=%DATE:/=-%\%TIME:~0,2%-%TIME:~3,2%\%COMPUTERNAME%\
if NOT EXIST %DIR% mkdir %DIR%
set ROOT=%CD%\%DIR%\
set BINDIR=bin\x86\Release

set NAME=Philosophers
pushd ..\%NAME%
%BINDIR%\%NAME% 0 0 > %ROOT%%NAME%-nospin.txt
%BINDIR%\%NAME% 50 200 > %ROOT%%NAME%-spin.txt
popd

set NAME=Semaphore
pushd ..\%NAME%
%BINDIR%\%NAME% 0 0 > %ROOT%%NAME%-nospin.txt
%BINDIR%\%NAME% 50 200 > %ROOT%%NAME%-spin.txt
popd

set NAME=Lock
pushd ..\%NAME%
%BINDIR%\%NAME% 0 0 > %ROOT%%NAME%-nospin.txt
%BINDIR%\%NAME% 50 200 > %ROOT%%NAME%-spin.txt
popd


set NAME=ProducerConsumer
pushd ..\%NAME%
%BINDIR%\%NAME% 0 0 > %ROOT%%NAME%-nospin.txt
%BINDIR%\%NAME% 10000 1000 > %ROOT%%NAME%-spin.txt
popd

set NAME=Rendezvous
pushd ..\%NAME%
%BINDIR%\%NAME% > %ROOT%%NAME%.txt
popd

set NAME=Barrier
pushd ..\%NAME%
%BINDIR%\%NAME% > %ROOT%%NAME%.txt
popd

:EOF