; Script d'installation pour Logiciel d'Impression 3D
; G�n�r� avec Inno Setup

#define MyAppName "Logiciel d'Impression 3D"
#define MyAppVersion "1.6.12.0"
#define MyAppPublisher "DemouFlette"
#define MyAppURL "https://github.com/demouflette/logiciel-impression-3d-updates"
#define MyAppExeName "logiciel d'impression 3d.exe"
#define MyAppIcon "imprimante.ico"

[Setup]
; Informations de base
AppId={{B7F2E8A9-3C1D-4F6B-9A2E-7D5C8B4F1A3E}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Chemins d'installation
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
AllowNoIcons=yes

; Fichiers de sortie
OutputDir=D:\projet_3d\logiciel d'impression 3d\Installer
OutputBaseFilename=Logiciel_Impression_3D_Setup_{#MyAppVersion}
SetupIconFile=D:\projet_3d\logiciel d'impression 3d\{#MyAppIcon}
Compression=lzma2/max
SolidCompression=yes

; Privil�ges requis
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Interface moderne
WizardStyle=modern

; Langue par d�faut
ShowLanguageDialog=auto

; Informations de version
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Installation de {#MyAppName}
VersionInfoCopyright=Copyright (C) 2024 {#MyAppPublisher}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "Cr�er une ic�ne dans la barre de lancement rapide"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Fichier principal (.exe)
Source: "D:\projet_3d\logiciel d'impression 3d\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\projet_3d\logiciel d'impression 3d\bin\Release\{#MyAppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Raccourcis dans le menu d�marrer
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; IconIndex: 0
Name: "{group}\D�sinstaller {#MyAppName}"; Filename: "{uninstallexe}"

; Raccourci sur le bureau
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"; IconIndex: 0

; Raccourci barre de lancement rapide
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon; IconFilename: "{app}\{#MyAppExeName}"; IconIndex: 0

[Run]
; Lancer l'application � la fin de l'installation
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Supprimer les fichiers de donn�es lors de la d�sinstallation (OPTIONNEL)
; users.dat est maintenant dans %APPDATA%\DemouFlette\Logiciel3D\ — pas supprimé ici
Type: files; Name: "{app}\parametres_impression.dat"
Type: files; Name: "{app}\imprimantes_specs_cache.dat"

[Code]
// V�rifier si .NET Framework 4.8 ou sup�rieur est install�
function IsDotNetInstalled: Boolean;
var
  success: Boolean;
  release: Cardinal;
begin
  success := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', release);
  Result := success and (release >= 528040);
end;

// Fonction appel�e au d�but de l'installation
function InitializeSetup: Boolean;
begin
  if not IsDotNetInstalled then
  begin
    MsgBox('Ce logiciel n�cessite .NET Framework 4.8 ou sup�rieur.'#13#13
           'Veuillez installer .NET Framework 4.8.1 avant de continuer.'#13#13
           'T�l�chargement : https://dotnet.microsoft.com/download/dotnet-framework/net481', 
           mbCriticalError, MB_OK);
    Result := False;
  end
  else
    Result := True;
end;