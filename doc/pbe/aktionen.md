# PBE-Aktionen

## Allgemein Aktionen

Die eigentliche Arbeit wird in den Aktionen ausgeführt.
Neben der FSConsole.exe können auch allgemeine Aktionen definiert werden.
So können z.B. entsprechende Ordner oder Dienste vorbereitet werden.

### Konten `<Batch>`

Damit kann eine beliebige Batch-Routine ausgeführt werden.

Attribute:

* **Name**: optional

* **Cmd**: erforderlich - gibt den Pfad für eine Exe oder eine Batch-Datei an.
  Die Angabe sollte mit komplettem Pfad erfolgen.

* **Args**: optional - hier können Kommandozeilen-Argumente übergeben werden.

  Args können auch als XML-Knoten angegeben werden.
  Diese werden hinter die im Attribut definierten Args gepackt.
  Das ist praktisch bei komplexeren Listen von Kommandozeilen-Argumenten.
  PBE kümmert sich dabei um die korrekte Behandlung von Leerzeichen und Anführungszeichen.

Liefert die aufgerufene Routine einen Exit-Code != 0, dann wird der Vorgang als Fehlerhaft beendet.

Wird als `Cmd="robocopy"` aufgrufen, dann führen die Exit-Codes 1-7 nicht zu einem Fehler (vergleiche auch <https://learn.microsoft.com/troubleshoot/windows-server/backup-and-storage/return-codes-used-robocopy-utility>)

Beispiele:

```xml
<Batch Name="IIS neu starten" Cmd="iisreset"/>
<Batch Name="IIS beenden" Cmd="NET" Args="STOP W3SVC"/>
<Batch Name="IIS starten" Cmd="NET" Args="START W3SVC"/>

<!-- MyProgram.exe Param1 "Param2 mit Leerzeichen" C:\temp\export -->
<Batch Name="Individuelle Aktion" Cmd="MyProgram.exe" Args="Param1">
    <Args>
        <Arg>Param2 mit Leerzeichen</Arg>
        <Arg>{ExportDir}</Arg>
    </Args>
</Batch>
```

### Knoten `<MD>`

Legt ein neues Verzeichnis an.

Attribute:

* **Name**: optional

* **Dir**: erforderlich - Der anzulegende Ordner

Beispiel

```xml
<MD Dir="{ExportDir}"/>
```

### Knoten `<RD>`

Löscht ein Verzeichnis samt ihrem Inhalt.

Attribute:

* **Name**: optional

* **Dir**: erforderlich - Der zu löschende Ordner

Beispiel

```xml
<RD Dir="{ExportDir}"/>
```

## FSConsole-Aktionen

Kern dieser Routine ist die Arbeit mit der **FSConsole.exe**.
Dazu werden folgende Aktionen angeboten.

### Knoten `<FSConsole>`

Startet eine FSConsole.exe

Attribute

* **Name**: optional

* **FS**: erforderlich - gibt die Version von Framework-Studio an.
  "3.8.0.0", "3.7.0.0", ...

* **Rep**: erforderlich - gibt das Repository an, mit dem gearbeitet werden soll

* **Args**: erforderlich - gibt die weiteren Kommandozeilen-Parameter an.

  Wie beim [Konten `<Batch>`](#konten-batch) können auch Args in Form eines XML-Knoten angegeben werden.

### Knoten `<CompileRun>`

Führt mithilfe der FSConsole.exe einen Compile-Run durch

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben)

* **Rep**: erforderlich (siehe oben)

* **Run**: erforderlich - Gibt den Compile-Run an. Dieser kann im Package-Manager an der Package-Version gepflegt werden.

* **MaxParallel**: optional - (ab FS 3.6) gibt an, wie viele Compiler innerhalb des CompileRuns parallel laufen dürfen.

Beispiel:

```xml
<!--Führt den CompileRun "1" für Framework-Studio 3.8 aus.-->
<CompileRun FS="3.8.0.0" Rep="{rep1}" Run="1"/>
```

### Knoten `<Export>`

Exportiert ein Package. Dieses wird in dem Ordner abgelegt, der im Parameter **ExportDir** angegeben wird.
Der Name der Datei setzt sich aus dem Namen des Packages und der Version zusammen.

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben)

* **Rep**: erforderlich (siehe oben)

* **Package**: erforderlich - gibt den Namen des zu exportierenden Packages an

* **Version**: erforderlich - gibt den Namen der zu exportierenden Package-Version an

* **Mode**: optional - wenn ein Bugfix / ServiceRelease exportiert werden soll, muss `Mode="Bugfix"` angegeben werden

* **Queue**: optional - Der Name der Import-Queue, mit der dieser Export wieder importiert werden soll. Siehe auch Knoten `<ImportQueue>`

* **Dir**: optional - der Ordner, in dem die Export-Dateien abgelegt werden sollen, falls dieser vom Parameter **{ExportDir}** abweicht.

* **ExportFileName**: optional - der Dateiname ohne Dateiendung, der für die Export-Datei verwendet werden soll.
  Wird dieses Attribut nicht angegeben, dann wird ein Dateiname nach folgendem Schema erzeugt: `{ExportFilePrefix}<Package>_<Version> (FS <fs-version>)`.
  Der Dateiname kann Parameter beinhalten.

* **IncludeBasePackages**: optional - gibt an ob Basis-Package in den Export eingeschlossen werden sollen.
  Mögliche Werte:
  * ServiceRelease - analog zum Package-Manager
  * Unsealed - analog zum Package-Manager
  * All - alle kompletten Base-Packages

