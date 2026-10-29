@echo off
setlocal
cd /d "%~dp0"

if "%~1"=="" goto usage
if /i "%~1"=="start"  goto start
if /i "%~1"=="stop"   goto stop
if /i "%~1"=="test"   goto test
if /i "%~1"=="status" goto status
goto usage

:usage
echo Usage: demo.bat [start^|stop^|test^|status]
echo   start   build and start the database, API and UI in Docker (UI: http://localhost:5300)
echo   stop    stop and remove the containers (the database data is kept)
echo   test    run the C# tests, then the Playwright UI tests (needs "start" first)
echo   status  show the containers
exit /b 1

:start
docker compose up -d --build
echo.
echo UI  http://localhost:5300   login: admin / Salon123!
echo API http://localhost:5301/api/health
exit /b %errorlevel%

:stop
docker compose down
exit /b %errorlevel%

:test
pushd api
dotnet test SalonApi.sln || (popd & exit /b 1)
popd
pushd ui
call npm install
call npx playwright install chromium
call npx playwright test
set RESULT=%errorlevel%
popd
exit /b %RESULT%

:status
docker compose ps
exit /b 0
