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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.VCProjectEngine;
using Process = System.Diagnostics.Process;

namespace ClangVSx
{
  /// <summary>
  /// Collection of build tools
  /// </summary>
  internal class CVXBuildSystem
  {
    private readonly String ConsoleDivider = String.Empty;
    private readonly bool DoGenerateBatchFiles;
    private readonly bool DoShowCommands;
    private readonly String LocationClangEXE;
    private readonly String LocationLIBExe;
    private readonly String LocationLINKExe;
    private readonly String LocationLLVM_LINK_EXE;
    private readonly String LocationLLVM_LLC_EXE;
    private readonly String PathToVSCommonIDE;
    private readonly String PathToVSTools;
    private StreamWriter BatchFileStream; // if DoGenerateBatchFiles == true, this is set to the output file stream
    protected OutputWindowPane OutputPane; // output panel configured by AddIn, passed in to ctor
    protected Window VSOutputWindow;


    public CVXBuildSystem(Window clangOutputWindow, OutputWindowPane clangOutputPane)
    {
      VSOutputWindow = clangOutputWindow;
      OutputPane = clangOutputPane;


      ConsoleDivider = ConsoleDivider.PadLeft(128, '-');

      // clear and focus the output window
      OutputPane.Clear();
      ShowOutputPane();

      // write out a version header
      {
        Assembly assem = Assembly.GetExecutingAssembly();
        Version vers = assem.GetName().Version;
        DateTime buildDate = new DateTime(2000, 1, 1).AddDays(vers.Build).AddSeconds(vers.Revision * 2);
        WriteToOutputPane("ClangVSx " + vers.Major.ToString() + "." + vers.Minor.ToString() +
                          " | LLVM/Clang C++ Compiler Bridge | www.ishani.org\n\n");

        // try to give the message queue time to bring up the output window
        Application.DoEvents();
      }

      // read out current executables' locations
      LocationClangEXE = CVXRegistry.PathToClang;
      LocationLLVM_LINK_EXE = Path.GetDirectoryName(LocationClangEXE) + @"\llvm-link.exe";
      LocationLLVM_LLC_EXE = Path.GetDirectoryName(LocationClangEXE) + @"\llc.exe";

      // work out where the MS linker / lib tools are, Clang/LLVM doesn't have a linker presently
      // eg "C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\Tools\"
#if CVSX_2013
      String pathToVS = Environment.GetEnvironmentVariable("VS120COMNTOOLS");
      if (pathToVS == null) {
          throw new Exception(
              "Could not find Visual Studio 2013 install directory (via VS120COMNTOOLS environment variable)");
      }
#endif
#if CVSX_2012
      String pathToVS = Environment.GetEnvironmentVariable("VS110COMNTOOLS");
      if (pathToVS == null)
      {
        throw new Exception(
            "Could not find Visual Studio 2012 install directory (via VS110COMNTOOLS environment variable)");
      }
#endif 
#if CVSX_2010
      String pathToVS = Environment.GetEnvironmentVariable("VS100COMNTOOLS");
      if (pathToVS == null)
      {
        throw new Exception(
            "Could not find Visual Studio 2010 install directory (via VS100COMNTOOLS environment variable)");
      }
#endif

      // have to go digging for the location of the MSVC linker/librarian
      PathToVSTools = Path.GetFullPath(pathToVS + @"..\..\VC\bin\");
      PathToVSCommonIDE = Path.GetFullPath(pathToVS + @"..\IDE\");
      LocationLIBExe = PathToVSTools + "lib.exe";
      LocationLINKExe = PathToVSTools + "link.exe";

      var PathCheck = new[] {
                                LocationLLVM_LINK_EXE,
                                LocationLLVM_LLC_EXE,
                                LocationClangEXE,
                                LocationLIBExe,
                                LocationLINKExe
                            };
      foreach (String path in PathCheck)
      {
        if (!File.Exists(path))
        {
          throw new Exception(String.Format("Could not find {0}, check ClangVSx Settings - tried:\n\n{1}",
                                            Path.GetFileName(path), path));
        }
      }

      // read more options from the registry
      DoShowCommands = CVXRegistry.ShowCommands;
      DoGenerateBatchFiles = CVXRegistry.MakeBatchFiles;
    }

    #region Utils

    internal void ShowOutputPane()
    {
      VSOutputWindow.Activate();
      OutputPane.Activate();
      Application.DoEvents();
    }

    internal void WriteToOutputPane(string text)
    {
      OutputPane.OutputString(text);
    }

    /// <summary>
    /// simple helper that creates a Process instance ready for running the compiler, linker, external tools, etc
    /// </summary>
    internal Process NewExternalProcess()
    {
      var result = new Process();
      result.StartInfo.UseShellExecute = false;
      result.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
      result.StartInfo.CreateNoWindow = true;
      result.StartInfo.RedirectStandardOutput = true;
      result.StartInfo.RedirectStandardError = true;
      return result;
    }

    private VCFileConfiguration GetFileConfiguration(IVCCollection configs, VCConfiguration cfg)
    {
      try
      {
        foreach (VCFileConfiguration vcr in configs)
        {
          if (vcr.ProjectConfiguration.ConfigurationName == cfg.ConfigurationName &&
              vcr.ProjectConfiguration.Platform.Name == cfg.Platform.Name)
          {
            return vcr;
          }
        }
      }
      catch (Exception)
      {
        WriteToOutputPane("Error: failed to resolve configuration for file - " + cfg.ConfigurationName + " | " +
                          cfg.Platform.Name + "\n");
        return null;
      }
      return null;
    }

    #endregion

    #region Custom Build Steps

    /// <summary>
    /// I'm probably showing C# inexperience here; I wanted this function to operate on any 
    /// object that had an Evaluate method, eg. VCConfiguration, VCFileConfiguration, et al.
    /// Reflection and generics is what I came up with!
    /// </summary>
    internal void ExecuteCustomBuildToolCommandLine<T>(String cmdLine, object evaluator, VCProject vcProject)
    {
      try
      {
        MethodInfo evalMethod = typeof(T).GetMethod("Evaluate");

        String[] cmdsToRun = cmdLine.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (String singleCmd in cmdsToRun)
        {
          var evaluatedCmd = evalMethod.Invoke(evaluator, new object[] { singleCmd }) as String;

          if (DoShowCommands)
          {
            WriteToOutputPane("  cmd.exe /C " + evaluatedCmd + "\n");
          }
          if (BatchFileStream != null)
          {
            BatchFileStream.WriteLine("@REM -- custom build tool --");
            BatchFileStream.WriteLine("  cmd.exe /C " + evaluatedCmd);
          }

          // run the specified tool
          Process cbStep = NewExternalProcess();
          cbStep.StartInfo.FileName = "cmd.exe";
          cbStep.StartInfo.Arguments = "/C " + evaluatedCmd.Replace("/", "\\");
          cbStep.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
          cbStep.Start();

          String outputStr = cbStep.StandardOutput.ReadToEnd();
          WriteToOutputPane(outputStr);

          cbStep.WaitForExit();
        }
      }
      catch (Exception e)
      {
        WriteToOutputPane("Exception During Execution : " + cmdLine + "\n" + e.Message + "\n" + e.Source +
                          "\n\n");
      }
    }

    /// <summary>
    /// bootstrap any custom build steps, run via cmd.exe
    /// </summary>
    internal void RunCustomBuildStep(VCCustomBuildTool customBuildTool, VCFile vcFile, VCProject vcProject,
                                     VCFileConfiguration vcFC, bool ignoreTimeStamp)
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
          var vcFileFI = new FileInfo(vcFile.FullPath);

          // cut the output files string into individual items
          String[] outputFiles = customBuildTool.Outputs.Split(new[] { ' ', ',', ';' },
                                                               StringSplitOptions.RemoveEmptyEntries);
          foreach (String outputFileName in outputFiles)
          {
            // find a full pathname for the file, relative or not
            String finalOutput = vcFC.Evaluate(outputFileName);
            if (!Path.IsPathRooted(outputFileName))
              finalOutput = vcProject.ProjectDirectory + outputFileName;

            String finalOutputDir = Path.GetDirectoryName(finalOutput);
            if (!Directory.Exists(finalOutputDir))
            {
              Directory.CreateDirectory(finalOutputDir);
            }

            // if it doesn't exist, safe to assume we should try and build it
            if (!File.Exists(finalOutput))
            {
              outputFilesNeedBuilding = true;
              break;
            }

            // compare timestamps with source file
            var outputFileFI = new FileInfo(finalOutput);
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

    #endregion

    #region VCFile Building

    /// <summary>
    /// internal function that does the work of compiling a C++ file using Clang
    /// </summary>
    internal bool InternalBuildVCFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg,
                                      String defaultCompilerString, ref StringBuilder compileString,
                                      HashSet<String> CompilationArtifacts, bool dryRun = false)
    {
      // get access to the per-file VCCL config data (eg stuff that augments / overwrites the global settings)
      // we use this to configure the compiler per file, as it should reflect the final config state of the file
      VCFileConfiguration vcFC = GetFileConfiguration((IVCCollection)vcFile.FileConfigurations, vcCfg);
      if (vcFC == null)
        return false;

      var perFileVCC = (VCCLCompilerTool)vcFC.Tool;

      // begin forming the command line string to send to Clang
      compileString.Append('"' + vcFile.FullPath.Replace("/", "\\") + '"');
      compileString.Append(defaultCompilerString);

      // sort out an output object file name
      String objectFileName = vcFC.Evaluate(perFileVCC.ObjectFile).Replace("/", "\\");
      objectFileName = Path.GetFullPath(objectFileName);

      String prebuild = Path.GetDirectoryName(objectFileName);
      if (!Directory.Exists(prebuild))
      {
        Directory.CreateDirectory(prebuild);
      }

      String outputFileExtension = ".obj";
      if (perFileVCC.WholeProgramOptimization)
      {
        outputFileExtension = ".bc";
        compileString.Append("-flto ");
      }

      // check to see if we should emit an obj at all
      String compileCmds = compileString.ToString();
      if (!compileCmds.Contains("-o\""))
      {
        // if no obj file specified, make a suitable filename (is this a robust check..??)
        if (!Path.HasExtension(objectFileName))
        {
          objectFileName += Path.ChangeExtension(vcFile.Name, outputFileExtension);
        }
        else
        {
          // force our extension?
          objectFileName = Path.ChangeExtension(objectFileName, outputFileExtension);
        }

        // record the .obj target in the cmd line and in the list to pass to the linker
        compileString.Append("-o\"");
        compileString.Append(objectFileName);
        compileString.Append("\" ");

        CompilationArtifacts.Add(objectFileName);
      }

      if (dryRun)
        return true;

      // stick the per-file defines, includes, etc 
      compileString.Append(GatherIncludesAndDefines<VCFileConfiguration>(perFileVCC, vcFC));

      // figure out a suitable mapping for optimization flags
      switch (perFileVCC.Optimization)
      {
        case optimizeOption.optimizeDisabled:
          compileString.Append("-O0 ");
          break;
        case optimizeOption.optimizeMaxSpeed:
          compileString.Append("-O2 ");
          break;
        case optimizeOption.optimizeFull:
          compileString.Append("-O3 ");
          break;
        case optimizeOption.optimizeMinSpace:
          compileString.Append("-Oz "); // opting for the 'smallest size at any cost' option
          break;
      }

      if (perFileVCC.OmitFramePointers || perFileVCC.Optimization == optimizeOption.optimizeFull)
      {
        compileString.Append("-fomit-frame-pointer ");
      }

      // arbitrarily turning a 5-point dial into a toggle switch :)
      if (perFileVCC.WarningLevel == warningLevelOption.warningLevel_0 ||
          perFileVCC.WarningLevel == warningLevelOption.warningLevel_1 ||
          perFileVCC.WarningLevel == warningLevelOption.warningLevel_2)
      {
        compileString.Append("-w "); // no warnings
      }
      else if (perFileVCC.WarningLevel == warningLevelOption.warningLevel_4)
      {
        compileString.Append("-Wall --pedantic "); // all warnings
      }

      if (perFileVCC.WarnAsError)
      {
        compileString.Append("-Werror ");
      }

      // stack overflow checking
      if (perFileVCC.BufferSecurityCheck)
      {
        // HDD TODO incompatible with MSVC atm
        // compileString.Append("-fstack-protector-all ");
      }
      else
      {
        compileString.Append("-fno-stack-protector ");
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

      // asm listing or preprocessor output both turn on the same flag, as the flag produces them both
      if (perFileVCC.AssemblerOutput != asmListingOption.asmListingNone ||
          perFileVCC.GeneratePreprocessedFile != preprocessOption.preprocessNo)
      {
        // -S ensures we just preprocess
        // -CC leaves comments in the output
        compileString.Append("-save-temps -S -Xpreprocessor -CC ");
      }

      string extension = (Path.GetExtension(vcFile.Name) ?? "").ToLowerInvariant();
      if (perFileVCC.CompileAs == CompileAsOptions.compileAsCPlusPlus || (perFileVCC.CompileAs == CompileAsOptions.compileAsDefault && extension == ".cpp"))
      {
          // compiler options from the settings page
          compileString.Append(CVXRegistry.COptCPP14 ? "-std=c++14 " : "-std=c++11 ");

          compileString.Append("-x c++ ");

        // RTTI disable?
        if (!perFileVCC.RuntimeTypeInfo)
        {
          compileString.Append("-fno-rtti ");
        }
        else
        {
          compileString.Append("-frtti ");
        }

        // EH
        try
        {
          // this explodes if it's set to 'off', MSBuild conversion layer problems :|
          if (perFileVCC.ExceptionHandling != cppExceptionHandling.cppExceptionHandlingNo)
          {
            compileString.Append("-fexceptions -fcxx-exceptions ");
          }
        }
        catch
        {
          compileString.Append("-fno-exceptions ");
        }
      }
      else if (perFileVCC.CompileAs == CompileAsOptions.compileAsC || (perFileVCC.CompileAs == CompileAsOptions.compileAsDefault && extension == ".c"))
      {
          compileString.Append(CVXRegistry.COptC11 ? "-std=c11 " : "-std=c99 ");
      }

      // add the 'additional options' verbatim
      if (perFileVCC.AdditionalOptions != null)
      {
        // add the 'additional options' verbatim
        // only pull across opts that begin with "-", ignoring VS "/" ones
        string[] opts = vcFC.Evaluate(perFileVCC.AdditionalOptions).Split(' ');
        foreach (string opt in opts)
        {
          if (opt.StartsWith("-"))
            compileString.Append(opt + " ");
        }
      }

      // ask for warnings/errors in MSVC format
      compileString.Append("-fdiagnostics-format=msvc ");

      String fullObjectPath = objectFileName;
      if (!Path.IsPathRooted(fullObjectPath))
        fullObjectPath = vcProject.ProjectDirectory + objectFileName;


      WriteToOutputPane(" - " + vcFile.UnexpandedRelativePath);

      if (DoShowCommands)
        WriteToOutputPane("\n    " + compileString + "\n");


      // crank the message pump; means our Output Window stuff should update properly
      Application.DoEvents();

      // execute the compiler
      Process compileProcess = NewExternalProcess();
      compileProcess.StartInfo.FileName = LocationClangEXE;
      compileProcess.StartInfo.Arguments = compileString.ToString();
      compileProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
      compileProcess.Start();

      // read any output from the compiler, indicative of some problem...
      String outputStr = compileProcess.StandardError.ReadToEnd();
      compileProcess.WaitForExit();

      if (outputStr.Length > 0)
      {
        WriteToOutputPane("\n" + outputStr);
        WriteToOutputPane(ConsoleDivider + "\n\n");

        if (outputStr.Contains("error:"))
          return false;
      }
      else
      {
        if (!DoShowCommands)
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

    #endregion

    #region Data Gathering

    /// <summary>
    /// 
    /// </summary>
    internal string GatherIncludesAndDefines<T>(VCCLCompilerTool vcCTool, object evaluator)
    {
      MethodInfo evalMethod = typeof(T).GetMethod("Evaluate");
      var result = new StringBuilder();

      // push additional includes into the default compiler string
      if (vcCTool.AdditionalIncludeDirectories != null)
      {
        String cIncDir = vcCTool.AdditionalIncludeDirectories.Replace(",", ";");
        String[] cIncDirs = cIncDir.Split(';');

        foreach (string inc in cIncDirs)
        {
          if (inc.Length > 0)
          {
            var parsedInc = (evalMethod.Invoke(evaluator, new object[] { inc }) as String);
            if (parsedInc != null && parsedInc.Length > 0)
            {
              result.Append("-I");
              String incCheck = parsedInc.Replace("\\", "/");

              // Clang doesn't like the trailing / on these
              if (incCheck == "./") incCheck = ".";
              if (incCheck == "../") incCheck = "..";

              // resolve any relative paths
              incCheck = Path.GetFullPath(incCheck);

              result.Append("\"");
              result.Append(incCheck);
              result.Append("\" ");
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
            var parsedInc = (evalMethod.Invoke(evaluator, new object[] { inc }) as String);
            if (parsedInc != null && parsedInc.Length > 0)
            {
              String incCheck = parsedInc.Replace("\\", "/");

              // resolve any relative paths
              incCheck = Path.GetFullPath(incCheck);

              result.Append("-include \"");
              result.Append(incCheck);
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
            var parsedPd = (evalMethod.Invoke(evaluator, new object[] { pd }) as String);
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

    /// <summary>
    /// common code to generate compiler settings for non-file-specific stuff
    /// </summary>
    internal String GenerateDefaultCompilerString(VCProject vcProject, VCConfiguration vcCfg)
    {
      // per cpp-file compiler setting default:
      //
      // -c for individual compilation without auto-linking
      //
      // -nostdinc to stop the compiler adding VS include directories itself; we want to control that
      //
      // define '__CLANG_VSX__'
      //
      var defaultCompilerString = new StringBuilder(" -c -nostdinc -D__CLANG_VSX__ ");

      String targetCmdOpt = "target";

      if (vcCfg.Platform.Name == "Win32")
        defaultCompilerString.AppendFormat("-{0} {1} ", targetCmdOpt, CVXRegistry.TripleWin32.Value);
      else if (vcCfg.Platform.Name == "x64")
        defaultCompilerString.AppendFormat("-{0} {1} ", targetCmdOpt, CVXRegistry.TripleX64.Value);
      else if (vcCfg.Platform.Name == "ARM")
        defaultCompilerString.AppendFormat("-{0} {1} ", targetCmdOpt, CVXRegistry.TripleARM.Value);

      if (CVXRegistry.ShowPhases)
      {
        defaultCompilerString.Append("-ccc-print-phases ");
      }

      if (CVXRegistry.COptSLPAgg)
      {
        defaultCompilerString.Append("-fslp-vectorize-aggressive ");
      }
      if (CVXRegistry.VerboseVectorize)
      {
        defaultCompilerString.Append("-Rpass=loop-vectorize -Rpass-missed=loop-vectorize -Rpass-analysis=loop-vectorize ");
      }


      // add the UNICODE defines?
      if (vcCfg.CharacterSet == charSet.charSetUnicode)
      {
        defaultCompilerString.Append("-DUNICODE -D_UNICODE ");
      }
      else if (vcCfg.CharacterSet == charSet.charSetMBCS)
      {
        defaultCompilerString.Append("-D_MBCS ");
      }

      // get the various VC tools to read out setup
      var fTools = (IVCCollection)vcCfg.Tools;
      var vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");
      var vcLinkerTool = (VCLinkerTool)fTools.Item("VCLinkerTool");
      var vcLibTool = (VCLibrarianTool)fTools.Item("VCLibrarianTool");

      // convert the project-wide includes, prepro defines, force includes
      defaultCompilerString.Append(GatherIncludesAndDefines<VCConfiguration>(vcCTool, vcCfg));

      String plt = vcCfg.Platform.Name;

      // sort out prolog/epilog settings based on cfg/subsystem
      if (vcCfg.ConfigurationType == ConfigurationTypes.typeApplication)
      {
      }
      else if (vcCfg.ConfigurationType ==
               ConfigurationTypes.typeDynamicLibrary)
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
        // only pull across opts that begin with "++"; we have to be specific here because VS has both / and -
        // prefixed arguments, so ++<arg> means we flag up clang-specific args we want to pass in ..
        // sadly that also means we can't interop smoothly with VS as it will reject "++", but this is an edge case anyway
        string[] opts = vcCfg.Evaluate(vcCTool.AdditionalOptions).Split(' ');
        foreach (string opt in opts)
        {
          if (opt.StartsWith("++"))
            defaultCompilerString.Append(opt.Substring(2) + " ");
        }
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

        // add the fully resolved directories into a string set, to weed out any duplicates
        var uniqueDirs = new HashSet<String>();
        foreach (string inc in cIncDirs)
        {
          if (inc.Length > 0)
          {
            uniqueDirs.Add(Path.GetFullPath(inc.Replace("\\", "/")));
          }
        }

        foreach (String inc in uniqueDirs)
        {
          defaultCompilerString.Append("-isystem \"");
          defaultCompilerString.Append(inc);
          defaultCompilerString.Replace("\\", "", defaultCompilerString.Length - 1, 1);
          defaultCompilerString.Append("\" ");
        }
      }
      defaultCompilerString.Append(" -fms-compatibility ");
      defaultCompilerString.Append(" -fms-extensions ");
      defaultCompilerString.Append(" -fmsc-version=1800 ");


      return defaultCompilerString.ToString();
    }

    #endregion

    public bool CompileSingleFile(VCFile vcFile, VCProject vcProject, VCConfiguration vcCfg,
                                  String additionalCmds = "")
    {
      var defaultCompilerString = new StringBuilder(GenerateDefaultCompilerString(vcProject, vcCfg));
      defaultCompilerString.Append(" ");
      defaultCompilerString.Append(additionalCmds);
      defaultCompilerString.Append(" ");

      var compileString = new StringBuilder(1024);
      var Artifacts = new HashSet<String>();

      if (InternalBuildVCFile(vcFile, vcProject, vcCfg, defaultCompilerString.ToString(), ref compileString,
                              Artifacts))
      {
        WriteToOutputPane("\nCompile Successful\n");
        return true;
      }
      return false;
    }


    /// <summary>
    /// main worker function to build a project, swapping in our own tools as desired
    /// </summary>
    public bool BuildProject(VCProject vcProject, VCConfiguration vcCfg, bool justLink)
    {
      String defaultCompilerString = GenerateDefaultCompilerString(vcProject, vcCfg);

      // get the various VC tools to read out setup
      var fTools = (IVCCollection)vcCfg.Tools;
      var vcLinkerTool = (VCLinkerTool)fTools.Item("VCLinkerTool");
      var vcLibTool = (VCLibrarianTool)fTools.Item("VCLibrarianTool");

      if (vcLinkerTool != null)
      {
        if (vcLinkerTool.SubSystem != subSystemOption.subSystemWindows &&
            vcLinkerTool.SubSystem != subSystemOption.subSystemConsole)
        {
          WriteToOutputPane("Unsupported subsystem type - " + vcLinkerTool.SubSystem.ToString() +
                            " - (Windows/Console only)\n");
          return false;
        }
      }

      // work out the path to the final linking result, report it
      String outLink = "";
      switch (vcCfg.ConfigurationType)
      {
        case ConfigurationTypes.typeApplication:
          outLink = vcCfg.Evaluate(vcLinkerTool.OutputFile);
          break;
        case ConfigurationTypes.typeDynamicLibrary:
          outLink = vcCfg.Evaluate(vcLinkerTool.OutputFile);
          break;
        case ConfigurationTypes.typeStaticLibrary:
          outLink = vcCfg.Evaluate(vcLibTool.OutputFile);
          break;
      }
      outLink.Replace("/", "\\");

      // resolve path to be local to the location of the project file, if required
      outLink = Path.GetFullPath(outLink);

      WriteToOutputPane("Linker Output : " + outLink + "\n");

      String outLinkDirectory = Path.GetDirectoryName(outLink);
      if (!Directory.Exists(outLinkDirectory))
      {
        Directory.CreateDirectory(outLinkDirectory);
      }

      // ------------------ log out a batch file of build commands ------------------

      if (DoGenerateBatchFiles)
      {
        String batchFileName = "clang_build_" + vcProject.Name.Replace(" ", "_") + "_" +
                               vcCfg.Name.Replace("|", "-").Replace(" ", "_") + ".bat";

        String buildCommandFileName = Path.GetFullPath(vcProject.ProjectDirectory + "\\" + batchFileName);
        BatchFileStream = new StreamWriter(buildCommandFileName);

        WriteToOutputPane("Emitting Batch : " + batchFileName + "\n");
      }


      // ---------------------------- build prep ------------------------------------

      // go and look up all the files that will be taking part in this build in some way
      var ProjectFiles = new List<ProjectFile>(32);
      var vcFileCollection = (IVCCollection)vcProject.Files;
      foreach (VCFile vcFile in vcFileCollection)
      {
        VCFileConfiguration vcFC = GetFileConfiguration((IVCCollection)vcFile.FileConfigurations, vcCfg);
        if (vcFC == null)
          return false;

        try
        {
          if (vcFC.ExcludedFromBuild)
            continue;

          ProjectFiles.Add(new ProjectFile(vcFile, vcFC));
        }
        catch (Exception)
        {
          // in 2012 RC we get the .filters file in here which throws an exception when ExcludedFromBuild is accessed
          // not sure yet how best else to check for this...

          WriteToOutputPane("Skipping : " + vcFile.ItemName + "\n");
        }
      }

      var CompilationArtifacts = new HashSet<String>();


      // ----------------------------------------------------------------------------
      // the MSVC build order we will be attempting to emulate:
      // ----------------------------------------------------------------------------
      // Pre-Build event
      // Custom build tools on individual files
      // MIDL
      // Resource compiler
      // The C/C++ compiler
      // Pre-Link event
      // Linker or Librarian (as appropriate)
      // Manifest Tool                                  TBD
      // BSCMake                                        TBD
      // Custom build step on the project               TBD
      // Post-Build event
      // ----------------------------------------------------------------------------
      if (justLink)
      {
        // dry-run the phases that generate lists of build artifacts
        return (
                   doResourceCompiler(vcProject, vcCfg, ProjectFiles, CompilationArtifacts, true) &&
                   doCompileCPP(vcProject, vcCfg, ProjectFiles, CompilationArtifacts, true) &&
                   doPreLink(vcProject, vcCfg) &&
                   doLink(vcProject, vcCfg, outLink, CompilationArtifacts)
               );
      }
      else
      {
        return (
                   doPreBuildEvents(vcProject, vcCfg) &&
                   doCustomBuildStepPerFile(vcProject, vcCfg, ProjectFiles) &&
                   doMIDL(vcProject, vcCfg, ProjectFiles) &&
                   doResourceCompiler(vcProject, vcCfg, ProjectFiles, CompilationArtifacts) &&
                   doCompileCPP(vcProject, vcCfg, ProjectFiles, CompilationArtifacts) &&
                   doPreLink(vcProject, vcCfg) &&
                   doLink(vcProject, vcCfg, outLink, CompilationArtifacts) &&
                   doPostBuild(vcProject, vcCfg)
               );
      }
    }

    #region Build Steps

    internal bool doPreBuildEvents(VCProject vcProject, VCConfiguration vcCfg)
    {
      var fTools = (IVCCollection)vcCfg.Tools;
      var preBuildTool = (VCPreBuildEventTool)fTools.Item("VCPreBuildEventTool");

      if (!preBuildTool.ExcludedFromBuild &&
          preBuildTool.CommandLine != null &&
          preBuildTool.CommandLine.Length > 0)
      {
        WriteToOutputPane("\nPre-Build Step : " + preBuildTool.Description + "\n");
        ExecuteCustomBuildToolCommandLine<VCConfiguration>(preBuildTool.CommandLine, vcCfg, vcProject);
      }

      return true;
    }

    internal bool doCustomBuildStepPerFile(VCProject vcProject, VCConfiguration vcCfg,
                                           List<ProjectFile> ProjectFiles)
    {
      WriteToOutputPane("\nCustom Build Steps...\n");

      // go looking for any relevant custom build steps
      foreach (ProjectFile pf in ProjectFiles)
      {
        if ((pf.Config.Tool as VCCustomBuildTool) != null)
        {
          var customBuildTool = pf.Config.Tool as VCCustomBuildTool;
          try
          {
            // a valid way to consider the custom tool to be 'active?
            if (customBuildTool.CommandLine.Length > 0)
            {
              RunCustomBuildStep(customBuildTool, pf.File, vcProject, pf.Config, false);
            }
          }
          catch
          {
            // every non-standard file (.txt, .ico, etc) will have a CustomBuildTool as its Tool, it seems; 
            // but that instance doesn't support any actual interrogation, so when we access CommandLine above
            // it just barfs. This try/catch is here to make sure we find the real custom tools
          }
        }
      }

      return true;
    }

    internal bool doMIDL(VCProject vcProject, VCConfiguration vcCfg, List<ProjectFile> ProjectFiles)
    {
      foreach (ProjectFile pf in ProjectFiles)
      {
        if ((pf.Config.Tool as VCMidlTool) != null)
        {
          WriteToOutputPane("MIDL Compile : " + pf.File.Name);
          try
          {
            pf.Config.Compile(false, true);

            if (BatchFileStream != null)
              BatchFileStream.WriteLine(
                  String.Format("REM -- warning - missing MIDL step in batch file for\nREM   {0}",
                                pf.File.Name));
          }
          catch (Exception ex)
          {
            WriteToOutputPane("\nException during MIDL compilation :\n" + ex.Message + "\n");
            return false;
          }
        }
      }

      return true;
    }

    internal bool doResourceCompiler(VCProject vcProject, VCConfiguration vcCfg, List<ProjectFile> ProjectFiles,
                                     HashSet<String> CompilationArtifacts, bool dryRun = false)
    {
      foreach (ProjectFile pf in ProjectFiles)
      {
        if ((pf.Config.Tool as VCResourceCompilerTool) != null)
        {
          try
          {
            var resourceTool = (VCResourceCompilerTool)pf.Config.Tool;
            String resourceFile = pf.Config.Evaluate(resourceTool.ResourceOutputFileName).Replace("/", "\\");

            String sourceRCPath = pf.File.FullPath;
            String rcFile = Path.GetFileNameWithoutExtension(sourceRCPath);
            resourceFile = resourceFile.Replace("%(Filename)", rcFile);

            if (!Path.IsPathRooted(resourceFile))
              resourceFile = vcProject.ProjectDirectory + resourceFile;

            CompilationArtifacts.Add(resourceFile);

            if (!dryRun)
            {
              WriteToOutputPane("\nCompiling Resource : " + sourceRCPath + "\n");
              pf.Config.Compile(false, true);

              if (BatchFileStream != null)
                BatchFileStream.WriteLine(
                    String.Format(
                        "REM -- warning - missing resource compiler step in batch file for\nREM   {0}",
                        pf.File.Name));

              var fi = new FileInfo(resourceFile);
              WriteToOutputPane(String.Format("Compiled Resource File : '{0}' [{1} bytes]\n", resourceFile,
                                              fi.Length));
            }
          }
          catch (Exception ex)
          {
            WriteToOutputPane("\nException during resource compilation :\n" + ex.Message + "\n");
            return false;
          }
        }
      }

      return true;
    }

    internal bool doCompileCPP(VCProject vcProject, VCConfiguration vcCfg, List<ProjectFile> ProjectFiles,
                               HashSet<String> CompilationArtifacts, bool dryRun = false)
    {
      var fTools = (IVCCollection)vcCfg.Tools;
      var vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");

      String defaultCompilerString = GenerateDefaultCompilerString(vcProject, vcCfg);

      if (!dryRun)
        WriteToOutputPane("\n" + ConsoleDivider + "\nCompiling :\n");
      int numCompilerErrors = 0;

      foreach (ProjectFile pf in ProjectFiles)
      {
        if ((pf.Config.Tool as VCCLCompilerTool) != null)
        {
          var compileString = new StringBuilder(1024);

          if (
              !InternalBuildVCFile(pf.File, vcProject, vcCfg, defaultCompilerString, ref compileString,
                                   CompilationArtifacts, dryRun))
            numCompilerErrors++;

          // log to the build file
          if (BatchFileStream != null)
            BatchFileStream.WriteLine(String.Format("\"{0}\" {1}", LocationClangEXE, compileString));

          if (numCompilerErrors > 5)
            break;
        }
      }

      if (dryRun)
        return true;

      if (BatchFileStream != null)
        BatchFileStream.Flush();

      if (numCompilerErrors > 0)
      {
        WriteToOutputPane("\n" + numCompilerErrors.ToString() + " Compilation Problem(s), Skipping Link.\n");
        Application.DoEvents();

        if (BatchFileStream != null)
          BatchFileStream.Close();
        return false;
      }
      else
      {
        WriteToOutputPane("\nCompilation Successful\n");
      }

      return true;
    }

    internal bool doPreLink(VCProject vcProject, VCConfiguration vcCfg)
    {
      var fTools = (IVCCollection)vcCfg.Tools;
      var preLinkTool = (VCPreLinkEventTool)fTools.Item("VCPreLinkEventTool");

      if (!preLinkTool.ExcludedFromBuild &&
          preLinkTool.CommandLine != null &&
          preLinkTool.CommandLine.Length > 0)
      {
        WriteToOutputPane("\nPre-Link Step : " + preLinkTool.Description + "\n");
        ExecuteCustomBuildToolCommandLine<VCConfiguration>(preLinkTool.CommandLine, vcCfg, vcProject);
      }

      return true;
    }

    internal bool doLLVMPreLink(VCProject vcProject, VCConfiguration vcCfg, String outLink,
                                ref HashSet<String> CompilationArtifacts)
    {
      WriteToOutputPane("\nLLVM LTO Link :\n");

      string ltoOutputPath = vcCfg.Evaluate(vcCfg.IntermediateDirectory).Replace("/", "\\");

      String newLinkObject = ltoOutputPath + "/__lto__.bc";
      bool firstRound = true;

      HashSet<String> NonCodeArtifacts = new HashSet<String>();

      // there is no way to put the linker input into a file, and the commandline can be easily overflowed if we had a lot of .bc files
      // so we call the linker for each .bc file and link them together into a composite
      foreach (String ca in CompilationArtifacts)
      {
        WriteToOutputPane(" + " + ca);

        // .res etc, skip prelink but retain for adding back into artifacts list later
        if (!ca.ToLower().EndsWith("bc"))
        {
          WriteToOutputPane(" [skipped]\n");
          NonCodeArtifacts.Add(ca);
          continue;
        }

        Process linkProcess = NewExternalProcess();
        linkProcess.StartInfo.FileName = LocationLLVM_LINK_EXE;

        String linkCA = ca.TrimEnd(' ').Replace("/", "\\");

        // execute the compiler
        linkProcess.StartInfo.Arguments = String.Format(@"-o=""{0}"" ""{1}"" {2} ", newLinkObject, linkCA,
                                                        firstRound ? "" : "\"" + newLinkObject + "\"");

        if (CVXRegistry.ShowCommands)
        {
          WriteToOutputPane("\n  " + linkProcess.StartInfo.FileName + " " + linkProcess.StartInfo.Arguments + "\n");
        }

        linkProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
        linkProcess.Start();

        linkProcess.WaitForExit();

        if (linkProcess.ExitCode != 0)
        {
          WriteToOutputPane("\nLTO-Linking failed.\n" + linkProcess.StartInfo.Arguments + "\n");
          return false;
        }

        WriteToOutputPane(" [ok]\n");
        firstRound = false;
      }

      // and finally turn the single file back into a COFF for the MS linker to work with
      {
        WriteToOutputPane("\nLLVM LTO Code Generation ...\n");

        string inputLinkObject = newLinkObject;
        newLinkObject = Path.ChangeExtension(newLinkObject, ".obj");

        Process linkProcess = NewExternalProcess();
        linkProcess.StartInfo.FileName = LocationLLVM_LLC_EXE;

        // execute the compiler
        linkProcess.StartInfo.Arguments = String.Format(@"-O3 -filetype=obj -o=""{0}"" ""{1}"" ",
                                                        newLinkObject, inputLinkObject);
        linkProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
        linkProcess.Start();

        String outputStr = linkProcess.StandardError.ReadToEnd();
        linkProcess.WaitForExit();

        if (linkProcess.ExitCode != 0)
        {
          WriteToOutputPane("LTO-LLC failed.\n" + linkProcess.StartInfo.Arguments + "\n");
          WriteToOutputPane(outputStr);
          return false;
        }
        WriteToOutputPane(outputStr);
      }

      WriteToOutputPane("\n");

      // replace all previous compilation artifacts with our single new one
      CompilationArtifacts.Clear();
      CompilationArtifacts.Add(newLinkObject);

      // push any non-code objects back in the list (.res etc)
      foreach (String nca in NonCodeArtifacts)
        CompilationArtifacts.Add(nca);

      return true;
    }

    internal bool doLink(VCProject vcProject, VCConfiguration vcCfg, String outLink,
                         HashSet<String> CompilationArtifacts)
    {
      WriteToOutputPane("\n" + ConsoleDivider + "\nLinking :\n");
      if (BatchFileStream != null)
        BatchFileStream.WriteLine("\n\n");

      var fTools = (IVCCollection)vcCfg.Tools;
      var vcLinkerTool = (VCLinkerTool)fTools.Item("VCLinkerTool");
      var vcLibTool = (VCLibrarianTool)fTools.Item("VCLibrarianTool");
      var vcCTool = (VCCLCompilerTool)fTools.Item("VCCLCompilerTool");

      if (vcLinkerTool.LinkTimeCodeGeneration == LinkTimeCodeGenerationOption.LinkTimeCodeGenerationOptionUse)
      {
        if (!doLLVMPreLink(vcProject, vcCfg, outLink, ref CompilationArtifacts))
          return false;
      }

      var linkString = new StringBuilder(1024);
      Process linkProcess = NewExternalProcess();

      // standard inputs and outputs
      linkString.Append("/OUT:\"");
      linkString.Append(outLink);
      linkString.Append("\" ");
      foreach (String ca in CompilationArtifacts)
      {
        linkString.Append("\"");
        linkString.Append(ca.TrimEnd(' ').Replace("/", "\\"));
        linkString.Append("\" ");
      }

      // link to application / DLL
      // LINK obj[,out[,map[,lib[,def[,res]]]]] 
      if (vcCfg.ConfigurationType == ConfigurationTypes.typeApplication ||
          vcCfg.ConfigurationType == ConfigurationTypes.typeDynamicLibrary)
      {
        linkProcess.StartInfo.FileName = LocationLINKExe;

        // defaults
        linkString.Append("/NOLOGO /DYNAMICBASE ");

        // machine type
        if (vcLinkerTool.TargetMachine == machineTypeOption.machineX86)
          linkString.Append("/MACHINE:X86 ");
        else if (vcLinkerTool.TargetMachine == machineTypeOption.machineAMD64)
          linkString.Append("/MACHINE:X64 ");
        else if (vcLinkerTool.TargetMachine == machineTypeOption.machineARM)
          linkString.Append("/MACHINE:ARM ");


        if (vcCfg.ConfigurationType ==
            ConfigurationTypes.typeDynamicLibrary)
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
        // only pull across opts that begin with "-", ignoring VS "/" ones
        string[] opts = vcCfg.Evaluate(vcLinkerTool.AdditionalOptions).Split(' ');
        foreach (string opt in opts)
        {
          if (opt.StartsWith("-"))
            linkString.Append(opt + " ");
        }


        // we want to create a unique list of libraries to link alongside our object files
        var librariesToLink = new HashSet<String>();

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
          var generalRule = (IVCRulePropertyStorage)vcCfg.Rules.Item("Link");
          String additionalDependency = generalRule.GetEvaluatedPropertyValue("AdditionalDependencies");

          if (vcLinkerTool.AdditionalDependencies != null)
          {
            additionalDependency += ";" + vcLinkerTool.AdditionalDependencies;
          }
          if (!String.IsNullOrEmpty(additionalDependency))
          {
            String[] cAddIncDirs = vcCfg.Evaluate(additionalDependency).Split(new[] { ';', ' ' });
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

        if (!String.IsNullOrEmpty(vcLinkerTool.EntryPointSymbol))
        {
          linkString.Append("/ENTRY:");
          linkString.Append(vcLinkerTool.EntryPointSymbol);
          linkString.Append(" ");
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
            var newLibEnv = new StringBuilder(512);
            String cAddIncDir = systemLibDirs.Replace(",", ";");
            String[] cAddIncDirs = cAddIncDir.Split(';');

            foreach (string pd in cAddIncDirs)
            {
              String evalPd = vcCfg.Evaluate(pd);
              if (!String.IsNullOrEmpty(evalPd))
              {
                newLibEnv.Append("/LIBPATH:\"");
                newLibEnv.Append(evalPd.Replace("/", "\\"));
                newLibEnv.Append("\" ");
              }
            }

            linkString.Append(newLibEnv);
          }
        }

        // otherwise...
      }
      else if (vcCfg.ConfigurationType ==
               ConfigurationTypes.typeStaticLibrary)
      {
        // link to static library
        linkProcess.StartInfo.FileName = LocationLIBExe;
      }

      // dump the linker cmdline
      if (BatchFileStream != null)
      {
        BatchFileStream.WriteLine(String.Format("\"{0}\" {1}\n\npause\n", linkProcess.StartInfo.FileName,
                                                linkString));
        BatchFileStream.Close();
      }

      // dump to the output window too, why not
      WriteToOutputPane(linkString + "\n");


      // put the linker options into a file, we only have ~2000 characters to play with if sending direct which will
      // overflow quickly on any decent sized project
      String respFileName = Path.GetRandomFileName();
      String respFilePath = Path.Combine(vcProject.ProjectDirectory, respFileName);
      var respFile = new StreamWriter(respFilePath);
      respFile.WriteLine(linkString);
      respFile.Close();


      linkProcess.StartInfo.EnvironmentVariables["PATH"] += ";" + PathToVSTools + ";" + PathToVSCommonIDE;

      // execute the compiler
      linkProcess.StartInfo.Arguments = "@\"" + respFilePath + '"';
      linkProcess.StartInfo.WorkingDirectory = vcProject.ProjectDirectory;
      linkProcess.Start();

      // read any output from the compiler, indicative of some problem...
      String outputStrLink = linkProcess.StandardOutput.ReadToEnd();
      linkProcess.WaitForExit();

      // kill the temporary linker resp file
      File.Delete(respFilePath);

      if (linkProcess.ExitCode != 0)
      {
        WriteToOutputPane("Linking failed.\n" + outputStrLink + "\n");
        return false;
      }
      else
      {
        // test that we got something out of the linker; it will delete the result if something bad happened (that we didn't catch above)
        // find a full pathname for the file, relative or not
        String fullResultPath = outLink;
        if (!Path.IsPathRooted(fullResultPath))
          fullResultPath = vcProject.ProjectDirectory + outLink;

        if (File.Exists(fullResultPath))
        {
          WriteToOutputPane("\nLinking Successful\n");

          var fi = new FileInfo(fullResultPath);
          WriteToOutputPane("Result Size(b) : " + fi.Length.ToString() + "\n");
        }
        else
        {
          WriteToOutputPane("\nLinking Successful? - could not find output file to verify success!\n");
        }

        return true;
      }
    }

    internal bool doPostBuild(VCProject vcProject, VCConfiguration vcCfg)
    {
      var fTools = (IVCCollection)vcCfg.Tools;
      var postBuildTool = (VCPostBuildEventTool)fTools.Item("VCPostBuildEventTool");

      if (!postBuildTool.ExcludedFromBuild && !String.IsNullOrEmpty(postBuildTool.CommandLine))
      {
        WriteToOutputPane("\nPost-Build Step : " + postBuildTool.Description + "\n");
        ExecuteCustomBuildToolCommandLine<VCConfiguration>(postBuildTool.CommandLine, vcCfg, vcProject);
      }

      return true;
    }

    #endregion

    #region Nested type: ProjectFile

    internal struct ProjectFile
    {
      public VCFileConfiguration Config;
      public VCFile File;

      public ProjectFile(VCFile _f, VCFileConfiguration _c)
      {
        File = _f;
        Config = _c;
      }
    }

    #endregion
  }
}