### Konten `<Import>`

Importiert ein Package. Ordner und Dateinamen werden wie beim Export verwendet.
Die Attribute müssen zum Knoten `<Export>` passen.
Bei der Verwendung der `<ImportQueue>` wird diese Aktion automatisch von der PBE.exe erzeugt.

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben)

* **Rep**: erforderlich (siehe oben)

* **Package**: erforderlich - gibt den Namen des zu exportierenden Packages an

* **Version**: erforderlich - gibt den Namen der zu exportierenden Package-Version an

* **Mode**: optional - wenn ein Bugfix / ServiceRelease exportiert werden soll, muss `Mode="Bugfix"` angegeben werden

* **Dir**: optional - der Ordner, aus dem die Export-Dateien gelesen werden sollen, falls dieser vom Parameter **{ExportDir}** abweicht.

### Knoten `<ApprovedExport>`

Arbeitet wie der Konten `<Export>` mit folgenden Unterschieden:

* Wenn nicht über das Attribut Dir anders definiert, dann wird für den Export-Ordner der Parameter **{ApprovedExportDir}** verwendet.

* Das Attribut **Queue** steht nicht zur Verfügung.

### Konten `<ApprovedImport>`

Importiert alle im Ordner enthaltenen Dateien in das angegebene Repository.
Nach erfolgreichem Import werden die Dateien in das **{HistoryDir}** verschoben.
Wenn dieser Ordner nicht angegeben ist – weder als Parameter noch als Attribut – dann werden die Dateien nach dem Import gelöscht, damit sie beim nächsten Lauf nicht noch einmal importiert werden.
Bei der Verarbeitung wird für jeden Import ein separater **Import**-Vorgang protokolliert.

Die FS-Version wird aus dem Dateinamen ermittelt. Dieser muss z.B. so aussehen: `2014-11-10_FSDemo_3.9 (`**`FS 3.9`**`).db`.
Beim Export wird bereits ein passender Dateiname erzeugt.
Dieser sollte nicht verändert werden.

Attribute:

* **Name**: optional

* **Rep**: erforderlich (siehe oben)

* **Dir**: optional - der Ordner, aus dem die zu importierenden Dateien gelesen werden sollen, falls dieser vom Parameter **{ApprovedImportDir}** abweicht.

* **HistoryDir**: optional - der Ordner, in den die Dateien nach erfolgreichen Import verschoben werden sollen, falls dieser vom Parameter **{ApprovedHistoryDir}** abweicht.

### Knoten `<Publish>`

Führt einen Publish-Vorgang aus.

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben)

* **Rep**: erforderlich (siehe oben)

* **Package**: erforderlich - gibt den Namen des zu exportierenden Packages an

* **Version**: erforderlich - gibt den Namen der zu exportierenden Package-Version an

* **Setting**: erforderlich - Name des Settings. Dieses muss im Publish-Wizard vorbereitet und abgespeichert werden.

### Knoten `<Publish2Go>`

Führt einen Publish2Go-Vorgang aus.

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben) (ab FS 3.11.8)

* **Rep**: erforderlich (siehe oben)

* **Package**: erforderlich - gibt den Namen des zu exportierenden Packages an

* **Version**: erforderlich - gibt den Namen der zu exportierenden Package-Version an

* **Setting**: erforderlich - Name des Settings.
  Dieses muss im Publish-Wizard vorbereitet und abgespeichert werden.
  Im Setting muss der Folder angegeben sein
  Dieser darf für den Publish2Go keinen Inhalt haben.

### Knoten `<ExportDoc>`

Exportiert die komplette Dokumentation der Package-Version im HTML-Format.

Attribute:

* **Name**: optional

* **FS**: erforderlich (siehe oben) (ab FS 3.11.8)

* **Rep**: erforderlich (siehe oben)

* **Package**: erforderlich - gibt den Namen des zu exportierenden Packages an

* **Version**: erforderlich - gibt den Namen der zu exportierenden Package-Version an

* **Iso**: erforderlich - der Iso-Code der zu exportierenden Sprache – z.B. „de“ oder „en“

* **Dir**: optional - der Ordner, in dem die Dokumentation abgelegt werden soll, falls dieser vom Parameter **{ExportDir}** abweicht.

  Für den Export wird ein Unter-Ordner mit dem folgenden Format erzeugt:
`{ExportFilePrefix}_<Package>_<Version>_Help_<Iso>`

  Beispiel: `ExportDir\2016-09-17_eNVenta_3.7_Help_de\...`

* **ExportDBTables**: optional - sollen die Tabellen-Beschreibungen exportiert werden, dann muss `ExportDBTables="1"` angegeben werden.

* **UseLicense**: optional - wenn dieser Parameter(`UseLicense="1"`) gesetzt ist, wird die Runtime-Lizenz aus dem Setting verwendet, welches via `Setting="SETTING_NAME"` übergeben wird.
  In der Folge wird nur der Teil der Dokumentation exportiert, der mit der Runtime-Lizenz sichtbar ist.

* **Setting**: erforderlich, wenn `UseLicense="1"` gesetzt ist - Name des Settings.
  Dieses muss im Run-Wizard vorbereitet und abgespeichert werden.

* **Args**: optional - Weitere Kommandozeilen-Argumente, die zusätzlich übergeben werden sollen.
  So können zukünftige Features integriert werden.

  Wie beim [Konten `<Batch>`](#konten-batch) können auch Args in Form eines XML-Knoten angegeben werden.
