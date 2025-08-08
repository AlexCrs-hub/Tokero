using System.ComponentModel.DataAnnotations.Schema;
using TokeroApp.Services;

namespace TokeroApp
{
    public partial class App : Application
    {
        public static LocalDbService Database { get; private set; }
        public App()
        {
            InitializeComponent();
            Database = new LocalDbService();

            MainPage = new AppShell();
        }
    }
}
