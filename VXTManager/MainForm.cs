using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VXTManager;

public class MainForm : Form
{
	private VXT OpenVXT;

	private byte[] VXTArray;

	private List<MarshalBitmap> Images = new List<MarshalBitmap>();

	private List<int> TrueIndexes = new List<int>();

	private IContainer components;

	private PictureBox IconBox;

	private GroupBox InfoGroup;

	private GroupBox IconGroup;

	private SaveFileDialog FileSave;

	private OpenFileDialog FileOpen;

	private ToolStripMenuItem AExtractAction;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem SaveAction;

	private ToolStripMenuItem AboutMenu;

	private ToolStripMenuItem OpenAction;

	private ToolStripMenuItem fileToolStripMenuItem;

	private MenuStrip MainMenu;

	private ContextMenuStrip IconContext;

	private ToolStripMenuItem ExtractAction;

	private ToolStripMenuItem ImportAction;

	private OpenFileDialog IconImport;
    private System.Windows.Forms.TreeView texture_list;
    private SaveFileDialog IconExport;

	public MainForm()
	{
		InitializeComponent();
	}

	private void OpenActionClick(object sender, EventArgs e)
	{
		if (FileOpen.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		using (FileStream fileStream = new FileStream(FileOpen.FileName, FileMode.Open))
		{

			OpenVXT = new VXT(fileStream);
			VXTArray = new byte[fileStream.Length];
			fileStream.Position = 0L;
			fileStream.Read(VXTArray, 0, (int)fileStream.Length);
			Images = new List<MarshalBitmap>();
			TrueIndexes = new List<int>();
            texture_list.Nodes.Clear();

            for (int i = 0; i < OpenVXT.Images.Count; i++)
			{
                string cString = "-Disabled (P4)";

                try
				{
					MarshalBitmap marshalBitmap = OpenVXT.Parse8(i);
					if (marshalBitmap != null)
					{
						List<Color> list = new List<Color>();
						for (int j = 0; j < marshalBitmap.Height; j++)
						{
							for (int k = 0; k < marshalBitmap.Width; k++)
							{
								list.Add(marshalBitmap.GetPixel(k, j));
							}
						}
						Images.Add(PS2.Unswizzle8(new Size(marshalBitmap.Width, marshalBitmap.Height), list));
						TrueIndexes.Add(i);
                        cString = String.Format("[{0}x{1}]", marshalBitmap.Width, marshalBitmap.Height);
                    }
                    else
                    {
                        Rectangle rect = new Rectangle(0, 0, 1, 1);
                        marshalBitmap = new MarshalBitmap(1, 1);

                        Images.Add(marshalBitmap);
                    }
                    
                }
				catch (Exception) { 
                    MarshalBitmap marshalBitmap = new MarshalBitmap(1, 1);

                    Images.Add(marshalBitmap);
                }
                texture_list.Nodes.Add("T" + i + cString);

            }
		}
        texture_list.EndUpdate();

        if (Images.Count > 0) { 
			IconBox.Image = Images[0].Bitmap;
			if (IconBox.Image.Width > 256 || IconBox.Image.Height > 256)
			{
				base.Size = new Size(173 + IconBox.Image.Width, 93 + IconBox.Image.Height);
			}
			else
			{
				base.Size = new Size(429, 349);
			}
        }
    }


	private void SaveActionClick(object sender, EventArgs e)
	{
		if (FileSave.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		File.WriteAllBytes(FileSave.FileName, VXTArray);
	}

	private void ExtractActionClick(object sender, EventArgs e)
	{
		if (IconExport.ShowDialog() == DialogResult.OK)
		{
			IconBox.Image.Save(IconExport.FileName);
		}
	}

    private int GetIndex(TreeNode node)
    {
        // Always make a way to exit the recursion.
        if (node.Parent == null)
            return node.Index;

        return node.Index + GetIndex(node.Parent);
    }
    private void ImportActionClick(object sender, EventArgs e)
	{
		if (IconImport.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		MarshalBitmap marshalBitmap = new MarshalBitmap(new Bitmap(IconImport.FileName));
		int index = GetIndex(texture_list.SelectedNode);
		Dictionary<Color, int> dictionary = new Dictionary<Color, int>();
		List<byte> list = new List<byte>();
		List<byte> list2 = new List<byte>();
		if (marshalBitmap.Width != Images[GetIndex(texture_list.SelectedNode)].Width || marshalBitmap.Height != Images[GetIndex(texture_list.SelectedNode)].Height)
		{
			MessageBox.Show("The dimensions of the given image does not equal to the source.\nPlease adjust the image accordingly.", "ERROR: Dimensions incorrect.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		int num = 0;
		for (int i = 0; i < marshalBitmap.Height; i++)
		{
			for (int j = 0; j < marshalBitmap.Width; j++)
			{
				Color pixel = marshalBitmap.GetPixel(j, i);
				if (!dictionary.ContainsKey(pixel))
				{
					dictionary.Add(pixel, num);
					num++;
				}
			}
		}
		if (dictionary.Count > OpenVXT.Images[index].ColorCount)
		{
			MessageBox.Show("The given image has more colors than the source.\nPlease quantize the image using something like PNGQuant.", "ERROR: Too many colors.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		for (int k = 0; k < OpenVXT.Images[index].ColorCount; k++)
		{
			try
			{
				Color color = dictionary.Keys.ElementAt(k);
				list.Add(color.R);
				list.Add(color.G);
				list.Add(color.B);
				list.Add((byte)((int)color.A / 2));
			}
			catch (ArgumentOutOfRangeException)
			{
				list.Add(byte.MaxValue);
				list.Add(byte.MaxValue);
				list.Add(byte.MaxValue);
				list.Add(byte.MaxValue);
			}
		}
		if (true)
		{
			List<Color> list3 = PS2.Swizzle8(marshalBitmap);
			for (int l = 0; l < list3.Count; l++)
			{
				list2.Add(PS2.CalcIndex((byte)dictionary[list3[l]]));
			}
			PS2.PS2Image value = OpenVXT.Images[index];
			PS2.PS2Image replacment = new PS2.PS2Image();
			replacment.DMAIndex = value.DMAIndex;
			replacment.DMASubIndex = value.DMASubIndex;
			replacment.TrueSize = new Size(value.TrueSize.Width, value.TrueSize.Height);
            replacment.FixedSize = new Size(value.FixedSize.Width, value.FixedSize.Height);
            replacment.ColorCount = value.ColorCount;

            replacment.Data = list2.ToArray();
            replacment.Palette = list.ToArray();
			OpenVXT.Images[index] = replacment;
			Images = new List<MarshalBitmap>();
			for (int m = 0; m < OpenVXT.Images.Count; m++)
			{
				try
				{
					MarshalBitmap marshalBitmap2 = OpenVXT.Parse8(m);
					if (marshalBitmap2 == null)
					{
                        marshalBitmap2 = new MarshalBitmap(1, 1);

                        Images.Add(marshalBitmap);
                        continue;
					}
					List<Color> list4 = new List<Color>();
					for (int n = 0; n < marshalBitmap2.Height; n++)
					{
						for (int num2 = 0; num2 < marshalBitmap2.Width; num2++)
						{
							list4.Add(marshalBitmap2.GetPixel(num2, n));
						}
					}
					Images.Add(PS2.Unswizzle8(new Size(marshalBitmap2.Width, marshalBitmap2.Height), list4));
				}
				catch (Exception)
				{
                }
			}
		}
		else
		{
			for (int num3 = 0; num3 < marshalBitmap.Height; num3++)
			{
				for (int num4 = 0; num4 < marshalBitmap.Width; num4++)
				{
					list2.Add(PS2.CalcIndex((byte)dictionary[marshalBitmap.GetPixel(num4, num3)]));
				}
			}
			PS2.PS2Image value2 = OpenVXT.Images[index];
			value2.Data = list2.ToArray();
			value2.Palette = list.ToArray();
			OpenVXT.Images[index] = value2;
			Images = new List<MarshalBitmap>();
            using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(VXTArray)))
            {
                    VXT.VXTInfo vXTInfo = OpenVXT.Info[value2.DMAIndex][value2.DMASubIndex];

                    binaryWriter.BaseStream.Position = vXTInfo.DataOffset;
                    binaryWriter.Write(value2.Data);
                    binaryWriter.BaseStream.Position = vXTInfo.ColorOffset;
                    binaryWriter.Write(value2.Palette);
            }
            for (int num5 = 0; num5 < OpenVXT.Images.Count; num5++)
			{
				try
				{
					MarshalBitmap item = OpenVXT.Parse8(num5);
                    if (item != null)
                    {
                        Images.Add(item);
                    }
                    else
                    {
                        Rectangle rect = new Rectangle(0, 0, 1, 1);
                        marshalBitmap = new MarshalBitmap(1, 1);

                        Images.Add(marshalBitmap);
                    }
				}
				catch (Exception)
				{
                    Rectangle rect = new Rectangle(0, 0, 1, 1);
                        marshalBitmap = new MarshalBitmap(1, 1);

                        Images.Add(marshalBitmap);
				}
			}
		}
		IconBox.Image = Images[GetIndex(texture_list.SelectedNode)].Bitmap;
	}

	private void AboutMenuClick(object sender, EventArgs e)
	{
		MessageBox.Show("A tool which allows a user to edit VXT Image Archives from Soulcalibur 3 for the PlayStation 2.\nCreated by TopazTK for the Project Soul Suite.", "About VXT Editor", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
            this.components = new System.ComponentModel.Container();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenAction = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAction = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.AExtractAction = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpen = new System.Windows.Forms.OpenFileDialog();
            this.FileSave = new System.Windows.Forms.SaveFileDialog();
            this.IconGroup = new System.Windows.Forms.GroupBox();
            this.IconBox = new System.Windows.Forms.PictureBox();
            this.IconContext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExtractAction = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportAction = new System.Windows.Forms.ToolStripMenuItem();
            this.InfoGroup = new System.Windows.Forms.GroupBox();
            this.texture_list = new System.Windows.Forms.TreeView();
            this.IconImport = new System.Windows.Forms.OpenFileDialog();
            this.IconExport = new System.Windows.Forms.SaveFileDialog();
            this.MainMenu.SuspendLayout();
            this.IconGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).BeginInit();
            this.IconContext.SuspendLayout();
            this.InfoGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.AboutMenu});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(413, 24);
            this.MainMenu.TabIndex = 0;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenAction,
            this.SaveAction,
            this.toolStripSeparator1,
            this.AExtractAction});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // OpenAction
            // 
            this.OpenAction.Name = "OpenAction";
            this.OpenAction.Size = new System.Drawing.Size(207, 22);
            this.OpenAction.Text = "Open Archive...";
            this.OpenAction.Click += new System.EventHandler(this.OpenActionClick);
            // 
            // SaveAction
            // 
            this.SaveAction.Name = "SaveAction";
            this.SaveAction.Size = new System.Drawing.Size(207, 22);
            this.SaveAction.Text = "Save Archive...";
            this.SaveAction.Click += new System.EventHandler(this.SaveActionClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(204, 6);
            // 
            // AExtractAction
            // 
            this.AExtractAction.Name = "AExtractAction";
            this.AExtractAction.Size = new System.Drawing.Size(207, 22);
            this.AExtractAction.Text = "Extract All from Archive...";
            // 
            // AboutMenu
            // 
            this.AboutMenu.Name = "AboutMenu";
            this.AboutMenu.Size = new System.Drawing.Size(52, 20);
            this.AboutMenu.Text = "About";
            this.AboutMenu.Click += new System.EventHandler(this.AboutMenuClick);
            // 
            // FileOpen
            // 
            this.FileOpen.Filter = "VXT Image Archives|*.vxt;*.vmp|Unknown Files|*.unk|All Files|*.*";
            this.FileOpen.Title = "Select an Image Archive to open...";
            // 
            // FileSave
            // 
            this.FileSave.Filter = "VXT Image Archives|*.vxt;*.vmp|Unknown Files|*.unk|All Files|*.*";
            // 
            // IconGroup
            // 
            this.IconGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IconGroup.Controls.Add(this.IconBox);
            this.IconGroup.Location = new System.Drawing.Point(146, 27);
            this.IconGroup.Name = "IconGroup";
            this.IconGroup.Size = new System.Drawing.Size(255, 274);
            this.IconGroup.TabIndex = 1;
            this.IconGroup.TabStop = false;
            this.IconGroup.Text = "Image Preview:";
            // 
            // IconBox
            // 
            this.IconBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IconBox.BackColor = System.Drawing.Color.Transparent;
            this.IconBox.ContextMenuStrip = this.IconContext;
            this.IconBox.Location = new System.Drawing.Point(6, 19);
            this.IconBox.Name = "IconBox";
            this.IconBox.Size = new System.Drawing.Size(253, 253);
            this.IconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.IconBox.TabIndex = 0;
            this.IconBox.TabStop = false;
            // 
            // IconContext
            // 
            this.IconContext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExtractAction,
            this.ImportAction});
            this.IconContext.Name = "IconContext";
            this.IconContext.Size = new System.Drawing.Size(161, 48);
            // 
            // ExtractAction
            // 
            this.ExtractAction.Name = "ExtractAction";
            this.ExtractAction.Size = new System.Drawing.Size(160, 22);
            this.ExtractAction.Text = "Extract Image...";
            this.ExtractAction.Click += new System.EventHandler(this.ExtractActionClick);
            // 
            // ImportAction
            // 
            this.ImportAction.Name = "ImportAction";
            this.ImportAction.Size = new System.Drawing.Size(160, 22);
            this.ImportAction.Text = "Replace Image...";
            this.ImportAction.Click += new System.EventHandler(this.ImportActionClick);
            // 
            // InfoGroup
            // 
            this.InfoGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.InfoGroup.Controls.Add(this.texture_list);
            this.InfoGroup.Location = new System.Drawing.Point(12, 27);
            this.InfoGroup.MinimumSize = new System.Drawing.Size(128, 0);
            this.InfoGroup.Name = "InfoGroup";
            this.InfoGroup.Size = new System.Drawing.Size(128, 274);
            this.InfoGroup.TabIndex = 2;
            this.InfoGroup.TabStop = false;
            this.InfoGroup.Text = "General Info:";
            // 
            // texture_list
            // 
            this.texture_list.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.texture_list.Location = new System.Drawing.Point(0, 19);
            this.texture_list.Name = "texture_list";
            this.texture_list.Size = new System.Drawing.Size(128, 249);
            this.texture_list.TabIndex = 0;
            this.texture_list.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.texture_list_AfterSelect);
            this.texture_list.MouseDown += new System.Windows.Forms.MouseEventHandler(this.texture_list_MouseDown);
            // 
            // IconImport
            // 
            this.IconImport.Filter = "Portable Network Graphics|*.png";
            this.IconImport.Title = "Open a PNG File...";
            // 
            // IconExport
            // 
            this.IconExport.Filter = "Portable Network Graphics|*.png";
            this.IconExport.Title = "Choose a location to export the image...";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(413, 311);
            this.Controls.Add(this.InfoGroup);
            this.Controls.Add(this.IconGroup);
            this.Controls.Add(this.MainMenu);
            this.MainMenuStrip = this.MainMenu;
            this.MinimumSize = new System.Drawing.Size(429, 349);
            this.Name = "MainForm";
            this.Text = "Project Soul - VXT Editor";
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.IconGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.IconBox)).EndInit();
            this.IconContext.ResumeLayout(false);
            this.InfoGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

	}

    private void texture_list_AfterSelect(object sender, TreeViewEventArgs e)
    {
        int idx = GetIndex(texture_list.SelectedNode);
        IconBox.Image = Images[idx].Bitmap;
        if (IconBox.Image.Width > 256 || IconBox.Image.Height > 256)
        {
            base.Size = new Size(173 + IconBox.Image.Width, 93 + IconBox.Image.Height);
        }
        else
        {
            base.Size = new Size(429, 349);
        }

    }

    private void texture_list_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            IconContext.Show(this, new Point(e.X, e.Y));
        }
    }
}
