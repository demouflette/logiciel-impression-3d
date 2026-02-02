@echo off
echo ========================================
echo Compilation de l'installateur
echo ========================================
echo.

echo Étape 1: Compilation du projet en Release...
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "D:\projet_3d\logiciel d'impression 3d\logiciel d'impression 3d.sln" /p:Configuration=Release /p:Platform="Any CPU"

if errorlevel 1 (
    echo ERREUR: La compilation a échoué!
    pause
    exit /b 1
)

echo.
echo Étape 2: Création de l'installateur avec Inno Setup...
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "D:\projet_3d\logiciel d'impression 3d\setup_script.iss"

if errorlevel 1 (
    echo ERREUR: La création de l'installateur a échoué!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Installation créée avec succès!
echo ========================================
echo Fichier: D:\projet_3d\logiciel d'impression 3d\Installer\Logiciel_Impression_3D_Setup_1.0.0.0.exe
echo.
pause