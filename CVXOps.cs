/*
 * ClangVSx - Compiler Bridge for CLang in MS Visual Studio
 * Harry Denholm, ishani.org 2011-2012
 * 
 * https://github.com/ishani/ClangVSx
 * http://www.ishani.org/web/articles/code/clangvsx/
 *
 * Released under LLVM Release License. See LICENSE.TXT for details.
 */

using System;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.VCProjectEngine;

namespace ClangVSx
{
  internal class ClangOps
  {
    #region Delegates

    public delegate void BuildEventDelegate(bool success);

    #endregion

    private readonly DTE2 _applicationObject;

    private readonly OutputWindowPane _outputPane;
    private readonly Window _vsOutputWindow;

    /// <summary>
    /// 
    /// </summary>
    public ClangOps(DTE2 appObj)
    {
      _applicationObject = appObj;

      const string owpName = "Clang C/C++";

      // go find the output window
      _vsOutputWindow = _applicationObject.Windows.Item(Constants.vsWindowKindOutput);
      _vsOutputWindow.Visible = true;
      var theOutputWindow = (OutputWindow)_vsOutputWindow.Object;

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
    public bool CompileSingleFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg,
                                  String additionalCmds = "")
    {
      CVXBuildSystem buildSystem;
      try
      {
        buildSystem = new CVXBuildSystem(_vsOutputWindow, _outputPane);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "ClangVSx Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      String prevEnv = Environment.CurrentDirectory;

      try
      {
        Environment.CurrentDirectory = vcProject.ProjectDirectory;
        WriteToOutputPane("Project Directory : " + Environment.CurrentDirectory + "\n");

        return buildSystem.CompileSingleFile(vcFile, vcProject, vcCfg, additionalCmds);
      }
      catch (Exception ex)
      {
        WriteToOutputPane("Exception During File Compile : \n" + ex.Message + "\n");
      }
      finally
      {
        Environment.CurrentDirectory = prevEnv;
      }

      return false;
    }

    /// <summary>
    /// Compile the project that is set as the 'startup project' in the solution
    /// </summary>
    public void BuildActiveProject(object buildConfig)
    {
      // cast the input object; this is designed to be run with ParameterizedThreadStart, so we have to accept 'object'...
      var config = (buildConfig as ProjectBuildConfig);
      if (config == null)
        throw new InvalidCastException(
            "BuildActiveProject called with invalid argument - ProjectBuildConfig required");

      // mark the build as ready-to-go
      config.BuildBegun(true);

      CVXBuildSystem buildSystem;
      try
      {
        buildSystem = new CVXBuildSystem(_vsOutputWindow, _outputPane);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "ClangVSx Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        config.BuildFinished(false);
        return;
      }

      String prevEnv = Environment.CurrentDirectory;

      try
      {
        // loop through the startup projects
        foreach (object startUpProj in (Array)_applicationObject.Solution.SolutionBuild.StartupProjects)
        {
          if (config.BuildShouldCancel.ShouldCancelBuild())
          {
            WriteToOutputPane("Stopping build... \n");
            break;
          }

          // is this project a VC++ one? the guid is hardcoded because it doesn't seem to be included
          // anywhere else in the constants, EnvDTE, etc..!
          Project p = _applicationObject.Solution.Item(startUpProj);

          if (p.Kind.ToUpper().Equals("{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}"))
          {
            WriteToOutputPane("Building Project : " + p.Name + "\n");

            var vcProject = p.Object as VCProject;
            if (vcProject == null)
            {
              WriteToOutputPane("Error : Could not cast project to VCProject.\n");
              config.BuildFinished(false);
              return;
            }

            Configuration cfg = p.ConfigurationManager.ActiveConfiguration;

            VCConfiguration vcCfg = null;
            try
            {
              var cfgArray = (IVCCollection)vcProject.Configurations;
              foreach (VCConfiguration vcr in cfgArray)
              {
                // we .ToLower() here as there are some weird occurances where names don't match as VS is holding
                // onto a capitalised version that isn't visible in any of the VS IDEs :Z
                if (vcr.ConfigurationName.ToLower() == cfg.ConfigurationName.ToLower() &&
                    vcr.Platform.Name.ToLower() == cfg.PlatformName.ToLower())
                {
                  vcCfg = vcr;
                }
              }
            }
            catch (Exception)
            {
              WriteToOutputPane("Error - failed to determine VC configuration\n");
            }

            if (vcCfg == null)
            {
              WriteToOutputPane("Error : Could not find '" + cfg.ConfigurationName + "' configuration!\n");
            }
            else
            {
              WriteToOutputPane("Configuration : " + vcCfg.Name + "\n");
            }

            Environment.CurrentDirectory = vcProject.ProjectDirectory;
            WriteToOutputPane("Project Directory : " + Environment.CurrentDirectory + "\n");

            bool result = buildSystem.BuildProject(vcProject, vcCfg, config.JustLink, config.BuildShouldCancel);
            config.BuildFinished(result);
            return;
          }
          else
          {
            WriteToOutputPane("Ignoring non-C++ Project : " + p.Name + "\n");
          }
        }
      }
      catch (Exception ex)
      {
        WriteToOutputPane("Exception During Build : \n" + ex.Message + "\n");
      }
      finally
      {
        config.BuildFinished(false);
        Environment.CurrentDirectory = prevEnv; 
      }
    }

    #region Nested type: ProjectBuildConfig

    public class ProjectBuildConfig
    {
      public BuildEventDelegate BuildBegun;
      public BuildEventDelegate BuildFinished;
      public IBuildCancellation BuildShouldCancel;
      public bool JustLink;
    }

    #endregion
  }
}