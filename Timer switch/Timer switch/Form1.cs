using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Timer_switch
{
    public partial class Form1 : Form
    {
        public class reboot
        {
            [DllImport("advapi32.dll", EntryPoint = "InitiateSystemShutdownEx")]
            static extern int InitiateSystemShutdown(string lpMachineName, string lpMessage, int dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown);
            [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall,
            ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);
            [DllImport("kernel32.dll", ExactSpelling = true)]
            internal static extern IntPtr GetCurrentProcess();
            [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
            [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
            [DllImport("user32.dll", EntryPoint = "LockWorkStation")]
            static extern bool LockWorkStation();
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal struct TokPriv1Luid
            {
                public int Count;
                public long Luid;
                public int Attr;
            }
            internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
            internal const int TOKEN_QUERY = 0x00000008;
            internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
            internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
            private void SetPriv()
            {
                TokPriv1Luid tkp;
                IntPtr htok = IntPtr.Zero;
                if (OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok))
                {
                    tkp.Count = 1;
                    tkp.Attr = SE_PRIVILEGE_ENABLED;
                    tkp.Luid = 0;
                    LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tkp.Luid);
                    AdjustTokenPrivileges(htok, false, ref tkp, 0, IntPtr.Zero, IntPtr.Zero);
                }
            }
            public int halt(bool RSh, bool Force)
            {
                SetPriv();
                return InitiateSystemShutdown(null, null, 0, Force, RSh);
            }
            public int Lock()
            {
                if (LockWorkStation())
                    return 1;
                else
                    return 0;
            }
        }
        ulong time = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (time <= 0)
            {
                timer1.Stop();
                label1.Visible = false;
                textBox1.Visible = true;
                button1.Enabled = true;
                button2.Enabled = false;
                textBox1.Text = time.ToString();
                if (radioButton1.Checked)
                {
                    Application.SetSuspendState(PowerState.Suspend, true, true);
                }
                if (radioButton2.Checked)
                {
                    reboot end = new reboot();
                    end.halt(false, true);
                }
                if (radioButton3.Checked)
                {
                    reboot end = new reboot();
                    end.halt(true, true);
                }
            }
            else
            {
                label1.Text = (--time).ToString();
                label1.Left = label2.Left - label1.Width;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (UInt64.TryParse(textBox1.Text, out time))
            {
                label1.Text = time.ToString();
                label1.Left = label2.Left - label1.Width;
                label1.Visible = true;
                textBox1.Visible = false;
                timer1.Interval = 1000;
                timer1.Enabled = true;
                timer1.Start();
                button1.Enabled = false;
                button2.Enabled = true;
            }
            else
            {
                MessageBox.Show("Недопустимое значение времени");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            label1.Visible = false;
            textBox1.Visible = true;
            button1.Enabled = true;
            button2.Enabled = false;
            textBox1.Text = time.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Size size = TextRenderer.MeasureText(textBox1.Text, textBox1.Font);
            size.Width += 5;
            if (size.Width < 50) size.Width = 50;
            textBox1.Width = size.Width;
            textBox1.Left = label2.Left - textBox1.Width;
        }
    }
}
