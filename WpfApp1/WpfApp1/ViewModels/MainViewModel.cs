using CommunityToolkit.Mvvm.ComponentModel; //(для свойств и команд), базовые типы .NET (коллекции, уведомления об изменениях, асинхронность) и внутренние слои проекта: Models и Services
using CommunityToolkit.Mvvm.Input; // [RelayCommand] — генерация команд ICommand для кнопок/привязок
using System.Collections.ObjectModel; //коллекция, уведомляющая UI о добавлении/удалении
using System.ComponentModel; // PropertyChangedEventArgs — событие изменения свойства объекта
using System.Threading.Tasks; //task для асинх методов
using TodoListApp.Models; //ToDoItem
using TodoListApp.Services;

namespace TodoListApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly TodoService _todoService; //хранит ссылку на сервис работы с базой данных

        [ObservableProperty]//Атрибут автоматически создаёт публичные свойства (Tasks, NewTaskTitle, SelectedCategory), которые обновляют UI при изменении значения.
        private ObservableCollection<TodoItem> _tasks = new(); //автоматически оповещает ListBox о добавлении или удалении задач.
        // Текст из поля ввода новой задачи (двусторонняя привязка с TextBox)
        [ObservableProperty]
        private string _newTaskTitle = string.Empty;

        // Список доступных категорий для выпадающего списка
        public ObservableCollection<string> Categories { get; } = new() { "Работа", "Личное", "Покупки", "Учеба" };
        //по умолч
        [ObservableProperty]
        private string _selectedCategory = "Личное";

        public MainViewModel()
        {
            string connectionString = "Data Source=todo.db"; //подключаемся к  sqlite
            _todoService = new TodoService(connectionString);//Создаёт экземпляр TodoService
            LoadTasksCommand.Execute(null);//сразу запускаем чтобы при открытии окна загрузить сохранённые задачи
        }

        [RelayCommand] //Атрибут [RelayCommand] генерирует команду LoadTasksCommand для привязки в интерфейсе.
        private async Task LoadTasksAsync()
        {
            var tasksFromDb = await _todoService.GetTasksAsync(); //получаем задачи из БД, очищаем текущую коллекцию Tasks и заполняет её заново
            Tasks.Clear();
            foreach (var task in tasksFromDb)
            {
                task.PropertyChanged += Task_PropertyChanged; //На каждую задачу вешается обработчик чтобы отсл. изменения
                Tasks.Add(task);
            }
        }

        [RelayCommand] 
        private async Task AddTaskAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            var newTask = new TodoItem //собираем объект
            {
                Title = NewTaskTitle,
                IsCompleted = false,
                Category = SelectedCategory 
            };

            await _todoService.AddTaskAsync(newTask);

            newTask.PropertyChanged += Task_PropertyChanged; //Сохраняет задачу в БД, подписывается на её PropertyChanged
            Tasks.Insert(0, newTask); //вставляет в начало списка

            NewTaskTitle = string.Empty; //очищает поле ввода
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TodoItem task)
        {
            await _todoService.DeleteTaskAsync(task.Id);
            task.PropertyChanged -= Task_PropertyChanged; //убираем связь-оповещение между задачей и обработчиком
            Tasks.Remove(task);
        }

        private async void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TodoItem.IsCompleted) && sender is TodoItem task) //когда у конкретной задачи меняется свойство IsCompleted (например, при клике по чекбоксу в списке)
            {
                await _todoService.UpdateTaskAsync(task); //сохраняем новый статус
            }
        }
    }
}