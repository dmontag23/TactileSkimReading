using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HBBrailleDisV2
{
    public partial class SplashSerialInfo : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashSerialInfo"/> class.
        /// </summary>
        /// <param name="seriennummer">The seriennummer.</param>
        /// <param name="withError">if set to <c>true</c> [with error].</param>
        public SplashSerialInfo(string seriennummer, bool withError)
        {
            InitializeComponent();
            
            if (withError)
            {
                this.Seriennummer.Font = this.Font;
                this.ControlBox = true;
            }
            else
            {
                this.timerToClose.Tick += new EventHandler(timerToClose_Tick);
                this.timerToClose.Start();
            }
            this.Seriennummer.Text = seriennummer;
        }

        void timerToClose_Tick(object sender, EventArgs e)
        {
            try
            {
                this.timerToClose.Stop();
                this.Close();
            }
            catch
            {
            }
        }

    }
}
