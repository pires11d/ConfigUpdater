# ConfigUpdater (Continuous Deployment Tool)

A tool that updates the configuration files in published applications, without overwriting existing settings.

### Supported file types for merging:
- *.exe.config
- appsettings.json
- web.config

### Usage:

#### 1) Merge files (only supported file types)

~~~ps1
ConfigUpdater.exe "path1" "path2"
~~~

`"path1"` = Source folder or file path

> Example: `"C:\Path\To\SourceFolder\appsettings.json"`

`"path2"` = Target folder or file path

> Example: `"C:\Path\To\TargetFolder\appsettings.json"`

<br>

#### 2) Replace text (any file type)

~~~ps1
ConfigUpdater.exe "path" "oldValue" "newValue"
~~~

`"path"` = File path

> Example: `"C:\Path\To\main.js"`

`"oldValue"` = Value to look for

> Example: `"http://localhost:9000"`

`"newValue"` = Value to be replaced by

> Example: `"https://mywebsite.mydomain.com"`

