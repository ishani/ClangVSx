/*
 * CLangVS - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Win32;

namespace ClangVSx
{
  /// <summary>
  /// Wrapper around registry access to load/save AddIn settings
  /// </summary>
  abstract class CVXRegistry
  {
    // root reg key 
    private const String CVXRegistryKey = "Software\\Ishani\\ClangVSx";

#region Generic registry access

    private static T LoadFromRegistry<T>(String keyName, T defaultValue)
    {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        T itemDefaultState = defaultValue;
        try
        {
          itemDefaultState = (T)key.GetValue(keyName, itemDefaultState);
        }
        finally
        {
          key.Close();
        }

        return ((itemDefaultState != null) ? itemDefaultState : defaultValue);
    }

    private static void SaveToRegistry<T>(String keyName, T saveValue)
    {
      RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
      if (key == null)
      {
        key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
      }

      try
      {
        key.SetValue(keyName, saveValue);
      }
      finally
      {
        key.Close();
      }
    }

#endregion

    public static String PathToClang
    {
      get
      {
        // sketchy way to get name of property from inside accessors
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        return LoadFromRegistry(ownerProperty, "C:\\clang\\clang.exe");
      }

      set
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        SaveToRegistry(ownerProperty, value);
      }
    }

    public static bool ShowCommands
    {
      get
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        return (LoadFromRegistry(ownerProperty, 0) == 1);
      }

      set
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        SaveToRegistry(ownerProperty, value ? 1 : 0);
      }
    }

    public static bool MakeBatchFiles
    {
      get
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        return (LoadFromRegistry(ownerProperty, 0) == 1);
      }

      set
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        SaveToRegistry(ownerProperty, value ? 1 : 0);
      }
    }

    public static String CommonArgs
    {
      get
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        return LoadFromRegistry(ownerProperty, "");
      }

      set
      {
        String ownerProperty = MethodBase.GetCurrentMethod().Name.Remove(0, 4);
        SaveToRegistry(ownerProperty, value);
      }
    }

  }
}
