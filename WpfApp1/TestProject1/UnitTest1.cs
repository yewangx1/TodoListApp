using Xunit;
using System.Threading.Tasks;
using TodoListApp.Models;
using TodoListApp.Services;
using System.Linq;

namespace TodoListApp.Tests
{
    public class TodoServiceTests
    {
        // [Fact] в xUnit — это пометка над методом: «это обычный тест, запусти его и проверь, что всё прошло без ошибок
        [Fact]
        public async Task AddTask_ShouldAddTaskToDatabase()
        {
            // Используем отдельный файл базы для тестов
            var service = new TodoService("Data Source=test_todo.db");
            var task = new TodoItem { Title = "Тестовая задача", IsCompleted = false, Category = "Тест" };

            await service.AddTaskAsync(task);
            var tasks = await service.GetTasksAsync();

            Assert.Contains(tasks, t => t.Title == "Тестовая задача");
        }

        [Fact]
        public async Task UpdateTask_ShouldChangeCompletionStatus()
        {
            var service = new TodoService("Data Source=test_todo.db");
            var task = new TodoItem { Title = "Задача для обновления", IsCompleted = false };
            await service.AddTaskAsync(task);

            var addedTask = (await service.GetTasksAsync()).First(t => t.Title == "Задача для обновления");

            addedTask.IsCompleted = true;
            await service.UpdateTaskAsync(addedTask);

            var updatedTask = (await service.GetTasksAsync()).First(t => t.Id == addedTask.Id);
            Assert.True(updatedTask.IsCompleted);
        }

        [Fact]
        public async Task DeleteTask_ShouldRemoveTask()
        {
            var service = new TodoService("Data Source=test_todo.db");
            var task = new TodoItem { Title = "Задача для удаления" };
            await service.AddTaskAsync(task);

            var addedTask = (await service.GetTasksAsync()).First(t => t.Title == "Задача для удаления");

            await service.DeleteTaskAsync(addedTask.Id);

            var tasks = await service.GetTasksAsync();
            Assert.DoesNotContain(tasks, t => t.Id == addedTask.Id);
        }
    }
}