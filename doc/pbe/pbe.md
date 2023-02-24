# Parallel Batch Engine (PBE)

Die **P**arallel **B**atch **E**ngine ist eine Routine, die es ermöglicht eine Batch-Verarbeitung zu automatisieren. Neben der Ausführung normaler Batch-Verarbeitung ist sie dafür optimiert, die FSConsole.exe von Framework Studio zu steuern und so Compile-Läufe, Package-Exporte, Package-Exporte und Publish-Aktionen auszuführen. Weitere Kern-Funktionen sind die parallele Verarbeitung und das erzeugen und Archivieren einer Log-Datei.

## Arbeitsweise

Ausgeliefert wird diese Routine zusammen mit Framework Studio. Sie ist im Programm-Verzeichnis von Framework Studio im Unterordner PBE abgelegt.

Das Programm besteht aus folgenden Dateien:

* PBE.exe - Das eigentliche Programm
* PBE.xml - hier wird die Konfiguration vorgenommen
* PBE.dtd - beinhaltet die Schema-Definition für die PBE.xml
* LogTemplate.htm - Vorlage für die Protokoll-Datei

Wenn die Konfiguration in der Datei PBE.xml vorgenommen wurde, dann kann die Routine durch ausführen der Datei PBE.exe gestartet werden. Es wird die XML-Datei ausgewertet und die darin konfigurierten Schritte werden alle abgearbeitet. Dabei wird ein Protokoll in Form einer HTML-Datei erzeugt. Dieses beinhaltet neben den Informationen, wann welche Aktion gestartet wurde und wie lange sie gedauert hat, auch das während der jeweiligen Aktion erzeugte Protokoll. Das Protokoll wird während der Ausführung regelmäßig aktualisiert. So kann man sich auch ein Bild vom Fortschritt der Aktion machen.

PBE ist dafür konzipiert in einem Lauf mit mehreren Versionen von Framework Studio parallel zu arbeiten. Unterstützt werden alle Versionen ab Framework Studio 3.2.

## Protokoll

Die Routine erzeugt ein Protokoll.
In dem Haupt-Knoten `<PBE>` kann eingestellt werden, wohin das Protokoll geschrieben werden soll.
In jeder Protokoll-Datei wird ein Link *Previous Logfile* erzeugt, in dem jeweils auf das vorherige "archivierte" Protokoll verwiesen wird.
So kann man sich einfach durch die Protokolle klicken.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!DOCTYPE PBE SYSTEM "PBE.dtd">
<PBE Logfile="U:\eNVenta\Nacht\Compile\Log.html"
     Logarchive="U:\eNVenta\Nacht\Compile\Archiv\{DateTime}.html">
  <Params>
    <Param Name="ExportDir" Value="C:\PackageExport\"/>
```

* **Logfile**: gibt den Pfad der Protokoll-Datei an.
  In Netzwerk-Umgebungen macht es Sinn, das Protokoll auf einem Netzlaufwerk abzulegen, so kann sie z.B. einfach über eine interne Web-Seite verlinkt werden.

* **Logarchive**: gibt an, wo die Historischen Protokolle gespeichert werden sollen.
  Dabei ist es sinnvoll den Parameter **{DateTime}** in den Dateinamen einzubauen, um einen eindeutigen Namen zu erhalten.

![Protokoll](media/protokoll.png)
