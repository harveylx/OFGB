using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Security.Principal;
using System.Windows.Input;

namespace OFGB;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    [LibraryImport("dwmapi.dll", EntryPoint = "DwmSetWindowAttribute")]
    internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int attr, [In] int[] attrValue, int attrSize);

    private const string CurVer = @"Software\Microsoft\Windows\CurrentVersion\";
    private static readonly string[] Cb1KeyNames = { "ShowSyncProviderNotifications" };
    private static readonly string[] Cb2KeyNames = { "RotatingLockScreenOverlayEnabled", "SubscribedContent-338387Enabled" };
    private static readonly string[] Cb3KeyNames = { "SubscribedContent-338393Enabled", "SubscribedContent-353694Enabled", "SubscribedContent-353696Enabled" };
    private static readonly string[] Cb4KeyNames = { "SubscribedContent-338389Enabled" };
    private static readonly string[] Cb5KeyNames = { "ScoobeSystemSettingEnabled" };
    private static readonly string[] Cb6KeyNames = { "SubscribedContent-310093Enabled" };
    private static readonly string[] Cb7KeyNames = { "Enabled" };
    private static readonly string[] Cb8KeyNames = { "TailoredExperiencesWithDiagnosticDataEnabled" };
    private static readonly string[] Cb9KeyNames = { "Start_IrisRecommendations" };
    private static readonly string[] Cb10KeyNames = { "Enabled" };

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
        SetCheckBoxState(cb1, CurVer + @"Explorer\Advanced", Cb1KeyNames);

        // Get fun facts, tips, tricks, and more on your lock screen
        SetCheckBoxState(cb2, CurVer + "ContentDeliveryManager", Cb2KeyNames);

        // Show suggested content in Settings app
        SetCheckBoxState(cb3, CurVer + "ContentDeliveryManager", Cb3KeyNames);

        // Get tips and suggestions when using Windows
        SetCheckBoxState(cb4, CurVer + "ContentDeliveryManager", Cb4KeyNames);

        // Suggest ways to get the most out of Windows and finish setting up this device
        SetCheckBoxState(cb5, CurVer + "UserProfileEngagement", Cb5KeyNames);

        // Show me the Windows welcome experience after updates and occasionally when I sign in to highlight what's new and suggested
        SetCheckBoxState(cb6, CurVer + "ContentDeliveryManager", Cb6KeyNames);

        // Let apps show me personalized ads by using my advertising ID
        SetCheckBoxState(cb7, CurVer + "AdvertisingInfo", Cb7KeyNames);

        // Tailored experiences
        SetCheckBoxState(cb8, CurVer + "Privacy", Cb8KeyNames);

        // "Show recommendations for tips, shortcuts, new apps, and more" on Start
        SetCheckBoxState(cb9, CurVer + @"Explorer\Advanced", Cb9KeyNames);

        // "Turn off notifications from <app>? We noticed you haven't opened these in a while."
        SetCheckBoxState(cb10, CurVer + @"Notifications\Settings\Windows.ActionCenter.SmartOptOut", Cb10KeyNames);

        // These Need To Be Run As Administrator
        if (IsRunningAsAdministrator())
        {
            // Show Bing Results in Windows Search (Inverted, 1 == Disabled)
            bool key14 = CreateKey("Software\\Policies\\Microsoft\\Windows\\Explorer", "DisableSearchBoxSuggestions");
            bool key15 = CreateKey(CurVer + "Search", "BingSearchEnabled");
            cb11.IsChecked = !key14 && key15;

            // Disable Edge desktop search widget bar
            bool key16 = CreateKey("Software\\Policies\\Microsoft\\Edge", "WebWidgetAllowed");
            cb12.IsChecked = key16;
        }
        else
        {
            cb11.IsEnabled = false;
            cb12.IsEnabled = false;
        }
    }

    private static void SetCheckBoxState(CheckBox checkBox, string keyPath, IEnumerable<string> keyNames, bool keyValueInverted = false)
    {
        var keyState = true;
        foreach (var keyName in keyNames)
        {
            keyState &= GetKeyState(keyPath, keyName);
        }
        checkBox.IsChecked = keyState;
    }

    private static bool GetKeyState(string keyPath, string keyName, bool keyValueInverted = false)
    {
        using var keyRef = Registry.CurrentUser.OpenSubKey(keyPath, true) ?? Registry.CurrentUser.CreateSubKey(keyPath);
        if (keyRef == null)
        {
            MessageBox.Show("Failed to create a registry subkey during initialization!", "OFGB: Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidOperationException("OFGB: Failed to create subkey during initialization!");
        }

        var value = Convert.ToInt32(keyRef.GetValue(keyName, 0));
        return keyValueInverted ? value != 0 : value == 0;
    }

    private static bool CreateKey(string loc, string key)
    {
        RegistryKey? keyRef;
        int value;

        if (Registry.CurrentUser.OpenSubKey(loc, true) is not null)
        {
            keyRef = Registry.CurrentUser.OpenSubKey(loc, true);
        }
        else
        {
            keyRef = Registry.CurrentUser.CreateSubKey(loc);
            keyRef.SetValue(key, 0);
        }

        if (keyRef is null)
        {
            MessageBox.Show("Failed to create a registry subkey during initialization!", "OFGB: Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new InvalidOperationException("OFGB: Failed to create subkey during initialization!");
        }

        value = Convert.ToInt32(keyRef.GetValue(key));
        keyRef.Close();

        return !(value != 0);
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