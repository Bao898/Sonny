// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using MessageBox = System.Windows.Forms.MessageBox;
using Application = System.Windows.Application;

namespace SonnyBIM
{
    [Obfuscation]
    public abstract class ViewModelBaseNew : INotifyPropertyChanged, IDataErrorInfo
    {

        public ViewModelBaseNew()
        {
            ChangeLanguageViewModelBaseNew(LanguageData.GetLanguageSetting());
        }

        #region OnPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [Obfuscation]
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion OnPropertyChanged

        [Obfuscation]
        public void OnPropertyChangedAll(dynamic viewModel)
        {
            foreach (PropertyInfo property in viewModel.GetType().GetProperties())
            {
                viewModel.OnPropertyChanged(property.Name);
            }
        }

        [Obfuscation]
        public virtual void LoadSetting<T>(T viewModel) { }

        [Obfuscation]
        public virtual void SaveSetting<T>(object viewModel, T dataSave, string pathSaveDate)
        {
            // 2 dòng này gây lỗi khi chạy tool Rebar. check kỹ về tác dụng của 2 dòng này đối với các tool cần Save Setting
            //string dllFolder = @"C:\ProgramData\Autodesk\ApplicationPlugins\SonnyBIM.bundle\Contents\2025";
            //AssemblyLoader.LoadAssemblies(dllFolder, "Newtonsoft.Json.dll");


            dataSave = MethodSaveSetting.ConvertDataSaveSeting(viewModel, dataSave);
            FileJson<T> fileJson = new FileJson<T>();
            fileJson.SaveFileJson(dataSave, pathSaveDate);
        }


        #region Binding language cho những button thông dụng

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string OkCap { get; set; } = "ĐỒNG Ý";

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string CancelCap { get; set; } = "HỦY BỎ";
        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>

        [JsonIgnore]
        [Obfuscation]
        public string OpenWebSiteToolTip { get; set; }
        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>

        [JsonIgnore]
        [Obfuscation]
        public string CustomDevelopmentToolTip { get; set; }

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>

        [JsonIgnore]
        [Obfuscation]
        public string LicenseLeftToolTip { get; set; }

        // /// <summary>
        // /// from ViewModelBaseNew
        // /// </summary>
        // [JsonIgnore]
        // [Obfuscation]
        // public string UserManualCaption { get; set; }

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string UserManualToolTip { get; set; }

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string UnitCaption { get; set; }

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string TitleCaption { get; set; } = "Title Caption";



        ///// <summary>
        ///// from ViewModelBaseNew
        ///// </summary>
        //public List<LanguageData> AllLanguage { get; set; }/* = LanguageData.AllLanguage;*/

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [Obfuscation]
        public void ChangeLanguageViewModelBaseNew(string languageCode)
        {
            OkCap = BindingUtils.ChangeLanguage(languageCode,
                "ĐỒNG Ý",
                "OK",
                "オーケー",
                 "Está bien",
                 "Baik",
                 "ตกลง"
                );

            CancelCap = BindingUtils.ChangeLanguage(languageCode,
                "HỦY BỎ",
                "CLOSE",
                "近い",
                "CERRAR",
                "TUTUP",
                "ปิด"
                );

            OpenWebSiteToolTip = BindingUtils.ChangeLanguage(languageCode,
                "Ghé thăm website của Sonny BIM",
                "Open up the Sonny BIM website",
                "Sonny BIM Web サイトを開く",
                "Abra el sitio web de Sonny BIM",
                "Buka situs web Sonny BIM",
                "เปิดเว็บไซต์ Sonny BIM"
                );

            CustomDevelopmentToolTip = BindingUtils.ChangeLanguage(languageCode,
                "Đóng góp ý tưởng/Phản hồi",
                "Custom Development/Feedback",
                "カスタム開発/フィードバック",
                "Desarrollo personalizado/Comentarios",
                "Pengembangan/Umpan Balik Khusus",
                "การพัฒนาแบบกำหนดเอง/คำติชม"
                );

            LicenseLeftToolTip = BindingUtils.ChangeLanguage(languageCode,
                "Thời gian bản quyền Sonny BIM Software còn lại?",
                "Number of days of Sonny BIM Software license remaining?",
                "Sonny BIM ソフトウェア ライセンスの残り日数は?",
                "¿Número de días restantes de licencia del software Sonny BIM?",
                "Berapa hari sisa lisensi Perangkat Lunak Sonny BIM?",
                "จำนวนวันที่เหลือของใบอนุญาตซอฟต์แวร์ Sonny BIM làเท่าไร?"
                );

            // UserManualCaption = BindingUtils.ChangeLanguage(languageCode,
            //     "HƯỚNG DẪN",
            //     "USER MANUAL",
            //     "ユーザーマニュアル",
            //     "MANUAL DE USUARIO",
            //     "PANDUAN PENGGUNA",
            //
            //     "คู่มือการใช้"
            //
            //     );

            UserManualToolTip = BindingUtils.ChangeLanguage(languageCode,
                "Hướng dẫn chi tiết cách sử dụng",
                "See the detailed instructions",
                "詳細な手順を参照してください",
                "Ver las instrucciones detalladas",
                "Lihat instruksi rinci",
                "ดูคำแนะนำโดยละเอียด"
                );


            UnitCaption = BindingUtils.ChangeLanguage(languageCode,
                "Chọn đơn vị",
                "Choose Unit",
                "ユニット",
                "Elija unidad",
                "Pilih Satuan",
                "เลือกหน่วย"
            );

            OnPropertyChangedAll(this);
        }

