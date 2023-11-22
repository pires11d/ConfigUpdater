# ConfigUpdater

A tool that updates the configuration files in published applications, without overwriting existing settings.

### Supported file types:
- *.exe.config
- appsettings.json
- web.config

### Usage:

~~~ps1
ConfigUpdater.exe "path1" "path2"
~~~

`"path1"` = Source folder or file path

> Example: `"C:\Path\To\SourceFolder\appsettings.json"`

`"path2"` = Target folder or file path 

> Example: `"C:\Path\To\TargetFolder\appsettings.json"`
