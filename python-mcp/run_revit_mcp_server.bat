@echo off
setlocal
REM Simple launcher to run the MCP server with the current Python
if "%PYTHON%"=="" set PYTHON=python
%PYTHON% -m pip install --quiet -r "%~dp0requirements.txt"
%PYTHON% "%~dp0revit_mcp_server.py"
