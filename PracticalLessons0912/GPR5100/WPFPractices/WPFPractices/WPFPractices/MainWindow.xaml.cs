﻿using System;
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
using System.Configuration;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;



namespace WPFPractices
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string COMPANY_NAME = "SAETestCompany";
        const string APPLICATION_NAME = "SAETestApp";

        private static string configCompanyPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), COMPANY_NAME);
        private static string configApplicationPath = System.IO.Path.Combine(configCompanyPath, APPLICATION_NAME); 
        private static string configPath =  System.IO.Path.Combine(configApplicationPath, "config.json");

        private Configuration config;

        public ObservableCollection<TodoItem> Todos { get; set; }

        public MainWindow()
        {
            LoadConfiguration();

            Todos = new ObservableCollection<TodoItem>();
            Todos.Add(new TodoItem() { Title = "Example 1", Completion = 0.1f });
            Todos.Add(new TodoItem() { Title = "Learn C#", Completion = 0.8f });
            Todos.Add(new TodoItem() { Title = "Learn Unity", Completion = 1 });

            InitializeComponent();
        }

        private void LoadConfiguration()
        {
            var loadedConfig = ConfigurationLoader.Load(configPath);

            //loading from file
            if (loadedConfig != null)
            {
                config = loadedConfig;
            }
            //first startup, config does not exist, creating
            else
            {
                config = new Configuration();
                Directory.CreateDirectory(configCompanyPath);
                Directory.CreateDirectory(configApplicationPath);
            }

            ApplyConfiguration();
        }

        private void ApplyConfiguration()
        {
            this.Height = config.Height;
            this.Width = config.Width;
            this.Left = config.PositionX;
            this.Top = config.PositionY;
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e)
        {
            Todos.Add(new TodoItem() { Title = "Added", Completion = 0 });
        }

        private void OnRemoveButtonClicked(object sender, RoutedEventArgs e)
        {
            Todos.RemoveAt(0);
        }

        private void OnSaveAsClicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Save todos as...";
            saveFileDialog.Filter = "Binary File | *.data";
            saveFileDialog.DefaultExt = "data";

            bool? success = saveFileDialog.ShowDialog();

            if (success.HasValue && success.Value)
            {
                SaveTodosTo(saveFileDialog.FileName);
            }
        }

        private void SaveTodosTo(string path)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.OpenWrite(path);
            binaryFormatter.Serialize(fileStream, Todos);
        }

        private void OnLoadClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Load todos from...";
            openFileDialog.Filter = "Binary File | *.data";

            bool? result = openFileDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                LoadTodosFrom(openFileDialog.FileName);
            }
        }

        private void LoadTodosFrom(string path)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.OpenRead(path);

            Todos = (ObservableCollection<TodoItem>)binaryFormatter.Deserialize(fileStream);
            todoItemsControl.ItemsSource = Todos;
        }

        public void ParseCommandLineArguments(string[] args)
        {
            if (args.Length == 2 && args[0] == "--load")
            {
                if (File.Exists(args[1]))
                {
                    LoadTodosFrom(args[1]);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to quit?", "Quit Warning", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            if (config != null)
            {
                ConfigurationLoader.Save(config, configPath);
            }
        }

        private void ConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var window = new ConfigEditor(config);
            window.Show();
        }
    }

    [System.Serializable]
    public class TodoItem
    {
        public string Title { get; set; }
        public float Completion { get; set; }
    }
}