        #endregion

        [JsonIgnore]
        [Obfuscation]
        public Window MainWindow { get; set; } = null;

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public UIDocument UiDoc;

        /// <summary>
        /// from ViewModelBaseNew
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public Document Doc;

        /// <summary>
        /// NameOfAddin - NameOfAddinToCheckLicenseLeft - Tdbc15b82847ccca4288f0339bf451469 :
        /// Biến này dùng để kiểm tra thời gian hết hạn của tool có tên là NameOfAddin
        /// </summary>
        [JsonIgnore]
        [Obfuscation]
        public string Tdbc15b82847ccca4288f0339bf451469 { get; set; } = string.Empty;

        [JsonIgnore]
        [Obfuscation]
        public bool IsCancel = false;

        [JsonIgnore]
        [Obfuscation]
        public double Percent
        {
            get => _percent;
            set { _percent = value; OnPropertyChanged(); }
        }

        private double _percent;

        // Cái này quan trọng, tránh gây treo máy khi save - load setting
        [JsonIgnore]
        [Obfuscation]
        public List<LanguageData> AllLanguage { get; set; } = LanguageData.AllLanguage;

        #region IDataErrorInfo

        string IDataErrorInfo.Error
        {
            get
            {
                throw new NotSupportedException("IDataErrorInfo.Error is not supported, use IDataErrorInfo.this[propertyName] instead.");
            }
        }
        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    throw new ArgumentException("Invalid property name", propertyName);
                }
                string error = string.Empty;
                var value = GetValue(propertyName);
                var results = new List<ValidationResult>(1);
                var result = Validator.TryValidateProperty(
                    value,
                    new ValidationContext(this, null, null)
                    {
                        MemberName = propertyName
                    },
                    results);
                if (!result)
                {
                    var validationResult = results.First();
                    error = validationResult.ErrorMessage;
                }
                return error;
            }
        }
        private object GetValue(string propertyName)
        {
            PropertyInfo propInfo = GetType().GetProperty(propertyName);
            return propInfo.GetValue(this);
        }

        #endregion


        [JsonIgnore]
        [Obfuscation]
        public ICommand OpenWebSite
        {
            get
            {
                return new RelayCommand<object>(o =>
                    {
                        BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://Sonnybimvn.com/en/");
                    });
            }
        }


        [JsonIgnore]
        [Obfuscation]
        public ICommand CustomDevelopment
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("http://bit.ly/3bNeJek");
                });
            }
        }

        [JsonIgnore]
        [Obfuscation]
        public ICommand MinimizeWindow
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    try
                    {

                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow != null)
                        {
                            // Thu nhỏ cửa sổ
                            mainWindow.WindowState = WindowState.Minimized;
                        }
                        else
                        {
                            // Nếu MainWindow là null, chúng ta ép đóng cửa sổ hiện tại
                            var currentWindow = System.Windows.Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                            if (System.Windows.Application.Current.Windows.Cast<Window>().Count() == 1 && currentWindow != null)
                            {
                                currentWindow.Close();  // Đóng cửa sổ hiện tại
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Please manually close WPF Window. (first time only)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi và thông báo cho người dùng
                        System.Windows.MessageBox.Show($"Error when minimizing the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }


        [JsonIgnore]
        [Obfuscation]
        public ICommand MaximizeWindow
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    try
                    {
                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow != null)
                        {
                            // Kiểm tra trạng thái cửa sổ và thay đổi
                            if (mainWindow.WindowState == WindowState.Normal)
                            {
                                mainWindow.WindowState = WindowState.Maximized;
                            }
                            else
                            {
                                mainWindow.WindowState = WindowState.Normal;
                            }
                        }
                        else
                        {
                            // Nếu MainWindow là null, chúng ta ép đóng cửa sổ hiện tại
                            var currentWindow = System.Windows.Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                            if (System.Windows.Application.Current.Windows.Cast<Window>().Count() == 1 && currentWindow != null)
                            {
                                currentWindow.Close();  // Đóng cửa sổ hiện tại
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Please manually close WPF Window. (first time only)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Xử lý lỗi và thông báo cho người dùng
                        System.Windows.MessageBox.Show($"Error when maximizing the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        [JsonIgnore]
        [Obfuscation]
        public ICommand CloseWindow
        {
            get
            {
                return new RelayCommand<object>(o =>
                {
                    try
                    {
                        var mainWindow = System.Windows.Application.Current.MainWindow;
                        if (mainWindow != null)
                        {
                            mainWindow.Close();
                        }
                        else
                        {
                            // Nếu MainWindow là null, chúng ta ép đóng cửa sổ hiện tại
                            var currentWindow = System.Windows.Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.IsActive);
                            if (System.Windows.Application.Current.Windows.Cast<Window>().Count()==1 && currentWindow != null)
                            {
                                currentWindow.Close();  // Đóng cửa sổ hiện tại
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Please manually close WPF Window. (first time only)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Application.Current.Windows[0].Close();

                        // Xử lý lỗi và thông báo cho người dùng
                        System.Windows.MessageBox.Show($"Error when closing the window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                });
            }
        }
}
}
