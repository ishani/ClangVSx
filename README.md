ClangVSx
========

AddIn for Visual Studio 2010 that allows use of the Clang C/C++ compiler in place of MSVC++. Translates VC project and build settings into gcc-friendly command line arguments, along with existing platform tools (MSVC Linker, Librarian, RC) to complete the build chain. 

One click 'Build Project' testing of Clang in Windows development environments!

![Output Example](http://www.ishani.org/ext/ClangVSx-output.png) 

Bear in mind that Clang support for Windows is incomplete and occasionally buggy. This AddIn is largely experimental and the code is likely to change a lot over time. Your milage will definately vary.

Some things **not** supported:

* LTCG (-emit-llvm and LLVM linker before MSVC Linker, presumably)
* PCH (ignored during translation)
* ATL / MFC projects (Clang can't handle certain ATL/COM headers properly yet)


Required Compiler Patches
-------------------------

Vanilla Clang will not (as of 2011-06-18) build Win32 projects with the MSVC linker successfully. A couple of patches to the compiler are required:

* [LLVM Bug 9277](http://llvm.org/bugs/show_bug.cgi?id=9277) - required for compiling any projects using Windows API (eg including windows.h)
* [LLVM Bug 9213](http://llvm.org/bugs/show_bug.cgi?id=9213) - fixing up problems with static initializers
  
  
- - -
  
  
Usage (v0.2)
------------
Copy *.AddIn* and built *ClangVSx.dll* into ``C:\Users\<username>\Documents\Visual Studio 2010\Addins\``  
In VS2010, a new top-level *Clang* menu will be created, offering a settings dialog (to choose location of compiler) and *Rebuild Active Project*

  
- - -
  

Release Notes
-------------

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
 
 