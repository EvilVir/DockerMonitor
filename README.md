# Docker Monitor
Purpose of this tool is to constantly monitor Docker containers and react when they state changes.

## Example use case
Some of your containers aren't coming back after machine restart or some of them are sometimes quiting. You can install this tool so it will check each couple of seconds if containers are up and if it will detect that they aren't it will run defined commands (so you can bring them back automatically, report the problem etc.).

## Usage
Tool requires JSON configuration file. By default it's `DockerMonitor.json` placed in same folder where tool's executable is but you can override that with `-config` command line argument.

## Configuration
You can find example configuration [here](https://github.com/EvilVir/DockerMonitor/blob/master/Docker%20Monitor/Config.example.json).

Configuration sections:

### Logging
You can log to stdout/console with `Console` logger or to Windows Events Log with `EventLog` logger. Possible logging levels:

* Trace
* Debug
* Information
* Warning
* Error
* Critical

### Docker
Standard way of connecting to Docker's engine is via pipes but you can also use [other means like TCP](https://docs.docker.com/engine/reference/commandline/dockerd/). You need to put address to the Docker's API endpoint in `Url` configuration property. You can also use either `BasicAuthorization` property or `CertificateAuthorization` property to add your authorization credentials.

#### BasicAuthorization configuration
```
{
    "Username" : "",
    "Password" : "",
    "UseTls" true/false
}
```

#### CertificateAuthorization configuration
```
{
    "File": "",
    "Password": ""
}
```

### Groups
This node defines all monitoring groups. Group is well-named set of containers that are monitored as one entity. If any of containers in group falls into certain state, whole group falls. Here's properties breakdown:

* Interval - how often (in seconds) group will be checked.
* Containers - array of container names to be monitored, exactly as they are named in your Docker.
* Actions - dictionary of states and actions to be taken if certain state. As keys of this dictionary you put comma-separated list of states, as value there should be a list of actions to be taken.

For example:

```
 "Nginx": {
      "Interval": 60,
      "Containers": [ "nginx", "nginx2" ],
      "Actions": {
        "AnyDown,AnyMissing": [
          {
            "Type": "ShellCommand",
            "Command": "powershell",
            "Arguments": "docker rm nginx",
            "ErrorDetecting": "ExitCode,StdErr"
          },
          {
            "Type": "ShellCommand",
            "Command": "powershell",
            "Arguments": "docker run -d --name \"nginx\" -p 8080:80 nginx:latest",
            "ErrorDetecting": "Ignore",
            "WorkingDirectory": "C:\\"
          }
        ]
      }
    }
```

This "Nginx" group will be monitored each `60` seconds. If any of two containers `nginx` or `nginx2` are either down or missing two `ShellCommand` actions will be executed in order they are configured above.

Possible states are:

* AnyDown - any container in group is stopped
* AnyUp - any container in group is running
* AnyMissing - any container in group doesn't exists
* AllDown - all containers in group are stopped
* AllUp - all containers in group are running
* AllMissing - all containers in group doesn't exists

You can mix and match these states, actions are executed if _any_ state is detected. So in example above these two shell commands will run when group will be in either `AnyDown` _or_ `AnyMissing` state.

## Running one-time
You can run this tool in one-time-check mode straight from CLI.

```
 DockerMonitor.exe [-config value] [-checknow value]
  - config   : Path to configuration file (string, default=DockerMonitor.json)
  - checknow : Will just check group, which name you need to pass here, and then exit without staying in background (string, default=)
```

## Running as a background service

### Windows
You can use Windows Service wrappers like great [WinSW](https://github.com/kohsuke/winsw) to setup your service wrapper. Here's example configuration for WinSW:

```
<configuration>
  <id>dockermonitor</id>
  <name>Docker Monitor</name>
  <description>Docker Monitor Service</description>
  <executable>%BASE%\bin\DockerMonitor.exe</executable>
  <workingdirectory>%BASE%\bin</workingdirectory>
</configuration>
```

Assuming following folders layout:

```
    |- bin <- folder where you should put all Docker Monitor's files
        |-- ..
        |-- DockerMonitor.exe <-- actual Docker Monitor's executable
        |-- DockerMonitor.json <-- actual Docker Monitor's configuration
        |-- *.* <-- other Docker Monitor's files
    |- DockerMonitor.exe <- copy of WinSW executable renamed
    |- DockerMonitor.xml <- configuration file shown above
```

For instructions on managing that service, check [WinSW repository here](https://github.com/kohsuke/winsw).

### Linux
Actual daemonization depends on your distribution. You can use `system.d` for example.

