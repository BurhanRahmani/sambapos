#include "scripts\products.iss"

#include "scripts\products\winversion.iss"
#include "scripts\products\fileversion.iss"
//#include "scripts\products\iis.iss"
//#include "scripts\products\kb835732.iss"

//#include "scripts\products\msi20.iss"
#include "scripts\products\msi31.iss"
//#include "scripts\products\ie6.iss"

//#include "scripts\products\dotnetfx11.iss"
//#include "scripts\products\dotnetfx11lp.iss"
//#include "scripts\products\dotnetfx11sp1.iss"

//#include "scripts\products\dotnetfx20.iss"
//#include "scripts\products\dotnetfx20lp.iss"
//#include "scripts\products\dotnetfx20sp1.iss"
//#include "scripts\products\dotnetfx20sp1lp.iss"

//#include "scripts\products\dotnetfx35.iss"
//#include "scripts\products\dotnetfx35lp.iss"
//#include "scripts\products\dotnetfx35sp1.iss"
//#include "scripts\products\dotnetfx35sp1lp.iss"

//#include "scripts\products\dotnetfx40.iss"
#include "scripts\lgpl.iss"
#include "scripts\products\ssce40.iss"
#include "scripts\products\dotnetfx40client.iss"
#include "scripts\products\wic.iss"

//#include "scripts\products\mdac28.iss"
//#include "scripts\products\jet4sp8.iss"

#define Version "2.62 "
#define FileVersion "262"

[CustomMessages]
win2000sp3_title=Windows 2000 Service Pack 3
winxpsp2_title=Windows XP Service Pack 2
winxpsp3_title=Windows XP Service Pack 3
en.full_setup=Full Setup
en.compact_setup=Compact Setup
en.custom_setup=Custom Setup
en.sample_data=Sample Data
en.handheld_terminal_app=Handheld terminal app
en.ce_install_sp3_required=Compact SQL 4.0 removed from packages list because Service Pack 3 required for Compact SQL 4.0 installation. Program will run with TXT database.

tr.full_setup=Tam Kurulum
tr.compact_setup=Normal Kurulum
tr.custom_setup=�zel Kurulum
tr.sample_data=�rnek Veri
tr.handheld_terminal_app=El terminali uygulamas�
tr.ce_install_sp3_required=Compact SQL 4.0 �al��t�rmak i�in Service Pack 3 gerekti�inden kurulum listesinden kald�r�ld�. Program TXT dosya veritaban� �zerinden �al��acak.

