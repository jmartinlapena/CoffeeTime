using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CoffeeTime;

/*
  Main application class.

  Implements a system tray utility that allows:
  - Preventing system sleep
  - Keeping the display awake
*/
internal static class Program
{
    /*
      Windows API flags for execution state control
    */
    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    /*
      Tray UI components
    */
    private static NotifyIcon? _trayIcon;
    private static ToolStripMenuItem? _toggleSleepItem;
    private static ToolStripMenuItem? _toggleDisplayItem;
    private static System.Windows.Forms.Timer? _keepAwakeTimer;

    /*
      Icons
    */
    private static Icon? _iconOn;
    private static Icon? _iconOff;

    /*
      Internal state flags
    */
    private static bool _preventSleep = false;
    private static bool _preventDisplayOff = false;

    /*
      Import Windows API to control execution state
    */
    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    /*
      Application entry point
    */
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        LoadIcons();

        /*
          Menu item: prevent system sleep
        */
        _toggleSleepItem = new ToolStripMenuItem("Prevent Sleep")
        {
            CheckOnClick = true,
            Checked = false
        };

        _toggleSleepItem.CheckedChanged += (_, _) =>
        {
            _preventSleep = _toggleSleepItem.Checked;

            // If sleep is disabled → also disable display override
            if (!_preventSleep && _toggleDisplayItem is not null && _toggleDisplayItem.Checked)
            {
                _toggleDisplayItem.Checked = false;
            }

            ApplyState();
        };

        /*
          Menu item: keep display awake
        */
        _toggleDisplayItem = new ToolStripMenuItem("Keep Display Awake")
        {
            CheckOnClick = true,
            Checked = false
        };

        _toggleDisplayItem.CheckedChanged += (_, _) =>
        {
            _preventDisplayOff = _toggleDisplayItem.Checked;

            // Enabling display awake requires sleep prevention
            if (_preventDisplayOff && _toggleSleepItem is not null && !_toggleSleepItem.Checked)
            {
                _toggleSleepItem.Checked = true;
            }

            ApplyState();
        };

        /*
          Exit menu item
        */
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Exit();

        /*
          Tray context menu
        */
        var menu = new ContextMenuStrip();
        menu.Items.Add(_toggleSleepItem);
        menu.Items.Add(_toggleDisplayItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        /*
          Initialize tray icon
        */
        _trayIcon = new NotifyIcon
        {
            Icon = _iconOff ?? SystemIcons.Application,
            Text = "CoffeeTime: Normal mode",
            Visible = true,
            ContextMenuStrip = menu
        };

        /*
          Timer to periodically reinforce execution state
          (Windows may ignore it if not refreshed)
        */
        _keepAwakeTimer = new System.Windows.Forms.Timer
        {
            Interval = 30000
        };

        _keepAwakeTimer.Tick += (_, _) => ApplyExecutionState();

        /*
          Initial state
        */
        SetThreadExecutionState(ES_CONTINUOUS);
        UpdateTrayState();

        Application.Run();

        Exit();
    }

    /*
      Load embedded icons from assembly resources
    */
    private static void LoadIcons()
    {
        var assembly = typeof(Program).Assembly;

        using var onStream = assembly.GetManifestResourceStream("CoffeeTime.Assets.icon_on.ico");
        using var offStream = assembly.GetManifestResourceStream("CoffeeTime.Assets.icon_off.ico");

        if (onStream != null)
            _iconOn = new Icon(onStream);

        if (offStream != null)
            _iconOff = new Icon(offStream);
    }

    /*
      Apply global state changes
    */
    private static void ApplyState()
    {
        if (_preventSleep || _preventDisplayOff)
        {
            _keepAwakeTimer?.Start();
        }
        else
        {
            _keepAwakeTimer?.Stop();
        }

        ApplyExecutionState();
        UpdateTrayState();
    }

    /*
      Apply Windows execution state based on current flags
    */
    private static void ApplyExecutionState()
    {
        if (!_preventSleep && !_preventDisplayOff)
        {
            SetThreadExecutionState(ES_CONTINUOUS);
            return;
        }

        uint flags = ES_CONTINUOUS;

        if (_preventSleep)
            flags |= ES_SYSTEM_REQUIRED;

        if (_preventDisplayOff)
            flags |= ES_DISPLAY_REQUIRED;

        SetThreadExecutionState(flags);
    }

    /*
      Update tray icon and tooltip text
    */
    private static void UpdateTrayState()
    {
        if (_trayIcon == null)
            return;

        _trayIcon.Icon = (_preventSleep || _preventDisplayOff)
            ? _iconOn ?? SystemIcons.Application
            : _iconOff ?? SystemIcons.Application;

        if (_preventSleep && _preventDisplayOff)
        {
            _trayIcon.Text = "CoffeeTime: Sleep and display prevented";
        }
        else if (_preventSleep)
        {
            _trayIcon.Text = "CoffeeTime: Sleep prevented";
        }
        else if (_preventDisplayOff)
        {
            _trayIcon.Text = "CoffeeTime: Display kept awake";
        }
        else
        {
            _trayIcon.Text = "CoffeeTime: Normal mode";
        }
    }

    /*
      Cleanup and exit application
    */
    private static void Exit()
    {
        _keepAwakeTimer?.Stop();

        // Restore normal system behavior
        SetThreadExecutionState(ES_CONTINUOUS);

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        _iconOn?.Dispose();
        _iconOff?.Dispose();

        Application.Exit();
    }
}