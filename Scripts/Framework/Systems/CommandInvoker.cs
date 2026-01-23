using System.Collections.Generic;

namespace Architecture.GlobalModules.Systems
{
    /// <summary>
    /// Invoker - инициатор, вызывает команды
    /// </summary>
    public class CommandInvoker
    {
        private ICommand _command;
        private Stack<ICommand> _commandHistory;

        public CommandInvoker()
        {
            _commandHistory = new Stack<ICommand>();
        }

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public void ExecuteCommand()
        {
            if (_command != null)
            {
                _command.Execute();
                _commandHistory.Push(_command); // Сохраняем команду для возможной отмены
            }
        }

        public void UndoLastCommand()
        {
            if (_commandHistory.Count > 0)
            {
                ICommand lastCommand = _commandHistory.Pop();
                lastCommand.Undo();
            }
        }

        public void ClearHistory()
        {
            _commandHistory.Clear();
        }

        public int HistoryCount => _commandHistory.Count;
    }
}