using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Gaze
{

    public class ThumbnailCreator
    {
        private Thread m_thread;
        private bool m_abort = false;
        private Image m_folderThumbnail = null;

        public interface IAddThumbnail
        {
            void AddThumbnail(Image image, int index, ImageList currentImageList);
        }

        public ThumbnailCreator(List<string> files, IAddThumbnail thumbnailProcessor, int thumbnailWidth, int thumbnailHeight, ImageList currentImageList)
        {
            m_thread = new Thread(() =>
            {
                int index = 0;
                foreach (string file in files)
                {
                    if (m_abort)
                    {
                        break;
                    }

                    if (file == null)
                    {
                        if (m_folderThumbnail == null)
                        {
                            using (var image = ImageLoader.Load(@".\\Icons\\Folder.png"))
                            {
                                m_folderThumbnail = CreateThumbnail(image, thumbnailWidth, thumbnailHeight);
                            }
                        }

                        thumbnailProcessor.AddThumbnail(m_folderThumbnail, index, currentImageList);
                    }
                    else
                    {
                        var thumbnail = ImageLoader.Load(file, thumbnailWidth, thumbnailHeight);
                        thumbnailProcessor.AddThumbnail(thumbnail, index, currentImageList);
                       
                        /*
                        using (var image = ImageLoader.Load(file))
                        {
                            var thumbnail = CreateThumbnail(image, thumbnailWidth, thumbnailHeight);
                            thumbnailProcessor.AddThumbnail(thumbnail, index, currentImageList);
                        }
                        */
                    }

                    index++;
                }
            });

            m_thread.IsBackground = true;
            m_thread.Start();
        }

        public void Abort()
        {
            m_abort = true;
        }

        private Image CreateThumbnail(Image image, int desiredWidth, int desiredHeight)
        {
            var bmp = new Bitmap(desiredWidth, desiredHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int width;
            int height;

            if (image.Width > image.Height)
            {
                float aspect = (float)image.Width / image.Height;
                width = desiredWidth;
                height = (int)(width / aspect);
            }
            else
            {
                float aspect = (float)image.Width / image.Height;
                height = desiredHeight;
                width = (int)(height * aspect);
            }

            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(image, (desiredWidth-width) / 2, (desiredHeight - height) / 2, width, height);
            }

            return bmp;
        }
    }
}
