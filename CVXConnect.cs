/*
 * CLangVS - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.CommandBars;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;
using NEnhancer.Common;

namespace ClangVSx
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class CVXConnect : IDTExtensibility2, IDTCommandTarget
	{
    private DTE2 _applicationObject;
    private DTEHelper _dteHelper;
    private AddIn _addInInstance;

    CommandBarEvents saveAndCompileCmdEvent;
    CommandBarEvents saveAndAnalyseCmdEvent;
 
    // instance of the compiler bridge; this offers up the main worker functions that go off and compile files, projects, etc.
    private ClangOps _CVXOps;

    #region Commands
    private const string COMMAND_CLANG_SETTINGS_DLG = "ShowSettingsDlg";
    private const string COMMAND_CLANG_REBUILD_ACTIVE = "RebuildActiveProject";

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
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			_applicationObject = (DTE2)application;
			_addInInstance = (AddIn)addInInst;
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
      object[] contextGUIDS = new object[] { };
      Commands2 commands = (Commands2)_applicationObject.Commands;
      Microsoft.VisualStudio.CommandBars.CommandBars _commandBars = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars);

      // add a top-level menu to stick some global options in
      {
        Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar =
            ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

        int nenhancerPopupIndex = menuBarCommandBar.Controls.Count + 1;
        CommandBarPopup clangMenuRoot = menuBarCommandBar.Controls.Add(
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
          Command commandToAdd = _applicationObject.Commands.Item(GetCommandFullName(COMMAND_CLANG_SETTINGS_DLG), 0);
          CommandBarButton newButton = commandToAdd.AddControl(clangMenuRoot.CommandBar, clangMenuRoot.CommandBar.Controls.Count + 1) as CommandBarButton;
        }

        // get active vcproj, rebuild it with Clang
        {
          _dteHelper.AddNamedCommand2(
                COMMAND_CLANG_REBUILD_ACTIVE,
                "Rebuild Active Project",
                "Rebuild Active Project",
                false,
                0);
          Command commandToAdd = _applicationObject.Commands.Item(GetCommandFullName(COMMAND_CLANG_REBUILD_ACTIVE), 0);
          CommandBarButton newButton = commandToAdd.AddControl(clangMenuRoot.CommandBar, clangMenuRoot.CommandBar.Controls.Count + 1) as CommandBarButton;
        }

      }
      // add a compile-this-file option to the editor window
      {
        Microsoft.VisualStudio.CommandBars.CommandBar codeWinCommandBar = _dteHelper.GetCommandBarByName("Code Window");

        int pmPopupIndex = codeWinCommandBar.Controls.Count + 1;
        CommandBarPopup pmPopup = codeWinCommandBar.Controls.Add(
            MsoControlType.msoControlPopup,
            Type.Missing,
            Type.Missing,
            pmPopupIndex,
            true) as CommandBarPopup;
        pmPopup.Caption = "Clang Compiler";

        CommandBarButton saveAndCompileCmd = _dteHelper.AddButtonToPopup(
          pmPopup,
          pmPopup.Controls.Count + 1,
          "Save && Compile File",
          "Save & Compile File");
        saveAndCompileCmdEvent = _applicationObject.Events.get_CommandBarEvents(saveAndCompileCmd) as CommandBarEvents;
        saveAndCompileCmdEvent.Click += new _dispCommandBarControlEvents_ClickEventHandler(cvxCompileFile_menuop);

        CommandBarButton saveAndAnalyseCmd = _dteHelper.AddButtonToPopup(
          pmPopup,
          pmPopup.Controls.Count + 1,
          "Run Static Analysis",
          "Run Static Analysis");
        saveAndAnalyseCmdEvent = _applicationObject.Events.get_CommandBarEvents(saveAndAnalyseCmd) as CommandBarEvents;
        saveAndAnalyseCmdEvent.Click += new _dispCommandBarControlEvents_ClickEventHandler(cvxAnalyseFile_menuop);
      }
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
      if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
      {
        if (commandName.StartsWith("ClangVSx.CVXConnect"))
        {
          status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported;
          
          if (commandName.EndsWith(COMMAND_CLANG_SETTINGS_DLG))
            status |= vsCommandStatus.vsCommandStatusEnabled;
          else if (commandName.EndsWith(COMMAND_CLANG_REBUILD_ACTIVE))
          {
            if (((Array)_applicationObject.ActiveSolutionProjects).Length > 0)
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
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
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

          case COMMAND_CLANG_REBUILD_ACTIVE:
            {
              handled = true;
              _CVXOps.BuildActiveProject();
            }
            break;
        }
      }
		}

    /// <summary>
    /// find the active VC++ project and pass it to our build system
    /// </summary>
    protected void buildActiveProject()
    {
      foreach (object startUpProj in (Array)_applicationObject.ActiveSolutionProjects)
      {
        Project p = (Project)startUpProj;

        // 8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942 GUID is VC++ Project
        if (p.Kind.ToUpper().Equals("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}"))
        {
          _CVXOps.WriteToOutputPane("Building Project : " + p.Name + "\n");

          VCProject vcProject = p.Object as VCProject;
          if (vcProject == null)
          {
            _CVXOps.WriteToOutputPane("Error : Could not cast project to VCProject.\n");
            continue;
          }

          EnvDTE.Configuration cfg = p.ConfigurationManager.ActiveConfiguration;
          IVCCollection cfgArray = (IVCCollection)vcProject.Configurations;
          VCConfiguration vcCfg = (VCConfiguration)cfgArray.Item(cfg.ConfigurationName);

          if (vcCfg == null)
          {
            _CVXOps.WriteToOutputPane("Error : Could not find '" + cfg.ConfigurationName + "' configuration!\n");
          }
          else
          {
            _CVXOps.WriteToOutputPane("Configuration : " + vcCfg.Name + "\n");
          }

          try
          {
            _CVXOps.BuildProject(vcProject, vcCfg);
          }
          catch (System.Exception e)
          {
            _CVXOps.WriteToOutputPane("Unhandled Exception in [buildActiveProject] : \n" + e.Message + "\n" + e.Source + "\n\n");
          }
        }
        else
        {
          _CVXOps.WriteToOutputPane("Ignoring non-C++ Project : " + p.Name + "\n");
        }
      }
    }

    /// <summary>
    /// get the active code file and compile it
    /// </summary>
    protected void cvxCompileFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
    {
      VCFile vcFile;
      VCProject vcProject;
      VCConfiguration vcCfg;

      if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
      {
        _CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg);
      }
      else
      {
        MessageBox.Show("Clang cannot compile files of this type / unrecognized code file (" + _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
      }
    }

    /// <summary>
    /// get the active code file and compile it, adding static analyzer arguments to the pile
    /// </summary>
    protected void cvxAnalyseFile_menuop(object CommandaBarControl, ref bool handled, ref bool cancelDefault)
    {
      VCFile vcFile;
      VCProject vcProject;
      VCConfiguration vcCfg;

      if (GetActiveVCFile(out vcFile, out vcProject, out vcCfg))
      {
        _CVXOps.CompileSingleFile(vcFile, vcProject, vcCfg, "--analyze --analyzer-output text");
      }
      else
      {
        MessageBox.Show("Clang cannot compile files of this type / unrecognized code file (" + _applicationObject.ActiveDocument.Language + ")", "ClangVSx");
      }
    }

    /// <summary>
    /// Get the active code file, project and configuration 
    /// </summary>
    /// <returns>true if we have found an active C/C++ document</returns>
    bool GetActiveVCFile(out VCFile vcFile, out VCProject vcProject, out VCConfiguration vcCfg)
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
            vcFile = (VCFile)_applicationObject.ActiveDocument.ProjectItem.Object;
            vcProject = (VCProject)vcFile.project;

            // save the file (should be optional!)
            if (!_applicationObject.ActiveDocument.Saved)
              _applicationObject.ActiveDocument.Save(vcFile.FullPath);

            // get current configuration to pass to the bridge
            EnvDTE.Configuration cfg = _applicationObject.ActiveDocument.ProjectItem.ConfigurationManager.ActiveConfiguration;
            IVCCollection cfgArray = (IVCCollection)vcProject.Configurations;
            vcCfg = (VCConfiguration)cfgArray.Item(cfg.ConfigurationName);

            return true;
          }
        }
      }

      return false;
    }


    /// <summary>
    /// workaround for providing IWin32Window interface for IntPtr HWNDs
    /// </summary>
    private class IWinWrapper : IWin32Window
    {
      public int mHandle;
      public IntPtr Handle
      {
        get { return (IntPtr)mHandle; }
      }
    }

    /// <summary>
    /// display our settings panel
    /// </summary>
    void ShowCVXSettingsDialog()
    {
      IWinWrapper wW = new IWinWrapper();
      wW.mHandle = _applicationObject.MainWindow.HWnd;

      CVXSettings dlg = new CVXSettings();
      dlg.ShowDialog(wW);
    }
	}
}