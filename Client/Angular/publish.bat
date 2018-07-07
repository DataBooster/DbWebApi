@ECHO OFF
CD /D %~dp0
call ng build --prod dbwebapi-client-lib
cd dist/dbwebapi-client-lib
call npm publish
