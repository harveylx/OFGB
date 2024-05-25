using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Security.Principal;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Linq;

namespace OFGB;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [LibraryImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
    internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, [In] int[] attrValue, int attrSize);

    private const string CurVer = @"Software\Microsoft\Windows\CurrentVersion\";
    private readonly IConfiguration _configuration;

    public MainWindow(IConfiguration configuration)
    {
        InitializeComponent();
        _configuration = configuration;
        LoadRegistryEntries();
        SetWindowAttribute();
    }

    private void LoadRegistryEntries()
    {
        var checkboxRegistryList = new Dictionary<string, List<RegistryEntry>>();
        _configuration.GetSection("RegistryEntries").Bind(checkboxRegistryList);

        foreach (var checkbox in checkboxRegistryList)
        {
            if (FindName(checkbox.Key) is CheckBox correspondingCheckbox)
            {
                SetCheckBoxState(correspondingCheckbox, checkbox.Value);
            }
        }
    }

    private static void SetWindowAttribute()
    {
        DwmSetWindowAttribute(new WindowInteropHelper(Application.Current.MainWindow).EnsureHandle(), 33, new int[2], sizeof(int));
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
        if (registryEntry.RequiresAdminPermissions && !IsRunningAsAdministrator())
            return false;

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