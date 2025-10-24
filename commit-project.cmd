@echo off
echo ====================================
echo 3DMolecules Git Commit Script
echo ====================================
echo.

echo Step 1: Killing any stuck git processes...
taskkill /F /IM git.exe 2>nul
timeout /t 2 /nobreak >nul

echo Step 2: Checking git status...
git status --short

echo.
echo Step 3: Staging all changes...
git add .

echo.
echo Step 4: Committing changes...
git commit -m "Add comprehensive README and fluid simulation features for v2.1"

echo.
echo Step 5: Pushing to origin/master...
git push origin master

echo.
echo ====================================
echo Done! Check output above for any errors.
echo ====================================
pause
