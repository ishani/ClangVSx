// from http://code.google.com/p/nenhancer/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;

namespace NEnhancer.Common
{
    public class DTEHelper
    {
        private DTE2 dte;
        private AddIn addin;

        public DTEHelper(DTE2 dte, AddIn addin)
        {
            this.dte = dte;
            this.addin = addin;
        }

        public string GetCulturedMenuName(string englishName)
        {
            string result = englishName;

            try
            {
                string resourceName;
                ResourceManager resourceManager = new ResourceManager("ClangVSx.CommandBar",
                                                                      Assembly.GetExecutingAssembly());
                CultureInfo cultureInfo = new CultureInfo(dte.LocaleID);

                if (cultureInfo.TwoLetterISOLanguageName == "zh")
                {
                    System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
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
            return ((CommandBars) dte.CommandBars)[cmdBarName];
        }

        public void AddNamedCommand2(string cmdName, string buttonText, string toolTip,
                                     bool useMsoButton, int iconIndex)
        {
            // Get commands collection
            Commands2 commands = (Commands2) dte.Commands;
            object[] contextGUIDS = new object[] {};

            try
            {
                // Add command
                Command command = commands.AddNamedCommand2(addin, cmdName, buttonText, toolTip,
                                                            useMsoButton, iconIndex, ref contextGUIDS,
                                                            (int) vsCommandStatus.vsCommandStatusSupported +
                                                            (int) vsCommandStatus.vsCommandStatusEnabled,
                                                            (int) vsCommandStyle.vsCommandStylePictAndText,
                                                            vsCommandControlType.vsCommandControlTypeButton);
            }
            catch (ArgumentException)
            {
                // Command already exists, so ignore the exception.
            }
        }

        public CommandBarButton AddButtonToCmdBar(CommandBar cmdBar, int beforeIndex, string caption, string tooltip)
        {
            CommandBarButton button = cmdBar.Controls.Add(MsoControlType.msoControlButton,
                                                          Type.Missing, Type.Missing, beforeIndex, true) as
                                      CommandBarButton;
            button.Caption = caption;
            button.TooltipText = tooltip;

            return button;
        }

        public CommandBarButton AddButtonToPopup(CommandBarPopup popup, int beforeIndex, string caption, string tooltip)
        {
            CommandBarButton button = popup.Controls.Add(MsoControlType.msoControlButton,
                                                         Type.Missing, Type.Missing, beforeIndex, true) as
                                      CommandBarButton;
            button.Caption = caption;
            button.TooltipText = tooltip;

            return button;
        }

        public UIHierarchy SolutionExplorerNode
        {
            get { return dte.ToolWindows.SolutionExplorer; }
        }

        public List<UIHierarchyItem> GetProjectNodes(Solution solution)
        {
            string solutionName = solution.Properties.Item("Name").Value.ToString();
            return GetProjectNodes(SolutionExplorerNode.GetItem(solutionName).UIHierarchyItems);
        }

        public List<UIHierarchyItem> GetProjectNodes(UIHierarchyItems topLevelItems)
        {
            List<UIHierarchyItem> projects = new List<UIHierarchyItem>();
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
            List<UIHierarchyItem> projects = new List<UIHierarchyItem>();

            if (IsSolutionFolder(item))
            {
                foreach (UIHierarchyItem subItem in item.UIHierarchyItems)
                {
                    if (IsProjectNode(subItem))
                    {
                        projects.Add(subItem);
                    }
                }
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
            return (item.Object is ProjectItem && ((ProjectItem) item.Object).Object is Project &&
                    ((Project) ((ProjectItem) item.Object).Object).Kind != ProjectKinds.vsProjectKindSolutionFolder);
        }

        public string GetSelectedText()
        {
            TextSelection selectedText = dte.ActiveDocument.Selection as TextSelection;
            return selectedText.Text;
        }

        public string GetSelectedLines()
        {
            TextSelection selectedText = dte.ActiveDocument.Selection as TextSelection;
            TextPoint topPoint = selectedText.TopPoint;
            EditPoint bottomPoint = selectedText.BottomPoint.CreateEditPoint();

            return bottomPoint.GetLines(topPoint.Line, bottomPoint.Line + 1);
        }

        private static bool IsBlank(string input)
        {
            return string.IsNullOrEmpty(input) || input.All(ch => char.IsWhiteSpace(ch));
        }

        public string GetCurrentWord()
        {
            TextSelection selectedText = dte.ActiveDocument.Selection as TextSelection;
            EditPoint topPoint = selectedText.TopPoint.CreateEditPoint();
            string currentLine = topPoint.GetLines(topPoint.Line, topPoint.Line + 1);

            if (IsBlank(currentLine))
            {
                return string.Empty;
            }

            string result = string.Empty;
            int charIndex = topPoint.LineCharOffset - 1;

            if (topPoint.AtStartOfLine ||
                (!char.IsWhiteSpace(currentLine[charIndex]) && char.IsWhiteSpace(currentLine[charIndex - 1])))
            {
                EditPoint rightPoint = topPoint.CreateEditPoint();
                rightPoint.WordRight(1);
                result = currentLine.Substring(topPoint.LineCharOffset - 1,
                                               rightPoint.LineCharOffset - topPoint.LineCharOffset).Trim();
            }
            else if (topPoint.AtEndOfLine ||
                     (!char.IsWhiteSpace(currentLine[charIndex - 1]) && char.IsWhiteSpace(currentLine[charIndex])))
            {
                EditPoint leftPoint = topPoint.CreateEditPoint();
                leftPoint.WordLeft(1);
                result = currentLine.Substring(leftPoint.LineCharOffset - 1,
                                               topPoint.LineCharOffset - leftPoint.LineCharOffset).Trim();
            }
            else if (char.IsLetterOrDigit(currentLine[charIndex - 1]) && char.IsLetterOrDigit(currentLine[charIndex + 1]))
            {
                topPoint.WordLeft(1);
                EditPoint rightPoint = topPoint.CreateEditPoint();
                rightPoint.WordRight(1);
                result = currentLine.Substring(topPoint.LineCharOffset - 1,
                                               rightPoint.LineCharOffset - topPoint.LineCharOffset);
            }
            else
            {
                result = GetSelectedText();
            }

            return result;
        }

        public string GetAddinAssemblyLocation()
        {
            Assembly asm = Assembly.GetEntryAssembly();
            return asm.Location;
        }

        public void Restart()
        {
            dte.Quit();
            System.Diagnostics.Process.Start(dte.FileName);
        }

        public Project GetProjectByName(Solution2 sln, string projName)
        {
            foreach (Project p in sln.Projects)
            {
                if (p.Name == projName)
                {
                    return p;
                }
            }

            return null;
        }
    }
}