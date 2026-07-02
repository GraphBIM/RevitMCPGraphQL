@echo off
setlocal
REM Simple launcher to run the MCP server with the current Python
REM Prefer py launcher on Windows over python command (avoids Windows Store alias issues)
if "%PYTHON%"=="" (
  where py >NUL 2>&1
  if not errorlevel 1 (
    set PYTHON=py
  ) else (
    set PYTHON=python
  )
)
%PYTHON% -m pip install --quiet -r "%~dp0requirements.txt"
%PYTHON% "%~dp0revit_mcp_server.py"
