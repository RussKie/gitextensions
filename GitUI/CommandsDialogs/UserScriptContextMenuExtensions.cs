﻿using System;
using System.Linq;
using System.Windows.Forms;
using GitUI.Script;

namespace GitUI.CommandsDialogs
{
    public static class UserScriptContextMenuExtensions
    {
        /// <summary>
        ///  Appends user scripts to the <paramref name="contextMenu"/>, or under <paramref name="hostMenuItem"/>,
        ///  if scripts are marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/>.
        /// </summary>
        /// <param name="contextMenu">The main context context menu to add user scripts too.</param>
        /// <param name="hostMenuItem">The context menu item to which to add user scripts marked as <see cref="ScriptInfo.AddToRevisionGridContextMenu"/>.</param>
        /// <param name="scriptInvoker">The handler that handles user script invocation.</param>
        public static void AppendUserScripts(this ContextMenuStrip contextMenu, ToolStripMenuItem hostMenuItem, EventHandler scriptInvoker)
        {
            contextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
            hostMenuItem = hostMenuItem ?? throw new ArgumentNullException(nameof(hostMenuItem));
            scriptInvoker = scriptInvoker ?? throw new ArgumentNullException(nameof(scriptInvoker));

            RemoveOwnScripts();
            AddOwnScripts();
            return;

            void RemoveOwnScripts()
            {
                hostMenuItem.DropDown.Items.Clear();

                var list = contextMenu.Items.Cast<ToolStripItem>().ToList();

                foreach (var item in list)
                {
                    if (item.Name.Contains("_ownScript"))
                    {
                        contextMenu.Items.RemoveByKey(item.Name);
                    }
                }

                if (contextMenu.Items[contextMenu.Items.Count - 1] is ToolStripSeparator)
                {
                    contextMenu.Items.RemoveAt(contextMenu.Items.Count - 1);
                }
            }

            void AddOwnScripts()
            {
                var scripts = ScriptManager.GetScripts();

                var lastIndex = contextMenu.Items.Count;

                foreach (var script in scripts)
                {
                    if (script.Enabled)
                    {
                        var item = new ToolStripMenuItem
                        {
                            Text = script.Name,
                            Name = script.Name + "_ownScript",
                            Image = script.GetIcon()
                        };
                        item.Click += scriptInvoker;

                        if (script.AddToRevisionGridContextMenu)
                        {
                            contextMenu.Items.Add(item);
                        }
                        else
                        {
                            hostMenuItem.DropDown.Items.Add(item);
                        }
                    }
                }

                if (lastIndex != contextMenu.Items.Count)
                {
                    contextMenu.Items.Insert(lastIndex, new ToolStripSeparator());
                }

                bool showScriptsMenu = hostMenuItem.DropDown.Items.Count > 0;
                hostMenuItem.Visible = showScriptsMenu;
            }
        }
    }
}
