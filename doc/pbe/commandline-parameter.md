# PBE - Kommandozeilen-Parameter

## -a / --auto

Automatischer Mode.

Zur Absicherung gegen versehentliches Ausführen verlangt die PBE.exe beim Start eine Bestätigung durch den Benutzer.

Wird dieser Parameter angegeben, erfolgt die Ausführung ohne Rückfrage.
Bei einer automatisierten Ausführung (z.B. über einen geplanten Task) muss dieser Parmater angegeben werden.

Beispiel:

```cmd
PBE.exe -a
PBE.exe --auto
```

## -c / --config

Definiert die [Konfigurations-Datei](config.md).

Der Paramter is optional. Wird der nicht angegeben, wird die Datei `PBE.xml` im Programm-Verzeichnis verwendet.

Beispiel:

```cmd
PBE.exe -c C:\temp\MyPBE.xml
PBE.exe --config C:\temp\MyPBE.xml
```

## -p / --param

Definiert Parameter-Werte, die in die Routine übergeben werden.
Diese werden in der Konfiguration bei den [Parametern](config.md#parameter) berücksichtigt.

Die hier angegebenen Parameter-Werte haben eine höhere Priorität als die Konfigurations-Datei.
Sie Überschreiben auch die Standard-Parameter wie z.B. `Weekday` oder `Title`.

> [!IMPORTANT]
> Es können nur Parameter "überschrieben" werden, die in der Konfiguration definiert sind oder als Standard-Parameter exisiteren.
> Frei definierte Parameter werden NICHT berücksichtigt.

Die Paramater werden in folgendem Format übergeben:

* Parameter und Wert werden mit `=` getrennt: `--param Param1=Wert1`
* Mehrere Parameter werden mit einem Semikolon (`;`) getrennt: `--param Param1=Wert1;Param2=Wert2`
* Enthält ein Parameter-Wert ein Semikolon, dann muss dieses doppelt geschrieben werden: `--param MyValue=Ein;;Semikolon`

Beispiel (einen Wochenend-Lauf simulieren):

```cmd
PBE.exe -p Weekday=Sa;ExportDir=C:\temp\TestExport
PBE.exe --param Weekday=Sa;ExportDir=C:\temp\TestExport
```

## -l / --logDir

Gibt das Verzeichnis an, in dem die Log-Dateien geschrieben werden sollen.

Beispiel:

```cmd
PBE.exe -c C:\temp\MyPBE.xml -l C:\temp\LogFiles
PBE.exe --config C:\temp\MyPBE.xml --logDir C:\temp\LogFiles
```

## -f / --filter

Es werden die Namen der Tasks angegeben, die ausgeführt werden sollen.
Es können auch Tasks in einer tieferen Ebene angegeben werden.
Die Umschließenden Tasks werden automatisch mit in die Ausführung eingeschlossen.

Mehrere Filter werden mit einem Doppelpunkt (`:`) getrennt.

Beispiel:

```cmd
PBE.exe -f Task1:Task2
PBE.exe --filter Task1:Task2
```
