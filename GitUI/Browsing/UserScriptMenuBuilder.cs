﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUI.CommandsDialogs;
using GitUI.Script;

namespace GitUI.Browsing
{
    /// <summary>
    /// Build and insert 'Run script' menu
    /// </summary>
    internal sealed class UserScriptMenuBuilder : IUserScriptMenuBuilder
    {
        private const string RunScriptMenuItemName = "runScriptToolStripMenuItem";
        private const string OwnScriptsSeparatorName = "ownScriptsSeparator";
        private const string OwnScriptPostfix = "_ownScript";
        private const string UserScript = "userscript";

        private bool _settingsLoaded;

        private readonly IScriptRunner _scriptRunner;
        private readonly ICanRefreshRevisions _canRefreshRevisions;
        private readonly GitUICommands _uiCommands;

        public UserScriptMenuBuilder(
            IScriptRunner scriptRunner,
            ICanRefreshRevisions canRefreshRevisions,
            GitUICommands uiCommands)
        {
            _scriptRunner = scriptRunner;
            _canRefreshRevisions = canRefreshRevisions;
            _uiCommands = uiCommands;
        }

        public void Build(ToolStrip tool)
        {
            var items = tool.Items
                .OfType<ToolStripItem>()
                .Where(x => x.Name == UserScript)
                .ToList();

            foreach (var item in items)
            {
                tool.Items.RemoveByKey(item.Name);
            }

            var scripts = ScriptManager.GetScripts()
                .Where(x => x.Enabled)
                .Where(x => x.OnEvent == ScriptEvent.ShowInUserMenuBar)
                .ToList();

            if (scripts.Count == 0)
            {
                return;
            }

            tool.Items.Add(new ToolStripSeparator { Name = UserScript });

            foreach (var script in scripts)
            {
                var button = new ToolStripButton
                {
                    Text = script.Name,
                    Name = UserScript,
                    Enabled = true,
                    Visible = true,
                    Image = script.GetIcon(),
                    DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                };

                button.Click += RunScript;

                // add to toolstrip
                tool.Items.Add(button);
            }
        }

        public void Build(ContextMenuStrip contextMenu)
        {
            var runScriptToolStripMenuItem = contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .First(x => x.Name == RunScriptMenuItemName);

            RemoveOwnScripts(contextMenu, runScriptToolStripMenuItem);
            AddOwnScripts(contextMenu, runScriptToolStripMenuItem);
        }

        private static void RemoveOwnScripts(ContextMenuStrip contextMenu, ToolStripMenuItem runScriptToolStripMenuItem)
        {
            runScriptToolStripMenuItem.DropDown.Items.Clear();

            var items = contextMenu.Items
                .OfType<ToolStripItem>()
                .Where(x => x.Name.EndsWith(OwnScriptPostfix) || x.Name == OwnScriptsSeparatorName)
                .ToList();

            foreach (var item in items)
            {
                contextMenu.Items.RemoveByKey(item.Name);
            }

            runScriptToolStripMenuItem.Visible = false;
        }

        private void AddOwnScripts(ContextMenuStrip contextMenu, ToolStripMenuItem runScriptToolStripMenuItem)
        {
            var lastIndex = contextMenu.Items.Count;
            var scripts = ScriptManager.GetScripts();
            var toRunScriptMenu = scripts.Where(x => x.Enabled)
                .Where(x => !x.AddToRevisionGridContextMenu)
                .Select(x => CreateToolStripMenuItem(x.Name, x.GetIcon()))
                .Cast<ToolStripItem>()
                .ToArray();

            runScriptToolStripMenuItem.DropDown.Items.AddRange(toRunScriptMenu);
            runScriptToolStripMenuItem.Visible = toRunScriptMenu.Any();

            var toMainContextMenu = scripts.Where(x => x.Enabled)
                .Where(x => x.AddToRevisionGridContextMenu)
                .Select(x => CreateToolStripMenuItem(x.Name, x.GetIcon()))
                .Cast<ToolStripItem>()
                .ToArray();

            contextMenu.Items.AddRange(toMainContextMenu);

            if (toMainContextMenu.Any())
            {
                contextMenu.Items.Insert(lastIndex, new ToolStripSeparator { Name = OwnScriptsSeparatorName });
            }
        }

        private ToolStripMenuItem CreateToolStripMenuItem(string name, Image image)
        {
            return new ToolStripMenuItem(name, image, RunScript)
            {
                Name = $"{name}{OwnScriptPostfix}"
            };
        }

        private void RunScript(object sender, EventArgs e)
        {
            //// Why here?
            if (_settingsLoaded == false)
            {
                new FormSettings(_uiCommands).LoadSettings();

                _settingsLoaded = true;
            }

            if (_scriptRunner.RunScript(sender.ToString()).NeedsGridRefresh)
            {
                _canRefreshRevisions.RefreshRevisions();
            }
        }
    }
}
