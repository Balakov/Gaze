using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Gaze
{
    public partial class GalleryForm : Form, ThumbnailCreator.IAddThumbnail
    {
        private readonly ImageList m_smallImageList = new ImageList();
        private IconListManager m_iconListManager;
        private readonly TreeState m_treeState = new TreeState();
        private ThumbnailCreator m_thumbnailCreator;
        private IWindowManager m_windowManager;

        private readonly Config m_config;
        private readonly string[] m_supportedExtensions;

        private bool m_preventDirectoryChangeEventOnTreeNodeSelect = false;
        
        private const int c_thumbnailWidth = 180;
        private const int c_thumbnailHeight = 100;

        private string m_initialPath = null;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public GalleryForm()
        {
            InitializeComponent();
        }
        
        public GalleryForm(string path, Config config, string[] supportedExtensions, IWindowManager windowManager)
        {
            m_config = config;
            m_supportedExtensions = supportedExtensions;
            m_windowManager = windowManager;
            m_initialPath = path;

            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LayoutFromConfig();

            ListViewItem_SetSpacing(GalleryListView, c_thumbnailWidth + 25, c_thumbnailHeight + 25);

            m_smallImageList.ColorDepth = ColorDepth.Depth32Bit;
            m_smallImageList.ImageSize = new System.Drawing.Size(16, 16);
            m_iconListManager = new IconListManager(m_smallImageList);
            DrivesTreeView.ImageList = m_smallImageList;
            BuildDrivesTreeView();

            string treeStateFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"YellowDroid\Gaze\tree.txt");
            m_treeState.LoadState(DrivesTreeView, treeStateFilename);

            if (!string.IsNullOrEmpty(m_initialPath))
            {
                OpenTreeViewToPath(m_initialPath);
            }
            
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                m_config.SetValue("maximised", (this.WindowState == FormWindowState.Minimized).ToString());
                m_config.SetValue("xpos", this.Location.X.ToString());
                m_config.SetValue("ypos", this.Location.Y.ToString());
                m_config.SetValue("width", this.Size.Width.ToString());
                m_config.SetValue("height", this.Size.Height.ToString());
            }

            m_config.SetValue("treeviewsplitterdistance", FilePaneSplitContainer.SplitterDistance.ToString());
             
            string treeStateFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"YellowDroid\Gaze\tree.txt");
            m_treeState.SaveState(DrivesTreeView, treeStateFilename);

            m_windowManager.ClosingGalleryWindow(this);
        }

        public void ListViewItem_SetSpacing(ListView listview, short leftPadding, short topPadding)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;

            int padding = (int)(((ushort)leftPadding) | (uint)(topPadding << 16));

            SendMessage(listview.Handle, LVM_SETICONSPACING, IntPtr.Zero, (IntPtr)padding);
        }

        private void LayoutFromConfig()
        {
            int xPos = Convert.ToInt32(m_config.GetValue("xpos", this.Location.X.ToString()));
            int yPos = Convert.ToInt32(m_config.GetValue("ypos", this.Location.Y.ToString()));
            int width = Convert.ToInt32(m_config.GetValue("width", this.Size.Width.ToString()));
            int height = Convert.ToInt32(m_config.GetValue("height", this.Size.Height.ToString()));
            this.Size = new Size(width, height);
            this.Location = new Point(xPos, yPos);

            bool isMaximised = Convert.ToBoolean(m_config.GetValue("maximised", "false"));
            if (isMaximised)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            FilePaneSplitContainer.SplitterDistance = Convert.ToInt32(m_config.GetValue("treeviewsplitterdistance", FilePaneSplitContainer.SplitterDistance.ToString()));
        }

        private string BuildDrivesTreeView()
        {
            DrivesTreeView.BeginUpdate();

            TreeNode root = DrivesTreeView.Nodes.Add("Computer");
            root.ImageIndex = m_iconListManager.AddStockIcon(StockIconID.DesktopPC);

            string firstReadyDrivePath = null;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    firstReadyDrivePath = firstReadyDrivePath ?? drive.RootDirectory.FullName;

                    AddDriveToTree(root, drive);
                }
            }

            root.Expand();

            DrivesTreeView.EndUpdate();

            return firstReadyDrivePath;
        }

        private void AddDriveToTree(TreeNode root, DriveInfo drive)
        {
            int folderIconIndex = m_iconListManager.AddDriveIcon(drive.RootDirectory.FullName, false);

            TreeNode node = new TreeNode($"{drive.Name} ({drive.VolumeLabel})", folderIconIndex, folderIconIndex)
            {
                Tag = new DirectoryTag(drive.RootDirectory.FullName)
            };

            node.Nodes.Add("Dummy");// Enable the "Expand" icon
            root.Nodes.Add(node);
        }

        private void DrivesTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Tag is PathTag)
            {
                string newDir = (e.Node.Tag as PathTag).Path;

                e.Node.Nodes.Clear();   // Remove the dummy
                e.Node.Tag = new DirectoryTag(newDir);  // Convert to a full directory from a lazy

                // Evaulate all of the directories in the new path
                try
                {
                    foreach (string dir in Directory.EnumerateDirectories(newDir).OrderBy(x => x))
                    {
                        try
                        {
                            DirectoryInfo di = new DirectoryInfo(dir);

                            if (!di.Attributes.HasFlag(FileAttributes.Hidden) &&
                                !di.Attributes.HasFlag(FileAttributes.System))
                            {
                                string leafName = Path.GetFileName(dir);

                                int folderIconIndex = m_iconListManager.AddFolderIcon(dir, false);

                                TreeNode node = new TreeNode(leafName, folderIconIndex, folderIconIndex)
                                {
                                    Tag = new DirectoryTag(dir)
                                };

                                node.Nodes.Add("Dummy");
                                e.Node.Nodes.Add(node);
                            }
                        }
                        catch (Exception ex)
                        {
                            e.Node.Nodes.Add(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    e.Node.Nodes.Add(ex.Message);
                }
            }
        }

        private void DrivesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!m_preventDirectoryChangeEventOnTreeNodeSelect)
            {
                if (e.Node.Tag is PathTag)
                {
                    FillListView((e.Node.Tag as PathTag).Path, syncDirectoryTreeView: false);
                }
            }
        }

        private void DrivesTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeViewHitTestInfo info = DrivesTreeView.HitTest(e.X, e.Y);
            if (info.Node != null)
            {
                if (e.Button == MouseButtons.Left)
                {
                    // If the selection is different we'll call the event handler anyway. Only need 
                    // to force the event handler call if the node is the same as the currently selected one.
                    if (DrivesTreeView.SelectedNode == info.Node)
                    {
                        DrivesTreeView_AfterSelect(sender, new TreeViewEventArgs(info.Node));
                    }
                }
                else
                {
                    m_preventDirectoryChangeEventOnTreeNodeSelect = true;
                    DrivesTreeView.SelectedNode = info.Node;
                    m_preventDirectoryChangeEventOnTreeNodeSelect = false;
                }
            }
        }

        private void FillListView(string directory, bool syncDirectoryTreeView)
        {
            BuildBreadcrumbs(directory);

            if (m_thumbnailCreator != null)
            {
                m_thumbnailCreator.Abort();
            }

            GalleryListView.SuspendLayout();
            GalleryListView.BeginUpdate();

            GalleryListView.Items.Clear();

            var oldImageList = GalleryListView.LargeImageList;
            GalleryListView.LargeImageList = null;

            if (oldImageList != null)
            {
                foreach (var image in oldImageList.Images)
                {
                    (image as Image).Dispose();
                }

                oldImageList.Dispose();
            }

            var imageList = new ImageList();
            imageList.ImageSize = new Size(c_thumbnailWidth, c_thumbnailHeight);
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            List<string> files = new List<string>();

            try
            {
                foreach (string dir in Directory.EnumerateDirectories(directory))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);

                    if (!di.Attributes.HasFlag(FileAttributes.Hidden) &&
                        !di.Attributes.HasFlag(FileAttributes.System))
                    {
                        files.Add(null);

                        ListViewItem lvi = new ListViewItem()
                        {
                            Text = Path.GetFileName(dir),
                            Tag = new DirectoryTag(dir)
                        };

                        GalleryListView.Items.Add(lvi);
                    }
                }

                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    var extension = Path.GetExtension(file).ToLower();

                    if (m_supportedExtensions.Contains(extension))
                    {
                        files.Add(file);

                        ListViewItem lvi = new ListViewItem()
                        {
                            Text = Path.GetFileName(file),
                            Tag = new ImageTag(file)
                        };

                        GalleryListView.Items.Add(lvi);
                    }
                }

                m_thumbnailCreator = new ThumbnailCreator(files, this, c_thumbnailWidth, c_thumbnailHeight, imageList);
            }
            catch (Exception)
            {
            }
            
            GalleryListView.LargeImageList = imageList;

            GalleryListView.ResumeLayout();
            GalleryListView.EndUpdate();

            if (syncDirectoryTreeView)
            {
                OpenTreeViewToPath(directory);
            }
        }

        private void OpenTreeViewToPath(string directory)
        {
            // Ignore the root node
            TreeNode root = DrivesTreeView.Nodes[0];

            string[] parts = directory.Split(Path.DirectorySeparatorChar);

            TreeNode node = FindTreeNodeByPathRecurse(root, parts, 0);

            if (node != null)
            {
                DrivesTreeView.SelectedNode = node;
            }
        }

        private static TreeNode FindTreeNodeByPathRecurse(TreeNode root, string[] directoryParts, int partIndex)
        {
            string directory = partIndex == 0 ? directoryParts[0] + Path.DirectorySeparatorChar :
                                                string.Join(Path.DirectorySeparatorChar.ToString(), directoryParts, 0, partIndex + 1);

            foreach (TreeNode node in root.Nodes)
            {
                if (node.Tag is DirectoryTag)
                {
                    DirectoryTag tag = node.Tag as DirectoryTag;
                    if (tag.Path == directory)
                    {
                        if (partIndex == directoryParts.Length - 1)
                        {
                            return node;
                        }
                        else
                        {
                            // Matched this part of the path! Onto the next.
                            node.Expand();

                            return FindTreeNodeByPathRecurse(node, directoryParts, partIndex + 1);
                        }
                    }
                }
            }

            return null;
        }

        private void PathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FillListView(e.Link.LinkData as string, syncDirectoryTreeView: true);
        }

        private void BuildBreadcrumbs(string directory)
        {
            string[] parts;
            if (directory.StartsWith(@"\\"))
            {
                parts = directory.Substring(2).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                parts[0] = @"\\" + parts[0];
            }
            else
            {
                parts = directory.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            }

            int ignoreIndex = 0;
            Size stringSize;
            int maxWidth = PathLinkLabel.Width - 30;

            do
            {
                PathLinkLabel.Links.Clear();

                string fullPath = parts[0];
                string fullLinkPath = parts[0];
                PathLinkLabel.Links.Add(0, fullPath.Length, fullLinkPath);

                for (int i = 1; i < parts.Length; i++)
                {
                    int currentStartIndex = fullPath.Length + 1;    // +1 for '\' we're about to add.
                    fullLinkPath += Path.DirectorySeparatorChar + parts[i];

                    if (i > ignoreIndex)
                    {
                        fullPath += Path.DirectorySeparatorChar + parts[i];
                        PathLinkLabel.Links.Add(currentStartIndex, parts[i].Length, fullLinkPath);
                    }
                    else
                    {
                        fullPath += Path.DirectorySeparatorChar + "...";
                    }
                }

                stringSize = TextRenderer.MeasureText(fullPath, PathLinkLabel.Font);
                if (stringSize.Width > maxWidth)
                {
                    if (ignoreIndex < parts.Length - 2) // Keep the first and last parts and rely on the LinkLabel to add ellipsis at the end.
                    {
                        ignoreIndex++;
                    }
                    else
                    {
                        // We can't ignore any more, we'll just have to go with what we have
                        PathLinkLabel.Text = fullPath;
                        stringSize.Width = int.MinValue;
                    }
                }
                else
                {
                    PathLinkLabel.Text = fullPath;
                }

            } while (stringSize.Width > maxWidth);

            PathLinkLabel.TabStop = false;
        }

        private void GalleryListView_ItemActivate(object sender, EventArgs e)
        {
            if (GalleryListView.SelectedItems.Count > 0)
            {
                var pathTag = GalleryListView.SelectedItems[0].Tag as PathTag;

                if (GalleryListView.SelectedItems[0].Tag is ImageTag)
                {
                    m_windowManager.OpenImageWindow(pathTag.Path);
                }
                else if (GalleryListView.SelectedItems[0].Tag is DirectoryTag)
                {
                    FillListView(pathTag.Path, syncDirectoryTreeView: true);
                }
            }
        }

        public void AddThumbnail(Image image, int index, ImageList currentImageList)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate { AddThumbnail(image, index, currentImageList); });
                }
                else
                {
                    // If the previous thumbnail creation was still going on we may get
                    // messages back from that thread. We don't want it messing with our
                    // new image list.
                    if (currentImageList == GalleryListView.LargeImageList)
                    {
                        GalleryListView.LargeImageList.Images.Add(image);
                        GalleryListView.Items[index].ImageIndex = index;
                    }
                }
            }
            catch
            {
            }
        }

    }   // Class

    public abstract class PathTag
    {
        public string Path { get; }
        public PathTag(string path) { Path = path; }
    }
    
    public class ImageTag : PathTag
    {
        public ImageTag(string path) : base(path) { }
    }
    
    public class DirectoryTag : PathTag
    {
        public DirectoryTag(string path) : base(path) { }
    }

} // Namespace
