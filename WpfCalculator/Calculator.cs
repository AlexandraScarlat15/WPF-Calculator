using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace WpfCalculator
{
    public class Calculator
    {
        private List<string> _expression = new List<string>(); 
        private string _currentInput = ""; 
        private List<double> _memoryStack = new List<double>(); 
        private double _currentValue = 0;
        private bool _newCalculation = false;

        private Memory _memoryManager = new Memory();

        public double CurrentValue
        {
            get { return _currentValue; }
        }

        #region Enter Number and Operator

        public void EnterNumber(string number)
        {
            if (_newCalculation)
            {
                if (!SettingsManager.IsProgrammerMode)
                {
                    _expression.Clear();
                }
                _currentInput = "";
                _newCalculation = false;
            }

            if (SettingsManager.IsProgrammerMode)
            {
                int baseValue = SettingsManager.NumericalBase;

                if ((baseValue == 2 && !"01".Contains(number)) ||
                    (baseValue == 8 && !"01234567".Contains(number)) ||
                    (baseValue == 10 && !char.IsDigit(number[0])) ||
                    (baseValue == 16 && !"0123456789ABCDEFabcdef".Contains(number)))
                {
                    return;
                }

                _currentInput += number;

                _currentValue = Convert.ToInt64(_currentInput, baseValue);
            }
            else
            {
                if (!char.IsDigit(number[0]) && number != "." && number != ",") return;

                if ((number == "." || number == ",") && _currentInput.Contains(".")) return;

                _currentInput += number;
                _currentValue = double.Parse(_currentInput, CultureInfo.InvariantCulture);
            }
        }





        public void SetValue(int value)
        {
            _currentValue = value;
            _currentInput = value.ToString();
            _expression.Clear(); 
        }

        public void SetOperator(string op)
        {
            if (_newCalculation)
            {
                _newCalculation = false;
            }

            if (!string.IsNullOrEmpty(_currentInput))
            {
                _expression.Add(_currentInput);
                _currentInput = "";
            }

            if (SettingsManager.InstantCalculationMode && _expression.Count >= 3)
            {
                Calculate();
            }

            if (_expression.Count > 0)
            {
                string last = _expression[_expression.Count - 1];

                if (SettingsManager.IsProgrammerMode)
                {
                    if ("&|^".Contains(last))
                    {
                        _expression[_expression.Count - 1] = op;
                    }
                    else
                    {
                        _expression.Add(op);
                    }
                }
                else
                {
                    if ("+-*/".Contains(last))
                    {
                        _expression[_expression.Count - 1] = op;
                    }
                    else
                    {
                        _expression.Add(op);
                    }
                }
            }
        }
        #endregion

        #region Calculs

        public void Calculate()
        {
            if (_currentInput != "")
            {
                _expression.Add(_currentInput);
                _currentInput = "";
            }

            if (_expression.Count == 0)
                return;

            try
            {
                string fullExpression = string.Join(" ", _expression);
                fullExpression = fullExpression.Replace(",", "");

                if (SettingsManager.IsProgrammerMode)
                {
                    int decimalResult = EvaluateProgrammerExpression(fullExpression);

                    _currentValue = decimalResult;
                    _expression.Clear();

                    _expression.Add(ConvertToSelectedBase(decimalResult));
                }
                else
                {
                    _currentValue = EvaluateExpression(fullExpression);
                    _expression.Clear();
                    _expression.Add(_currentValue.ToString(CultureInfo.InvariantCulture));
                }

                _currentInput = "";
                _newCalculation = false;
            }
            catch (Exception)
            {
                _currentValue = 0;
                _expression.Clear();
                _currentInput = "Error";
            }
        }


        private int EvaluateProgrammerExpression(string expression)
        {
            string[] tokens = expression.Split(' ');
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();

            foreach (string token in tokens)
            {
                if (int.TryParse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hexValue) && SettingsManager.NumericalBase == 16)
                {
                    values.Push(hexValue);
                }
                else if (int.TryParse(token, out int decValue) && SettingsManager.NumericalBase == 10)
                {
                    values.Push(decValue);
                }
                else if (int.TryParse(token, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out int binValue) && SettingsManager.NumericalBase == 2)
                {
                    values.Push(Convert.ToInt32(token, 2));
                }
                else if ("+-*/&|^".Contains(token))
                {
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                int b = values.Pop();
                int a = values.Pop();
                string op = operators.Pop();

                switch (op)
                {
                    case "+": values.Push(a + b); break;
                    case "-": values.Push(a - b); break;
                    case "*": values.Push(a * b); break;
                    case "/": values.Push(a / b); break;
                    case "&": values.Push(a & b); break;
                    case "|": values.Push(a | b); break;
                    case "^": values.Push(a ^ b); break;
                }
            }

            return values.Pop();
        }

        public string ConvertToSelectedBase(int value)
        {
            switch (SettingsManager.NumericalBase)
            {
                case 2: return Convert.ToString(value, 2);  
                case 8: return Convert.ToString(value, 8);  
                case 10: return value.ToString();           
                case 16: return value.ToString("X");        
                default: return value.ToString();
            }
        }

        private double EvaluateExpression(string expression)
        {
            try
            {
                expression = expression.Replace(" ", "").Replace("÷", "/");

                int baseValue = SettingsManager.NumericalBase;
                var table = new DataTable();

                if (SettingsManager.IsProgrammerMode)
                {
                    long decimalValue = Convert.ToInt64(expression, baseValue);
                    return decimalValue;
                }
                else
                {
                    table.Columns.Add("expression", typeof(string), expression);
                    DataRow row = table.NewRow();
                    table.Rows.Add(row);
                    object result = row["expression"];

                    return Convert.ToDouble(result);
                }
            }
            catch
            {
                return double.NaN;
            }
        }

        #endregion

        #region Buttons logic


        public void Clear()
        {
            _expression.Clear();
            _currentInput = ""; 
            _currentValue = 0;
        }



        public void ClearEntry()
        {
            _currentInput = "";
        }
        public void Backspace()
        {
            if (!string.IsNullOrEmpty(_currentInput) && _currentInput != "0")
            {
                _currentInput = _currentInput.Length > 1 ? _currentInput.Substring(0, _currentInput.Length - 1) : "";
            }
            else if (_expression.Count > 0)
            {
                _expression.RemoveAt(_expression.Count - 1); 
            }

            if (string.IsNullOrEmpty(_currentInput) && _expression.Count == 0)
            {
                _currentInput = "0"; 
            }
        }

        #endregion

        #region Setters and Getters

        public void SetNewCalculation()
        {
            _newCalculation = true;
        }



        public string GetCurrentInput()
        {
            return _currentInput == "" ? "0" : _currentInput;
        }

       
        public string GetExpression()
        {
            string expression = string.Join(" ", _expression) + (_currentInput != "" ? " " + _currentInput : "");

            if (SettingsManager.DigitGrouping)
            {
                expression = ApplyDigitGrouping(expression);
            }
            else
            {
                expression = expression.Replace(",", ""); 
            }

            return expression;
        }

        #endregion

        #region Diggit Groupping


        private string ApplyDigitGrouping(string expression)
        {
            string[] tokens = expression.Split(' ');

            for (int i = 0; i < tokens.Length; i++)
            {
                if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
                {
                    if (SettingsManager.DigitGrouping)
                    {
                        if (tokens[i].Contains(".")) 
                        {
                            string[] parts = tokens[i].Split('.');
                            string integerPart = double.Parse(parts[0], CultureInfo.InvariantCulture)
                                                .ToString("N0", CultureInfo.CurrentCulture);
                            tokens[i] = integerPart + "." + parts[1]; 
                        }
                        else
                        {
                            tokens[i] = num.ToString("N0", CultureInfo.CurrentCulture); 
                        }
                    }
                    else
                    {
                        tokens[i] = num.ToString(CultureInfo.InvariantCulture); 
                    }
                }
            }

            return string.Join(" ", tokens);
        }

        public bool HasExpression()
        {
            return _expression.Count > 0 || !string.IsNullOrEmpty(_currentInput);
        }

        #endregion

        #region Math Functions

        public void SquareRoot()
        {
            double value;

            if (!string.IsNullOrEmpty(_currentInput))
            {
                value = double.Parse(_currentInput, CultureInfo.InvariantCulture);
            }
            else
            {
                value = _currentValue; 
            }

            if (value < 0)
            {
                _currentInput = "Error"; 
            }
            else
            {
                _currentValue = Math.Sqrt(value);
                _currentInput = _currentValue.ToString(CultureInfo.InvariantCulture);
            }

            _expression.Clear();
        }



        public void Square()
        {
            if (!string.IsNullOrEmpty(_currentInput))
            {
                double value = double.Parse(_currentInput, CultureInfo.InvariantCulture);
                _expression.Clear(); 
                _expression.Add(Math.Pow(value, 2).ToString(CultureInfo.InvariantCulture));
                _currentInput = ""; 
            }
        }

        public void Invert()
        {
            double value;

            if (!string.IsNullOrEmpty(_currentInput))
            {
                value = double.Parse(_currentInput, CultureInfo.InvariantCulture);
            }
            else
            {
                value = _currentValue; 
            }

            if (value == 0)
            {
                _currentInput = "Error";
            }
            else
            {
                _currentValue = 1 / value;
                _currentInput = _currentValue.ToString(CultureInfo.InvariantCulture);
            }

            _expression.Clear();
        }



        public void ToggleSign()
        {
            if (!string.IsNullOrEmpty(_currentInput))
            {
                double value = double.Parse(_currentInput, CultureInfo.InvariantCulture);
                _currentInput = (-value).ToString(CultureInfo.InvariantCulture);
            }
            else if (_expression.Count > 0)
            {
                double value = double.Parse(_expression.Last(), CultureInfo.InvariantCulture);
                value = -value;
                _expression[_expression.Count - 1] = value.ToString(CultureInfo.InvariantCulture);
                _currentValue = value;
            }
            else if (_currentValue != 0)
            {
                _currentValue = -_currentValue;
                _currentInput = _currentValue.ToString(CultureInfo.InvariantCulture);
            }
        }



        #endregion

        #region Memory Functions

    
        public Calculator()
        {
            _memoryManager.MemoryUpdated += OnMemoryUpdated; 
        }

        private void OnMemoryUpdated()
        {
            MemoryUpdated?.Invoke();
        }

        public event Action MemoryUpdated;

        public void MemoryStore()
        {
            _memoryManager.Store(CurrentValue);
        }

        public double MemoryRecall()
        {
            return _memoryManager.Recall();
        }

        public void MemoryAdd()
        {
            _memoryManager.Add(CurrentValue);
        }

        public void MemorySubtract()
        {
            _memoryManager.Subtract(CurrentValue);
        }

        public void MemoryClear()
        {
            _memoryManager.Clear();
        }

        public List<double> MemoryList()
        {
            return _memoryManager.GetMemoryList();
        }
    }

    #endregion
}

