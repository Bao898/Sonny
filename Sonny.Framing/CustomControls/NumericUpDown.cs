// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Text.RegularExpressions;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;
using Control = System.Windows.Controls.Control;
using DataObject = System.Windows.DataObject;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TextBox = System.Windows.Controls.TextBox;

namespace SonnyBIM.WPF.Controls
{
    /// <summary>
    /// Control NumericUpDown cho phép nhập và điều chỉnh giá trị số
    /// </summary>
    public class NumericUpDown : Control
    {
        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(NumericUpDown),
                new PropertyMetadata(double.MinValue, OnMinimumChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(NumericUpDown),
                new PropertyMetadata(double.MaxValue, OnMaximumChanged));

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register(
                nameof(Increment),
                typeof(double),
                typeof(NumericUpDown),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register(
                nameof(DecimalPlaces),
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0, OnDecimalPlacesChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
                nameof(IsReadOnly),
                typeof(bool),
                typeof(NumericUpDown),
                new PropertyMetadata(false));

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(NumericUpDown),
                new PropertyMetadata(TextAlignment.Left));

        #endregion

        #region Properties

        /// <summary>
        /// Giá trị hiện tại của control
        /// </summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Giá trị nhỏ nhất cho phép
        /// </summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// Giá trị lớn nhất cho phép
        /// </summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Giá trị tăng/giảm mỗi lần nhấn nút
        /// </summary>
        public double Increment
        {
            get => (double)GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        /// <summary>
        /// Số chữ số thập phân hiển thị
        /// </summary>
        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        /// Xác định nếu control chỉ đọc
        /// </summary>
        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        /// <summary>
        /// Căn chỉnh văn bản trong TextBox
        /// </summary>
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        #endregion

        #region Events

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ValueChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<double>),
                typeof(NumericUpDown));

        /// <summary>
        /// Sự kiện khi giá trị thay đổi
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        #endregion

        #region Fields

        private TextBox _textBox;
        private ButtonBase _increaseButton;
        private ButtonBase _decreaseButton;
        private string _numberFormat;

        #endregion

        #region Constructor

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(typeof(NumericUpDown)));
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Lấy các phần tử từ template
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _increaseButton = GetTemplateChild("PART_IncreaseButton") as ButtonBase;
            _decreaseButton = GetTemplateChild("PART_DecreaseButton") as ButtonBase;

            if (_textBox != null)
            {
                _textBox.PreviewTextInput += TextBox_PreviewTextInput;
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                _textBox.LostFocus += TextBox_LostFocus;
                DataObject.AddPastingHandler(_textBox, TextBox_Pasting);

                UpdateTextBoxText();
            }

            if (_increaseButton != null)
            {
                _increaseButton.Click += IncreaseButton_Click;
            }

            if (_decreaseButton != null)
            {
                _decreaseButton.Click += DecreaseButton_Click;
            }

            UpdateNumberFormat();
        }

        #endregion

        #region Event Handlers

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Chỉ cho phép nhập số và dấu thập phân
            string text = _textBox.Text.Insert(_textBox.CaretIndex, e.Text);

            // Cho phép nhập dấu âm ở đầu
            if (e.Text == "-" && _textBox.CaretIndex == 0 && !_textBox.Text.Contains("-"))
            {
                e.Handled = false;
                return;
            }

            // Cho phép nhập dấu thập phân nếu chưa có
            if (e.Text == "." && !_textBox.Text.Contains(".") && DecimalPlaces > 0)
            {
                e.Handled = false;
                return;
            }

            // Kiểm tra xem text có phải là số hợp lệ không
            e.Handled = !IsTextNumeric(text);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                IncreaseValue();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                DecreaseValue();
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_textBox != null)
            {
                double parsedValue;
                if (double.TryParse(_textBox.Text, out parsedValue))
                {
                    Value = parsedValue;
                }
                else
                {
                    UpdateTextBoxText();
                }
            }
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = e.DataObject.GetData(typeof(string)) as string;
                if (!IsTextNumeric(_textBox.Text.Substring(0, _textBox.SelectionStart) + text + _textBox.Text.Substring(_textBox.SelectionStart + _textBox.SelectionLength)))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseValue();
        }

        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseValue();
        }

        #endregion

        #region Private Methods

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;
            control.UpdateTextBoxText();

            // Kích hoạt sự kiện ValueChanged
            control.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            NumericUpDown control = (NumericUpDown)d;
            double value = (double)baseValue;

            if (value < control.Minimum)
                return control.Minimum;
            if (value > control.Maximum)
                return control.Maximum;

            return baseValue;
        }

        private static void OnMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;
            double newMinimum = (double)e.NewValue;

            // Đảm bảo Minimum không lớn hơn Maximum
            if (newMinimum > control.Maximum)
            {
                control.Maximum = newMinimum;
            }

            // Cập nhật giá trị hiện tại nếu cần
            if (control.Value < newMinimum)
            {
                control.Value = newMinimum;
            }
        }

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;
            double newMaximum = (double)e.NewValue;

            // Đảm bảo Maximum không nhỏ hơn Minimum
            if (newMaximum < control.Minimum)
            {
                control.Minimum = newMaximum;
            }

            // Cập nhật giá trị hiện tại nếu cần
            if (control.Value > newMaximum)
            {
                control.Value = newMaximum;
            }
        }

        private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericUpDown control = (NumericUpDown)d;
            control.UpdateNumberFormat();
            control.UpdateTextBoxText();
        }

        private void OnValueChanged(double oldValue, double newValue)
        {
            RoutedPropertyChangedEventArgs<double> args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);
            RaiseEvent(args);
        }

        private void IncreaseValue()
        {
            if (!IsReadOnly)
            {
                Value = Math.Min(Value + Increment, Maximum);
            }
        }

        private void DecreaseValue()
        {
            if (!IsReadOnly)
            {
                Value = Math.Max(Value - Increment, Minimum);
            }
        }

        private void UpdateNumberFormat()
        {
            if (DecimalPlaces > 0)
            {
                _numberFormat = "F" + DecimalPlaces;
            }
            else
            {
                _numberFormat = "F0";
            }
        }

        private void UpdateTextBoxText()
        {
            if (_textBox != null)
            {
                _textBox.Text = Value.ToString(_numberFormat);
            }
        }

        private bool IsTextNumeric(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            // Cho phép số âm
            if (text == "-")
                return true;

            // Kiểm tra định dạng số
            string pattern = @"^-?\d*\.?\d*$";
            if (DecimalPlaces == 0)
            {
                pattern = @"^-?\d*$";
            }

            return Regex.IsMatch(text, pattern);
        }

        #endregion
    }
}

