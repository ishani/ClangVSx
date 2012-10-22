/*
 * CLangVS - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.VCProjectEngine;
using NEnhancer.Common;
using Thread = System.Threading.Thread;

namespace ClangVSx
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class CVXConnect : IDTExtensibility2, IDTCommandTarget
    {
        private readonly List<String> TemporaryFilesCreatedDuringThisSession = new List<String>();
        private CommandBarEvents AnalyseCmdEvent;
        private bool BuildInProgress;
        private CommandBarEvents CompileCmdEvent;
        private CommandBarEvents DasmCmdEvent;
        private CommandBarEvents PreproCmdEvent;
        private CommandBarButton RebuildActiveProjectButton;
        private CommandBarButton SettingsButton;

        // instance of the compiler bridge; this offers up the main worker functions that go off and compile files, projects, etc.
        private ClangOps _CVXOps;
        private AddIn _addInInstance;
        private DTE2 _applicationObject;
        private DTEHelper _dteHelper;

        #region Commands

        private const string COMMAND_CLANG_SETTINGS_DLG = "ShowSettingsDlg";
        private const string COMMAND_CLANG_REBUILD_ACTIVE = "RebuildActiveProject";
        private const string COMMAND_CLANG_RELINK_ACTIVE = "RelinkActiveProject";

        private string GetCommandFullName(string cmdName)
        {
            return "ClangVSx.CVXConnect." + cmdName;
        }

        private string GetCommandShortName(string fullCmdName)
        {
            return fullCmdName.Remove(0, ("ClangVSx.CVXConnect.").Length);
        }

        #endregion

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public CVXConnect()
        {
            BuildInProgress = false;
        }

        #region IDTCommandTarget Members

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status,
                                ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName.StartsWith("ClangVSx.CVXConnect"))
                {
                    status = vsCommandStatus.vsCommandStatusSupported;

                    if (commandName.EndsWith(COMMAND_CLANG_SETTINGS_DLG))
                    {
                        // don't change settings mid-build
                        if (!BuildInProgress)
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                    else if (commandName.EndsWith(COMMAND_CLANG_REBUILD_ACTIVE))
                    {
                        if (!BuildInProgress && _applicationObject.Solution.Count != 0)
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                    else if (commandName.EndsWith(COMMAND_CLANG_RELINK_ACTIVE))
                    {
                        if (!BuildInProgress && _applicationObject.Solution.Count != 0)
                            status |= vsCommandStatus.vsCommandStatusEnabled;
                    }
                }
            }
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut,
                         ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                String cmd = GetCommandShortName(commandName);
                switch (cmd)
                {
                    case COMMAND_CLANG_SETTINGS_DLG:
                        {
                            handled = true;
                            ShowCVXSettingsDialog();
                        }
                        break;

                    case COMMAND_CLANG_RELINK_ACTIVE:
                    case COMMAND_CLANG_REBUILD_ACTIVE:
                        {
                            handled = true;
                            _applicationObject.Documents.SaveAll();

                            try
                            {
                                // build a config options block, set the delegates for build events to toggle
                                // our local build-is-running variable that will disable all other Clang actions until it finishes
                                var pbc = new ClangOps.ProjectBuildConfig();
                                pbc.BuildBegun = (bool success) => { BuildInProgress = true; };
                                pbc.BuildFinished = (bool success) => { BuildInProgress = false; };
                                pbc.JustLink = (cmd == COMMAND_CLANG_RELINK_ACTIVE);

                                // start the build on another thread so the output pane updates asynchronously
                                ParameterizedThreadStart buildDelegate =
                                    _CVXOps.BuildActiveProject;
                                var newThread = new Thread(buildDelegate);
                                newThread.Start(pbc);
                            }
                            catch (Exception ex)
                            {
                                BuildInProgress = false;
                                MessageBox.Show(ex.Message, "ClangVSx - Project Build Error", MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region IDTExtensibility2 Members

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2) application;
            _addInInstance = (AddIn) addInInst;
            _dteHelper = new DTEHelper(_applicationObject, _addInInstance);

            _CVXOps = new ClangOps(_applicationObject);
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
            var contextGUIDS = new object[] {};
            var commands = (Commands2) _applicationObject.Commands;
            var _commandBars =
                ((CommandBars) _applicationObject.CommandBars);

            // add a top-level menu to stick some global options in
            {
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar =
                    ((CommandBars) _applicationObject.CommandBars)["MenuBar"];

                int nenhancerPopupIndex = menuBarCommandBar.Controls.Count + 1;
                var clangMenuRoot = menuBarCommandBar.Controls.Add(
                    MsoControlType.msoControlPopup,
                    Type.Missing,
                    Type.Missing,
                    nenhancerPopupIndex,
                    true) as CommandBarPopup;
                clangMenuRoot.Caption = "Clang";

                // open the settings dialog
                {
                    _dteHelper.AddNamedCommand2(
                        COMMAND_CLANG_SETTINGS_DLG,
                        "Settings...",
                        "Settings...",
                        false,
                        0);
                    Command commandToAdd =
                        _applicationObject.Commands.Item(GetCommandFullName(COMMAND_CLANG_SETTINGS_DLG), 0);
                    SettingsButton =
                        commandToAdd.AddControl(clangMenuRoot.CommandBar, clangMenuRoot.CommandBar.Controls.Count + 1)
                        as CommandBarButton;
                }

                // get active vcproj, rebuild it with Clang
                {
                    _dteHelper.AddNamedCommand2(
                        COMMAND_CLANG_REBUILD_ACTIVE,
                        "Rebuild Active Project",
                        "Rebuild Active Project",
                        false,
                        0);
                    Command commandToAdd =
                        _applicationObject.Commands.Item(GetCommandFullName(COMMAND_CLANG_REBUILD_ACTIVE), 0);
                    RebuildActiveProjectButton =
                        commandToAdd.AddControl(clangMenuRoot.CommandBar, clangMenuRoot.CommandBar.Controls.Count + 1)
                        as CommandBarButton;
                }
                {
                    _dteHelper.AddNamedCommand2(
                        COMMAND_CLANG_RELINK_ACTIVE,
                        "Relink",
                        "Relink",
                        false,
                        0);
                    Command commandToAdd =
                        _applicationObject.Commands.Item(GetCommandFullName(COMMAND_CLANG_RELINK_ACTIVE), 0);
                    RebuildActiveProjectButton =
                        commandToAdd.AddControl(clangMenuRoot.CommandBar, clangMenuRoot.CommandBar.Controls.Count + 1)
                        as CommandBarButton;
                }
            }
            // add a compile-this-file option to the editor window
            {
                Microsoft.VisualStudio.CommandBars.CommandBar codeWinCommandBar =
                    _dteHelper.GetCommandBarByName("Code Window");

                int pmPopupIndex = codeWinCommandBar.Controls.Count + 1;
                var pmPopup = codeWinCommandBar.Controls.Add(
                    MsoControlType.msoControlPopup,
                    Type.Missing,
                    Type.Missing,
                    pmPopupIndex,
                    true) as CommandBarPopup;
                pmPopup.Caption = "Clang Compiler";

                CommandBarButton saveAndCompileCmd = _dteHelper.AddButtonToPopup(
                    pmPopup,
                    pmPopup.Controls.Count + 1,
                    "Compile",
                    "Compile");
                CompileCmdEvent = _applicationObject.Events.get_CommandBarEvents(saveAndCompileCmd) as CommandBarEvents;
                CompileCmdEvent.Click += cvxCompileFile_menuop;

                CommandBarButton saveAndAnalyseCmd = _dteHelper.AddButtonToPopup(
                    pmPopup,
                    pmPopup.Controls.Count + 1,
                    "Run Static Analysis",
                    "Run Static Analysis");
                AnalyseCmdEvent = _applicationObject.Events.get_CommandBarEvents(saveAndAnalyseCmd) as CommandBarEvents;
                AnalyseCmdEvent.Click += cvxAnalyseFile_menuop;

                CommandBarButton saveAndDasmCmd = _dteHelper.AddButtonToPopup(
                    pmPopup,
                    pmPopup.Controls.Count + 1,
                    "View Disassembly (LLVM)",
                    "View Disassembly (LLVM)");
                DasmCmdEvent = _applicationObject.Events.get_CommandBarEvents(saveAndDasmCmd) as CommandBarEvents;
                DasmCmdEvent.Click += cvxDasmnFile_menuop;

                CommandBarButton dasmAndPreproCmd = _dteHelper.AddButtonToPopup(
                    pmPopup,
                    pmPopup.Controls.Count + 1,
                    "View Preprocessor Result",
                    "View Preprocessor Result");
                PreproCmdEvent = _applicationObject.Events.get_CommandBarEvents(dasmAndPreproCmd) as CommandBarEvents;
                PreproCmdEvent.Click += cvxPreProFile_menuop;
            }
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
            // try and be tidy!
            foreach (String tempF in TemporaryFilesCreatedDuringThisSession)
            {
                if (File.Exists(tempF))
                    File.Delete(tempF);
            }
        }

        #endregion

        internal void ReportBuildInProgress()
        {
            MessageBox.Show("The Clang compiler is already in use, please wait.", "ClangVSx", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        /// <summary>
        /// get the active code file and compile it
        /// </summary>
        protected void cvxCompileFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (BuildInProgress)
            {
                ReportBuildInProgress();
                return;
            }

            VCFile vcFile;
            VCProject vcProject;
            VCConfiguration vcCfg;

            if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
            {
                _CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg);
            }
            else
            {
                MessageBox.Show(
                    "Clang cannot compile files of this type / unrecognized code file (" +
                    _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
            }
        }

        /// <summary>
        /// ask clang to emit assembly listing for the given file, then pop that result open in VS
        /// </summary>
        protected void cvxDasmnFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (BuildInProgress)
            {
                ReportBuildInProgress();
                return;
            }

            VCFile vcFile;
            VCProject vcProject;
            VCConfiguration vcCfg;

            if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
            {
                String dasmOut = Path.GetTempFileName();
                TemporaryFilesCreatedDuringThisSession.Add(dasmOut);

                if (_CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg,
                                              String.Format("-save-temps -S -emit-llvm -o\"{0}\"", dasmOut)))
                {
                    _applicationObject.ItemOperations.OpenFile(dasmOut, "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}");
                }
            }
            else
            {
                MessageBox.Show(
                    "Clang cannot compile files of this type / unrecognized code file (" +
                    _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
            }
        }

        /// <summary>
        /// dump the preprocessor and native-assembly format listing
        /// </summary>
        protected void cvxPreProFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (BuildInProgress)
            {
                ReportBuildInProgress();
                return;
            }

            VCFile vcFile;
            VCProject vcProject;
            VCConfiguration vcCfg;

            if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
            {
                String ppOut = Path.GetTempFileName();
                TemporaryFilesCreatedDuringThisSession.Add(ppOut);

                if (_CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg, String.Format("-save-temps -E -o\"{0}\"", ppOut)))
                {
                    _applicationObject.ItemOperations.OpenFile(ppOut, "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}");
                }
            }
            else
            {
                MessageBox.Show(
                    "Clang cannot compile files of this type / unrecognized code file (" +
                    _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
            }
        }

        /// <summary>
        /// get the active code file and compile it, adding static analyzer arguments to the pile
        /// </summary>
        protected void cvxAnalyseFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
        {
            if (BuildInProgress)
            {
                ReportBuildInProgress();
                return;
            }

            VCFile vcFile;
            VCProject vcProject;
            VCConfiguration vcCfg;

            if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
            {
                _CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg, "--analyze --analyzer-output text");
            }
            else
            {
                MessageBox.Show(
                    "Clang cannot analyse files of this type / unrecognized code file (" +
                    _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
            }
        }

        /// <summary>
        /// Get the active code file, project and configuration 
        /// </summary>
        /// <returns>true if we have found an active C/C++ document</returns>
        private bool GetActiveVCFile(out VCFile vcFile, out VCProject vcProject, out VCConfiguration vcCfg)
        {
            vcFile = null;
            vcProject = null;
            vcCfg = null;

            if (_applicationObject.ActiveDocument != null)
            {
                // GUID equates to 'code file' as far as I can make out
                if (_applicationObject.ActiveDocument.Kind == "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}" &&
                    _applicationObject.ActiveDocument.Language == "C/C++")
                {
                    // GUID equates to physical file on disk [http://msdn.microsoft.com/en-us/library/z4bcch80(VS.80).aspx]
                    if (_applicationObject.ActiveDocument.ProjectItem.Kind == "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}")
                    {
                        // leap of faith
                        vcFile = (VCFile) _applicationObject.ActiveDocument.ProjectItem.Object;
                        vcProject = (VCProject) vcFile.project;

                        if (vcFile.FileType != eFileType.eFileTypeCppCode)
                            return false;

                        // save the file (should be optional!)
                        if (!_applicationObject.ActiveDocument.Saved)
                            _applicationObject.ActiveDocument.Save(vcFile.FullPath);

                        // get current configuration to pass to the bridge
                        Configuration cfg =
                            _applicationObject.ActiveDocument.ProjectItem.ConfigurationManager.ActiveConfiguration;

                        try
                        {
                            var cfgArray = (IVCCollection) vcProject.Configurations;
                            foreach (VCConfiguration vcr in cfgArray)
                            {
                                if (vcr.ConfigurationName == cfg.ConfigurationName &&
                                    vcr.Platform.Name == cfg.PlatformName)
                                {
                                    vcCfg = vcr;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// display our settings panel
        /// </summary>
        private void ShowCVXSettingsDialog()
        {
            var wW = new IWinWrapper();
            wW.mHandle = _applicationObject.MainWindow.HWnd;

            var dlg = new CVXSettings();
            dlg.ShowDialog(wW);
        }

        #region Nested type: IWinWrapper

        /// <summary>
        /// workaround for providing IWin32Window interface for IntPtr HWNDs
        /// </summary>
        private class IWinWrapper : IWin32Window
        {
            public int mHandle;

            #region IWin32Window Members

            public IntPtr Handle
            {
                get { return (IntPtr) mHandle; }
            }

            #endregion
        }

        #endregion
    }
}