using System;

namespace Architecture.GlobalModules
{
    /// <summary>
    /// Глобальный модуль для управления событиями между архитектурными слоями.
    /// Реализует паттерн Publisher-Subscriber через строгую типизацию событий.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Публикация события всем подписчикам
        /// </summary>
        void Publish<TEvent>(TEvent @event) where TEvent : class;

        /// <summary>
        /// Подписка на событие с указанием обработчика
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        /// <summary>
        /// Отписка от события
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    }
}