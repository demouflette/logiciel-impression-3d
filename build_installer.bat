@echo off
echo ========================================
echo Compilation de l'installateur
echo ========================================
echo.

echo �tape 1: Compilation du projet en Release...
"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" "D:\projet_3d\logiciel d'impression 3d\logiciel d'impression 3d.csproj" /p:Configuration=Release /v:minimal

if errorlevel 1 (
    echo ERREUR: La compilation a �chou�!
    pause
    exit /b 1
)

echo.
echo �tape 2: Cr�ation de l'installateur avec Inno Setup...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "D:\projet_3d\logiciel d'impression 3d\setup_script.iss"

if errorlevel 1 (
    echo ERREUR: La cr�ation de l'installateur a �chou�!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Installation cr��e avec succ�s!
echo ========================================
echo Fichier: D:\projet_3d\logiciel d'impression 3d\Installer\Logiciel_Impression_3D_Setup_1.1.0.0.exe
echo.
pause