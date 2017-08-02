using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitUI.Properties;
using Gravatar;
using ResourceManager;
using Settings = GitCommands.AppSettings;

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

            _gravatarService.ConfigureCache(Settings.GravatarCachePath, Settings.AuthorImageCacheDays);
        }

        [Browsable(false)]
        public string Email { get; private set; }


        public async void LoadImage(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                RefreshImage(Resources.User);
                return;
            }

            Email = email;
            await UpdateGravatar();
        }


        private DefaultImageType GetDefaultImageType()
        {
            DefaultImageType defaultImageType;
            if (!Enum.TryParse(Settings.GravatarDefaultImageType, true, out defaultImageType))
            {
                Settings.GravatarDefaultImageType = DefaultImageType.None.ToString();
                defaultImageType = DefaultImageType.None;
            }
            return defaultImageType;
        }

        private void RefreshImage(Image image)
        {
            _gravatarImg.Image = image ?? Resources.User;
            _gravatarImg.Refresh();
        }

        private async Task UpdateGravatar()
        {
            // resize our control (I'm not using AutoSize for a reason)
            Size = new Size(Settings.AuthorImageSize, Settings.AuthorImageSize);
            _gravatarImg.Size = new Size(Settings.AuthorImageSize, Settings.AuthorImageSize);

            if (!Settings.ShowAuthorGravatar || string.IsNullOrEmpty(Email))
            {
                RefreshImage(Resources.User);
                return;
            }

            var image = await _gravatarService.GetAvatarAsync(Email, Settings.AuthorImageSize, GetDefaultImageType());
            RefreshImage(image);
        }


        private async void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            await _gravatarService.RemoveAvatarAsync(Email);
            await UpdateGravatar();
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
            await UpdateGravatar();
        }

        private async void noImageService_Click(object sender, EventArgs e)
        {
            var tag = (sender as ToolStripMenuItem)?.Tag;
            if (!(tag is DefaultImageType))
            {
                return;
            }
            Settings.GravatarDefaultImageType = ((DefaultImageType)tag).ToString();
            await _gravatarService.ClearCacheAsync();
            await UpdateGravatar();
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