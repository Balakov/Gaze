using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Gaze
{
    public interface IWindowManager
    {
        void OpenGalleryWindow(string path);
        void OpenImageWindow(string path);
        void ClosingGalleryWindow(Form form);
        void ClosingImageWindow(Form form);
    }

    public class MainForm : Form, IWindowManager
    {
        private readonly Config m_config = new Config();
        private Form m_openGalleryWindow = null;
        private List<Form> m_openImageWindows = new List<Form>();

        private readonly string[] m_supportedExtensions =
        {
            ".afdesign",
            ".bmp",
            ".dds",
            ".emf",
            //".eps",
            ".gif",
            ".ico",
            ".jpeg",
            ".jpg",
            ".png",
            ".pcx",
            //".pdf",
            ".psd",
            ".svg",
            ".tiff",
            ".tif",
            ".tga",
            //".wmf"
        };

        public MainForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.Load += MainForm_Load;

            string configFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"YellowDroid\Gaze\settings.txt");
            m_config.Load(configFilename);

            // If we have a commandline file then don't load the tree state
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                OpenImageWindow(args[1]);
            }
            else
            {
                OpenGalleryWindow(null);
            }

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MyOnClosing);
        }

        private void MyOnClosing(object o, System.ComponentModel.CancelEventArgs e)
        {
            m_config.Save();
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            this.Size = new Size(0, 0);
        }

        private void CheckForClose()
        {
            if (m_openGalleryWindow == null && m_openImageWindows.Count == 0)
            {
                Close();
            }
        }

        public void OpenGalleryWindow(string path)
        {
            if (m_openGalleryWindow != null)
            {
                m_openGalleryWindow.Activate();
            }
            else
            {
                var galleryForm = new GalleryForm(path, m_config, m_supportedExtensions, this);
                m_openGalleryWindow = galleryForm;
                galleryForm.Show();
            }
        }

        public void OpenImageWindow(string path)
        {
            var imageForm = new ImageForm(path, m_config, m_supportedExtensions, this);
            m_openImageWindows.Add(imageForm);
            imageForm.Show();
        }

        public void ClosingGalleryWindow(Form form)
        {
            m_openGalleryWindow = null;
            CheckForClose();

            // Copy the collection as it will be modified when the forms close
            var openWindowsListCopy = m_openImageWindows.ToList();

            foreach (Form imageForm in openWindowsListCopy) 
            {
                imageForm.Close();
            }
        }

        public void ClosingImageWindow(Form form)
        {
            m_openImageWindows.Remove(form);
            CheckForClose();
        }
    }
}
