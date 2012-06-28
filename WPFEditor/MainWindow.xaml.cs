using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MegaMan.LevelEditor;

namespace MegaMan.WPFEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            var projectEditor = ProjectEditor.CreateNew(@"C:\Users\mafcho\C--MegaMan-Engine\Demo Project");
            var stageDocument = new StageDocument(projectEditor);
            Resources.Add("stageDocument", stageDocument);
            
            InitializeComponent();
        }
    }
}
