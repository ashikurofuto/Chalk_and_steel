using System;
using System.Collections.Generic;
using System.Linq;

namespace Architecture.GlobalModules
{

    /// <summary>
    /// Глобальный модуль уровня 1 для управления кастомными событиями.
    /// Изолирован от Unity API для тестируемости.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers;
        private readonly object _lockObject = new object();

        public EventBus()
        {
            _subscribers = new Dictionary<Type, List<Delegate>>();
        }

        /// <summary>
        /// Публикует событие всем зарегистрированным подписчикам.
        /// Выполняется синхронно в порядке подписки.
        /// </summary>
        /// <typeparam name="TEvent">Тип события (должен быть классом)</typeparam>
        /// <param name="event">Экземпляр события для публикации</param>
        /// <exception cref="ArgumentNullException">Если передан null</exception>
        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event), "Event cannot be null");

            Type eventType = typeof(TEvent);
            List<Delegate> handlers;

            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers) || handlers.Count == 0)
                    return;

                // Создаем копию списка для безопасной итерации
                handlers = new List<Delegate>(handlers);
            }

            // Вызываем все обработчики вне блокировки для предотвращения deadlock
            foreach (var handler in handlers.OfType<Action<TEvent>>())
            {
                try
                {
                    handler(@event);
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не прерываем выполнение других обработчиков
                    // В Unity используем Debug.LogError, но модуль изолирован от Unity API
                    // В продакшене следует использовать ILogger
                    Console.WriteLine($"Error in event handler for {eventType.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Регистрирует обработчик для указанного типа события.
        /// Не допускает дублирование подписок с тем же обработчиком.
        /// </summary>
        /// <typeparam name="TEvent">Тип события (должен быть классом)</typeparam>
        /// <param name="handler">Обработчик события</param>
        /// <exception cref="ArgumentNullException">Если обработчик null</exception>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            Type eventType = typeof(TEvent);

            lock (_lockObject)
            {
                if (!_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers = new List<Delegate>();
                    _subscribers[eventType] = handlers;
                }

                // Проверяем, не подписан ли уже этот обработчик
                if (!handlers.Contains(handler))
                {
                    handlers.Add(handler);
                }
                else
                {
                    Console.WriteLine($"Handler already subscribed for {eventType.Name}");
                }
            }
        }

        /// <summary>
        /// Удаляет обработчик из списка подписчиков для указанного типа события.
        /// </summary>
        /// <typeparam name="TEvent">Тип события (должен быть классом)</typeparam>
        /// <param name="handler">Обработчик для удаления</param>
        /// <exception cref="ArgumentNullException">Если обработчик null</exception>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            Type eventType = typeof(TEvent);

            lock (_lockObject)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);

                    // Очищаем список, если подписчиков не осталось
                    if (handlers.Count == 0)
                    {
                        _subscribers.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Очищает все подписки для указанного типа события.
        /// Используется при перезагрузке систем.
        /// </summary>
        /// <typeparam name="TEvent">Тип события для очистки</typeparam>
        public void ClearSubscriptions<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);

            lock (_lockObject)
            {
                _subscribers.Remove(eventType);
            }
        }

        /// <summary>
        /// Возвращает количество подписчиков для указанного типа события.
        /// Используется для отладки и тестирования.
        /// </summary>
        public int GetSubscriberCount<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);

            lock (_lockObject)
            {
                return _subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
            }
        }

        /// <summary>
        /// Проверяет, есть ли подписчики для указанного типа события.
        /// Оптимизация для предотвращения создания объектов событий.
        /// </summary>
        public bool HasSubscribers<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);

            lock (_lockObject)
            {
                return _subscribers.TryGetValue(eventType, out var handlers) && handlers.Count > 0;
            }
        }
    }
}