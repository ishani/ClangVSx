/*
 * CLangVS - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace ClangVSx
{
  class CVXRegistry
  {
    const String CVXRegistryKey = "Software\\Ishani\\ClangVSx";

    public static String PathToClang
    {
      get
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        String pathToClang = "C:\\dm\\bin\\dmc.exe";
        try
        {
          pathToClang = (String)key.GetValue("pathToClang", pathToClang);
        }
        finally
        {
          key.Close();
        }

        return pathToClang;
      }

      set
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        try
        {
          key.SetValue("pathToClang", value, RegistryValueKind.String);
        }
        finally
        {
          key.Close();
        }
      }
    }

    public static bool ShowCommands
    {
      get
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        int showCmds = 0;
        try
        {
          showCmds = (int)key.GetValue("ShowCommands", 0);
        }
        finally
        {
          key.Close();
        }

        return (showCmds != 0);
      }

      set
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        try
        {
          key.SetValue("ShowCommands", value ? 1 : 0, RegistryValueKind.DWord);
        }
        finally
        {
          key.Close();
        }
      }
    }

    public static bool MakeBatchFiles
    {
      get
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        int makeBatch = 0;
        try
        {
          makeBatch = (int)key.GetValue("MakeBatchFiles", 0);
        }
        finally
        {
          key.Close();
        }

        return (makeBatch != 0);
      }

      set
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        try
        {
          key.SetValue("MakeBatchFiles", value ? 1 : 0, RegistryValueKind.DWord);
        }
        finally
        {
          key.Close();
        }
      }
    }

    public static String CommonArgs
    {
      get
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        String commonArgs = "";
        try
        {
          commonArgs = (String)key.GetValue("CommonArgs", "");
        }
        finally
        {
          key.Close();
        }

        return commonArgs;
      }

      set
      {
        RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
        if (key == null)
        {
          key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
        }

        try
        {
          key.SetValue("CommonArgs", value, RegistryValueKind.String);
        }
        finally
        {
          key.Close();
        }
      }
    }

  }
}
