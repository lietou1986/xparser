cd /d %~dp0
set HOME=%cd%
echo 当前目录是：%HOME%  
InstallUtil XParserService.exe 
net start XParserService