[Setup]
AppName=Samba POS
Uninstallable=true
DirExistsWarning=no
CreateAppDir=true
OutputDir=bin
OutputBaseFilename=SambaSetup{#FileVersion}
SourceDir=.
AppCopyright=Copyright � �zg� Teknoloji 2011
AppVerName=Samba POS {#Version}

DefaultGroupName=Samba POS 2
AllowNoIcons=true
AppPublisher=�zg� Teknoloji
AppVersion={#Version}
UninstallDisplayIcon={app}\Samba.Presentation.exe
UninstallDisplayName=Samba POS 2
UsePreviousGroup=true
UsePreviousAppDir=true
DefaultDirName={pf}\SambaPOS2
VersionInfoVersion={#Version}
VersionInfoCompany=�zg� Teknoloji
VersionInfoCopyright=Copyright � Ozgu 2010
ShowUndisplayableLanguages=false
LanguageDetectionMethod=locale
InternalCompressLevel=fast
SolidCompression=true
Compression=lzma/fast

;required by products
MinVersion=4.1,5.0
PrivilegesRequired=admin
ArchitecturesAllowed=
VersionInfoProductName=Samba POS Setup
AppID={{7FFA6CCB-2A30-4F09-A216-0CDFC031E870}


[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: de; MessagesFile: compiler:Languages\German.isl
Name: tr; MessagesFile: compiler:Languages\Turkish.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Languages: ; Components: 
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
Source: src\Norm.dll; DestDir: {app}
Source: src\EntityFramework.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.MefExtensions.dll; DestDir: {app}
Source: src\Microsoft.Practices.Prism.Interactivity.dll; DestDir: {app}
Source: src\Microsoft.Practices.EnterpriseLibrary.Common.dll; DestDir: {app}
Source: src\Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.dll; DestDir: {app}
Source: src\Microsoft.Practices.EnterpriseLibrary.ExceptionHandling.Logging.dll; DestDir: {app}
Source: src\Microsoft.Practices.EnterpriseLibrary.Logging.dll; DestDir: {app}
Source: src\Microsoft.Practices.EnterpriseLibrary.Validation.dll; DestDir: {app}
Source: src\Microsoft.Practices.ServiceLocation.dll; DestDir: {app}
Source: src\Microsoft.Practices.Unity.dll; DestDir: {app}
Source: src\Microsoft.Practices.Unity.Interception.dll; DestDir: {app}
Source: src\PropertyTools.Wpf.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Domain.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Infrastructure.Data.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Infrastructure.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Login.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.MessagingServer.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.BasicReports.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.BasicReports.pdb; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.CashModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.CustomerModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.DashboardModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.MenuModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.InventoryModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.NavigationModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.SettingsModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.TableModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.TicketModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Modules.UserModule.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Persistance.DBMigration.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Persistance.Data.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.Common.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe.config; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.exe.manifest; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Presentation.Terminal.exe; DestDir: {app}; Flags: ignoreversion; Components: terminal
Source: src\Samba.Presentation.Terminal.exe.config; DestDir: {app}; Flags: ignoreversion; Components: terminal
Source: src\AxInterop.cidv5callerid.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Interop.cidv5callerid.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Samba.Modules.CidMonitor.dll; DestDir: {app}; Flags: ignoreversion; Components: cid
Source: src\Samba.Presentation.ViewModels.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Samba.Services.dll; DestDir: {app}; Flags: ignoreversion
Source: src\System.Windows.Interactivity.dll; DestDir: {app}
Source: src\UIControls.dll; DestDir: {app}; Flags: ignoreversion
Source: src\Images\apple-icon.png; DestDir: {app}\Images\
Source: src\Images\apple.ico; DestDir: {app}\Images\
Source: src\Images\empty.png; DestDir: {app}\Images\
Source: src\Images\logo.png; DestDir: {app}\Images\; Flags: onlyifdoesntexist
Source: src\Imports\menu.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\table.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\menu_tr.txt; DestDir: {app}\Imports\; Components: veri
Source: src\Imports\table_tr.txt; DestDir: {app}\Imports\; Components: veri
Source: src\FlexButton.dll; DestDir: {app}; Flags: ignoreversion
Source: src\DataGridFilterLibrary.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentMigrator.dll; DestDir: {app}; Flags: ignoreversion
Source: src\FluentMigrator.Runner.dll; DestDir: {app}; Flags: ignoreversion
Source: src\GongSolutions.Wpf.DragDrop.dll; DestDir: {app}; Flags: ignoreversion
Source: src\migrate.txt; DestDir: {userappdata}\Ozgu Tech\SambaPOS2; Flags: ignoreversion
Source: C:\Windows\Fonts\lucon.ttf; DestDir: {fonts}; Flags: onlyifdoesntexist uninsneveruninstall; FontInstall: Lucida Console
Source: src\Samba.Localization.dll; DestDir: {app}; Flags: ignoreversion
Source: src\tr\Samba.Localization.resources.dll; DestDir: {app}\tr\; Flags: ignoreversion

[Components]
Name: pos; Description: Samba POS; Types: full compact custom; Flags: fixed
Name: terminal; Description: {cm:handheld_terminal_app}; Languages: ; Types: full
Name: sqlce; Description: Compact SQL 4.0; Languages: ; Types: full compact custom
Name: veri; Description: {cm:sample_data}; Languages: ; Types: full compact custom
Name: cid; Description: Caller Id; Languages: ; Types: full custom

[Types]
Name: compact; Description: {cm:compact_setup}
Name: full; Description: {cm:full_setup}
Name: custom; Description: {cm:custom_setup}; Flags: iscustom

[Icons]
Name: {group}\Samba POS 2; Filename: {app}\Samba.Presentation.exe
Name: {group}\{cm:UninstallProgram,Samba POS}; Filename: {uninstallexe}
Name: {commondesktop}\Samba POS 2; Filename: {app}\Samba.Presentation.exe; IconIndex: 0; Flags: createonlyiffileexists; Components: 
Name: {commondesktop}\Samba Terminal; Filename: {app}\Samba.Presentation.Terminal.exe; Flags: createonlyiffileexists
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Samba POS; Filename: {app}\Samba.Presentation.exe; Tasks: quicklaunchicon
Name: {group}\Samba Data; Filename: {commonappdata}\Ozgu Tech\SambaPOS2\

[Run]
Filename: {app}\Samba.Presentation.exe; Description: {cm:LaunchProgram,Samba POS}; Flags: nowait postinstall skipifsilent unchecked

[Code]

procedure CurPageChanged(CurPageID: Integer);
begin
if (CurPageId = wpSelectProgramGroup) then
  begin
    RemoveProducts();
    msi31('3.0');
    wic();
    dotnetfx40client();
    if IsComponentSelected('sqlce') then
    if not minwinspversion(5, 1, 3) then begin
		MsgBox(FmtMessage(CustomMessage('ce_install_sp3_required'), [CustomMessage('winxpsp3_title')]), mbError, MB_OK);
    end else begin
		ssce40();
    end;
  end;
end;

procedure InitializeWizard();
begin
  LGPL_InitializeWizard();
end;

function InitializeSetup(): Boolean;
begin
	initwinversion();

	if not minwinspversion(5, 0, 3) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('win2000sp3_title')]), mbError, MB_OK);
		exit;
	end;
	if not minwinspversion(5, 1, 2) then begin
		MsgBox(FmtMessage(CustomMessage('depinstall_missing'), [CustomMessage('winxpsp2_title')]), mbError, MB_OK);
		exit;
	end;

	//if (not iis()) then exit;

	//msi20('2.0');

	//ie6('5.0.2919');

	//dotnetfx11();
	//dotnetfx11lp();
	//dotnetfx11sp1();

	//kb835732();

	//if (minwinversion(5, 0) and minspversion(5, 0, 4)) then begin
	//	dotnetfx20sp1();
		//dotnetfx20sp1lp();
	//end else begin
	//	dotnetfx20();
		//dotnetfx20lp();
	//end;

	//dotnetfx35();
	//dotnetfx35lp();
	//dotnetfx35sp1();
	//dotnetfx35sp1lp();

	//mdac28('2.7');
	//jet4sp8('4.0.8015');

  Result := true;
end;

[Dirs]
Name: {app}\Images
Name: {app}\Imports
Name: {app}\tr
Name: {commonappdata}\Ozgu Tech\SambaPOS2
