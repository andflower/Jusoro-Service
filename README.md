# Jusoro-Service

[Jusoro Service Setting]
<br>base path : c:\jusoro-2.0.0-win64-internet
<br>ref : https://business.juso.go.kr/addrlink/tchnlgySport/adresSearchSlutn.do

[Windows Service Publish]
<br>![image](https://github.com/andflower/Jusoro-Service/assets/55326840/2449dfd2-463c-4f4f-afed-abd8fa7e0a51)
![image](https://github.com/andflower/Jusoro-Service/assets/55326840/133019ed-f36e-4cf0-8c3d-3db8ea3b896f)
![image](https://github.com/andflower/Jusoro-Service/assets/55326840/2f3fd863-6c9f-47e7-bdef-1b61a50caf26)

[Windows 11 Test(in PowerShell) - Service start]
<br>ref : https://learn.microsoft.com/ko-kr/dotnet/core/extensions/windows-service?pivots=dotnet-7-0#create-the-windows-service

1. sc.exe create "{Service Name}" binpath="{Project bin path}"
    <br>ex) sc.exe create "Jusoro Service" binpath="{Project path}\bin\Release\net7.0\win-64\publish\win-64\JusoroService.exe"
   
3. sc.exe failure "Jusoro Service" reset=0 actions=restart/60000/restart/60000/run/1000
    <br>- 1st fail -> restart 60000 ms -> after delay 6s restart
    <br>- 2nd fail -> restart 60000 ms -> after delay 6s restart
    <br>- 3th fail -> run 1000 ms -> after delay 1s Program start
    <br>ex) sc.exe failure "Jusoro Service" reset=0 actions=restart/60000/restart/60000/run/1000

4. windows service confirm command -> sc qfailure "{Service Name}"
    <br>ex) windows service confirm command -> sc qfailure "Jusoro Service"

5. sc.exe start "{Service Name}"
    <br>ex) sc.exe start "Jusoro Service"

[Troubleshooting - Service stop & delete]
<br>ref : https://learn.microsoft.com/ko-kr/dotnet/core/extensions/windows-service?pivots=dotnet-7-0#stop-the-windows-service
<br>
1. sc.exe stop "{Service Name}"                      // Service stop
    <br>ex) sc.exe stop "Jusoro Service"
2. sc.exe delete "{Service Name}"
    <br>ex) sc.exe delete "Jusoro Service"               // Service delete
