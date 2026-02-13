// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace SonnyBIM
{
    public static class BrowserHelper
    {
        public static void OpenUrlWithBrowserOrCopyToClipboard(string url)
        {
            if (url.Contains("mailto:"))
            {
                try
                {
                    Clipboard.SetText(url);
                    MessageBox.Show("The email address of Sonny BIM has been copied to the clipboard.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred when copying the email address to the clipboard: " + ex.Message);
                }
            }
            else
            {
                string[] browserPaths = new string[]
                {
                    @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files\Mozilla Firefox\firefox.exe",
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
                };

                bool browserFound = false;

                foreach (var browserPath in browserPaths)
                {
                    if (System.IO.File.Exists(browserPath))
                    {
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = browserPath,
                                Arguments = url,
                                UseShellExecute = true
                            };
                            Process.Start(psi);
                            browserFound = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }

                if (!browserFound)
                {
                    try
                    {
                        Clipboard.SetText(url);
                        MessageBox.Show("Browser not found. The link has been copied to the clipboard.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred when copying the link to the clipboard: " + ex.Message);
                    }
                }

            }

        }

        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                // Thực hiện mở Windows Explorer với thư mục đã chỉ định
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = folderPath,
                        UseShellExecute = true
                    };

                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
            }
        }

        public static void OpenFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = filePath,
                        UseShellExecute = true
                    };

                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }

    }
}

