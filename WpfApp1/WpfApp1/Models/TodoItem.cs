using CommunityToolkit.Mvvm.ComponentModel; //подключает инструменты для уведомления интерфейса об изменении данных
 
namespace TodoListApp.Models
{
    public partial class TodoItem : ObservableObject
    {
        public int Id { get; set; }

        [ObservableProperty]
        private string _title = string.Empty; //текст/название задачи

        [ObservableProperty]
        private bool _isCompleted; //флаг
 
        [ObservableProperty]
        private string _category = string.Empty; //категория

    }
}