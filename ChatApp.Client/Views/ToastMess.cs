using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp.Client.Views
{
    public partial class ToastMess : Form
    {
        int toastX, toastY;
        string _title, _message;
        public ToastMess(string t, string m)
        {
            InitializeComponent();

            _title = t;
            _message = m;
        }

        private void ToastMess_Load(object sender, EventArgs e)
        {
            title.Text = _title;
            title.ForeColor = Color.FromArgb(0, 120, 215);
            title.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            mess.Text = _message;
            mess.ForeColor = Color.FromArgb(0, 120, 215);
            mess.Font = new Font("Segoe UI", 14, FontStyle.Regular);
            Position();
        }

        private void Position()
        {
            int ScreenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int ScreenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            toastX = ScreenWidth - this.Width - 10;
            toastY = ScreenHeight - this.Height - 10;

            this.Location = new Point(toastX, toastY);
        }

        private void toastTimer_Tick(object sender, EventArgs e)
        {
            toastY -= 10;
            this.Location = new Point(toastX, toastY);
            if (toastY < 850)
            {
                toastTimer.Stop();
                toastHidden.Start();
            }
        }

        int y = 200;
        private void toastHidden_Tick(object sender, EventArgs e)
        {
            y--;
            if (y <= 0)
            {
                toastY += 1;
                this.Location = new Point(toastX, toastY += 10);
                
                if (toastY > 800)
                {
                    toastHidden.Stop();
                    y = 100;
                    this.Close();
                }
            }           
        }
    }
}
