using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JSLexer {
    public partial class MainWindow : Window {

        private List<string> output;
        private bool prevCom = false;
        private string htmlHeader = "<head>" +
            "<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\">" +
            "</head><body>{CONTENT}</body>";

        public MainWindow() {
            InitializeComponent();
            this.output = new List<string>();
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e) {
            this.Browser.Visibility = System.Windows.Visibility.Collapsed;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JavaScript files (*.js)|*.js";
            if (openFileDialog.ShowDialog() == true){
                string code = File.ReadAllText(openFileDialog.FileName);
                this.OpenCode(code);
            }
        }

        private void FromBuffer_Click(object sender, RoutedEventArgs e) {
            this.Browser.Visibility = System.Windows.Visibility.Collapsed;
            this.OpenCode(Clipboard.GetText());
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e) {
            this.ProcessBar.Visibility = System.Windows.Visibility.Visible;
            this.OpenButton.IsEnabled = false;
            this.ValidateButton.IsEnabled = false;
            this.FromBuffer.IsEnabled = false;
            string text = this.CodeBlock.Text;
            Thread vThread = new Thread(() => this.Validate(text));
            vThread.Start();
        }

        private void CloseBrowserButton_Click(object sender, RoutedEventArgs e) {
            this.Browser.Visibility = System.Windows.Visibility.Collapsed;
        }    

        private void Validate(string text) {
            string[] split = { "\n" };
            string[] lines = text.Split(split, StringSplitOptions.None);
            this.output.Clear();
            int count = 0;
            /*Parallel.ForEach(lines, (s) => {
                System.Threading.Interlocked.Increment(ref count);
                FSM fsm = new FSM(s, count);
                this.output.Add(fsm.ToString());
            });*/
            foreach (var s in lines){
                System.Threading.Interlocked.Increment(ref count);
                FSM fsm = new FSM(s, count, this.prevCom);
                this.prevCom = fsm.comment;
                this.output.Add(fsm.ToString());
            };
            Dispatcher.Invoke(new Action(() => { this.ParceComplete(); this.CreateHTML(); }));
        }

        private async void CreateHTML() {
            string s = String.Empty;
            foreach(var t in this.output){
                s += t;
            }
            using (StreamWriter w = new StreamWriter("out.html", false)) {
                await w.WriteAsync(this.htmlHeader.Replace("{CONTENT}", s));
            }
            this.Browser.Visibility = System.Windows.Visibility.Visible;
            string st = String.Format("file:///{0}/out.html", AppDomain.CurrentDomain.BaseDirectory);
            this.Browser.Navigate(String.Format("file:///{0}out.html", AppDomain.CurrentDomain.BaseDirectory));
        }

        private void ParceComplete() {
            this.ProcessBar.Visibility = System.Windows.Visibility.Hidden;
            this.OpenButton.IsEnabled = true;
            this.ValidateButton.IsEnabled = true;
            this.FromBuffer.IsEnabled = true;
        }

        private void OpenCode(string code) {
            this.CodeBlock.Text = code;
            string[] opt = { "function" };
            int funcs = code.Split(opt, StringSplitOptions.None).Length - 1;
            string[] opt3 = { "\n" };
            int strings = code.Split(opt3, StringSplitOptions.None).Length - 1;
            FileContents.Text = String.Format("Contains {0} functions, {1} lines in total", funcs, strings);
            this.ValidateButton.IsEnabled = true;
        }
    }
}
