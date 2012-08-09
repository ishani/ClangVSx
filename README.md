ClangVSx
========

AddIn for Visual Studio 2010 that allows use of the Clang C/C++ compiler in place of MSVC++. Translates VC project and build settings into gcc-friendly command line arguments, along with existing platform tools (MSVC Linker, Librarian, RC) to complete the build chain. 

One click 'Build Project' testing of Clang in Windows development environments!

Futher information and screenshots are [here](http://www.ishani.org/web/articles/code/clangvsx/).

Bear in mind that Clang support for Windows is incomplete and occasionally buggy. This AddIn is largely experimental and the code is likely to change a lot over time. Your milage will definately vary.

Some things **not** supported:

* PCH (ignored during translation)
* ATL / MFC projects (Clang can't handle certain ATL/COM headers properly yet)


Added in v0.3.2

* Support for latest Clang 3.2 changes - specifically the removal of llvm-ld during LTO phase
* Support added for Visual Studio 2012 RC

Added in v0.3

* Support for LTCG (LTO) - ClangVSx will compile to LLVM bitcode, link and optimise the artifacts into a single object file before doing native code generation. 


Required Compiler Patches
-------------------------

ClangVSx requires Clang 3.0 or above - preferably 3.2.
However, 3.2 still does not cleanly compile projects using the Windows Platform SDK. I maintain a pre-built version that contains a few patches in this area. Read more [here](http://www.ishani.org/web/articles/code/clang-win32/).
  
  
- - -
  

Usage (v0.3.2)
------------
There is a directory for each Visual Studio platform supported in the pre-built zip. 

VS2010:
Copy *.AddIn* and built *ClangVSx.dll* from the VS_2010/ directory into ``C:\Users\<username>\Documents\Visual Studio 2010\Addins\``  
VS2012:
Copy *.AddIn* and built *ClangVSx.dll* from the VS_2012/ directory into ``C:\Users\<username>\Documents\Visual Studio 2012\Addins\``  
Also you may have to add a folder to the Add-in Security panel, [information here](http://www.ishani.org/web/2012/getting-add-ins-to-work-on-visual-studio-2012-rc/).

A new top-level *Clang* menu will be created, offering a settings dialog (to choose location of compiler) and *Rebuild Active Project* or *Relink*
A context-menu option is added to code editor windows that allows for Clang-specific tasks to be performed on that code.
  
- - -
  

Release Notes
-------------

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
 
 