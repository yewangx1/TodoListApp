using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using TodoListApp.Models;
using TodoListApp.Services;
using Xunit;

namespace TodoListApp.Tests
{
    public class TodoServiceTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly TodoService _service;

        // Конструктор выполняется перед КАЖДЫМ тестом, создавая чистую базу данных
        public TodoServiceTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"test_todo_{Guid.NewGuid()}.db");
            _service = new TodoService($"Data Source={_dbPath}");
        }

        // [Fact] в xUnit — это пометка над методом: «это обычный тест, запусти его и проверь, что всё прошло без ошибок»
        [Fact]
        public async Task AddTask_ShouldAddTaskToDatabase()
        {
            // Arrange
            var task = new TodoItem { Title = "Тестовая задача", IsCompleted = false, Category = "Тест" };

            // Act (одно целевое действие)
            await _service.AddTaskAsync(task);

            // Assert (проверка в чистой базе)
            var tasks = await _service.GetTasksAsync();
            var addedTask = Assert.Single(tasks);
            Assert.Equal("Тестовая задача", addedTask.Title);
            Assert.False(addedTask.IsCompleted);
            Assert.Equal("Тест", addedTask.Category);
        }

        [Fact]
        public async Task UpdateTask_ShouldChangeCompletionStatus()
        {
            // Arrange
            var task = new TodoItem { Title = "Задача для обновления", IsCompleted = false };
            await _service.AddTaskAsync(task);
            var addedTask = (await _service.GetTasksAsync()).Single();
            addedTask.IsCompleted = true;

            // Act (одно целевое действие)
            await _service.UpdateTaskAsync(addedTask);

            // Assert
            var updatedTask = (await _service.GetTasksAsync()).Single();
            Assert.True(updatedTask.IsCompleted);
        }

        [Fact]
        public async Task DeleteTask_ShouldRemoveTask()
        {
            // Arrange
            var task = new TodoItem { Title = "Задача для удаления" };
            await _service.AddTaskAsync(task);
            var addedTask = (await _service.GetTasksAsync()).Single();

            // Act (одно целевое действие)
            await _service.DeleteTaskAsync(addedTask.Id);

            // Assert
            var tasks = await _service.GetTasksAsync();
            Assert.Empty(tasks);
        }

        // Выполняется после КАЖДОГО теста и удаляет временный файл БД
        public void Dispose()
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
    }
}
