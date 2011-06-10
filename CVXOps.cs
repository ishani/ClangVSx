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
    private String            m_locationClangEXE;
    private String            m_locationLIBExe;
    private String            m_locationLINKExe;
    private String            m_pathToVS10Tools;
    private String            m_pathToVS10CommonIDE;
    private bool              m_doShowCommands;
    private bool              m_doGenerateBatchFiles;
    private OutputWindowPane  m_outputPane;

    private DTE2              m_applicationObject;

    public ClangOps(DTE2 appObj)
    {
      m_applicationObject = appObj;
    }


    /**
     * function called before any other build functions; sorts out output window, gets locations of executables, etc.
     * returns false if something went wrong - eg. invalid paths to compiler tools
     */
    public bool BeginBuild()
    {
      const string owpName = "Clang C/C++";

      // go find the output window
      Window myOutputWindow = m_applicationObject.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      myOutputWindow.Visible = true;
      OutputWindow castOutputWindow = (OutputWindow)myOutputWindow.Object;

      // add or acquire the output pane
      try
      {
        m_outputPane = castOutputWindow.OutputWindowPanes.Item(owpName);
      }
      catch
      {
        m_outputPane = castOutputWindow.OutputWindowPanes.Add(owpName);
      }

      // clear and focus it
      m_outputPane.Clear();
      m_outputPane.Activate();

      // write out a version header
      {
        Assembly assem = Assembly.GetExecutingAssembly();
        Version vers = assem.GetName().Version;
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(vers.Build).AddSeconds(vers.Revision * 2);
        WriteToOutputPane("ClangVSx " + vers.Major.ToString() + "." + vers.Minor.ToString() + " | Clang Compiler Bridge | built on " + buildDate + "\n\n");
      }

      // read out current executables' locations
      m_locationClangEXE = CVXRegistry.PathToClang;

      // work out where the MS linker / lib tools are, Clang/LLVM doesn't have a linker presently
      // "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\Tools\"
      String pathToVS10 = Environment.GetEnvironmentVariable("VS100COMNTOOLS");
      if (pathToVS10 == null)
      {
        MessageBox.Show("Compilation Error : Could not find Visual Studio 2010 install directory (via VS1010COMNTOOLS environment variable)", "ClangVSx", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      m_pathToVS10Tools = System.IO.Path.GetFullPath(pathToVS10 + @"..\..\VC\bin\");
      m_pathToVS10CommonIDE = System.IO.Path.GetFullPath(pathToVS10 + @"..\IDE\");
      m_locationLIBExe = m_pathToVS10Tools + "lib.exe";
      m_locationLINKExe = m_pathToVS10Tools + "link.exe";

      if (!System.IO.File.Exists(m_locationClangEXE))
      {
        MessageBox.Show("Compilation Error : Could not find CLANG.EXE, check ClangVSx Settings - tried:\n\n" + m_locationClangEXE, "ClangVSx", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      if (!System.IO.File.Exists(m_locationLIBExe))
      {
        MessageBox.Show("Compilation Error : Could not find Visual Studio LIB.EXE - tried:\n\n" + m_locationLIBExe, "ClangVSx", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
      if (!System.IO.File.Exists(m_locationLINKExe))
      {
        MessageBox.Show("Compilation Error : Could not find Visual Studio LINK.EXE - tried:\n\n" + m_locationLINKExe, "ClangVSx", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // read more options from the registry
      m_doShowCommands = CVXRegistry.ShowCommands;
      m_doGenerateBatchFiles = CVXRegistry.MakeBatchFiles;

      return true;
    }


    /**
     * simple wrapper to output text to output pane
     */
    public void WriteToOutputPane(string text)
    {
      m_outputPane.OutputString(text);
    }

    /**
     * simple helper that creates a Process instance ready for running
     * the compiler, linker, external tools, etc
     */
    public System.Diagnostics.Process NewExternalProcess()
    {
      System.Diagnostics.Process result = new System.Diagnostics.Process();
      result.StartInfo.UseShellExecute = false;
      result.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      result.StartInfo.CreateNoWindow = true;
      result.StartInfo.RedirectStandardOutput = true;
      result.StartInfo.RedirectStandardError = true;

      return result;
    }

    /**
     * common code to generate compiler settings for non-file-specific stuff
     */
    public String GenerateDefaultCompilerString(VCProject vcProject, VCConfiguration vcCfg)
    {
      // per cpp-file compiler setting default
      // -c for individual compilation without auto-linking
      StringBuilder defaultCompilerString = new StringBuilder(" -c ");

      // add the UNICODE defines?
      if (vcCfg.CharacterSet == Microsoft.VisualStudio.VCProjectEngine.charSet.charSetUnicode)
      {
        defaultCompilerString.Append("-DUNICODE -D_UNICODE ");
      }
      else if (vcCfg.CharacterSet == Microsoft.VisualStudio.VCProjectEngine.charSet.charSetMBCS)
      {
        defaultCompilerString.Append("-D_MBCS ");
      }

      // get the various VC tools to read out setup
      IVCCollection fTools = (IVCCollection)vcCfg.Tools;
      VCCLCompilerTool vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");
      VCLinkerTool vcLinkerTool = (VCLinkerTool)fTools.Item("VCLinkerTool");
      VCLibrarianTool vcLibTool = (VCLibrarianTool)fTools.Item("VCLibrarianTool");

      // convert the project-wide includes, prepro defines, force includes
      defaultCompilerString.Append(GatherIncludesAndDefines<VCConfiguration>(vcCTool, vcCfg));

      // sort out prolog/epilog settings based on cfg/subsystem
      if (vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeApplication)
      {
      }
      else if (vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
      {
        defaultCompilerString.Append("-D_WINDLL ");
      }

      // sort out defines for the runtime library choice
      {

        // Static MultiThread          /MT       LIBCMT     _MT
        // Debug Static MultiThread    /MTd      LIBCMTD    _DEBUG and _MT
        // 
        // Dynamic Link (DLL)          /MD       MSVCRT     _MT and _DLL
        // Debug Dynamic Link (DLL)    /MDd      MSVCRTD    _DEBUG, _MT, and _DLL
        //
        switch (vcCTool.RuntimeLibrary)
        {
          case runtimeLibraryOption.rtMultiThreaded:
            defaultCompilerString.Append("-D_MT ");
            break;
          case runtimeLibraryOption.rtMultiThreadedDebug:
            defaultCompilerString.Append("-D_MT -D_DEBUG ");
            break;
          case runtimeLibraryOption.rtMultiThreadedDLL:
            defaultCompilerString.Append("-D_MT -D_DLL ");
            break;
          case runtimeLibraryOption.rtMultiThreadedDebugDLL:
            defaultCompilerString.Append("-D_MT -D_DLL -D_DEBUG ");
            break;
        }
      }

      // allow Clang args to be stuck in the 'additional options' textbox in C/C++ properties page
      if (vcCTool.AdditionalOptions != null)
      {
        defaultCompilerString.Append(vcCfg.Evaluate(vcCTool.AdditionalOptions) + " ");
      }

      // bolt on any arguments from the settings dialog
      defaultCompilerString.Append(" ");
      defaultCompilerString.Append(CVXRegistry.CommonArgs);
      defaultCompilerString.Append(" ");

      // gather up default include paths (eg Windows SDK)
      if (vcCTool.FullIncludePath != null)
      {
        String cIncDir = vcCTool.FullIncludePath.Replace(",", ";");
        String[] cIncDirs = cIncDir.Split(';');

        foreach (string inc in cIncDirs)
        {
          if (inc.Length > 0)
          {
            defaultCompilerString.Append("-isystem \"");

            defaultCompilerString.Append(inc.Replace("\\", "/"));
            defaultCompilerString.Append("\" ");
          }
        }
      }

      return defaultCompilerString.ToString();
    }

    /**
     * compile a single VCFile, do nothing with the OBJ
     */
    public void CompileSingleFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg, String additionalCmds = "")
    {
      if (!BeginBuild())
        return;

      StringBuilder defaultCompilerString = new StringBuilder(GenerateDefaultCompilerString(vcProject, vcCfg));
      defaultCompilerString.Append(" ");
      defaultCompilerString.Append(additionalCmds);
      defaultCompilerString.Append(" ");

      StringBuilder compileString = new StringBuilder(1024);
      StringBuilder objFiles = new StringBuilder(256);

      if (InternalBuildVCFile(vcFile, vcProject, vcCfg, defaultCompilerString.ToString(), ref compileString, ref objFiles))
      {
        WriteToOutputPane("\nCompile Successful\n");
      }
    }


    /**
     * Compile the project that is set as the 'startup project' in the solution
     */
    public void BuildActiveProject()
    {
      if (!BeginBuild())
        return;

      // loop through the startup projects
      foreach (object startUpProj in (Array)m_applicationObject.Solution.SolutionBuild.StartupProjects)
      {
        // is this project a VC++ one? the guid is hardcoded because it doesn't seem to be included
        // anywhere else in the constants, EnvDTE, etc..!
        Project p = m_applicationObject.Solution.Item(startUpProj);

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

          BuildProject(vcProject, vcCfg);
        }
        else
        {
          WriteToOutputPane("Ignoring non-C++ Project : " + p.Name + "\n");
        }
      }
    }


    /**
     * I'm probably showing C# inexperience here; I wanted this function to operate on any 
     * object that had an Evaluate method, eg. VCConfiguration, VCFileConfiguration, et al.
     * Reflection and generics is what I came up with!
     */
    void ExecuteCustomBuildToolCommandLine<T>(String cmdLine, object evaluator, VCProject vcProject)
    {
      try
      {
        MethodInfo evalMethod = typeof(T).GetMethod("Evaluate");

        String[] cmdsToRun = cmdLine.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (String singleCmd in cmdsToRun)
        {
          String evaluatedCmd = evalMethod.Invoke(evaluator, new object[] { singleCmd }) as String;

          if (m_doShowCommands)
            WriteToOutputPane("  cmd.exe /C " + evaluatedCmd + "\n");

          // run the specified tool
          System.Diagnostics.Process cbStep = NewExternalProcess();
          cbStep.StartInfo.FileName = "cmd.exe";
          cbStep.StartInfo.Arguments = "/C " + evaluatedCmd.Replace("/", "\\");
          cbStep.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
          cbStep.Start();

          String outputStr = cbStep.StandardOutput.ReadToEnd();
          WriteToOutputPane(outputStr);

          cbStep.WaitForExit();
        }
      }
      catch (System.Exception e)
      {
        WriteToOutputPane("Exception During Execution : " + cmdLine + "\n" + e.Message + "\n" + e.Source + "\n\n");
      }
    }


    /**
     * 
     */
    void RunCustomBuildStep(VCCustomBuildTool customBuildTool, VCFile vcFile, VCProject vcProject, VCFileConfiguration vcFC, bool ignoreTimeStamp)
    {
      // check to see if the supplied outputs are older than the file in question
      // I assume this is how MSVS does it...
      bool outputFilesNeedBuilding = false;
      if (ignoreTimeStamp)
      {
        outputFilesNeedBuilding = true;
      }
      else
      {
        if (customBuildTool.Outputs.Length > 0)
        {
          System.IO.FileInfo vcFileFI = new System.IO.FileInfo(vcFile.FullPath);

          // cut the output files string into individual items
          String[] outputFiles = customBuildTool.Outputs.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
          foreach (String outputFileName in outputFiles)
          {
            // find a full pathname for the file, relative or not
            String finalOutput = outputFileName;
            if (!System.IO.Path.IsPathRooted(outputFileName))
              finalOutput = vcProject.ProjectDirectory + outputFileName;

            String finalOutputDir = System.IO.Path.GetDirectoryName(finalOutput);
            if(!System.IO.Directory.Exists(finalOutputDir))
            {
              System.IO.Directory.CreateDirectory(finalOutputDir);
            }

            // if it doesn't exist, safe to assume we should try and build it
            if (!System.IO.File.Exists(finalOutput))
            {
              outputFilesNeedBuilding = true;
              break;
            }

            // compare timestamps with source file
            System.IO.FileInfo outputFileFI = new System.IO.FileInfo(finalOutput);
            TimeSpan timeDiffCodeToObj = outputFileFI.LastWriteTime.Subtract(vcFileFI.LastWriteTime);
            if (timeDiffCodeToObj.Seconds < 0)
            {
              outputFilesNeedBuilding = true;
              break;
            }
          }
        }
      }

      // nothing to do?
      if (!outputFilesNeedBuilding)
      {
        WriteToOutputPane("[skipping] " + customBuildTool.Description + "\n");
        return;
      }

      WriteToOutputPane(customBuildTool.Description + "\n");

      ExecuteCustomBuildToolCommandLine<VCFileConfiguration>(customBuildTool.CommandLine, vcFC, vcProject);
    }


    /**
     * 
     */
    string GatherIncludesAndDefines<T>(VCCLCompilerTool vcCTool, object evaluator)
    {
      MethodInfo evalMethod = typeof(T).GetMethod("Evaluate");
      StringBuilder result = new StringBuilder();

      // push additional includes into the default compiler string
      if (vcCTool.AdditionalIncludeDirectories != null)
      {
        String cIncDir = vcCTool.AdditionalIncludeDirectories.Replace(",", ";");
        String[] cIncDirs = cIncDir.Split(';');

        foreach (string inc in cIncDirs)
        {
          if (inc.Length > 0)
          {
            String parsedInc = (evalMethod.Invoke(evaluator, new object[] { inc }) as String);
            if (parsedInc != null && parsedInc.Length > 0)
            {
              result.Append("-I");
              String incCheck = parsedInc.Replace("\\", "/");
              
              // Clang doesn't like the trailing / on thse
              if (incCheck == "./") incCheck = ".";
              if (incCheck == "../") incCheck = "..";

              result.Append(incCheck);
              result.Append(" ");
            }
          }
        }
      }

      // same for force-includes, route to -include
      if (vcCTool.ForcedIncludeFiles != null)
      {
        String cForceInc = vcCTool.ForcedIncludeFiles.Replace(",", ";");
        String[] cForceIncs = cForceInc.Split(';');

        foreach (string inc in cForceIncs)
        {
          if (inc.Length > 0)
          {
            String parsedInc = (evalMethod.Invoke(evaluator, new object[] { inc }) as String);
            if (parsedInc != null && parsedInc.Length > 0)
            {
              result.Append("-include \"");
              result.Append(parsedInc.Replace("\\", "/"));
              result.Append("\" ");
            }
          }
        }
      }

      // get default preprocessor directives
      if (vcCTool.PreprocessorDefinitions != null)
      {
        String cPDefine = vcCTool.PreprocessorDefinitions.Replace(",", ";");
        String[] cPDefines = cPDefine.Split(';');

        foreach (string pd in cPDefines)
        {
          if (pd.Length > 0)
          {
            String parsedPd = (evalMethod.Invoke(evaluator, new object[] { pd }) as String);
            if (parsedPd != null && parsedPd.Length > 0)
            {
              result.Append("-D");
              result.Append(parsedPd);
              result.Append(" ");
            }
          }
        }
      }

      return result.ToString();
    }


    /**
     * internal function that does the work of compiling a C++ file using Clang
     */
    public bool InternalBuildVCFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg, String defaultCompilerString, ref StringBuilder compileString, ref StringBuilder objectFilesToLink)
    {
      String strDivider = "";
      strDivider = strDivider.PadLeft(128, '-');

      // get access to the per-file VCCL config data (eg stuff that augments / overwrites the global settings)
      // we use this to configure the compiler per file, as it should reflect the final config state of the file
      VCFileConfiguration vcFC = (VCFileConfiguration)((IVCCollection)vcFile.FileConfigurations).Item(vcCfg.ConfigurationName);
      VCCLCompilerTool perFileVCC = (VCCLCompilerTool)vcFC.Tool;
      

      // begin forming the command line string to send to Clang
      compileString.Append(vcFile.FullPath.Replace("/", "\\"));
      compileString.Append(defaultCompilerString.ToString());

      // sort out an output object file name
      String objectFileName = vcFC.Evaluate(perFileVCC.ObjectFile).Replace("/", "\\");

      String prebuild = (vcProject.ProjectDirectory + "\\" + objectFileName);
      if (!System.IO.Directory.Exists(prebuild))
      {
        System.IO.Directory.CreateDirectory(prebuild);
      }

      // if no obj file specified, make a suitable filename (is this a robust check..??)
      if (!System.IO.Path.HasExtension(objectFileName))
      {
        objectFileName += System.IO.Path.ChangeExtension(vcFile.Name, ".obj");
      }

      // record the .obj target in the cmd line and in the list to pass to the linker
      compileString.Append("-o\"");
      compileString.Append(objectFileName);
      compileString.Append("\" ");
      objectFilesToLink.Append("\"");
      objectFilesToLink.Append(objectFileName);
      objectFilesToLink.Append("\" ");

      // stick the per-file defines, includes, etc 
      compileString.Append(GatherIncludesAndDefines<VCFileConfiguration>(perFileVCC, vcFC));

      // figure out a suitable mapping for optimization flags
      switch (perFileVCC.Optimization)
      {
        case optimizeOption.optimizeDisabled:
          compileString.Append("-O0 -fcatch-undefined-behavior ");
          break;
        case optimizeOption.optimizeMaxSpeed:
          compileString.Append("-O2 ");
          break;
        case optimizeOption.optimizeFull:
          compileString.Append("-O3 -fomit-frame-pointer ");
          break;
        case optimizeOption.optimizeMinSpace:
          compileString.Append("-Os ");
          break;
      }

      // arbitrarily turning a 5-point dial into a toggle switch :)
      if (perFileVCC.WarningLevel == warningLevelOption.warningLevel_0 ||
          perFileVCC.WarningLevel == warningLevelOption.warningLevel_1 ||
          perFileVCC.WarningLevel == warningLevelOption.warningLevel_2)
      {
        compileString.Append("-w ");
      }
      else if (perFileVCC.WarningLevel == warningLevelOption.warningLevel_4)
      {
        compileString.Append("-Wall ");
      }

      if (perFileVCC.WarnAsError)
      {
        compileString.Append("-Werror ");
      }

      // stack overflow checking
      if (perFileVCC.BufferSecurityCheck)
      {
        // HDD TODO - clang doesn't support this GCC flag
        // compileString.Append("-fstack-check ");
      }

      // debug info generation
      if (perFileVCC.DebugInformationFormat != debugOption.debugDisabled)
      {
        // HDD TODO - clang currently can't embed debug info when targeting windows
        // compileString.Append("-g ");
      }

      // no inline function expansion
      if (perFileVCC.InlineFunctionExpansion == inlineExpansionOption.expandDisable)
      {
        // bafflingly, should the option be set to 'Default', we get told it's 'inlineExpansionOption.expandDisable', even
        // if it will be set by virtue of the optimisation configuration...

        // we have to guess that this choice is only valid when the user hasn't chosen optimisation mode max/size/full
        if (perFileVCC.Optimization == optimizeOption.optimizeDisabled ||
            perFileVCC.Optimization == optimizeOption.optimizeCustom)
        {
          compileString.Append("-fno-inline ");
        }
      }

      // RTTI disable?
      if (!perFileVCC.RuntimeTypeInfo)
      {
        compileString.Append("-fno-rtti ");
      }

      // EH
      if (perFileVCC.ExceptionHandling != cppExceptionHandling.cppExceptionHandlingNo)
      {
        compileString.Append("-fexceptions ");
      }

      // asm listing or preprocessor output both turn on the same flag, as the flag produces them both
      if (perFileVCC.AssemblerOutput != asmListingOption.asmListingNone ||
          perFileVCC.GeneratePreprocessedFile != preprocessOption.preprocessNo)
      {
        compileString.Append("-save-temps ");
      }

      if (perFileVCC.CompileAs == CompileAsOptions.compileAsCPlusPlus)
      {
        compileString.Append("-x c++ ");
      }

      // add the 'additional options' verbatim
      if (perFileVCC.AdditionalOptions != null)
      {
        compileString.Append(vcFC.Evaluate(perFileVCC.AdditionalOptions) + " ");
      }

      // ask for warnings/errors in MSVC format
      compileString.Append("-fdiagnostics-format=msvc");

      String fullObjectPath = objectFileName;
      if (!System.IO.Path.IsPathRooted(fullObjectPath))
        fullObjectPath = vcProject.ProjectDirectory + objectFileName;


      WriteToOutputPane(" - " + vcFile.UnexpandedRelativePath);

      if (m_doShowCommands)
        WriteToOutputPane("\n    " + compileString.ToString() + "\n");


      // crank the message pump; means our Output Window stuff should update properly
      System.Windows.Forms.Application.DoEvents();

      // execute the compiler
      System.Diagnostics.Process compileProcess = NewExternalProcess();
      compileProcess.StartInfo.FileName = m_locationClangEXE;
      compileProcess.StartInfo.Arguments = compileString.ToString();
      compileProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
      compileProcess.Start();

      // read any output from the compiler, indicative of some problem...
      String outputStr = compileProcess.StandardError.ReadToEnd();
      compileProcess.WaitForExit();

      if (outputStr.Length > 0)
      {
        WriteToOutputPane("\n" + outputStr);
        WriteToOutputPane(strDivider + "\n\n");

        if (outputStr.Contains("error:"))
          return false;
      }
      else
      {
        if (!m_doShowCommands)
        {
          if (vcFile.UnexpandedRelativePath.Length < 80)
          {
            String tabPad = "";
            tabPad = tabPad.PadLeft(80 - vcFile.UnexpandedRelativePath.Length, ' ');
            WriteToOutputPane(tabPad);
          }
          WriteToOutputPane(" [ok]\n");
        }
      }

      return true;
    }

    /**
     * 
     */
    public bool BuildProject(VCProject vcProject, VCConfiguration vcCfg)
    {
      String strDivider = "";
      strDivider = strDivider.PadLeft(128, '-');

      String defaultCompilerString = GenerateDefaultCompilerString(vcProject, vcCfg);

      // get the various VC tools to read out setup
      IVCCollection fTools = (IVCCollection)vcCfg.Tools;
      VCCLCompilerTool vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");
      VCLinkerTool vcLinkerTool = (VCLinkerTool)fTools.Item("VCLinkerTool");
      VCLibrarianTool vcLibTool = (VCLibrarianTool)fTools.Item("VCLibrarianTool");

      if (vcLinkerTool != null)
      {
        if (vcLinkerTool.SubSystem != subSystemOption.subSystemWindows &&
            vcLinkerTool.SubSystem != subSystemOption.subSystemConsole)
        {
          WriteToOutputPane("Unsupported subsystem type (Windows/Console only)\n");
          return false;
        }
      }


      // work out the path to the final linking result, report it
      String outLink = "";
      switch (vcCfg.ConfigurationType)
      {
        case Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeApplication:
          outLink = vcCfg.Evaluate(vcLinkerTool.OutputFile);
          break;
        case Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary:
          outLink = vcCfg.Evaluate(vcLinkerTool.OutputFile);
          break;
        case Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeStaticLibrary:
          outLink = vcCfg.Evaluate(vcLibTool.OutputFile);
          break;
      }
      outLink.Replace("/", "\\");
      WriteToOutputPane("Linker Output : " + outLink + "\n");


      // -------------- pre-build task --------------

      VCPreBuildEventTool preBuildTool = (VCPreBuildEventTool)fTools.Item("VCPreBuildEventTool");
      VCPostBuildEventTool postBuildTool = (VCPostBuildEventTool)fTools.Item("VCPostBuildEventTool");
      VCPreLinkEventTool preLinkTool = (VCPreLinkEventTool)fTools.Item("VCPreLinkEventTool");


      if (!preBuildTool.ExcludedFromBuild && 
          preBuildTool.CommandLine != null && 
          preBuildTool.CommandLine.Length > 0)
      {
        WriteToOutputPane("\nPre-Build Step : " + preBuildTool.Description + "\n");
        ExecuteCustomBuildToolCommandLine<VCConfiguration>(preBuildTool.CommandLine, vcCfg, vcProject);
      }     


      // -------------- log out a batch file of build commands --------------

      System.IO.StreamWriter buildCommandFile = null;
      if (m_doGenerateBatchFiles)
      {
        String buildCommandFileName = vcProject.ProjectDirectory + "\\clang_build_" + vcProject.Name.Replace(" ", "_") + "_" + vcCfg.Name.Replace("|", "-").Replace(" ", "_") + ".bat";
        buildCommandFile = new System.IO.StreamWriter(buildCommandFileName);
      }


      // -------------- prepare to walk project files looking for work --------------

      int numCompilerErrors = 0;
      StringBuilder objectFilesToLink = new StringBuilder(1024);
      IVCCollection vcFileCollection = (IVCCollection)vcProject.Files;


      // -------------- custom build steps --------------

      WriteToOutputPane("\nCustom Build Steps...\n");

      // while walking the project for custom build steps, may as well gather code files to build later on
      Dictionary<VCFile, VCFileConfiguration> codeFilesToCompile = new Dictionary<VCFile, VCFileConfiguration>();

      // go looking for any relevant custom build steps
      foreach (VCFile vcFile in vcFileCollection)
      {
        VCFileConfiguration vcFC = (VCFileConfiguration)((IVCCollection)vcFile.FileConfigurations).Item(vcCfg.ConfigurationName);
        if (vcFC.ExcludedFromBuild)
          continue;

        if ((vcFC.Tool as VCCustomBuildTool) != null)
        {
          VCCustomBuildTool customBuildTool =  vcFC.Tool as VCCustomBuildTool;                    

          try
          {
            // a valid way to consider the custom tool to be 'active?
            if (customBuildTool.CommandLine.Length > 0)
            {
              RunCustomBuildStep(customBuildTool, vcFile, vcProject, vcFC, false);
            }
          }
          catch 
          {
          }
        }
        // do this cast-check here, saves re-doing the VCFileConfiguration look-up again later
        else if ((vcFC.Tool as VCCLCompilerTool) != null)
        {
          codeFilesToCompile.Add(vcFile, vcFC);
        }
        // compile resources on the first pass through
        else if ((vcFC.Tool as VCResourceCompilerTool) != null)
        {
          VCResourceCompilerTool resourceTool = (VCResourceCompilerTool)vcFC.Tool;
          String resourceFile = vcFC.Evaluate(resourceTool.ResourceOutputFileName).Replace("/", "\\");

          String sourceRCPath = (vcFC.File as VCFile).FullPath;
          String rcFile = System.IO.Path.GetFileNameWithoutExtension(sourceRCPath);
          resourceFile = resourceFile.Replace("%(Filename)", rcFile);

          if (!System.IO.Path.IsPathRooted(resourceFile))
            resourceFile = vcProject.ProjectDirectory + resourceFile;

          objectFilesToLink.Append("\"");
          objectFilesToLink.Append(resourceFile);
          objectFilesToLink.Append("\" ");

          WriteToOutputPane("\nCompiling Resources : " + sourceRCPath + "\n");
          vcFC.Compile(false, true);

          /*
          // make sure the directory exists, or we won't be able to copy the .res file there later
          String resourceTargetDir = System.IO.Path.GetDirectoryName(resourceFile);
          if (!System.IO.Directory.Exists(resourceTargetDir))
          {
            System.IO.Directory.CreateDirectory(resourceTargetDir);
          }
           
          // i would just use vcFC.Compile() here but it doesn't seem to work very reliably
          StringBuilder rcCompilerArgs = new StringBuilder(" /r ");

          // convert pre-pro defines
          String resSymb = resourceTool.PreprocessorDefinitions.Replace(",", ";");          
          String[] resSymbs = resSymb.Split(';');
          foreach (string sym in resSymbs)
          {
            if (sym.Length > 0)
            {
              rcCompilerArgs.Append("/d ");
              rcCompilerArgs.Append(sym);
              rcCompilerArgs.Append(" ");
            }
          }

          // character set config
          if (vcCfg.CharacterSet == Microsoft.VisualStudio.VCProjectEngine.charSet.charSetUnicode)
          {
            rcCompilerArgs.Append("/d UNICODE /d _UNICODE ");
          }
          else if (vcCfg.CharacterSet == Microsoft.VisualStudio.VCProjectEngine.charSet.charSetMBCS)
          {
            rcCompilerArgs.Append("/d _MBCS ");
          }

          // can't tell how MSVS determines how to toggle this on...
          if (vcCfg.ProgramDatabase != null)
          {
            rcCompilerArgs.Append("/d DEBUG /d _DEBUG ");
          }
          else
          {
            rcCompilerArgs.Append("/d NDEBUG ");
          }

          String incPath = resourceTool.FullIncludePath.Replace(",", ";");
          String[] incPaths = incPath.Split(';');
          foreach (string inc in incPaths)
          {
            if (inc.Length > 0)
            {
              rcCompilerArgs.Append("/i \"");
              rcCompilerArgs.Append(inc);
              rcCompilerArgs.Append("\" ");
            }
          }

          rcCompilerArgs.Append(resourceTool.AdditionalOptions);

          rcCompilerArgs.Append(" \"");
          rcCompilerArgs.Append(vcFile.FullPath);
          rcCompilerArgs.Append("\"");

          if (m_doShowCommands)
            WriteToOutputPane("\nRunning RC Compiler : " + rcCompilerArgs.ToString() + "\n");
          else
            WriteToOutputPane("\nCompiling Resources : " + vcFile.Name + "\n");


          String rcExePath = System.IO.Path.GetDirectoryName(m_locationClangEXE) + "\\rc.exe";

          System.Diagnostics.Process resCompilerProcess = NewExternalProcess();
          resCompilerProcess.StartInfo.FileName = rcExePath;
          resCompilerProcess.StartInfo.Arguments = rcCompilerArgs.ToString();
          resCompilerProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
          resCompilerProcess.Start();

          String outputStr = resCompilerProcess.StandardOutput.ReadToEnd();
          resCompilerProcess.WaitForExit();


          try
          {
            if (System.IO.File.Exists(resourceFile))
              System.IO.File.Delete(resourceFile);

            // RC compiler just seems to dump the .res next to the .rc file, so move it to the chosen location manually
            String pathToCompiledRES = System.IO.Path.ChangeExtension(vcFile.FullPath, ".res");
            System.IO.File.Move(pathToCompiledRES, resourceFile);
          }
          catch (System.Exception e)
          {
            WriteToOutputPane("\nError during RC compilation : " + e.Message + "\n");
          }
          */
        }
      }


      // -------------- compiling --------------
      
      WriteToOutputPane("\n" + strDivider + "\nCompiling :\n");

      foreach (KeyValuePair<VCFile, VCFileConfiguration> kvp in codeFilesToCompile)
      {
        StringBuilder compileString = new StringBuilder(1024);

        if (!InternalBuildVCFile(kvp.Key, vcProject, vcCfg, defaultCompilerString, ref compileString, ref objectFilesToLink))
          numCompilerErrors++;

        // log to the build file
        if (buildCommandFile != null)
          buildCommandFile.WriteLine(m_locationClangEXE + " " + compileString.ToString());

        if (numCompilerErrors > 5)
          break;        
      }

      if (buildCommandFile != null)
        buildCommandFile.Flush();

      if (numCompilerErrors > 0)
      {
        WriteToOutputPane("\n" + numCompilerErrors.ToString() + " Compilation Problem(s), Skipping Link.\n");
        System.Windows.Forms.Application.DoEvents();

        if (buildCommandFile != null)
          buildCommandFile.Close();
        return false;
      }
      else
      {
        WriteToOutputPane("\nCompilation Successful\n");
      }


      // -------------- pre-link task --------------

      if (!preLinkTool.ExcludedFromBuild &&
          preLinkTool.CommandLine != null &&
          preLinkTool.CommandLine.Length > 0)
      {
        WriteToOutputPane("\nPre-Link Step : " + preLinkTool.Description + "\n");
        ExecuteCustomBuildToolCommandLine<VCConfiguration>(preLinkTool.CommandLine, vcCfg, vcProject);
      }


      // -------------- linking --------------

      WriteToOutputPane("\n" + strDivider + "\nLinking :\n");
      if (buildCommandFile != null)
        buildCommandFile.WriteLine("\n\n");

      StringBuilder linkString = new StringBuilder(1024);
      System.Diagnostics.Process linkProcess = NewExternalProcess();

      // standard inputs and outputs
      linkString.Append("/OUT:\"");
      linkString.Append(outLink);
      linkString.Append("\" ");
      linkString.Append(objectFilesToLink.ToString().TrimEnd(' ').Replace("/", "\\"));
      linkString.Append(" ");

      // link to application / DLL
      // LINK obj[,out[,map[,lib[,def[,res]]]]] 
      if (vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeApplication ||
          vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
      {
        linkProcess.StartInfo.FileName = m_locationLINKExe;

        // defaults
        linkString.Append("/NOLOGO /MACHINE:X86 /DYNAMICBASE ");

        if (vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeDynamicLibrary)
        {
          if (!String.IsNullOrEmpty(vcLinkerTool.ImportLibrary))
          {
            // import library setting for DLLs
            linkString.Append("/IMPLIB:\"");
            linkString.Append(vcCfg.Evaluate(vcLinkerTool.ImportLibrary).Replace("/", "\\"));
            linkString.Append("\" ");
          }

          linkString.Append("/DLL ");
        }
        else
        {
          // subsystem target
          if (vcLinkerTool.SubSystem == subSystemOption.subSystemConsole)
          {
            linkString.Append("/SUBSYSTEM:CONSOLE ");
          }
          else if (vcLinkerTool.SubSystem == subSystemOption.subSystemWindows)
          {
            linkString.Append("/SUBSYSTEM:WINDOWS ");
          }
        }

        if (vcLinkerTool.GenerateDebugInformation)
        {
          linkString.Append("/DEBUG ");
        }

        if (vcLinkerTool.OptimizeReferences == optRefType.optReferences)
        {
          linkString.Append("/OPT:REF ");
        }

        if (vcLinkerTool.IgnoreAllDefaultLibraries)
        {
          linkString.Append("/NODEFAULTLIB ");
        }

        // get the linker to spew info
        if (vcLinkerTool.ShowProgress != linkProgressOption.linkProgressNotSet)
        {
          linkString.Append("/INFORMATION ");
        }

        if (vcLinkerTool.EnableCOMDATFolding == optFoldingType.optNoFolding)
        {
          linkString.Append("/NOPACKFUNCTIONS ");
        }

        // add the 'additional options' verbatim
        linkString.Append(vcCfg.Evaluate(vcLinkerTool.AdditionalOptions) + " ");


        // we want to create a unique list of libraries to link alongside our object files
        HashSet<String> librariesToLink = new HashSet<String>();

        // sort out libraries for the runtime library choice
        {
          // Static MultiThread          /MT       LIBCMT     _MT
          // Debug Static MultiThread    /MTd      LIBCMTD    _DEBUG and _MT
          // 
          // Dynamic Link (DLL)          /MD       MSVCRT     _MT and _DLL
          // Debug Dynamic Link (DLL)    /MDd      MSVCRTD    _DEBUG, _MT, and _DLL
          //
          switch (vcCTool.RuntimeLibrary)
          {
            case runtimeLibraryOption.rtMultiThreaded:
              librariesToLink.Add("libcmt.lib");
              break;
            case runtimeLibraryOption.rtMultiThreadedDebug:
              librariesToLink.Add("libcmtd.lib");
              break;
            case runtimeLibraryOption.rtMultiThreadedDLL:
              librariesToLink.Add("msvcrt.lib");
              break;
            case runtimeLibraryOption.rtMultiThreadedDebugDLL:
              librariesToLink.Add("msvcrtd.lib");
              break;
          }
        }

        // gather system + additional libs
        try
        {
          IVCRulePropertyStorage generalRule = (IVCRulePropertyStorage)vcCfg.Rules.Item("Link");
          String additionalDependency = generalRule.GetEvaluatedPropertyValue("AdditionalDependencies");

          if (vcLinkerTool.AdditionalDependencies != null)
          {
            additionalDependency += ";" + vcLinkerTool.AdditionalDependencies;
          }
          if (!String.IsNullOrEmpty(additionalDependency))
          {
            String[] cAddIncDirs = vcCfg.Evaluate(additionalDependency).Split(new char[] {';', ' '});
            foreach (string pd in cAddIncDirs)
            {
              if (!String.IsNullOrEmpty(pd))
              {
                librariesToLink.Add(pd.Trim());
              }
            }
          }
        }
        catch (Exception e)
        {
          WriteToOutputPane("Exception during 'AdditionalDependencies' extraction:\n");
          WriteToOutputPane(e.Message + "\n");
        }

        // we have now got a list of additional libraries to hand to the linker, so add them
        // to the linker argument list
        foreach (String lib in librariesToLink)
        {
          linkString.Append(lib);
          linkString.Append(" ");
        }

        // optional map file
        if (vcLinkerTool.GenerateMapFile)
        {
          linkString.Append("/MAP");
          if (!String.IsNullOrEmpty(vcLinkerTool.MapFileName))
          {
            linkString.Append(":\"");
            linkString.Append(vcCfg.Evaluate(vcLinkerTool.MapFileName).Replace("/", "\\"));
            linkString.Append("\" ");
          }
        }

        // .def files for DLLs
        if (!String.IsNullOrEmpty(vcLinkerTool.ModuleDefinitionFile))
        {
          linkString.Append("/DEF:\"");
          linkString.Append(vcCfg.Evaluate(vcLinkerTool.ModuleDefinitionFile).Replace("/", "\\"));
          linkString.Append("\" ");
        }

        // add extra search directories
        {
          String systemLibDirs = vcCfg.Evaluate("$(LibraryPath)");
          if (vcLinkerTool.AdditionalLibraryDirectories != null)
          {
            systemLibDirs += ";" + vcLinkerTool.AdditionalLibraryDirectories;
          }
          if (!String.IsNullOrEmpty(systemLibDirs))
          {
            StringBuilder newLibEnv = new StringBuilder(512);
            String cAddIncDir = systemLibDirs.Replace(",", ";");
            String[] cAddIncDirs = cAddIncDir.Split(';');

            foreach (string pd in cAddIncDirs)
            {
              if (!String.IsNullOrEmpty(pd))
              {
                newLibEnv.Append("/LIBPATH:\"");
                newLibEnv.Append(pd.Replace("/", "\\"));
                newLibEnv.Append("\" ");
              }
            }

            linkString.Append(newLibEnv);
          }
        }

        // otherwise...
      }
      else if (vcCfg.ConfigurationType == Microsoft.VisualStudio.VCProjectEngine.ConfigurationTypes.typeStaticLibrary)
      {
        // link to static library
        linkProcess.StartInfo.FileName = m_locationLIBExe;
      }

      // dump the linker cmdline
      if (buildCommandFile != null)
      {
        buildCommandFile.WriteLine(linkProcess.StartInfo.FileName + " " + linkString + "\n\npause\n");
        buildCommandFile.Close();
      }

      // dump to the output window too, why not
      WriteToOutputPane(linkString + "\n");


      // put the linker options into a file, we only have ~2000 characters to play with if sending direct which will
      // overflow quickly on any decent sized project
      String respFileName = System.IO.Path.GetRandomFileName();
      String respFilePath = vcProject.ProjectDirectory + "\\" + respFileName;
      System.IO.StreamWriter respFile = new System.IO.StreamWriter(respFilePath);
      respFile.WriteLine(linkString);
      respFile.Close();


      linkProcess.StartInfo.EnvironmentVariables["PATH"] += ";" + m_pathToVS10Tools + ";" + m_pathToVS10CommonIDE;

      // execute the compiler
      linkProcess.StartInfo.Arguments = "@" + respFilePath;
      linkProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
      linkProcess.Start();

      // read any output from the compiler, indicative of some problem...
      String outputStrLink = linkProcess.StandardOutput.ReadToEnd();
      linkProcess.WaitForExit();

      // kill the temporary linker resp file
      System.IO.File.Delete(respFilePath);

      if (linkProcess.ExitCode != 0)
      {
        WriteToOutputPane("Linking failed.\n" + outputStrLink + "\n");
      }
      else
      {
        // test that we got something out of the linker; it will delete the result if something bad happened (that we didn't catch above)
        // find a full pathname for the file, relative or not
        String fullResultPath = outLink;
        if (!System.IO.Path.IsPathRooted(fullResultPath))
          fullResultPath = vcProject.ProjectDirectory + outLink;

        if (System.IO.File.Exists(fullResultPath))
        {
          WriteToOutputPane("\nLinking Successful\n");

          System.IO.FileInfo fi = new System.IO.FileInfo(fullResultPath);
          WriteToOutputPane("Result Size(b) : " + fi.Length.ToString() + "\n");
        }
        else
        {
          WriteToOutputPane("\nLinking Successful? - could not find output file to verify success!\n");
        }


        // -------------- post-build task --------------

        if (!postBuildTool.ExcludedFromBuild && !String.IsNullOrEmpty(postBuildTool.CommandLine))
        {
          WriteToOutputPane("\nPost-Build Step : " + postBuildTool.Description + "\n");
          ExecuteCustomBuildToolCommandLine<VCConfiguration>(postBuildTool.CommandLine, vcCfg, vcProject);
        }
      }

      return true;
    }
  }
}
