using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfCalculator.Properties;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;



namespace WpfCalculator
{
    public partial class MainWindow : Window
    {
        
        private Calculator _calculator = new Calculator();
        private bool isStandardMode = true;
        private string _manualClipboard = "";
        private bool _digitGroupingEnabled = false;
       


        public MainWindow()
        {
          
           
            CultureInfo customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;
            Thread.CurrentThread.CurrentUICulture = customCulture;

            InitializeComponent();
            UpdateDisplay();
            
            this.KeyDown += MainWindow_KeyDown;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            string number = (sender as Button).Content.ToString();

            if (SettingsManager.IsProgrammerMode)
            {
                int baseValue = SettingsManager.NumericalBase;

                if ((baseValue == 2 && !"01".Contains(number)) ||
                    (baseValue == 8 && !"01234567".Contains(number)) ||
                    (baseValue == 16 && !"0123456789ABCDEF".Contains(number.ToUpper())))
                {
                    return;
                }

                _calculator.EnterNumber(number);
            }
            else
            {
                if (number == "." || number == ",")
                {
                    if (_calculator.GetCurrentInput().Contains(".")) return; 
                }

                _calculator.EnterNumber(number);
            }

            UpdateDisplay();
        }


        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            string op = (sender as Button).Content.ToString();

            if (op == "×") op = "*";
            if (op == "÷") op = "/";

            if (SettingsManager.IsProgrammerMode)
            {
                
                if (!"+-*/&|^".Contains(op)) return;
            }

            _calculator.SetOperator(op);
            UpdateDisplay();
        }

