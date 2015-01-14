ClangVSx
========

AddIn for Visual Studio 2013 that allows use of the Clang 3.5 C/C++ compiler in place of MSVC++. Translates VC project and build settings into gcc-friendly command line arguments, along with existing platform tools (MSVC Linker, Librarian, RC) to complete the build chain. 

One click 'Build Project' testing of Clang in Windows development environments!

Futher information and screenshots are [here](http://www.ishani.org/web/articles/code/clangvsx/).

Bear in mind that Clang support for Windows is incomplete and occasionally buggy. This AddIn is largely experimental and the code is likely to change a lot over time. Your milage will definately vary.

Some things **not** supported:

* PCH (ignored during translation)
* ATL / MFC projects (Clang can't handle certain ATL/COM headers properly yet)

Changes in v0.5

* Compatibility with Clang 3.5 - deprecate previous versions and add new features, fix LTO pipeline
* Support for VS2010/2012 will likely still work but I won't be testing or deploying those

Changes in v0.4.5

* Compatibility with Clang 3.3 (-ccc-host-triple => -target) with 3.2 fallback

Changes in v0.4.0

* Code clean-up courtesy of [Ilibis](https://github.com/ilibis) fork
* Settings support for toggling C++11, MS C++ ABI
* Settings support for different platform targets (proper support for x86/x64/ARM variants)
* Further testing on VS2012 RTM

Changes in v0.3.2

* Support for latest Clang 3.2 changes - specifically the removal of llvm-ld during LTO phase
* Support added for Visual Studio 2012 RC

Changes in v0.3

* Support for LTCG (LTO) - ClangVSx will compile to LLVM bitcode, link and optimise the artifacts into a single object file before doing native code generation. 


Required Compiler Patches
-------------------------

The latest version is tested against Clang 3.5.1. 

A pre-built version of Clang for Windows, tracked to the latest code changes from SVN, is available [here](http://www.ishani.org/projects/ClangVSX/).
  
  
- - -
  

Usage 
------------
There is a directory for each Visual Studio platform supported in the pre-built zip. 

VS2013:
Copy *.AddIn* and built *ClangVSx.dll* from the VS_2013/ directory into ``C:\Users\<username>\Documents\Visual Studio 2013\Addins\``  

A new top-level *Clang* menu will be created, offering a settings dialog (to choose location of compiler) and *Rebuild Active Project* or *Relink*
A context-menu option is added to code editor windows that allows for Clang-specific tasks to be performed on that code.
  
- - -
  

Release Notes
-------------

**Version 0.5.0** (2015-01-15)

* Support for Clang 3.5
* Support for Visual Studio 2013

**Version 0.4.5** (2012-12-04)

* Support for Clang 3.3

**Version 0.4.0** (2012-10-22)

* Testing against VS 2012 RTM, Windows 8
* More settings options, better support for 32/64 bit project targets in VS

**Version 0.3.2** (2012-08-09)

* Removed use of llvm-ld, no longer available in recent Clang 3.2 builds
* Added support for Visual Studio 2012 RC

**Version 0.3** (2012-05-28)

* Tidied up preferences, added more diagnotics options in the settings dialog
* Tested with latest Clang 3.2 builds and some more adventurous Win32 projects
* Added support for LTO / LTCG

**Version 0.2** (2011-06-18)

* Refactor build system to be much cleaner and easier to read and maintain
* Added MIDL support
* Successfully tested against [smallpt](http://www.kevinbeason.com/smallpt/) - requires static init patch above
* Tested with a stripped-down DirectX 9 demo - requires a little IUnknown hack

**Version 0.1** (2011-06-09)

* Initial commit of working version, tested with Clang 3.0
* Tested against LAME plus a few simple wizard-generated Win32 apps from VS2010 :
 * DLL, Static Library, Application
 * Console Application
 
 