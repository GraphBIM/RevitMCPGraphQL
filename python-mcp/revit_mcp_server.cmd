@echo off
REM Windows launcher for the Revit GraphQL MCP server.
REM Allows invoking the server as an "executable" without explicitly calling python.
REM Fixes spawn errors where a client tries to execute revit_mcp_server.py directly.
setlocal ENABLEDELAYEDEXPANSION

REM Resolve script directory (handles spaces)
set "SCRIPT_DIR=%~dp0"
pushd "%SCRIPT_DIR%" >NUL 2>&1

if not defined PYTHON set "PYTHON=python"
where "%PYTHON%" >NUL 2>&1
if errorlevel 1 (
  echo [ERROR] Python interpreter '%PYTHON%' not found. 1>&2
  echo         Set the PYTHON environment variable to a valid interpreter path. 1>&2
  exit /b 9009
)

echo [INFO] Ensuring dependencies are installed...
"%PYTHON%" -m pip install --quiet -r requirements.txt
if errorlevel 1 (
  echo [ERROR] Failed to install dependencies. 1>&2
  exit /b 1
)

echo [INFO] Launching Revit GraphQL MCP server (stdio)...
"%PYTHON%" "%SCRIPT_DIR%revit_mcp_server.py" %*
set EXITCODE=%ERRORLEVEL%

if %EXITCODE% NEQ 0 (
  echo [ERROR] Server exited with code %EXITCODE%. 1>&2
)

popd >NUL 2>&1
exit /b %EXITCODE%
