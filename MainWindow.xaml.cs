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
using System.Diagnostics;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace DemoExam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<ItemPanel> ItemPanelCollection { get; set; }
        public string ConnectionString = "Data Source=DBSRV\\SQL2021;Initial Catalog=Dyachenko_Demoexam;Integrated Security=True";
        public int Fetch = 4;
        public int Page = 1;
        public int Pages = 0;
        public string Search = string.Empty;
        public string Sort = "MaterialName";
        public string Filter = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            Pages = GetPages(Filter, Search);

            this.DataContext = this;
            ItemPanelCollection = new ObservableCollection<ItemPanel>();
            RenderPage(Sort, Filter, Search);

            GetMaterialTypes();

            CmbSort.Items.Add("MaterialName");
            CmbSort.Items.Add("CurrentAmount");
            CmbSort.SelectedIndex = 0;

            CreateButtons(Pages);
        }

        public void GetMaterialTypes()
        {
            using var sqlConn = new SqlConnection() { ConnectionString = ConnectionString };
            var sql = "select Materials.[Тип_материала] from Materials group by Materials.[Тип_материала];";
            sqlConn.Open();
            var sqlCommand = new SqlCommand(sql, sqlConn);
            var rd = sqlCommand.ExecuteReader();
            CmbFilter.Items.Add("");
            while (rd.Read())
            {
                CmbFilter.Items.Add(rd.GetString(0));
            }
        }
        public void CreateButtons(int pages)
        {
            this.GridPages.Children.Clear();
            var next = new Button
            {
                Content = ">",
                Height = 24,
                Width = 24,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(360, 0, 0, 0)
            };
            next.Click += new RoutedEventHandler(BtnNextPage_Click);
            this.GridPages.Children.Add(next);

            for (var i = pages; i > 0; i--)
            {
                var btn = new Button()
                {
                    Content = i.ToString(),
                    Tag = i
                };
                btn.Click += delegate (object sender, RoutedEventArgs args)
                {
                    var x = (Button)sender;
                    Page = (int)x.Tag;
                    RenderPage(Sort, Filter, Search);
                };
                btn.Height = 24;
                btn.Width = 24;
                btn.HorizontalAlignment = HorizontalAlignment.Left;
                btn.VerticalAlignment = VerticalAlignment.Center;
                btn.Margin = new Thickness(360 - (pages - i + 1) * 24, 0, 0, 0);
                this.GridPages.Children.Add(btn);
            }

            var prev = new Button
            {
                Content = "<",
                Height = 24,
                Width = 24,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(360 - (pages + 1) * 24, 0, 0, 0)
            };
            prev.Click += new RoutedEventHandler(BtnPrevPage_Click);
            this.GridPages.Children.Add(prev);
        }

        public void RenderPage(string sort, string filter, string search)
        {
            using var sqlConn = new SqlConnection { ConnectionString = ConnectionString };
            sqlConn.Open();
            string fil = "";
            if (filter != String.Empty)
            {
                fil = "and Materials.[Тип_материала] = '" + filter + "' ";
            }

            string src = "";
            if (search != String.Empty)
            {
                src = $"and Materials.[Наименование_материала] LIKE '%{search}%' ";
            }

            var sql =
                "select Materials.[Наименование_материала] as MaterialName, Materials.[Тип_материала] as MaterialsType, " +
                "Materials.[Минимальное_количество] as MinimalAmount, CAST(Materials.[Количество_на_складе] as int) as CurrentAmount, " +
                "STRING_AGG(ISNULL(MaterialsSuppliers.[Возможный поставщик], ''), ', ') as Suppliers, " +
                "Materials.[Изображение] as ImageSource " +
                "from Materials, MaterialsSuppliers " +
                $"where Materials.[Наименование_материала] = MaterialsSuppliers.[Наименование материала] {fil}{src}" +
                "group by Materials.[Наименование_материала], Materials.[Тип_материала], " +
                "Materials.[Количество_на_складе], Materials.[Минимальное_количество], " +
                "Materials.[Изображение] " +
                $"order by {sort} " +
                $"offset {Fetch * (Page - 1)} rows " +
                $"fetch next {Fetch} rows only;";
            var sqlCommand = new SqlCommand(sql, sqlConn);
            var rd = sqlCommand.ExecuteReader();

            ItemPanelCollection = new ObservableCollection<ItemPanel>();
            Lbox.ItemsSource = ItemPanelCollection;
            while (rd.Read())
            {
                var record = (IDataRecord)rd;
                var path_prefix = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"../../../images"));
                ItemPanelCollection.Add(new ItemPanel()
                {
                    MaterialName = record.GetString(0),
                    MaterialType = record.GetString(1),
                    MinimalAmount = record.GetInt32(2).ToString(),
                    CurrentAmount = record.GetInt32(3).ToString(),
                    Sellers = record.GetString(4),
                    ImageSource = File.Exists(System.IO.Path.GetFullPath(System.IO.Path.Join(path_prefix, record.GetString(5))))
                        ? System.IO.Path.GetFullPath(System.IO.Path.Join(path_prefix, record.GetString(5)))
                        : System.IO.Path.GetFullPath(System.IO.Path.Combine(path_prefix, @"picture.png"))
                });
            }


            sqlConn.Close();
        }

        public int GetPages(string filter, string search)
        {
            using var sqlConn = new SqlConnection() { ConnectionString = ConnectionString };
            sqlConn.Open();
            var fil = string.Empty;
            var src = string.Empty;

            if (filter != string.Empty)
            {
                fil = "and Materials.[Тип_материала] = '" + filter + "' ";
            }

            if (search != string.Empty)
            {
                src = $"and Materials.[Наименование_материала] LIKE '%{search}%' ";
            }

            var sql =
                "select count (*) " +
                "from Materials, MaterialsSuppliers " +
                $"where Materials.[Наименование_материала] = MaterialsSuppliers.[Наименование материала] {fil}{src}" +
                "group by Materials.[Наименование_материала], Materials.[Тип_материала], " +
                "Materials.[Количество_на_складе], Materials.[Минимальное_количество], " +
                "Materials.[Изображение];";

            var sqlCommand = new SqlCommand(sql, sqlConn);
            var rd = sqlCommand.ExecuteReader();
            var count = 0;
            while (rd.Read())
            {
                count++;
            }
            return (int)Math.Ceiling(count / (double)Fetch);

        }
        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (Page < Pages)
            {
                Page += 1;
                RenderPage(Sort, Filter, Search);
            }
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (Page > 1)
            {
                Page -= 1;
                RenderPage(Sort, Filter, Search);
            }
        }

        private void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Page = 1;
            Search = TextSearch.Text;
            RenderPage(Sort, Filter, Search);
            Pages = GetPages(Filter, Search);
            CreateButtons(Pages);
        }

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Page = 1;
            Filter = (string)CmbFilter.SelectedItem;
            RenderPage(Sort, Filter, Search);
            Pages = GetPages(Filter, Search);
            CreateButtons(Pages);
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Page = 1;
            Sort = (string)CmbSort.SelectedItem;
            RenderPage(Sort, Filter, Search);
        }
    }
}
