// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

#region Namespaces

using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB.Structure;
using MessageBox = System.Windows.Forms.MessageBox;
#endregion

namespace SonnyBIM
{
    public static class OfficeUtils
    {
        /// <summary>
        /// isNotPermission = true: không cho phép ghi file lên ổ cứng
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <param name="isNotPermission"></param>
        /// <param name="isHidden"></param>
        public static void WriteTextToFile(string content, string path,
             ref bool isNotPermission, bool isHidden = true)
        {
            try
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    MessageBox.Show("Addin need permission for write setting files on " +
                                    path +
                                    "\nIf your computer doesn't have permission access, you can't use this Add-in.\n" +
                                    "Please allow access to that file and try again!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isNotPermission = true;
                }

                File.WriteAllText(path, content);

                try
                {
                    if (isHidden)
                    {
                        File.SetAttributes(path, FileAttributes.Hidden);
                    }
                }
                catch
                {
                    MessageBox.Show("Addin need permission for write setting files on " +
                                    path +
                                    "\nIf your computer doesn't have permission access, you can't use this Add-in.\n" +
                                    "Please allow access to that file and try again!",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isNotPermission = true;
                }
            }
            catch
            {
                MessageBox.Show("Add-in need permission for write setting files on " +
                                path +
                                "\nIf your computer doesn't have permission access, you can't use this Add-in.\n" +
                                "Please allow access to that file and try again!",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isNotPermission = true;
            }
        }

        public static void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Add-in need permission for create new directory on " +
                    //"C:\\ProgramData\\Autodesk\\ApplicationPlugins\\QApps Plus_2017.bundle\\Contents\\Resources\\Setting\n" +
                    path +
                    "\nIf your computer doesn't have permission access, you can't use this Add-in.\n" +
                    "Please allow access to that directory and try again!",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ReadAllText(string path)
        {
            string result = string.Empty;
            try
            {
                result = File.ReadAllText(path);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Add-in need permission for read value on " +
                    path +
                    "\nIf your computer doesn't have permission access, you can't use this Add-in.\n" +
                    "Please allow access to this file and try again!",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }

        public static DataTable ReadFile(string path, int numberColumn)
        {
            char[] tabs = { '\t' };         // tab ngang
            char[] quotes = { '\"' };       // dấu nháy kép "

            DataTable table = new DataTable("dataFromFile");


            for (int i = 0; i < numberColumn; i++)
            {
                table.Columns.Add(new DataColumn("col" + i, typeof(string)));
            }

            using (StreamReader sr = new StreamReader(path))
            {
                table.BeginLoadData();
                string line;
                //int rowsCount = 0;

                string firstLine = sr.ReadLine();
                // string otherLine = sr.ReadToEnd();

                //  string[] firstLineData = firstLine.Split(new[] { "\"", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                string[] firstLineData = (
                    from s in firstLine.Split(tabs)
                    select s.Trim(quotes)).ToArray();

                if (firstLineData.Length == numberColumn)
                {
                    table.LoadDataRow(firstLineData, true);
                    //rowsCount++;
                }
                else
                {
                    foreach (string item in firstLineData)
                    {
                        if (item != string.Empty)
                        {
                            table.Rows.Add();
                            table.Rows[0][0] = item;
                            //  rowsCount++;
                            break;
                        }
                    }
                }

                while (true)
                {
                    line = sr.ReadLine();
                    if (line == null) break;

                    string[] array = (
                        from s in line.Split(tabs)
                        select s.Trim(quotes)).ToArray();

                    if (array.Length == numberColumn)
                    {
                        table.LoadDataRow(array, true);
                    }
                }


                //while ((line = sr.ReadLine()) != null)
                //{
                //    //string[] data = line.Split(new[] { "\"", "\t" }, StringSplitOptions.RemoveEmptyEntries);

                //    //table.Rows.Add();

                //    //for (int i = 0; i < data.Length; i++)
                //    //{
                //    //    table.Rows[rowsCount][i] = data[i];
                //    //    //if (data[i].Contains(" "))
                //    //    //    data[i] = data[i].Replace(" ", "");
                //    //    //if (!data[i].Equals("\t"))
                //    //    //    table.Rows[rowsCount][i] = data[i];
                //    //}
                //    //rowsCount++;


                //    string[] data1 = line.Split(new[] { "\"" }, StringSplitOptions.None); // một chuỗi gồm string, tab ngang và empty

                //    string[] data2 = new string[data1.Length - 2]; // bõ 2 phần tử empty đầu và cuối của data1

                //    int k = 0;

                //    for (int i = 1; i < data1.Length - 1; i++)
                //    {
                //        if (data1[i] != "\t")
                //        {
                //            data2[k] = data1[i];
                //            k++;
                //        }
                //    }

                //    string[] data = new string[k]; // mảng gồm k phần tử

                //    for (int i = 0; i < k; i++)
                //    {
                //        data[i] = data2[i];
                //    }

                //    table.Rows.Add();

                //    for (int i = 0; i < data.Length; i++)
                //    {
                //        table.Rows[rowsCount][i] = data[i];
                //    }

                //    rowsCount++;
                //}
            }

            table.EndLoadData();
            return table;
        }

        public static string SaveFileDialogXML()
        {
            string fileName = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = "Save file as...",
                Filter = "XML files (*.XML)|*.XML",
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
            }
            return fileName;
        }

