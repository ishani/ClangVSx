// from http://code.google.com/p/nenhancer/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using Process = System.Diagnostics.Process;

namespace NEnhancer.Common
{
  public class DTEHelper
  {
    private readonly AddIn _addin;
    private readonly DTE2 _dte;

    public DTEHelper(DTE2 dte, AddIn addin)
    {
      _dte = dte;
      _addin = addin;
    }

    public UIHierarchy SolutionExplorerNode
    {
      get { return _dte.ToolWindows.SolutionExplorer; }
    }

    public string GetCulturedMenuName(string englishName)
    {
      string result;

      try
      {
        string resourceName;
        var resourceManager = new ResourceManager("ClangVSx.CommandBar",
                                                  Assembly.GetExecutingAssembly());
        var cultureInfo = new CultureInfo(_dte.LocaleID);

        if (cultureInfo.TwoLetterISOLanguageName == "zh")
        {
          CultureInfo parentCultureInfo = cultureInfo.Parent;
          resourceName = String.Concat(parentCultureInfo.Name, englishName);
        }
        else
        {
          resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, englishName);
        }

        result = resourceManager.GetString(resourceName);
      }
      catch
      {
        result = englishName;
      }

      return result;
    }

    public CommandBar GetCommandBarByName(string cmdBarName)
    {
      return ((CommandBars)_dte.CommandBars)[cmdBarName];
    }

    public void DumpCommandBars()
    {
      foreach (CommandBar cb in ((CommandBars)_dte.CommandBars))
      {
        string sb = cb.Name;
        Debug.WriteLine(sb);
      }
    }

    public void AddNamedCommand2(string cmdName, string buttonText, string toolTip,
                                 bool useMsoButton, int iconIndex)
    {
      // Get commands collection
      var commands = (Commands2)_dte.Commands;
      var contextGuids = new object[] { };

      try
      {
        // Add command
        commands.AddNamedCommand2(_addin, cmdName, buttonText, toolTip,
                                  useMsoButton, iconIndex, ref contextGuids);
      }
      catch (ArgumentException)
      {
        // Command already exists, so ignore the exception.
      }
    }

    public CommandBarButton AddButtonToCmdBar(CommandBar cmdBar, int beforeIndex, string caption, string tooltip)
    {
      var button = cmdBar.Controls.Add(
          MsoControlType.msoControlButton,
          Type.Missing,
          Type.Missing,
          beforeIndex,
          true) as CommandBarButton;

      if (button != null)
      {
        button.Caption = caption;
        button.TooltipText = tooltip;
      }

      return button;
    }

    public CommandBarButton AddButtonToPopup(CommandBarPopup popup, int beforeIndex, string caption, string tooltip)
    {
      var button = popup.Controls.Add(
          MsoControlType.msoControlButton,
          Type.Missing,
          Type.Missing,
          beforeIndex,
          true) as CommandBarButton;

      if (button != null)
      {
        button.Caption = caption;
        button.TooltipText = tooltip;
      }

      return button;
    }

    public List<UIHierarchyItem> GetProjectNodes(Solution solution)
    {
      string solutionName = solution.Properties.Item("Name").Value.ToString();
      return GetProjectNodes(SolutionExplorerNode.GetItem(solutionName).UIHierarchyItems);
    }

    public List<UIHierarchyItem> GetProjectNodes(UIHierarchyItems topLevelItems)
    {
      var projects = new List<UIHierarchyItem>();
      foreach (UIHierarchyItem item in topLevelItems)
      {
        if (IsProjectNode(item))
        {
          projects.Add(item);
        }
        else if (IsSolutionFolder(item))
        {
          projects.AddRange(GetProjectNodesInSolutionFolder(item));
        }
      }

      return projects;
    }

    public List<UIHierarchyItem> GetProjectNodesInSolutionFolder(UIHierarchyItem item)
    {
      var projects = new List<UIHierarchyItem>();

      if (IsSolutionFolder(item))
      {
        projects.AddRange(item.UIHierarchyItems.Cast<UIHierarchyItem>().Where(IsProjectNode));
      }

      return projects;
    }

