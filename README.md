ClangVSx
========

AddIn for Visual Studio 2010 that allows use of the Clang C/C++ compiler in place of MSVC++. Translates VC project and build settings into gcc-friendly command line arguments, along with existing platform tools (MSVC Linker, Librarian, RC) to complete the build chain. 

One click 'Build Project' testing of Clang in Windows development environments!

Bear in mind that Clang support for Windows is incomplete and occasionally buggy. This AddIn is largely experimental and the code is likely to change a lot over time. Your milage will definately vary.

Some things **not** supported:

* LTCG (would require LLVM linker instead of MSVC Linker)
* PCH (ignored during translation)
* ATL / MFC projects (Clang can't handle certain ATL/COM headers properly yet)

- - -
  
Usage (v0.1)
------------
Copy *.AddIn* and built *ClangVSx.dll* into ``C:\Users\<username>\Documents\Visual Studio 2010\Addins\``  
In VS2010, a new top-level *Clang* menu will be created, offering a settings dialog (to choose location of compiler) and *Rebuild Active Project*

Release Notes
-------------

**Version 0.1** (2011-06-09)

* Initial commit of working version, tested with Clang 3.0 (+ minor inline patch)
* See my comment on [LLVM Bug 9277](http://llvm.org/bugs/show_bug.cgi?id=9277) for temporary fix; required for compiling any projects using Windows API (eg including windows.h)
* Tested against LAME plus a few simple wizard-generated Win32 apps from VS2010 :
 * DLL, Static Library, Application
 * Console Application