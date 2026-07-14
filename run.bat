@echo off
echo ===================================================
echo   Starting DTS Loyalty Hub Services...
echo ===================================================

echo Starting Admin API...
start "Admin API" cmd /k "cd api/admin/Api && dotnet run"

echo Starting Customer API...
start "Customer API" cmd /k "cd api/customer/Api && dotnet run"

echo.
echo All services have been started in separate windows!
echo To stop them, simply close the new command prompt windows.
pause