    public bool IsSolutionFolder(UIHierarchyItem item)
    {
      return ((item.Object is Project) &&
              ((item.Object as Project).Kind == ProjectKinds.vsProjectKindSolutionFolder));
    }

    public bool IsProjectNode(UIHierarchyItem item)
    {
      return IsDirectProjectNode(item) || IsProjectNodeInSolutionFolder(item);
    }

    public bool IsDirectProjectNode(UIHierarchyItem item)
    {
      return ((item.Object is Project) &&
              ((item.Object as Project).Kind != ProjectKinds.vsProjectKindSolutionFolder));
    }

    public bool IsProjectNodeInSolutionFolder(UIHierarchyItem item)
    {
      return (item.Object is ProjectItem && ((ProjectItem)item.Object).Object is Project &&
              ((Project)((ProjectItem)item.Object).Object).Kind != ProjectKinds.vsProjectKindSolutionFolder);
    }

    public string GetSelectedText()
    {
      var selectedText = _dte.ActiveDocument.Selection as TextSelection;
      return selectedText != null ? selectedText.Text : string.Empty;
    }

    public string GetSelectedLines()
    {
      var selectedText = _dte.ActiveDocument.Selection as TextSelection;
      if (selectedText != null)
      {
        TextPoint topPoint = selectedText.TopPoint;
        EditPoint bottomPoint = selectedText.BottomPoint.CreateEditPoint();

        return bottomPoint.GetLines(topPoint.Line, bottomPoint.Line + 1);
      }
      return string.Empty;
    }

    private static bool IsBlank(string input)
    {
      return string.IsNullOrEmpty(input) || input.All(char.IsWhiteSpace);
    }

    public string GetCurrentWord()
    {
      var selectedText = _dte.ActiveDocument.Selection as TextSelection;
      if (selectedText != null)
      {
        EditPoint topPoint = selectedText.TopPoint.CreateEditPoint();
        string currentLine = topPoint.GetLines(topPoint.Line, topPoint.Line + 1);

        if (IsBlank(currentLine))
        {
          return string.Empty;
        }

        string result;
        int charIndex = topPoint.LineCharOffset - 1;

        if (topPoint.AtStartOfLine ||
            (!char.IsWhiteSpace(currentLine[charIndex]) && char.IsWhiteSpace(currentLine[charIndex - 1])))
        {
          EditPoint rightPoint = topPoint.CreateEditPoint();
          rightPoint.WordRight();
          result = currentLine.Substring(topPoint.LineCharOffset - 1,
                                         rightPoint.LineCharOffset - topPoint.LineCharOffset).Trim();
        }
        else if (topPoint.AtEndOfLine ||
                 (!char.IsWhiteSpace(currentLine[charIndex - 1]) && char.IsWhiteSpace(currentLine[charIndex])))
        {
          EditPoint leftPoint = topPoint.CreateEditPoint();
          leftPoint.WordLeft();
          result = currentLine.Substring(leftPoint.LineCharOffset - 1,
                                         topPoint.LineCharOffset - leftPoint.LineCharOffset).Trim();
        }
        else if (char.IsLetterOrDigit(currentLine[charIndex - 1]) &&
                 char.IsLetterOrDigit(currentLine[charIndex + 1]))
        {
          topPoint.WordLeft();
          EditPoint rightPoint = topPoint.CreateEditPoint();
          rightPoint.WordRight();
          result = currentLine.Substring(topPoint.LineCharOffset - 1,
                                         rightPoint.LineCharOffset - topPoint.LineCharOffset);
        }
        else
        {
          result = GetSelectedText();
        }

        return result;
      }
      return string.Empty;
    }

    public string GetAddinAssemblyLocation()
    {
      Assembly asm = Assembly.GetEntryAssembly();
      return asm.Location;
    }

    public void Restart()
    {
      _dte.Quit();
      Process.Start(_dte.FileName);
    }

    public Project GetProjectByName(Solution2 sln, string projName)
    {
      return sln.Projects.Cast<Project>().FirstOrDefault(p => p.Name == projName);
    }
  }
}