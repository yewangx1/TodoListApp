using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoListApp.Models;
using TodoListApp.Services;

namespace TodoListApp.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly TodoService _todoService; // Хранит ссылку на сервис работы с БД

        [ObservableProperty] // Генерирует свойство Tasks и обновляет UI при изменении коллекции/свойства
        private ObservableCollection<TodoItem> _tasks = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AddTaskCommand))] // При изменении текста пересчитывает доступность кнопки '+'
        private string _newTaskTitle = string.Empty; // Объявляем один раз — без дублей

        public ObservableCollection<string> Categories { get; } = new() { "Работа", "Личное", "Покупки", "Учеба" };

        [ObservableProperty] // Генерирует свойство SelectedCategory
        private string _selectedCategory = "Личное";

        public MainViewModel()
        {
            string connectionString = "Data Source=todo.db";
            _todoService = new TodoService(connectionString);
            LoadTasksCommand.Execute(null); // Загружаем задачи из БД при создании ViewModel
        }

        [RelayCommand] // Генерирует LoadTasksCommand
        private async Task LoadTasksAsync()
        {
            var tasksFromDb = await _todoService.GetTasksAsync();
            Tasks.Clear();

            foreach (var task in tasksFromDb)
            {
                task.PropertyChanged += Task_PropertyChanged;
                Tasks.Add(task);
            }
        }

        private bool CanAddTask() => !string.IsNullOrWhiteSpace(NewTaskTitle); // Активно только если есть текст (не пусто и не пробелы)

        [RelayCommand(CanExecute = nameof(CanAddTask))] // Генерирует AddTaskCommand с проверкой CanAddTask
        private async Task AddTaskAsync()
        {
            var newTask = new TodoItem
            {
                Title = NewTaskTitle.Trim(),
                IsCompleted = false,
                Category = SelectedCategory
            };

            await _todoService.AddTaskAsync(newTask);

            newTask.PropertyChanged += Task_PropertyChanged;
            Tasks.Insert(0, newTask);

            NewTaskTitle = string.Empty; // Очищаем поле — кнопка '+' снова становится неактивной
        }

        [RelayCommand] // Генерирует DeleteTaskCommand
        private async Task DeleteTaskAsync(TodoItem task)
        {
            if (task is null) return;

            await _todoService.DeleteTaskAsync(task.Id);
            task.PropertyChanged -= Task_PropertyChanged;
            Tasks.Remove(task);
        }

        private async void Task_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TodoItem.IsCompleted) && sender is TodoItem task)
            {
                await _todoService.UpdateTaskAsync(task);
            }
        }
    }
}