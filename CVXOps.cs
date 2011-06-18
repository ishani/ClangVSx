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
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.VisualStudio.VCProject;
using Microsoft.VisualStudio.VCProjectEngine;

namespace ClangVSx
{
  class ClangOps
  {
    private OutputWindowPane  _outputPane;
    private Window            _vsOutputWindow;
    private DTE2              _applicationObject;

    /// <summary>
    /// 
    /// </summary>
    public ClangOps(DTE2 appObj)
    {
      _applicationObject = appObj;

      const string owpName = "Clang C/C++";

      // go find the output window
      _vsOutputWindow = _applicationObject.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      _vsOutputWindow.Visible = true;
      OutputWindow theOutputWindow = (OutputWindow)_vsOutputWindow.Object;

      // add or acquire the output pane
      try
      {
        _outputPane = theOutputWindow.OutputWindowPanes.Item(owpName);
      }
      catch
      {
        _outputPane = theOutputWindow.OutputWindowPanes.Add(owpName);
      }
    }

    /// <summary>
    /// simple wrapper for writing text to the output pane
    /// </summary>
    public void WriteToOutputPane(string text)
    {
      _outputPane.OutputString(text);
    }

    /// <summary>
    /// compile a single VCFile, do nothing with the OBJ
    /// </summary>
    public void CompileSingleFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg, String additionalCmds = "")
    {
      CVXBuildSystem buildSystem;
      try
      {
        buildSystem = new CVXBuildSystem(_vsOutputWindow, _outputPane);
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message, "ClangVSx Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        buildSystem.CompileSingleFile(vcFile, vcProject, vcCfg, additionalCmds);
      }
      catch (System.Exception ex)
      {
        WriteToOutputPane("Exception During File Compile : \n" + ex.Message + "\n");
      }
    }

    /// <summary>
    /// Compile the project that is set as the 'startup project' in the solution
    /// </summary>
    public void BuildActiveProject()
    {
      CVXBuildSystem buildSystem;
      try
      {
        buildSystem = new CVXBuildSystem(_vsOutputWindow, _outputPane);
      }
      catch (System.Exception ex)
      {
        MessageBox.Show(ex.Message, "ClangVSx Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
      }

      try
      {
        // loop through the startup projects
        foreach (object startUpProj in (Array)_applicationObject.Solution.SolutionBuild.StartupProjects)
        {
          // is this project a VC++ one? the guid is hardcoded because it doesn't seem to be included
          // anywhere else in the constants, EnvDTE, etc..!
          Project p = _applicationObject.Solution.Item(startUpProj);

          if (p.Kind.ToUpper().Equals("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}"))
          {
            WriteToOutputPane("Building Project : " + p.Name + "\n");

            VCProject vcProject = p.Object as VCProject;
            if (vcProject == null)
            {
              WriteToOutputPane("Error : Could not cast project to VCProject.\n");
              return;
            }


            EnvDTE.Configuration cfg = p.ConfigurationManager.ActiveConfiguration;
            IVCCollection cfgArray = (IVCCollection)vcProject.Configurations;
            VCConfiguration vcCfg = (VCConfiguration)cfgArray.Item(cfg.ConfigurationName);

            if (vcCfg == null)
            {
              WriteToOutputPane("Error : Could not find '" + cfg.ConfigurationName + "' configuration!\n");
            }
            else
            {
              WriteToOutputPane("Configuration : " + vcCfg.Name + "\n");
            }

            buildSystem.BuildProject(vcProject, vcCfg);
          }
          else
          {
            WriteToOutputPane("Ignoring non-C++ Project : " + p.Name + "\n");
          }
        }
      }
      catch (System.Exception ex)
      {
        WriteToOutputPane("Exception During Build : \n" + ex.Message + "\n");
      }
    }
  }
}
