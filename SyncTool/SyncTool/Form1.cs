using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SyncTool
{
    public partial class Form1 : Form
    {
        List<List<string>> slots = new List<List<string>>();
        List<Object> syncs = new List<object>();

        public Form1()
        {
            InitializeComponent();
        }

        private int interval = 500;
        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 1;
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "SyncTool";
            notifyIcon1.Icon = this.Icon;

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "winstart.ppst")))
                checkBox1.Checked = true;
            else
                checkBox1.Checked = false;

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "interval.ppst")))
            {
                StreamReader sr = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "interval.ppst"));
                interval = Convert.ToInt32(sr.ReadLine());
                sr.Close();
                textBox1.Text = interval.ToString();
            }
            else
            {
                interval = 1000;
                textBox1.Text = interval.ToString();
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "slots.ppst")))
            {
                StreamReader sr = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "slots.ppst"));
                while (!sr.EndOfStream)
                    slots.Add(sr.ReadLine().Split(';').ToList<string>());
                sr.Close();
                foreach (List<string> slot in slots) {
                    ListViewItem lvi = listView1.Items.Add(slot[0]);
                    lvi.SubItems.Add(slot[1]);
                    lvi.SubItems.Add(slot[2]);
                }
            }
            if (Environment.GetCommandLineArgs().ToList<string>().Contains("/silent"))
            {
                if (!checkBox1.Checked)
                    Application.Exit();
                foreach (ListViewItem lvi in listView1.Items)
                {
                    if (lvi.SubItems[0].Text == "Folder")
                    {
                        SyncFolderObject s = new SyncFolderObject();
                        s.f1 = lvi.SubItems[1].Text;
                        s.f2 = lvi.SubItems[2].Text;
                        s.Interval = interval;
                        s.Start();
                        syncs.Add(s);
                    }
                    else
                    {
                        SyncFileObject s = new SyncFileObject();
                        s.f1 = lvi.SubItems[1].Text;
                        s.f2 = lvi.SubItems[2].Text;
                        s.Interval = interval;
                        s.Start();
                        syncs.Add(s);
                    }
                }
                button4.Text = "Stop Service";
                this.Opacity = 0;
                this.ShowInTaskbar = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
   
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 1)
                folderBrowserDialog1.ShowDialog();
            else
                openFileDialog1.ShowDialog();
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            folderBrowserDialog2.ShowDialog();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            ListViewItem lvi = listView1.Items.Add(comboBox1.Text);
            if (comboBox1.SelectedIndex == 1){
                lvi.SubItems.Add(folderBrowserDialog1.SelectedPath);
                lvi.SubItems.Add(folderBrowserDialog2.SelectedPath);
                slots.Add(new List<string>() { lvi.SubItems[0].Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text });
            }
            else
            {
                lvi.SubItems.Add(openFileDialog1.FileName);
                FileInfo fi = new FileInfo(openFileDialog1.FileName);
                lvi.SubItems.Add(Path.Combine(folderBrowserDialog2.SelectedPath,fi.Name));
                slots.Add(new List<string>() { lvi.SubItems[0].Text, lvi.SubItems[1].Text, lvi.SubItems[2].Text });
            }
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "slots.ppst"));
            StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "slots.ppst"), true);
            foreach (List<string> slot in slots)
            {
                sw.WriteLine(slot[0] + ";" + slot[1] + ";" + slot[2]);
            }
            sw.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (button4.Text == "Start Service")
            {
                foreach (ListViewItem lvi in listView1.Items)
                {
                    if (lvi.SubItems[0].Text == "Folder")
                    {
                        SyncFolderObject s = new SyncFolderObject();
                        s.f1 = lvi.SubItems[1].Text;
                        s.f2 = lvi.SubItems[2].Text;
                        s.Interval = interval;
                        s.Start();
                        syncs.Add(s);
                    }
                    else
                    {
                        SyncFileObject s = new SyncFileObject();
                        s.f1 = lvi.SubItems[1].Text;
                        s.f2 = lvi.SubItems[2].Text;
                        s.Interval = interval;
                        s.Start();
                        syncs.Add(s);
                    }
                }

                notifyIcon1.BalloonTipTitle = "SyncTool";
                notifyIcon1.BalloonTipText = "Synchronizing started!";
                notifyIcon1.ShowBalloonTip(1000);
                button4.Text = "Stop Service";
                this.Opacity = 0;
                this.ShowInTaskbar = false;
            }
            else {
                foreach (Object sync in syncs)
                {
                    if (sync.GetType().ToString() == "SyncTool.SyncFileObject")
                        ((SyncFileObject)sync).Stop();
                    else
                        ((SyncFolderObject)sync).Stop();
                }
                System.Diagnostics.Process.Start(Application.ExecutablePath);
                Application.ExitThread();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Opacity = 0;
            this.ShowInTaskbar = false;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Opacity = 1;
            this.ShowInTaskbar = true;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                ContextMenu c = new ContextMenu();
                MenuItem m = new MenuItem();
                m.Index = 0;
                m.Text = "&Beenden";
                m.Click += new EventHandler(ExitClick);
                c.MenuItems.Add(m);
                notifyIcon1.ContextMenu = c;
            }
        }

        private void ExitClick(object sender, EventArgs e) {
            foreach (Object sync in syncs)
            {
                if (sync.GetType().ToString() == "SyncTool.SyncFileObject")
                    ((SyncFileObject)sync).Stop();
                else
                    ((SyncFolderObject)sync).Stop();
            }
            Application.ExitThread();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            slots.RemoveAt(listView1.SelectedIndices[0]);
            listView1.Items.RemoveAt(listView1.SelectedIndices[0]);
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"slots.ppst"));
            StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "slots.ppst"), true);
            foreach (List<string> slot in slots)
            {
                sw.WriteLine(slot[0] + ";" + slot[1] + ";" + slot[2]);
            }
            sw.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "interval.ppst"));
            sw.Write(textBox1.Text);
            sw.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "winstart.ppst"));
            else
            {
                File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "winstart.ppst")).Close();
            }
        }
    }
}
