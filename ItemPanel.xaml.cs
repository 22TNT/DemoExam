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

namespace DemoExam
{
    /// <summary>
    /// Логика взаимодействия для ItemPanel.xaml
    /// </summary>
    public partial class ItemPanel : UserControl
    {
        public ItemPanel()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string MaterialType { get; set; }
        public string MaterialName { get; set; }
        public string MinimalAmount { get; set; }
        public string CurrentAmount { get; set; }
        public string Sellers { get; set; }
        public string ImageSource { get; set; }
    }
}
