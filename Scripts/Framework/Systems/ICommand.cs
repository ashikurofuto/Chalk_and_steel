using System;

namespace Architecture.GlobalModules.Systems
{
    /// <summary>
    /// Простой интерфейс команды по образцу паттерна Command
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Выполняет команду
        /// </summary>
        void Execute();

        /// <summary>
        /// Отменяет выполнение команды
        /// </summary>
        void Undo();
    }
}
