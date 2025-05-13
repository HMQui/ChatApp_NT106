using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Client.Services
{
    public class CircularPictureBoxService
    {
        public void MakePictureBoxCircular(PictureBox pictureBox)
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddEllipse(0, 0, pictureBox.Width - 1, pictureBox.Height - 1);
            pictureBox.Region = new Region(path);
        }
    }
}
