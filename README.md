# SharpUpdate - Installable package, multiple files version
SharpUpdate is written in C#. It reads a xml file on the server that contains update information such as version, MD5, and update log, download the update, close the current app, and launch desired file.

## Multifiles
This version is capable to define multiple files to download. They are downloaded to the temporary user folder (AppData) and after that, it launches the installer previous exiting the program.

## For INSTALLERS only
Instead of original SharpUpdate, it doesn't replace the files. You need to create an installer wich must be capable to uninstall current version of your program, and then install the new one.

You can use Visual Studio Extension [Microsoft Visual Studio 2017 Installer Projects] (https://marketplace.visualstudio.com/items?itemName=VisualStudioProductTeam.MicrosoftVisualStudio2017InstallerProjects)

# Usage
## The program
SharpUpdate is an INTERFACE, so in your form you need to define like:

```csharp
public partial class frmMain : Form, ISharpUpdatable
```

And then, inside your form class:

```csharp
#region SharpUpdate
private SharpUpdater updater;
public string ApplicationName { get { return "MyDesktopApp"; } }
public string ApplicationID { get { return "MyDesktopApp"; } }
public Assembly ApplicationAssembly { get { return Assembly.GetExecutingAssembly(); } }
public Icon ApplicationIcon { get { return Icon; } }
public Uri UpdateXmlLocation { get { return new Uri("https://raw.githubusercontent.com/henryxrl/SharpUpdate/master/project.xml"); } }
public Form Context { get { return this; } }
#endregion
```

Finally, anywhere in the form where you want to check for updates:

```csharp
updater = new SharpUpdater(this);
updater.DoUpdate();
```

## The update hosting
You'll need a web/file hosting to place:

* The files to be downloaded
* The XML - [example](https://raw.githubusercontent.com/telecotxesco/SharpUpdate/master/project.xml)

The XML is like:

```xml
<?xml version="1.0"?> 
<sharpUpdate> 
    <update appID="MyDesktopApp"> 
        <version>1.1.0.0</version> 
        <files>
            <file>
                <url>https://example.com/updates/setup.exe</url> 
                <fileName>setup.exe</fileName> 
                <md5>F16F9270971EAEAE8E313B32E8FF0814</md5>
            </file>
            <file>
                <url>https://example.com/updates/Library.dll</url> 
                <fileName>Library.dll</fileName> 
                <md5>AE8E313B9270971EAE81432E8FF0F16F</md5>
            </file>
        </files>
        <description>Update: 14/03/2018:
1. Multiple files.
2. Executable file.</description> 
        <launchFile>setup.exe</launchFile> 
        <launchArgs></launchArgs> 
    </update> 
</sharpUpdate>
```

Where:

* Define a <files> node for each file you want to download.
* In the <launchFile> you define the executable.

# Original author
This is a fork from https://github.com/henryxrl/SharpUpdate

# Credit
SharpUpdate is modified from Auto Updater by BetterCoder on Youtube. His tutorial can be found here: http://goo.gl/n7btY
