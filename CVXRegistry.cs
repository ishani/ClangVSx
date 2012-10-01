/*
 * CLangVS - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using System.Linq.Expressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ClangVSx
{

    #region Generic registry access

    public static class RegistryBooleanSupport
    {
        public static object GetValueFromKey(this RegistryKey reg, String keyName, object state)
        {
            if (state.GetType() == typeof (Boolean))
            {
                var stateAsBool = (Boolean) state;

                Int32 defaultState = (stateAsBool ? 1 : 0);
                defaultState = (Int32) reg.GetValue(keyName, defaultState);

                return (defaultState != 0);
            }
            else
            {
                return reg.GetValue(keyName, state);
            }
        }

        public static void SetKeyFromValue(this RegistryKey reg, String keyName, object state)
        {
            if (state.GetType() == typeof (Boolean))
            {
                var stateAsBool = (Boolean) state;

                Int32 saveValue = (stateAsBool ? 1 : 0);
                reg.SetValue(keyName, saveValue);
            }
            else
            {
                reg.SetValue(keyName, state);
            }
        }
    }

    internal class Win32Registry
    {
        // root reg key 
        private const String CVXRegistryKey = "Software\\Ishani\\ClangVSx";

        public T LoadFrom<T>(String keyName, T defaultValue)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
            }

            T itemDefaultState = defaultValue;
            try
            {
                itemDefaultState = (T) key.GetValueFromKey(keyName, itemDefaultState);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error when reading setting from registry:\n\n" + e.StackTrace);
            }
            finally
            {
                key.Close();
            }

            return ((itemDefaultState != null) ? itemDefaultState : defaultValue);
        }

        public void SaveTo<T>(String keyName, T saveValue)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(CVXRegistryKey, true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(CVXRegistryKey);
            }

            try
            {
                key.SetKeyFromValue(keyName, saveValue);
            }
            finally
            {
                key.Close();
            }
        }
    }

    #endregion

    internal class CVXRegistryItem<T>
    {
        private readonly T _default_value;
        private readonly string _name;
        private readonly Win32Registry _registry = new Win32Registry();

        public CVXRegistryItem(Expression<Func<CVXRegistryItem<T>>> expr, T defaultValue)
        {
            var body = ((MemberExpression) expr.Body);
            _name = body.Member.Name;
            _default_value = defaultValue;
        }

        // push or pull values from the registry automatically
        public T Value
        {
            get
            {
                // sketchy way to get name of property from inside accessors
                return _registry.LoadFrom(_name, _default_value);
            }

            set { _registry.SaveTo(_name, value); }
        }

        // this is just sugar that allows for
        // String foo = ShowCommands;
        public static implicit operator T(CVXRegistryItem<T> d)
        {
            return d.Value;
        }
    }

    /// <summary>
    /// Wrapper around registry access to load/save AddIn settings
    /// </summary>
    internal abstract class CVXRegistry
    {
        public static CVXRegistryItem<String> PathToClang = new CVXRegistryItem<String>(() => PathToClang,
                                                                                        "C:\\clang\\clang.exe");

        public static CVXRegistryItem<Boolean> ShowCommands = new CVXRegistryItem<Boolean>(() => ShowCommands, false);
        public static CVXRegistryItem<Boolean> MakeBatchFiles = new CVXRegistryItem<Boolean>(() => MakeBatchFiles, false);
        public static CVXRegistryItem<Boolean> EchoInternal = new CVXRegistryItem<Boolean>(() => EchoInternal, false);
        public static CVXRegistryItem<Boolean> ShowPhases = new CVXRegistryItem<Boolean>(() => ShowPhases, false);

        public static CVXRegistryItem<String> CommonArgs = new CVXRegistryItem<String>(() => CommonArgs, "");

        public static CVXRegistryItem<String> TripleWin32 = new CVXRegistryItem<String>(() => TripleWin32,
                                                                                        "i686-pc-win32");

        public static CVXRegistryItem<String> TripleX64 = new CVXRegistryItem<String>(() => TripleX64, "x86_64-pc-win32");

        public static CVXRegistryItem<String> TripleARM = new CVXRegistryItem<String>(() => TripleARM,
                                                                                      "armv7-apple-darwin10");
    }
}