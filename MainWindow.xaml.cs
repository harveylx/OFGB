using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Security.Principal;

namespace OFGB;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [LibraryImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
    internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, [In] int[] attrValue, int attrSize);

    private const string CurVer = @"Software\Microsoft\Windows\CurrentVersion\";

    private static readonly List<RegistryEntry> Cb1RegistryEntries = new()
    {
        new RegistryEntry(CurVer + @"Explorer\Advanced", "ShowSyncProviderNotifications")
    };
    private static readonly List<RegistryEntry> Cb2RegistryEntries = new()
    {
        new RegistryEntry(CurVer + "ContentDeliveryManager", "RotatingLockScreenOverlayEnabled"),
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-338387Enabled")
    };
    private static readonly List<RegistryEntry> Cb3RegistryEntries = new ()
    {
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-338393Enabled"),
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-353694Enabled"),
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-353696Enabled")
    };
    private static readonly List<RegistryEntry> Cb4RegistryEntries = new ()
    {
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-338389Enabled")
    };
    private static readonly List<RegistryEntry> Cb5RegistryEntries = new ()
    {
        new RegistryEntry(CurVer + "UserProfileEngagement", "ScoobeSystemSettingEnabled")
    };
    private static readonly List<RegistryEntry> Cb6RegistryEntries = new()
    {
        new RegistryEntry(CurVer + "ContentDeliveryManager", "SubscribedContent-310093Enabled")
    };
    private static readonly List<RegistryEntry> Cb7RegistryEntries = new()
    {
        new RegistryEntry(CurVer + "AdvertisingInfo", "Enabled")
    };
    private static readonly List<RegistryEntry> Cb8RegistryEntries = new()
    {
        new RegistryEntry(CurVer + "Privacy", "TailoredExperiencesWithDiagnosticDataEnabled")
    };
    private static readonly List<RegistryEntry> Cb9RegistryEntries = new()
    {
        new RegistryEntry(CurVer + @"Explorer\Advanced", "Start_IrisRecommendations")
    };
    private static readonly List<RegistryEntry> Cb10RegistryEntries = new()
    {
        new RegistryEntry(CurVer + @"Notifications\Settings\Windows.ActionCenter.SmartOptOut", "Enabled")
    };
    private static readonly List<RegistryEntry> Cb11RegistryEntries = new()
    {
        new RegistryEntry(@"Software\Policies\Microsoft\Windows\Explorer\", "DisableSearchBoxSuggestions", true),
        new RegistryEntry(CurVer + "Search", "BingSearchEnabled")
    };
    private static readonly List<RegistryEntry> Cb12RegistryEntries = new()
    {
        new RegistryEntry(@"Software\Policies\Microsoft\Edge", "WebWidgetAllowed")
    };

    public MainWindow()
    {
        InitializeComponent();
        InitialiseKeys();
        SetWindowAttribute();
    }

    private static void SetWindowAttribute()
    {
        DwmSetWindowAttribute(new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle(), 33, new int[2], sizeof(int));
    }

    private void InitialiseKeys()
    {
        // Sync provider notifications in File Explorer
        SetCheckBoxState(cb1, Cb1RegistryEntries);

        // Get fun facts, tips, tricks, and more on your lock screen
        SetCheckBoxState(cb2, Cb2RegistryEntries);

        // Show suggested content in Settings app
        SetCheckBoxState(cb3, Cb3RegistryEntries);

        // Get tips and suggestions when using Windows
        SetCheckBoxState(cb4, Cb4RegistryEntries);

        // Suggest ways to get the most out of Windows and finish setting up this device
        SetCheckBoxState(cb5, Cb5RegistryEntries);

        // Show me the Windows welcome experience after updates and occasionally when I sign in to highlight what's new and suggested
        SetCheckBoxState(cb6, Cb6RegistryEntries);

        // Let apps show me personalized ads by using my advertising ID
        SetCheckBoxState(cb7, Cb7RegistryEntries);

        // Tailored experiences
        SetCheckBoxState(cb8, Cb8RegistryEntries);

        // "Show recommendations for tips, shortcuts, new apps, and more" on Start
        SetCheckBoxState(cb9, Cb9RegistryEntries);

        // "Turn off notifications from <app>? We noticed you haven't opened these in a while."
        SetCheckBoxState(cb10, Cb10RegistryEntries);

        // These Need To Be Run As Administrator
        if (IsRunningAsAdministrator())
        {
            // Show Bing Results in Windows Search (Inverted, 1 == Disabled)
            SetCheckBoxState(cb11, Cb11RegistryEntries);

            // Disable Edge desktop search widget bar
            SetCheckBoxState(cb12, Cb12RegistryEntries);
        }
        else
        {
            cb11.IsEnabled = false;
            cb12.IsEnabled = false;
        }
    }

    private static void SetCheckBoxState(CheckBox checkBox, IEnumerable<RegistryEntry> registryEntries)
    {
        var keyDisabled = true;
        foreach (var registryEntry in registryEntries)
        {
            keyDisabled &= IsKeyDisabled(registryEntry);
        }
        checkBox.IsChecked = keyDisabled;
    }

    private static bool IsKeyDisabled(RegistryEntry registryEntry)
    {
        using var keyRef = Registry.CurrentUser.OpenSubKey(registryEntry.KeyPath, true) ?? Registry.CurrentUser.CreateSubKey(registryEntry.KeyPath);
        if (keyRef == null)
        {
            MessageBox.Show("Failed to create a registry subkey during initialization!", "OFGB: Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidOperationException("OFGB: Failed to create subkey during initialization!");
        }

        var value = Convert.ToInt32(keyRef.GetValue(registryEntry.KeyName, 0));
        return registryEntry.ValueInverted ? value != 0 : value == 0;
    }

    private static void ToggleOptions(string checkboxName, bool enable)
    {
        switch (checkboxName)
        {
            case "cb1":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "Explorer\\Advanced\\", "ShowSyncProviderNotifications", Convert.ToInt32(!enable));
                break;
            case "cb2":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", Convert.ToInt32(!enable));
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-338387Enabled", Convert.ToInt32(!enable));
                break;
            case "cb3":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-338393Enabled", Convert.ToInt32(!enable));
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-353694Enabled", Convert.ToInt32(!enable));
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-353696Enabled", Convert.ToInt32(!enable));
                break;
            case "cb4":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-338389Enabled", Convert.ToInt32(!enable));
                break;
            case "cb5":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "UserProfileEngagement", "ScoobeSystemSettingEnabled", Convert.ToInt32(!enable));
                break;
            case "cb6":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "ContentDeliveryManager", "SubscribedContent-310093Enabled", Convert.ToInt32(!enable));
                break;
            case "cb7":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "AdvertisingInfo", "Enabled", Convert.ToInt32(!enable));
                break;
            case "cb8":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", Convert.ToInt32(!enable));
                break;
            case "cb9":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "Explorer\\Advanced", "Start_IrisRecommendations", Convert.ToInt32(!enable));
                break;
            case "cb10":
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "Notifications\\Settings\\Windows.ActionCenter.SmartOptOut", "Enabled", Convert.ToInt32(!enable));
                break;
            case "cb11":
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Windows\\Explorer", "DisableSearchBoxSuggestions", Convert.ToInt32(enable)); // <- Inverted
                Registry.SetValue("HKEY_CURRENT_USER\\" + CurVer + "Search", "BingSearchEnabled", Convert.ToInt32(!enable));
                break;
            case "cb12":
                Registry.SetValue("HKEY_CURRENT_USER\\Software\\Policies\\Microsoft\\Edge", "WebWidgetAllowed", Convert.ToInt32(!enable));
                break;
        }
    }

    public static bool IsRunningAsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private void Checked(object sender, RoutedEventArgs e)
    {
        ToggleOptions(((CheckBox)sender).Name, true);
    }

    private void Unchecked(object sender, RoutedEventArgs e)
    {
        ToggleOptions(((CheckBox)sender).Name, false);
    }

    private void Close(object sender, RoutedEventArgs e)
    {
        Close();
    }
}