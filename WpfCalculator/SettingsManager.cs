
using System;

namespace WpfCalculator
{

    public static class SettingsManager
    {

        #region Standard Mode
        public static bool DigitGrouping
        {
            get => Properties.Settings.Default.DigitGrouping;
            set
            {
                Properties.Settings.Default.DigitGrouping = value;
                Save();
            }
        }

        public static string CalculatorMode
        {
            get => Properties.Settings.Default.CalculatorMode;
            set
            {
                Properties.Settings.Default.CalculatorMode = value;
                Save();
            }
        }

        public static bool InstantCalculationMode
        {
            get => Properties.Settings.Default.InstantCalculationMode;
            set
            {
                Properties.Settings.Default.InstantCalculationMode = value;
                Save();
            }
        }

        public static void ToggleDigitGrouping()
        {
            DigitGrouping = !DigitGrouping;
            Save();
        }

        public static void ToggleCalculationMode()
        {
            InstantCalculationMode = !InstantCalculationMode;
            Save();
        }


        public static void Save()
        {
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Programmer Mode


        public static bool IsProgrammerMode
        {
            get => Properties.Settings.Default.IsProgrammerMode;
            set
            {
                Properties.Settings.Default.IsProgrammerMode = value;
                Save();
            }
        }


        public static int NumericalBase
        {
            get => Properties.Settings.Default.NumericalBase;
            set
            {
                Properties.Settings.Default.NumericalBase = value;
                Save();
            }
        }



        #endregion
    }
}
