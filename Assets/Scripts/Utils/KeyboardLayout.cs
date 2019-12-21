using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utils
{
    public static class KeyboardLayout
    {
        private static bool? _isAzerty;

        public static bool IsAzerty
        {
            get
            {
                if (_isAzerty == null)
                    DetectKeyboardLayout();
                return _isAzerty == true;
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR

        private const int KeyboardLayoutNameLength = 9;
        private static readonly string[] AzertyKeyboardLayouts = {
            "0001080c", // Belgian (Comma)
            "00000813", // Belgian (Period)
            "0000080c", // Belgian French
            "0000040c" // French
        };

        [DllImport("user32.dll")]
        private static extern long GetKeyboardLayoutName(StringBuilder stringBuilder); 
        public static void DetectKeyboardLayout()
        {
            var name = new StringBuilder(KeyboardLayoutNameLength);
            GetKeyboardLayoutName(name);
            _isAzerty = AzertyKeyboardLayouts.Contains(name.ToString().ToLower());
        }
    
#else
    
     public static void DetectKeyboardLayout()
    {
        _isAzerty = Application.systemLanguage == SystemLanguage.French;
    }

#endif
    }
}