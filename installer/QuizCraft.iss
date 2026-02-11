; ============================================================================
; QuizCraft - Inno Setup Installer Script
; Version: 1.0.0
; Requires: Inno Setup 6.0+
; ============================================================================

#define MyAppName "QuizCraft"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "QuizCraft Software"
#define MyAppURL "https://github.com/quizcraft/quizcraft"
#define MyAppExeName "QuizCraft.Presentation.exe"
#define MyAppMutex "QuizCraft_SingleInstance_Mutex"

[Setup]
; Identifiers
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Version info
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Instalador do {#MyAppName}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

; Directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=Output
OutputBaseFilename=QuizCraft_Setup_v{#MyAppVersion}

; Compression
Compression=lzma2/max
SolidCompression=yes

; Appearance
SetupIconFile=..\assets\quizcraft.ico
UninstallDisplayIcon={app}\quizcraft.ico
WizardStyle=modern
WizardSizePercent=110

; Privileges
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

; License (placeholder - substitua pelo arquivo real quando disponivel)
; LicenseFile=..\LICENSE.txt

; Minimum Windows version (Windows 10+)
MinVersion=10.0

; Mutex - impede execucao do instalador enquanto o app estiver aberto
AppMutex={#MyAppMutex}

; Uninstaller
Uninstallable=yes
UninstallDisplayName={#MyAppName}
CreateUninstallRegKey=yes

; Misc
AllowNoIcons=yes
DisableProgramGroupPage=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
brazilianportuguese.CreateDesktopIcon=Criar atalho na &Area de Trabalho
brazilianportuguese.LaunchAfterInstall=Executar {#MyAppName} apos a instalacao
brazilianportuguese.DotNetRequired=Este aplicativo requer o .NET 9.0 Runtime. Deseja abrir a pagina de download?
english.CreateDesktopIcon=Create a &desktop shortcut
english.LaunchAfterInstall=Launch {#MyAppName} after installation
english.DotNetRequired=This application requires .NET 9.0 Runtime. Would you like to open the download page?

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Publish output - todos os arquivos da pasta publish
Source: "..\src\QuizCraft.Presentation\bin\Release\net9.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Icone do aplicativo (placeholder - garanta que o arquivo exista antes do build)
; Source: "..\assets\quizcraft.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Menu Iniciar
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\quizcraft.ico"; Comment: "Abrir {#MyAppName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"; IconFilename: "{app}\quizcraft.ico"; Comment: "Desinstalar {#MyAppName}"

; Area de Trabalho (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\quizcraft.ico"; Tasks: desktopicon; Comment: "Abrir {#MyAppName}"

[Run]
; Executar o aplicativo apos a instalacao (opcional)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchAfterInstall}"; Flags: nowait postinstall skipifsilent shellexec

[UninstallDelete]
; Limpar arquivos de log e cache criados durante o uso
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\logs"
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\backups"
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}\attachments"
Type: dirifempty; Name: "{userappdata}\{#MyAppName}"

[Code]
// ============================================================================
// Secao de codigo Pascal Script
// ============================================================================

const
  DOTNET_DOWNLOAD_URL = 'https://dotnet.microsoft.com/pt-br/download/dotnet/9.0';

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

// Verifica se o .NET Runtime esta instalado
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
            and (ResultCode = 0);
end;

// Evento pos-instalacao: cria diretorios AppData
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    CreateAppDataDirectories();
  end;
end;

// Evento de inicializacao: verifica pre-requisitos
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;

  // Verificar se o .NET 9.0 esta instalado (apenas para builds nao self-contained)
  if not IsDotNetInstalled() then
  begin
    if MsgBox(CustomMessage('DotNetRequired'), mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', DOTNET_DOWNLOAD_URL, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
    end;
    // Nao bloqueia a instalacao, pois o build pode ser self-contained
  end;
end;

// Evento de desinstalacao: limpar dados restantes
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  BasePath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    BasePath := ExpandConstant('{userappdata}\{#MyAppName}');

    // Perguntar se deseja remover dados do usuario
    if DirExists(BasePath) then
    begin
      if MsgBox('Deseja remover todos os dados do aplicativo (logs, backups, anexos)?',
                 mbConfirmation, MB_YESNO) = IDYES then
      begin
        DelTree(BasePath, True, True, True);
      end;
    end;
  end;
end;
