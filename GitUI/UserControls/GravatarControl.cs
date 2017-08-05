﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using Gravatar;
using ResourceManager;

namespace GitUI
{
    public partial class GravatarControl : GitExtensionsControl
    {
        private readonly IGravatarService _gravatarService = new GravatarService();

        public GravatarControl()
        {
            InitializeComponent();
            Translate();

            noneToolStripMenuItem.Tag = DefaultImageType.None;
            identiconToolStripMenuItem.Tag = DefaultImageType.Identicon;
            monsterIdToolStripMenuItem.Tag = DefaultImageType.MonsterId;
            wavatarToolStripMenuItem.Tag = DefaultImageType.Wavatar;
            retroToolStripMenuItem.Tag = DefaultImageType.Retro;

            _gravatarService.ConfigureCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
        }

        [Browsable(false)]
        public string Email { get; private set; }


        public void LoadImage(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                RefreshImage(Resources.User);
                return;
            }

            Email = email;
            UpdateGravatar();
        }


        private DefaultImageType GetDefaultImageType()
        {
            DefaultImageType defaultImageType;
            if (!Enum.TryParse(AppSettings.GravatarDefaultImageType, true, out defaultImageType))
            {
                AppSettings.GravatarDefaultImageType = DefaultImageType.None.ToString();
                defaultImageType = DefaultImageType.None;
            }
            return defaultImageType;
        }

        private void RefreshImage(Image image)
        {
            _gravatarImg.Image = image ?? Resources.User;
            _gravatarImg.Refresh();
        }

        private async void UpdateGravatar()
        {
            // resize our control (I'm not using AutoSize for a reason)
            var size = new Size(AppSettings.AuthorImageSize, AppSettings.AuthorImageSize);
            Size = _gravatarImg.Size = size;

            if (!AppSettings.ShowAuthorGravatar || string.IsNullOrEmpty(Email))
            {
                RefreshImage(Resources.User);
                return;
            }

            var image = await _gravatarService.GetAvatarAsync(Email, AppSettings.AuthorImageSize, GetDefaultImageType());
            RefreshImage(image);
        }


        private async void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            await _gravatarService.RemoveAvatarAsync(Email);
            UpdateGravatar();
        }

        private void RegisterAtGravatarcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"http://www.gravatar.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private async void ClearImagecacheToolStripMenuItemClick(object sender, EventArgs e)
        {
            await _gravatarService.ClearCacheAsync();
            UpdateGravatar();
        }

        private async void noImageService_Click(object sender, EventArgs e)
        {
            var tag = (sender as ToolStripMenuItem)?.Tag;
            if (!(tag is DefaultImageType))
            {
                return;
            }
            AppSettings.GravatarDefaultImageType = ((DefaultImageType)tag).ToString();
            await _gravatarService.ClearCacheAsync();
            UpdateGravatar();
        }

        private void noImageGeneratorToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var defaultImageType = GetDefaultImageType();
            ToolStripMenuItem selectedItem = null;
            foreach (ToolStripMenuItem menu in noImageGeneratorToolStripMenuItem.DropDownItems)
            {
                menu.Checked = false;
                if ((DefaultImageType)menu.Tag == defaultImageType)
                {
                    selectedItem = menu;
                }
            }

            if (selectedItem == null)
            {
                selectedItem = noneToolStripMenuItem;
            }
            selectedItem.Checked = true;
        }
    }
}