using System;
using System.Collections.Generic;

/// <summary>
/// Двусвязный список для хранения RoomNode
/// </summary>
public class RoomDoublyLinkedList
{
    private RoomNode head;
    private RoomNode tail;
    private RoomNode currentNode;
    private int count;

    public int Count => count;
    public bool IsEmpty => count == 0;
    public RoomNode Head => head;
    public RoomNode Tail => tail;
    public RoomNode CurrentNode => currentNode;

    public RoomDoublyLinkedList()
    {
        head = null;
        tail = null;
        currentNode = null;
        count = 0;
    }

    /// <summary>
    /// Добавляет элемент в начало списка
    /// </summary>
    /// <param name="node">Элемент для добавления</param>
    public void AddFirst(RoomNode node)
    {
        if (head == null)
        {
            head = tail = node;
            node.Previous = null;
            node.Next = null;
            // Если это первый элемент, он становится текущим
            if (currentNode == null)
            {
                currentNode = node;
            }
        }
        else
        {
            node.Next = head;
            head.Previous = node;
            node.Previous = null;
            head = node;
        }

        count++;
    }

    /// <summary>
    /// Добавляет элемент в конец списка
    /// </summary>
    /// <param name="node">Элемент для добавления</param>
    public void AddLast(RoomNode node)
    {
        if (tail == null)
        {
            head = tail = node;
            node.Previous = null;
            node.Next = null;
            // Если это первый элемент, он становится текущим
            if (currentNode == null)
            {
                currentNode = node;
            }
        }
        else
        {
            node.Previous = tail;
            tail.Next = node;
            node.Next = null;
            tail = node;
        }

        count++;
    }

    /// <summary>
    /// Удаляет первый элемент из списка
    /// </summary>
    /// <returns>Удаленный элемент или null, если список пуст</returns>
    public RoomNode RemoveFirst()
    {
        if (head == null)
        {
            return null;
        }

        var removedNode = head;
        head = head.Next;

        if (head != null)
        {
            head.Previous = null;
        }
        else
        {
            tail = null; // Если список стал пустым
        }

        // Если удаляемый элемент был текущим, сбрасываем текущий
        if (removedNode == currentNode)
        {
            currentNode = head;
        }

        count--;

        // Отсоединяем удаляемый узел от списка
        removedNode.Next = null;
        removedNode.Previous = null;

        return removedNode;
    }

    /// <summary>
    /// Удаляет последний элемент из списка
    /// </summary>
    /// <returns>Удаленный элемент или null, если список пуст</returns>
    public RoomNode RemoveLast()
    {
        if (tail == null)
        {
            return null;
        }

        var removedNode = tail;
        tail = tail.Previous;

        if (tail != null)
        {
            tail.Next = null;
        }
        else
        {
            head = null; // Если список стал пустым
        }

        // Если удаляемый элемент был текущим, сбрасываем текущий
        if (removedNode == currentNode)
        {
            currentNode = tail;
        }

        count--;

        // Отсоединяем удаляемый узел от списка
        removedNode.Next = null;
        removedNode.Previous = null;

        return removedNode;
    }

    /// <summary>
    /// Получает первый элемент списка
    /// </summary>
    /// <returns>Первый элемент или null, если список пуст</returns>
    public RoomNode First()
    {
        return head;
    }

    /// <summary>
    /// Получает последний элемент списка
    /// </summary>
    /// <returns>Последний элемент или null, если список пуст</returns>
    public RoomNode Last()
    {
        return tail;
    }

    /// <summary>
    /// Проверяет, содержится ли элемент в списке
    /// </summary>
    /// <param name="node">Элемент для проверки</param>
    /// <returns>True, если элемент содержится в списке, иначе false</returns>
    public bool Contains(RoomNode node)
    {
        var currentNode = head;

        while (currentNode != null)
        {
            if (currentNode == node)
            {
                return true;
            }

            currentNode = currentNode.Next;
        }

        return false;
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
        head = null;
        tail = null;
        currentNode = null;
        count = 0;
    }

    /// <summary>
    /// Перебирает элементы списка
    /// </summary>
    public void ForEach(Action<RoomNode> action)
    {
        var currentNode = head;

        while (currentNode != null)
        {
            action(currentNode);
            currentNode = currentNode.Next;
        }
    }
    
    /// <summary>
    /// Устанавливает текущую ноду
    /// </summary>
    /// <param name="node">Ноду для установки как текущую</param>
    public void SetCurrentNode(RoomNode node)
    {
        if (Contains(node))
        {
            currentNode = node;
        }
    }
    
    /// <summary>
    /// Получает текущую ноду
    /// </summary>
    /// <returns>Текущая нода или null, если нет текущей ноды</returns>
    public RoomNode GetCurrentNode()
    {
        return currentNode;
    }
    
    /// <summary>
    /// Переходит к следующей ноде в списке
    /// </summary>
    /// <returns>Следующая нода или null, если следующей ноды нет</returns>
    public RoomNode MoveToNext()
    {
        if (currentNode != null && currentNode.Next != null)
        {
            currentNode = currentNode.Next;
            return currentNode;
        }
        return null;
    }
    
    /// <summary>
    /// Переходит к предыдущей ноде в списке
    /// </summary>
    /// <returns>Предыдущая нода или null, если предыдущей ноды нет</returns>
    public RoomNode MoveToPrevious()
    {
        if (currentNode != null && currentNode.Previous != null)
        {
            currentNode = currentNode.Previous;
            return currentNode;
        }
        return null;
    }
    
    /// <summary>
    /// Получает все ноды в списке
    /// </summary>
    /// <returns>Список всех нод</returns>
    public List<RoomNode> GetAllNodes()
    {
        var nodes = new List<RoomNode>();
        var current = head;
        
        while (current != null)
        {
            nodes.Add(current);
            current = current.Next;
        }
        
        return nodes;
    }
}
