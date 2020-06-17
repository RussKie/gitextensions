﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ConEmu.WinForms;
using GitCommands;
using GitUI.Properties;

namespace GitUI
{
    public static class ShellHelper
    {
        public static IReadOnlyList<string> SupportedShells => new[] { "bash", "cmd", "powershell", "pwsh" };
        internal static string GetCommandLineForCurrentShell()
        {
            string shell = AppSettings.ConEmuTerminal.ValueOrDefault.ToLower();
            string result = GetShellPath(shell);

            if (result == null)
            {
                return ConEmuConstants.DefaultConsoleCommandLine;
            }

            result = result.Quote();
            if (shell == "bash")
            {
                result = $"{result} --login -i";
            }

            return result;
        }

        internal static string GetShellPath(string shell)
        {
            string[] exeList;
            switch (shell?.ToLower())
            {
                case "cmd":
                case "powershell":
                case "pwsh":
                    exeList = new[] { $"{shell.ToLower()}.exe" };
                    break;
                case "bash":
                    const string justBash = "bash.exe"; // Generic bash, should generally be in the git dir
                    const string justSh = "sh.exe"; // Fallback to SH
                    exeList = new[] { justBash, justSh };
                    break;
                default:
                    exeList = null;
                    break;
            }

            return exeList?.Select(exe => PathUtil.TryFindShellPath(exe, out var exePath) ? exePath : null)
                  .FirstOrDefault(exePath => exePath != null);
        }

        public static Bitmap GetShellIcon(string shell)
        {
            switch (shell)
            {
                case "pwsh":
                    return Images.pwsh;
                case "powershell":
                    return Images.powershell;
                case "cmd":
                    return Images.cmd;
                case "bash":
                    return Images.GitForWindows;
                default:
                    return Images.Console;
            }
        }

        internal static bool ShellIsOnPath(string shell) => GetShellPath(shell) != null;

        internal static void ChangeFolder(this ConEmuControl terminal, string path)
        {
            if (terminal?.RunningSession == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            switch (AppSettings.ConEmuTerminal.ValueOrDefault.ToLower())
            {
                case "bash":
                    if (PathUtil.TryConvertWindowsPathToPosix(path, out var posixPath))
                    {
                        terminal.ClearCurrentLineWithMacroAndRunCommand($"cd {posixPath.QuoteNE()}");
                    }

                    break;
                case "cmd":
                    terminal.ClearCurrentLineWithEscapeAndRunCommand($"cd /D {path.QuoteNE()}");
                    break;
                case "powershell":
                case "pwsh":
                    terminal.ClearCurrentLineWithEscapeAndRunCommand($"cd {path.QuoteNE()}");
                    break;
                default:
                    break;
            }
        }

        private static void ClearCurrentLineWithMacroAndRunCommand(this ConEmuControl terminal, string command)
        {
            // Use a ConEmu macro to send the sequence for clearing the bash command line
            terminal.RunningSession.BeginGuiMacro("Keys").WithParam("^A").WithParam("^K").ExecuteSync();
            terminal.RunningSession.WriteInputTextAsync(command + Environment.NewLine);
        }

        private static void ClearCurrentLineWithEscapeAndRunCommand(this ConEmuControl terminal, string command)
        {
            terminal.RunningSession.WriteInputTextAsync("\x1B" + command + Environment.NewLine);
        }
    }
}
