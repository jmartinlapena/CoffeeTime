# CoffeeTime

CoffeeTime is a lightweight Windows application that runs in the background and adds a system tray icon to prevent your computer from going to sleep.

It is designed to start automatically with Windows (e.g., by placing it in `shell:startup`) and be enabled only when you need to keep your system awake.

## Features

* Tray icon with visual state.
* Option to prevent system sleep.
* Option to keep the screen on.
* Easy integration with Windows startup.
* Silent execution, no main window.

## Requirements

* Windows 10 or later.
* .NET 8 SDK to build the project.

## Usage

1. Build the application in `Release` mode.
2. Copy the generated executable to your preferred location.
3. Open `shell:startup` from the Run dialog to access the Windows startup folder.
4. Create a shortcut to the executable in that folder if you want it to start with your session.
5. Launch the application from the shortcut.
6. Use the tray icon to:

   * `Prevent Sleep`
   * `Keep Screen On`
   * `Exit`

## Behavior

* When `Prevent Sleep` is enabled, the application requests Windows to prevent the system from sleeping.
* When `Keep Screen On` is enabled, it also prevents the display from turning off.
* Disabling both options restores the default system behavior.

## Build

```powershell
dotnet build -c Release
```

## Structure

* `Program.cs`: main application logic and tray menu.
* `AwakeTray.csproj`: project configuration.
* `Assets/`: application icons.

## Notes

* The application does not modify Windows global power settings.
* Behavior is only active while the app is running.