        private void MemoryListBox_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MemoryListBox.SelectedItem != null)
            {
                string selectedMemory = MemoryListBox.SelectedItem.ToString();

                if (double.TryParse(selectedMemory, out double memoryValue))
                {
                    _calculator.EnterNumber(selectedMemory);
                    UpdateDisplay();
                }
                else
                {
                    MessageBox.Show("Invalid memory value!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
       
        private void SetBase_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                switch (clickedButton.Content.ToString())
                {
                    case "BIN":
                        SettingsManager.NumericalBase = 2;
                        break;
                    case "OCT":
                        SettingsManager.NumericalBase = 8;
                        break;
                    case "DEC":
                        SettingsManager.NumericalBase = 10;
                        break;
                    case "HEX":
                        SettingsManager.NumericalBase = 16;
                        break;
                }

                UpdateDisplay(); 
            }
        }



        private void SaveSettings()
        {
            try
            {
                string settingsFile = "settings.txt";
                string data = $"DigitGrouping={SettingsManager.DigitGrouping}";
                File.WriteAllText(settingsFile, data); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingsManager.InstantCalculationMode || _calculator.HasExpression())
            {
                _calculator.Calculate();
                _calculator.SetNewCalculation();
            }

            UpdateDisplay();
        }


        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _calculator.Clear();
            UpdateDisplay();
        }

        private void ClearEntry_Click(object sender, RoutedEventArgs e)
        {
            _calculator.ClearEntry();

            if (SettingsManager.IsProgrammerMode)
            {  
                DisplayProgrammer.Text = "0";
            }
            else
            {
                UpdateDisplay();
            }
        }



        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            _calculator.Backspace();

            if (string.IsNullOrEmpty(_calculator.GetCurrentInput()) || _calculator.GetCurrentInput() == "0")
            {
                _calculator.ClearEntry();  
            }

            UpdateDisplay();
        }



        private void ToggleSign_Click(object sender, RoutedEventArgs e)
        {
            _calculator.ToggleSign();
            UpdateDisplay();
        }

        private void SquareRoot_Click(object sender, RoutedEventArgs e)
        {
            _calculator.SquareRoot();
            UpdateDisplay();
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {
            _calculator.Square();
            UpdateDisplay();
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            _calculator.Invert();
            UpdateDisplay();
        }

        private void Decimal_Click(object sender, RoutedEventArgs e)
        {
            _calculator.EnterNumber(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
            UpdateDisplay();
        }


        private void MemoryStore_Click(object sender, RoutedEventArgs e)
        {
            _calculator.MemoryStore();
            UpdateMemoryList(); 
        }

        private void MemoryRecall_Click(object sender, RoutedEventArgs e)
        {
            double recalledValue = _calculator.MemoryRecall();
            _calculator.SetValue((int)recalledValue); 
            UpdateDisplay();
        }

        private void MemoryAdd_Click(object sender, RoutedEventArgs e)
        {
            _calculator.MemoryAdd();
            UpdateMemoryList();
        }

        private void MemorySubtract_Click(object sender, RoutedEventArgs e)
        {
            _calculator.MemorySubtract();
            UpdateMemoryList();
        }

        private void MemoryClear_Click(object sender, RoutedEventArgs e)
        {
            _calculator.MemoryClear();
            UpdateMemoryList();
        }

        private void MemoryList_Click(object sender, RoutedEventArgs e)
        {
            MemoryListBox.Visibility = MemoryListBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            UpdateMemoryList(); 
        }

        private void UpdateMemoryList()
        {
            MemoryListBox.Items.Clear();
            foreach (var value in _calculator.MemoryList())
            {
                MemoryListBox.Items.Add(value.ToString(CultureInfo.InvariantCulture));
            }
        }


        private void SwitchMode_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.IsProgrammerMode = !SettingsManager.IsProgrammerMode;

            if (SettingsManager.IsProgrammerMode)
            {
                StandardModeGrid.Visibility = Visibility.Collapsed;
                ProgrammerModeGrid.Visibility = Visibility.Visible;
                ModeSwitch.Content = "Programmer";

                
                bool enableHex = (SettingsManager.NumericalBase == 16);
                foreach (Button btn in new List<Button> { BtnA, BtnB, BtnC, BtnD, BtnE, BtnF })
                {
                    btn.IsEnabled = enableHex;
                }

                Display.Text = ""; 
            }
            else
            {
                StandardModeGrid.Visibility = Visibility.Visible;
                ProgrammerModeGrid.Visibility = Visibility.Collapsed;
                ModeSwitch.Content = "Standard";

                DisplayProgrammer.Text = ""; 
            }

            UpdateDisplay();
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Made by Scarlat Alexandra\n", "About");
        }


        private void ToggleCalculationMode_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ToggleCalculationMode();

            MessageBox.Show($"Calculation mode set to: {(SettingsManager.InstantCalculationMode ? "ON (Instant)" : "OFF (Manual)")}",
                            "Settings", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateDisplay();  
        }

        private void ToggleDigitGrouping_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.ToggleDigitGrouping(); 

            MessageBox.Show($"Digit Grouping is now {(SettingsManager.DigitGrouping ? "ON" : "OFF")}",
                            "Settings", MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateDisplay();  
        }




        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            _manualClipboard = _calculator.GetExpression(); 
            _calculator.Clear(); 
            UpdateDisplay();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            _manualClipboard = _calculator.GetExpression(); 
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_manualClipboard))
            {
                _calculator.EnterNumber(_manualClipboard);
                UpdateDisplay();
            }
        }




        private void UpdateDisplay()
        {
            if (SettingsManager.IsProgrammerMode)
            {
                int value = (int)_calculator.CurrentValue;
                DisplayProgrammer.Text = _calculator.ConvertToSelectedBase(value);
                Display.Text = "";
            }
            else
            {
                string displayText = _calculator.HasExpression() ? _calculator.GetExpression() : _calculator.CurrentValue.ToString(CultureInfo.InvariantCulture);

                if (SettingsManager.DigitGrouping)
                {
                    if (double.TryParse(displayText, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                    {
                        if (num % 1 == 0) 
                        {
                            displayText = ((int)num).ToString("N0", CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            displayText = num.ToString("N", CultureInfo.CurrentCulture); 
                        }
                    }
                }

                Display.Text = displayText;
                DisplayProgrammer.Text = "";
            }
        }



        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine($"Key Pressed: {e.Key}");

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.C when Keyboard.Modifiers == ModifierKeys.Control:
                        _manualClipboard = _calculator.GetExpression();
                        e.Handled = true;
                        return;
                    case Key.X when Keyboard.Modifiers == ModifierKeys.Control:
                        _manualClipboard = _calculator.GetExpression();
                        _calculator.Clear();
                        UpdateDisplay();
                        e.Handled = true;
                        return;
                    case Key.V when Keyboard.Modifiers == ModifierKeys.Control:
                        if (!string.IsNullOrEmpty(_manualClipboard))
                        {
                            _calculator.EnterNumber(_manualClipboard);
                            UpdateDisplay();
                        }
                        e.Handled = true;
                        return;
                }
            }

          
            switch (e.Key)
            {
                case Key.D0: case Key.NumPad0: _calculator.EnterNumber("0"); break;
                case Key.D1: case Key.NumPad1: _calculator.EnterNumber("1"); break;
                case Key.D2: case Key.NumPad2: _calculator.EnterNumber("2"); break;
                case Key.D3: case Key.NumPad3: _calculator.EnterNumber("3"); break;
                case Key.D4: case Key.NumPad4: _calculator.EnterNumber("4"); break;
                case Key.D5: case Key.NumPad5: _calculator.EnterNumber("5"); break;
                case Key.D6: case Key.NumPad6: _calculator.EnterNumber("6"); break;
                case Key.D7: case Key.NumPad7: _calculator.EnterNumber("7"); break;
                case Key.D8:
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        _calculator.SetOperator("*"); 
                    else
                        _calculator.EnterNumber("8");   
                    break;
                case Key.D9: case Key.NumPad9: _calculator.EnterNumber("9"); break;

                
                case Key.Add:
                case Key.OemPlus when Keyboard.Modifiers == ModifierKeys.Shift:
                    _calculator.SetOperator("+");
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    _calculator.SetOperator("-");
                    break;
                case Key.Multiply:
                    _calculator.SetOperator("*");
                    break;

                case Key.Divide:      
                case Key.OemQuestion:
                    _calculator.SetOperator("/");
                    break;

             
                case Key.Enter:
                case Key.OemPlus:
                    Equals_Click(null, null);
                    break;

                case Key.Back:
                    Backspace_Click(null, null);
                    break;

                case Key.Escape:
                    Clear_Click(null, null);
                    break;

                case Key.OemPeriod:
                case Key.OemComma:
                    Decimal_Click(null, null);
                    break;

                case Key.Oem3:
                    ToggleSign_Click(null, null);
                    break;
            }

            e.Handled = true; 
            UpdateDisplay();
        }


        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings(); 
            base.OnClosing(e);
        }



        private void Window_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (char.IsDigit(e.Text[0]))
            {
                _calculator.EnterNumber(e.Text);
                e.Handled = true;
            }
        }
    }
}
