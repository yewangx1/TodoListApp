using Dapper; //выполнения SQL-запросов с маппингом строк в объекты
using Microsoft.Data.Sqlite; // дают инструменты для подключения к SQLite
using System.Collections.Generic; // Для IEnumerable<T>
using System.Threading.Tasks; // Для асинхронных методов (Task)
using TodoListApp.Models;

namespace TodoListApp.Services
{// Класс-сервис инкапсулирует всю работу с базой данных (ViewModel не пишет SQL напрямую)
    public class TodoService
    { // Строка подключения к SQLite
        private readonly string _connectionString;
        // Конструктор принимает строку подключения и сразу проверяет/создаёт таблицу в БД
        public TodoService(string connectionString)
        {
            _connectionString = connectionString;
            InitializeDatabase();
        }
        // Создаёт таблицу TodoItems, если её ещё нет (идемпотентная инициализация при старте/тестах)
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString); //создаёт подключение к SQLite и гарантированно закрывает/освобождает его в конце области видимости метода (using).
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS TodoItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL, 
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    Category TEXT
                )");
        }

        public async Task<IEnumerable<TodoItem>> GetTasksAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<TodoItem>("SELECT * FROM TodoItems");
            //подключаемся к бд, забираем все табл. из TodoItems, dapper превращает каждую строку в объект TodoItem и возвращает список задач
        }

        public async Task AddTaskAsync(TodoItem task)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "INSERT INTO TodoItems (Title, IsCompleted, Category) VALUES (@Title, @IsCompleted, @Category)";
            await connection.ExecuteAsync(sql, task);
        }

        public async Task UpdateTaskAsync(TodoItem task)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "UPDATE TodoItems SET Title = @Title, IsCompleted = @IsCompleted, Category = @Category WHERE Id = @Id";
            await connection.ExecuteAsync(sql, task);
        }

        public async Task DeleteTaskAsync(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM TodoItems WHERE Id = @Id", new { Id = id });
        }
    }
}