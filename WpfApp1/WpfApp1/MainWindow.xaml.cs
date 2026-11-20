using System.Windows; //Подключает библиотеку WPF, чтобы класс знал, что такое Window

namespace TodoListApp
{
    public partial class MainWindow : Window
    {
        public MainWindow() //Конструктор окна: Вызывается в момент создания (запуска) окна.
        {
            InitializeComponent(); //метод, который «собирает» и отрисовывает все элементы интерфейса, описанные в файле
        }
    }
}