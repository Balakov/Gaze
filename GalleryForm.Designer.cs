namespace Gaze
{
    partial class GalleryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GalleryForm));
            this.FilePaneSplitContainer = new System.Windows.Forms.SplitContainer();
            this.DrivesTreeView = new System.Windows.Forms.TreeView();
            this.GalleryListView = new System.Windows.Forms.ListView();
            this.PathLinkLabel = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.FilePaneSplitContainer)).BeginInit();
            this.FilePaneSplitContainer.Panel1.SuspendLayout();
            this.FilePaneSplitContainer.Panel2.SuspendLayout();
            this.FilePaneSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // FilePaneSplitContainer
            // 
            this.FilePaneSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FilePaneSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.FilePaneSplitContainer.Name = "FilePaneSplitContainer";
            // 
            // FilePaneSplitContainer.Panel1
            // 
            this.FilePaneSplitContainer.Panel1.Controls.Add(this.DrivesTreeView);
            // 
            // FilePaneSplitContainer.Panel2
            // 
            this.FilePaneSplitContainer.Panel2.Controls.Add(this.GalleryListView);
            this.FilePaneSplitContainer.Panel2.Controls.Add(this.PathLinkLabel);
            this.FilePaneSplitContainer.Size = new System.Drawing.Size(1011, 737);
            this.FilePaneSplitContainer.SplitterDistance = 337;
            this.FilePaneSplitContainer.SplitterWidth = 5;
            this.FilePaneSplitContainer.TabIndex = 0;
            // 
            // DrivesTreeView
            // 
            this.DrivesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DrivesTreeView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DrivesTreeView.ItemHeight = 18;
            this.DrivesTreeView.Location = new System.Drawing.Point(0, 0);
            this.DrivesTreeView.Name = "DrivesTreeView";
            this.DrivesTreeView.Size = new System.Drawing.Size(337, 737);
            this.DrivesTreeView.TabIndex = 0;
            this.DrivesTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.DrivesTreeView_BeforeExpand);
            this.DrivesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.DrivesTreeView_AfterSelect);
            this.DrivesTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DrivesTreeView_MouseClick);
            // 
            // GalleryListView
            // 
            this.GalleryListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GalleryListView.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GalleryListView.HideSelection = false;
            this.GalleryListView.Location = new System.Drawing.Point(0, 27);
            this.GalleryListView.Margin = new System.Windows.Forms.Padding(0);
            this.GalleryListView.Name = "GalleryListView";
            this.GalleryListView.Size = new System.Drawing.Size(669, 710);
            this.GalleryListView.TabIndex = 0;
            this.GalleryListView.UseCompatibleStateImageBehavior = false;
            this.GalleryListView.ItemActivate += new System.EventHandler(this.GalleryListView_ItemActivate);
            this.GalleryListView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.GalleryListView_KeyUp);
            // 
            // PathLinkLabel
            // 
            this.PathLinkLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.PathLinkLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PathLinkLabel.Location = new System.Drawing.Point(0, 0);
            this.PathLinkLabel.Margin = new System.Windows.Forms.Padding(0);
            this.PathLinkLabel.Name = "PathLinkLabel";
            this.PathLinkLabel.Size = new System.Drawing.Size(669, 27);
            this.PathLinkLabel.TabIndex = 1;
            this.PathLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.PathLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.PathLinkLabel_LinkClicked);
            // 
            // GalleryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 737);
            this.Controls.Add(this.FilePaneSplitContainer);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GalleryForm";
            this.Text = "Gaze Image Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.FilePaneSplitContainer.Panel1.ResumeLayout(false);
            this.FilePaneSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FilePaneSplitContainer)).EndInit();
            this.FilePaneSplitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer FilePaneSplitContainer;
        private System.Windows.Forms.TreeView DrivesTreeView;
        private System.Windows.Forms.ListView GalleryListView;
        private System.Windows.Forms.LinkLabel PathLinkLabel;
    }
}

