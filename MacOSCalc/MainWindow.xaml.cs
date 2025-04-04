using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MacOSCalc
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);

            appWindow.Resize(new SizeInt32(300, 450));

            this.ExtendsContentIntoTitleBar = true;
            SetTitleBar(null);

            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);

            // Supprime les boutons de contr�le natifs (fermeture, minimiser, maximiser)
            OverlappedPresenter? presenter = appWindow.Presenter as OverlappedPresenter;
            if (presenter != null)
            {
                presenter.IsMinimizable = false;
                presenter.IsMaximizable = false;
                presenter.IsResizable = false;
            }

            HideSystemButtons(hWnd);
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const int WS_CAPTION = 0x00C00000;

       
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

       
        private void HideSystemButtons(IntPtr hWnd)
        {
            int style = GetWindowLong(hWnd, GWL_STYLE);
            style &= ~WS_SYSMENU; 
            style &= ~WS_CAPTION; 
            SetWindowLong(hWnd, GWL_STYLE, style);
        }

        private void CloseApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }

        private bool lastResult = false;

        public void Reset(object sender, RoutedEventArgs e)
        {
            Display.Text = "0";
        }

        public void AddDigit(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button != null && button.Content is string digit)
            {
                if (Display.Text == "0" && digit != ".")
                {
                    Display.Text = digit;
                }
                else
                {
                    Display.Text += digit;
                }
            }
        }

        private void addOperator(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (button != null && button.Content is string op)
            {
                // V�rifie si le dernier caract�re est un op�rateur
                if (!"+-��".Contains(Display.Text.Last()))
                {
                    Display.Text += op;
                }
            }
        }

        private void Invert(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Display.Text)) return;

            string expression = Display.Text;
            int lastOperatorIndex = expression.LastIndexOfAny(new char[] { '+', '-', '�', '�' });

            if (lastOperatorIndex == -1 || expression.StartsWith("("))
            {
                if (expression.StartsWith("(-"))
                    Display.Text = expression.Substring(2, expression.Length -3); 
                else
                    Display.Text = "(-" + expression + ")";
            }
            else
            {
                string beforeLastNumber = expression.Substring(0, lastOperatorIndex + 1);
                string lastNumber = expression.Substring(lastOperatorIndex + 1).Trim();

                Display.Text = lastNumber.StartsWith("(-") ? beforeLastNumber + lastNumber.Substring(2, lastNumber.Length - 3) : beforeLastNumber + "(-" + lastNumber + ")";
            }
        }

        private void Evaluate(object sender, RoutedEventArgs e)
        {
            try
            {
                
                var table = new DataTable();
                var expression = Display.Text.Replace("�", "*").Replace("�", "/").Replace(",",".");
                
                while (!char.IsDigit(expression.Last()) && expression.Last() != '.')
                {
                    expression = expression.Substring(0, expression.Length - 1);
                }
                var result = table.Compute(expression, string.Empty);
                double numericResult;
                if (double.TryParse(result.ToString(), out numericResult))
                {
                    Display.Text = numericResult % 1 == 0 ? ((int)numericResult).ToString() : numericResult.ToString();
                }
                else
                {
                    Display.Text = "erreur";
                }
                lastResult = true;
            }
            catch
            {
                Display.Text = "erreur";
            }
        }
    }
}
