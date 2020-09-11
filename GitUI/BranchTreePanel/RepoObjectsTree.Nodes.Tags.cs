﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.BranchTreePanel.Interfaces;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    partial class RepoObjectsTree
    {
        [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
        private class TagNode : BaseBranchNode, IGitRefActions, ICanDelete
        {
            public TagNode(Tree tree, in ObjectId objectId, string fullPath, bool visible)
                : base(tree, fullPath, visible)
            {
                ObjectId = objectId;
            }

            [CanBeNull]
            public ObjectId ObjectId { get; }

            internal override void OnSelected()
            {
                if (Tree.IgnoreSelectionChangedEvent)
                {
                    return;
                }

                base.OnSelected();
                SelectRevision();
            }

            internal override void OnDoubleClick()
            {
                CreateBranch();
            }

            public bool CreateBranch()
            {
                return UICommands.StartCreateBranchDialog(TreeViewNode.TreeView, ObjectId);
            }

            public bool Delete()
            {
                return UICommands.StartDeleteTagDialog(TreeViewNode.TreeView, Name);
            }

            public bool Merge()
            {
                return UICommands.StartMergeBranchDialog(TreeViewNode.TreeView, FullPath);
            }

            protected override void ApplyStyle()
            {
                base.ApplyStyle();
                TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.TagHorizontal);
            }

            public bool Checkout()
            {
                using (var form = new FormCheckoutRevision(UICommands))
                {
                    form.SetRevision(FullPath);
                    return form.ShowDialog(TreeViewNode.TreeView) != DialogResult.Cancel;
                }
            }
        }

        private sealed class TagTree : Tree
        {
            private readonly ICheckRefs _refsSource;

            // Retains the list of currently loaded tags.
            // This is needed to apply filtering without reloading the data.
            // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
            private IReadOnlyList<IGitRef> _loadedTags;

            public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
                : base(treeNode, uiCommands)
            {
                _refsSource = refsSource;
            }

            protected override bool SupportsFiltering => true;

            protected override Task OnAttachedAsync()
            {
                IsApplyFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override Task PostRepositoryChangedAsync()
            {
                IsApplyFiltering.Value = false;
                return ReloadNodesAsync(LoadNodesAsync);
            }

            protected override async Task<Nodes> LoadNodesAsync(CancellationToken token)
            {
                await TaskScheduler.Default;
                token.ThrowIfCancellationRequested();

                if (!IsApplyFiltering.Value || _loadedTags is null)
                {
                    _loadedTags = Module.GetRefs(tags: true, branches: false);
                }

                return FillTagTree(_loadedTags, token);
            }

            private Nodes FillTagTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
            {
                var nodes = new Nodes(this);
                var pathToNodes = new Dictionary<string, BaseBranchNode>();
                foreach (IGitRef tag in tags)
                {
                    token.ThrowIfCancellationRequested();

                    bool isVisible = !IsApplyFiltering.Value || _refsSource.Contains(tag.ObjectId);
                    var tagNode = new TagNode(this, tag.ObjectId, tag.Name, isVisible);
                    var parent = tagNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));
                    if (parent != null)
                    {
                        nodes.AddNodeBeforeHidden(parent);
                    }
                }

                return nodes;
            }

            protected override void PostFillTreeViewNode(bool firstTime)
            {
                if (firstTime)
                {
                    TreeViewNode.Collapse();
                }
            }
        }
    }
}
