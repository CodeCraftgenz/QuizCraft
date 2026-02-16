; ============================================================================
; QuizCraft - Inno Setup Installer Script
; Version: 1.0.0
; Requires: Inno Setup 6.0+
; Build: Self-contained (nao requer .NET instalado)
; ============================================================================

#define MyAppName "QuizCraft"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "CodeCraft GenZ"
#define MyAppURL "https://codecraftgenz.com.br"
#define MyAppExeName "QuizCraft.Presentation.exe"
#define MyAppMutex "QuizCraft_SingleInstance_Mutex"
#define PublishDir "..\src\QuizCraft.Presentation\bin\Release\net9.0-windows\win-x64\publish"

[Setup]
; Identificadores
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Informacoes de versao
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Instalador do {#MyAppName} - Plataforma de Estudos Inteligente
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}
VersionInfoCopyright=Copyright (C) 2025 {#MyAppPublisher}

; Diretorios
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=QuizCraft_Setup_v{#MyAppVersion}

; Compressao (lzma2 maximo para reduzir tamanho do instalador)
Compression=lzma2/ultra64
SolidCompression=yes
LZMANumBlockThreads=4

; Aparencia
WizardStyle=modern
WizardSizePercent=110
SetupIconFile=quizcraft.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; Privilegios (instalar sem admin, com opcao de elevar)
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; Windows 10 minimo
MinVersion=10.0

; Mutex - impede execucao do instalador com o app aberto
AppMutex={#MyAppMutex}

; Desinstalador
Uninstallable=yes
UninstallDisplayName={#MyAppName}
CreateUninstallRegKey=yes

; Diversos
AllowNoIcons=yes
DisableProgramGroupPage=yes
CloseApplications=yes
RestartApplications=no
ArchitecturesInstallIn64BitMode=x64compatible
ArchitecturesAllowed=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
brazilianportuguese.CreateDesktopIcon=Criar atalho na &Area de Trabalho
brazilianportuguese.LaunchAfterInstall=Executar {#MyAppName} apos a instalacao
english.CreateDesktopIcon=Create a &desktop shortcut
english.LaunchAfterInstall=Launch {#MyAppName} after installation

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Todos os arquivos do publish self-contained (inclui .NET runtime)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Banco de dados pre-populado com ~1000 questoes (so copia se nao existir)
Source: "quizcraft.db"; DestDir: "{userappdata}\{#MyAppName}"; Flags: onlyifdoesntexist uninsneveruninstall

[Icons]
; Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Comment: "Abrir {#MyAppName} - Plataforma de Estudos"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"; Comment: "Desinstalar {#MyAppName}"

; Area de Trabalho (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "Abrir {#MyAppName}"

[Run]
; Executar apos instalacao
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchAfterInstall}"; Flags: nowait postinstall skipifsilent shellexec

[UninstallDelete]
; Limpar arquivos criados durante o uso
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\logs"
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\backups"
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\attachments"
Type: dirifempty; Name: "{userappdata}\{#MyAppName}"

[Code]
// ============================================================================
// Secao de codigo Pascal Script
// ============================================================================

// Cria os diretorios de dados do aplicativo no AppData do usuario
procedure CreateAppDataDirectories();
var
  BasePath: String;
begin
  BasePath := ExpandConstant('{userappdata}\{#MyAppName}');

  if not DirExists(BasePath) then
    CreateDir(BasePath);

  if not DirExists(BasePath + '\logs') then
    CreateDir(BasePath + '\logs');

  if not DirExists(BasePath + '\backups') then
    CreateDir(BasePath + '\backups');

  if not DirExists(BasePath + '\attachments') then
    CreateDir(BasePath + '\attachments');

  Log('Diretorios AppData criados em: ' + BasePath);
end;

// Evento pos-instalacao: cria diretorios AppData
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    CreateAppDataDirectories();
  end;
end;

// Evento de desinstalacao: perguntar se quer limpar dados
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  BasePath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    BasePath := ExpandConstant('{userappdata}\{#MyAppName}');

    if DirExists(BasePath) then
    begin
      if MsgBox('Deseja remover todos os dados do aplicativo (banco de dados, logs, backups)?',
                 mbConfirmation, MB_YESNO) = IDYES then
      begin
        DelTree(BasePath, True, True, True);
      end;
    end;
  end;
end;
