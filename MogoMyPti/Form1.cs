﻿using MogoMyPti.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace MogoMyPti
{
    public partial class Form1 : Form
    {
        private readonly int concurrentRequests;
        private readonly Login browserForm;
        private MyPtiGrabber ptiGrabber;
        private List<string> licensePlates;
        private int originalLicensePlateCount;
        private List<ScrapeResult> finalResult = new List<ScrapeResult>();
        private bool pauseState = false;

        public Form1()
        {
            try
            {
                concurrentRequests = int.Parse(ConfigurationManager.AppSettings["ConcurrentRequests"]);
            }
            catch (Exception)
            {
                concurrentRequests = 2;
                MessageBox.Show("კონფიგურაცია ვერ მოიძებნა. 2 პასუხი ერთად my.pti.ge დან");
            }

            browserForm = new Login();

            InitializeComponent();

            browserForm.OnLoggedIn += BrowserForm_LoggedIn;
            browserForm.FormClosed += BrowserForm_FormClosed;

            browserForm.ShowDialog();
        }

        private void BrowserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
                Application.Exit();
        }

        private async void BrowserForm_LoggedIn()
        {
            browserForm.Hide();
            ptiGrabber = new MyPtiGrabber(browserForm.Username, browserForm.Password);
            try
            {
                await ptiGrabber.Login();
            }
            catch (LoginException e)
            {
#if DEBUG
                Console.WriteLine("დალოგინების შეცდომა");
                Console.WriteLine(e);
                Console.ReadLine();
#endif
                MessageBox.Show("დალოგინების შეცდომა", "შეცდომა", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Restart();
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            startButton.Enabled = false;
            downloadButton.Enabled = false;
            if (openSourceFileDialog.ShowDialog() == DialogResult.OK)
            {
                startButton.Enabled = true;

                licensePlates = new ExcelFileToCarLicenseProcessor(openSourceFileDialog.FileName).GetResult();
                originalLicensePlateCount = licensePlates.Count;
                SetUngrabbedCount(licensePlates.Count);
                SetGrabbedCount(0);
            }
        }

        private async void ProcessSourceFile(string filePath)
        {
            var ungrabbedCount = licensePlates.Count;

            while (licensePlates.Count > 0 && pauseState == false)
            {
                finalResult.AddRange(await ptiGrabber.GrabDataFromPtiAsync(licensePlates.Take(concurrentRequests).ToList()));

                try
                {
                    licensePlates.RemoveRange(0, concurrentRequests);
                    ungrabbedCount = licensePlates.Count;
                }
                catch (Exception)
                {
                    licensePlates.Clear();
                    ungrabbedCount = 0;
                }
                SetUngrabbedCount(ungrabbedCount);
                SetGrabbedCount(Math.Abs(ungrabbedCount - originalLicensePlateCount));
            }

            downloadButton.Enabled = true;
        }

        private void downloadButton_Click(object sender, EventArgs e)
        {
            exportExcelDialog.FileName = $"{DateTime.Now.Month}_{DateTime.Now.Day}_{DateTime.Now.Hour}_{DateTime.Now.Minute}_export.xlsx";
            exportExcelDialog.ShowDialog();
            var scGenerator = new ScrapeResultToExcelGenerator(finalResult);
            scGenerator.GenerateExcel(exportExcelDialog.FileName);
        }

        private void SetUngrabbedCount(int count)
        {
            label3.Text = count.ToString();
        }

        private void SetGrabbedCount(int count)
        {
            label4.Text = count.ToString();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            pauseState = false;
            startButton.Enabled = false;
            pauseButton.Enabled = true;
            ProcessSourceFile(openSourceFileDialog.FileName);
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            pauseState = true;
            startButton.Enabled = true;
            pauseButton.Enabled = false;
        }
    }
}
