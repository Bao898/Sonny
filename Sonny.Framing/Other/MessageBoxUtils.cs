// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

#region Namespaces

using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

//using System.Windows.Forms;
#endregion

namespace SonnyBIM
{
    public class MessageBoxUtils
    {
        public static void Cancel()
        {
            MessageBox.Show("Process is canceled!", SonnyBIMConstraint.MessageBoxCaption,
                              MessageBoxButton.OK, MessageBoxImage.Stop);
        }

        /// <summary>
        /// Hết hạn sử dụng phiên bản Pro của Add-in nameOfAddin
        /// </summary>
        public static void ExpireDate(bool isExpiredPro,
            bool isExpiredTrial = false,
            bool isFirstUsingForTrial = false)
        {
            string languageCode = LanguageData.GetLanguageSetting();

            if (isExpiredPro)
            {
                //MessageBox.Show("If you have purchased a Pro License or send to me an email to receive trial key, " +
                //                               "please check your email to receive key and active this Add-in.\n" +
                //                "If not, please rebuy new Pro License or send to me an email to receive trial key(only for the first time).",
                //"Check License", MessageBoxButton.OK, MessageBoxImage.Information);

                // MessageBoxResult result = MessageBox.Show("The license has expired or the license cannot be checked from your internet system!" + "\n" +
                //                                        "Please contact with me via email contact@Sonnybimvn.com or via Fanpage https://facebook.com/RevitAPI",
                //     "Check License", MessageBoxButton.OK, MessageBoxImage.Information);
                //
                //


                string text = BindingUtils.ChangeLanguage(languageCode,
                    "Cảm ơn bạn đã sử dụng Sonny BIM." + "\n" +
                    "Tài khoản của bạn đã hết hạn sử dụng " +
                    "hoặc do bạn chưa đăng nhập tài khoản." + "\n\n" +
                    "Hãy đăng nhập tài khoản Sonny BIM hoặc mua bản quyền mới để tiếp tục sử dụng Sonny BIM plugin!",

                    // Anh
                    "Thanks for using Sonny BIM." + "\n" +
                    "Your license was expired " +
                    "or because you are not logged into Sonny BIM account." + "\n\n" +
                    "Please login Sonny BIM account or purchase new Sonny BIM license to continue use Sonny BIM plugin!"

                    );


                if (languageCode.Equals(LanguageData.CodeVi))
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(text,
                        SonnyBIMConstraint.MessageBoxCaption,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);

                    if (result == DialogResult.Yes)
                    {
                        BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://Sonnybimvn.com/vi/buy/");
                    }
                }
                else
                {
                    DialogResult result = System.Windows.Forms.MessageBox.Show(text,
                        SonnyBIMConstraint.MessageBoxCaption,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        BrowserHelper.OpenUrlWithBrowserOrCopyToClipboard("https://Sonnybimvn.com/en/buy/");
                    }
                }

                return;
            }
            if (isExpiredTrial)
            {
                MessageBox.Show("Trial License was expired. Please buy Pro License!",
                    "Trial Expired", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        /// <summary>
        /// Hết hạn sử dụng phiên bản Trial của Add-in nameOfAddin
        /// </summary>
        public static void ExpireDateTrial(string nameOfAddin)
        {
            MessageBox.Show(string.Concat("Trial license for add-in \"", nameOfAddin,
                        "\" was expired. Rebuy this to continue using!"),
                        "Trial Expired", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public static void CheckMailToGetKey(string nameOfAddin)
        {
            MessageBox.Show(string.Concat("We will send an email with activation key for Add-in \"", nameOfAddin,
                        "\" to your email address in 2 hours!.\nGet this key to fill into active form to activate this add-in."),
                        "Check email to receive key", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}

