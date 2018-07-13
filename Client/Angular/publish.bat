@ECHO OFF
CD /D %~dp0
call ng build dbwebapi-client-lib --prod
cd dist/dbwebapi-client-lib
call npm publish
