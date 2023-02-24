# Konfiguration der PBE

Die Konfiguration erfolgt mithilfe der Datei **PBE.xml**. Diese wird am besten mit Visual Studio bearbeitet, weil es dort eine IntelliSense-Unterstützung und eine Validierung der Eingaben gibt.

## Programm-Verzeichnisse

Für die unterschiedlichen FS-Versionen werden die Programm-Verzeichnisse ermittelt. Dabei geht die Routine für jede FS-Version mit den folgenden Prioritäten vor und verwendet das Verzeichnis, welches zuerst gefunden wird:

1. Konfiguration in der XML-Datei

    Beispiel:

    ```xml
    <FSVersions>
        <FSVersion FS="3.7.0.0" Dir="C:\Programme\Framework Systems\FrameworkStudio 3.7.3"/>
        <FSVersion FS="3.8.0.0" Dir="C:\Programme\Framework Systems\FrameworkStudio 3.8"/>
    </FSVersions>
    ```

2. Order `C:\FS\Framework Studio X.Y.0.0\` - das ist der Standard im Haus von Nissen & Velten.

3. Standard Installations-Verzeichnis `%ProgramFiles%\Framework Systems\Framework Studio X.Y`

    Dabei werden auch ServiceRelease- und Beta-Versionen erkannt.

## Parameter

Für eine einfachere Konfiguration wird mit Parametern gearbeitet. Diese werden am Anfang der Datei angegeben. Unterhalb des XML-Knoten `<Params>` werden die Parameter mit `<Param>`-Knoten aufgelistet. Diese Parameter können bei den Aktionen verwendet werden. Dazu müssen sie in geschweiften Klammern geschrieben werden: `{parameter}`

Beispiel:

```xml
<Params>
    <Param Name="ExportDir" Value="E:\temp\export123\"/>
    <Param Name="rep" Value="\ConnectionType SqlServer \Server NV261 \Database FSDemo37 \DBUser sa \DBPassword sql2005"/>
</Params>

<Sequence>
    <CompileRun FS="3.7.0.0" Rep="{rep}" Run="1"/>