        public static string SaveFileDialogCSV(string title)
        {
            string fileName = "";
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = title,
                Filter = "Text files (*.csv)|*.csv",
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveFileDialog.FileName;
            }
            return fileName;
        }


        /// <summary>
        /// Trả về path của file được Save. Nếu ko Save thì trả về string.Empty
        /// </summary>
        /// <param name="title"></param>
        /// <param name="fileName"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string SaveFileDialog(string title,
            string fileName, string filter = "All files|*.*")
        {
            string filePath = string.Empty;
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Title = title,
                Filter = filter,
                RestoreDirectory = true,
                FileName = fileName,
                //AddExtension = true,
            };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = saveFileDialog.FileName;
            }
            return filePath;

        }

        /// <summary>
        /// Trả về String.Empty khi không chọn file
        /// </summary>
        /// <param name="title"></param>
        /// <param name="caption">Hiện lên khi không chọn file từ hộp thoại hiện ra</param>
        /// <param name="filter"></param>
        /// <param name="mainInstruction">Hiện lên khi không chọn file từ hộp thoại hiện ra</param>
        /// <returns></returns>
        public static string GetPathDialog(string title, string mainInstruction, string caption,
            string filter = "All files|*.*")
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = title;
            dlg.Filter = filter;
            //dlg.Filter = String.Concat("All files|*.*",
            //    "|Text files|*.txt",
            //    "|Excel spreadsheet files|*.xls;*.xlsx");
            if (DialogResult.OK != dlg.ShowDialog())
            {
                MessageBox.Show(mainInstruction,
                    caption, MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return string.Empty;
            }
            else
            {
                return dlg.FileName;
            }
        }

        /// <summary>
        /// Lấy về đường dẫn file .txt tạm ở thư mục Path.GetTempPath().
        /// nếu chưa có thì tạo mới file tạm
        /// </summary>
        /// <param name="title"></param>
        /// <param name="mainInstruction"></param>
        /// <param name="caption"></param>
        /// <param name="filter"></param>
        public static string GetTempFile(string fileName = "ALB_SharedParameter.txt")
        {
            //string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            //string filePath = Path.Combine(path, fileName);

            string filePath = Path.Combine(PathSetting.PathUserLicense, fileName);
            if (!File.Exists(filePath)) File.Create(filePath);

            return filePath;
        }

        public static string RemoveSpecialCharacter(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return String.Empty;
            List<string> listSpecialCharacter = new List<string>()
            {
                "\\", ":", "{","}","[","]","|",
                ";","<",">","?","`","~"
            };
            foreach (var s in listSpecialCharacter)
                text = text.Replace(s, String.Empty);

            return text.Trim();
        }

    }
}

