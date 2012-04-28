using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

/*
* Copyright 2012 Hakim Mazouz
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace SetWallpaper
{
    public partial class MainForm : Form
    {
        #region DllImport
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 1;
        private const int SPIF_SENDCHANGE = 2;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SystemParametersInfo(
          int uAction, int uParam, string lpvParam, int fuWinIni);
        #endregion

        private enum Style
        {
            Centered,
            Stretched,
            Tiled
        }

        private Bitmap mBitmap = null;
        private string mImagePath = null;

        public MainForm()
        {
            InitializeComponent();
        }

        #region Event Handlers
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.bmp;*.gif;*.jpg;*.jpeg;*.png)|*.bmp;*.gif;*.jpg;*.jpeg;*.png";
            ofd.FilterIndex = 0;
            ofd.Multiselect = false;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                this.mImagePath = ofd.FileName;

                if (this.mBitmap != null) 
                {
                    this.mBitmap.Dispose();
                }

                this.toolStripStatusLabel.Text = Path.GetFileName(this.mImagePath);

                this.mBitmap = new Bitmap(this.mImagePath);
                this.pictureBox.Image = this.mBitmap;
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.pictureBox.Image != null)
            {
                this.contextMenuStrip.Show(this.pictureBox, e.Location);
            }
        }

        private void setCenteredToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.setWallpaper(this.mImagePath, Style.Centered);
        }

        private void setStretchedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.setWallpaper(this.mImagePath, Style.Stretched);
        }

        private void setTiledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.setWallpaper(this.mImagePath, Style.Tiled);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the chose wallpaper
        /// </summary>
        /// <param name="image">the path to the image used as wallpaper</param>
        /// <param name="style">the wallpaper style (centered, stretched or tiled)</param>
        private void setWallpaper(string image, Style style)
        {
            try
            {
                // convert the image and save it in <user>\Local Settings\Application Data\SetWallpaper
                string destDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SetWallpaper");

                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                string destImg = Path.Combine(destDir, "wallpaper.bmp");

                Image src = Image.FromFile(image);
                src.Save(destImg, ImageFormat.Bmp);

                // save the settings in registry
                //for tiled,    use TileWallpaper=1 WallpaperStyle=0
                //for centered, use TileWallpaper=0 WallpaperStyle=0
                //for strech,   use TileWallpaper=0 WallpaperStyle=2
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);

                switch (style)
                {
                    case Style.Centered:
                        key.SetValue("TileWallpaper", "0");
                        key.SetValue("WallpaperStyle", "0");
                        break;

                    case Style.Stretched:
                        key.SetValue("TileWallpaper", "0");
                        key.SetValue("WallpaperStyle", "2");
                        break;

                    case Style.Tiled:
                        key.SetValue("TileWallpaper", "1");
                        key.SetValue("WallpaperStyle", "0");
                        break;
                }

                // finally, set the wallpaper
                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, destImg, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occurred while setting the wallpaper: " + e.Message, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
        #endregion
}