</Sequence>
```

Die im Folgenden aufgelisteten Parameter sind immer gefüllt:

* `ExportDir` Beinhaltet einen Ordner, in dem die Dateien der Package-Exporte abgelegt werden. Die Package-Importe werden ebenfalls in diesen Ordner gesucht. Dieser Parameter wird bei den Aktionen `<Export>` und `<Import>` automatisch verwendet.

* `ApprovedExportDir` Beinhaltet einen Ordner, in dem die Dateien der Approved Package-Exporte abgelegt werden.
Dieser Parameter wird bei der Aktion `<ApprovedExport>` automatisch verwendet.

* `ApprovedImportDir` Beinhaltet einen Ordner, in dem die Dateien abgelegt werden, die mit der Aktion `<ApprovedImport>` verbarbeitet werden sollen.

* `ApprovedHistoryDir` Beinhaltet einen Ordner, in den die Dateien verschoben werden, nachdem sie mit der Aktion <ApprovedImport> verbarbeitet wurden.

* `Date` Start-Datum im Format "yyyyMMdd"

    z.B. 20140526

* `DateTime` Start-Zeit im Format "yyyyMMdd-HHmmss"
  
  z.B. 20140526-142457

* `DateTimeText` Start-Zeit im Format "yyyy-MM-dd HH:mm (dddd)"

  z.B. 2014-05-26 14:24 (Montag)

* `ExportFilePrefix` Start-Datum mit dem Format "yyyy-MM-dd_"

  z.B. 2014-05-26_

  Dieses Präfix wird vor den Dateinamen der Export-Dateien gestellt. So haben die Dateinamen dasselbe Format wie es auch der Package-Manager beim Export vorschlägt.

  z.B. 2014-05-26_eNVenta_3.4.db

* `Weekday` Aktueller Wochentag in Deutsch: Mo, Di, Mi, Do, Fr, Sa, So

  Kann z.B. in einer `<Condition>` verwendet werden um wöchentliche Aktionen zu definieren.

* `Title` Hat standardmäßig den Wert "Nachtlauf {DateTimeText}". Dieser Parameter kann bei Bedarf überdefiniert werden.

* `Machine` Name des Rechners (Environment.MachineName)

Darüber hinaus können auch eigene Parameter definiert werden. So können z.B. die verwendeten Repository-Connections an zentraler Stelle definiert werden.

Bei Parametern kann auch auf vorher definierte Parameter verwiesen werden – wie z.B. bei dem vordefinierten Parameter `Title`.

> [!TIP]
> Alle bekannten Parameter können der PBE.exe mit dem [Kommandozeilen-Parameter `-p` / `--param`](commandline-parameter.md#-p----param) übergeben und so "überschrieben" werden.

## Organisation

Die Aktionen werden in der XML-Datei als Sequenzen oder Parallel-Verarbeitungen organisiert. Die unterschiedlichen Knoten können beliebig ineinander verschachtelt werden. Einzige Ausnahme ist der oberste Knoten – dieser muss immer `<Sequence>` sein.

Jeder Knoten kann Optional ein Attribut "Name" erhalten. Dieser Name wird in der Protokoll-Datei ausgegeben.

### Konten `<Sequence>`

Alle darunter aufgeführten Aktionen werden nacheinander verarbeitet. In der Protokoll-Datei werden die Einträge untereinander ausgegeben.

Attribute:

* **Name**: optional

### Knoten `<Parallel>`

Alle darunter definierten Aktionen werden parallel verarbeitet. In der Protokoll-Datei werden die Einträge nebeneinander ausgegeben.

Attribute:

* **Name**: optional

* **MaxTasks**: optional - gibt die Anzahl der maximal parallel ausgeführten Aktionen an. Bei einer sehr langen Liste an Aktionen macht es Sinn, die Parallelität z.B. auf 4 zu begrenzen.

  > [!IMPORTANT]
  > Wenn dieses Attribut nicht angegeben ist, erfolgt die Verarbeitung komplett parallel – egal wie viele Aktionen definiert wurden.

### Konten `<Condition>`

Arbeitet wie eine `<Sequence>` - die darunter aufgeführten Aktionen werden nacheinander ausgeführt. Die Ausführung erfolgt aber nur dann, wenn die beiden Attribute "Value" und "Equals" denselben Wert haben.

Attribute:

* **Name**: optional

* **Value**: erforderlich – gibt den linken Wert für den Vergleich an. Üblicherweise wird hier ein Parameter angegeben - z.B. `"{Weekday}"`

* **Equals**: erforderlich – gibt den Wert an, mit dem verglichen werden soll – z.B. `"So"`.

Im Value kann eine Funktion `"#EXISTS(<dateipfad>)"` verwendet werden. Diese liefert den Wert `"True"` oder `"False"`. Der Dateipfad kann auch Parameter beinhalten.

Beispiel:

```xml
<Condition Name="Prüfen ob Publish2Go existiert"
        Value="#Exists({InputDir}\sqlitedb.p2go)"
        Equals="True">
```

### Knoten `<ImportQueue>`

Damit können die Package-Importe so organisiert werden, dass diese parallel zu anderen Aktionen durchgeführt werden können. Die Export-Aktionen packen, wenn sie fertig sind, entsprechende Import-Aktionen in diese Queue. Diese wird dann sofort mit der Abarbeitung beginnen. Dabei werden aber alle Importe nacheinander verarbeitet, weil parallele Importe auf einem Repository nicht möglich sind.

Attribute:

* **Name**: optional

* **QueueName**: erforderlich – gibt den Namen der Queue an. Diese kann bei einer `<Export>`-Aktion verwendet werden.

* **Rep**: erforderlich – gibt das Repository an, in dem die Packages importiert werden sollen.

Beispiel
Es laufen parallel 2 Compile-Läufe und Exporte auf Repository1. Daneben werden zeitgleich die Exporte in Repository2 importiert.

Beispiel:

```xml
<Parallel>
    <ImportQueue QueueName="ImportQueue" Rep="{rep2}"/>

    <Sequence Name="Compile-Lauf FS 3.7">
        <CompileRun FS="3.7.0.0" Rep="{rep1}" Run="1"/>
        <Export FS="3.7.0.0" Rep="{rep1}" Package="Cust1" Version="3.2" Queue="ImportQueue"/>
        <Export FS="3.7.0.0" Rep="{rep1}" Package="Cust2" Version="3.2" Queue="ImportQueue"/>
    </Sequence>

    <Sequence Name="Compile-Lauf FS 3.8">
        <CompileRun FS="3.8.0.0" Rep="{rep1}" Run="1"/>
        <Export FS="3.8.0.0" Rep="{rep1}" Package="Cust1" Version="3.3" Queue="ImportQueue"/>
        <Export FS="3.8.0.0" Rep="{rep1}" Package="Cust2" Version="3.3" Queue="ImportQueue"/>
    </Sequence>
</Parallel>
```
