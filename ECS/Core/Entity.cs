using System;
using System.Collections;
using System.Collections.Generic;

namespace ECS.Core
{
    public delegate void EntityComponentChanged(Entity entity, ComponentBase componentBase);
    
    /// <summary>
    /// Класс, описывающий сущность.
    /// </summary>
    public class Entity : IEnumerable<KeyValuePair<Type, ComponentBase>>
    {
        #region Поля.
        private readonly Dictionary<Type, ComponentBase> _components = new Dictionary<Type, ComponentBase>();
        #endregion

        #region События.
        public event EntityComponentChanged ComponentAdded;
        public event EntityComponentChanged ComponentRemoved;
        #endregion.

        #region Методы.
        public IEnumerator<KeyValuePair<Type, ComponentBase>> GetEnumerator() => _components.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Метод добавляет компонент в сущность.
        /// Если сущность уже имеет данный компонент, то новый компонент не добавляется.
        /// </summary>
        /// <param name="componentBase">Компонент <see cref="ComponentBase"/>.</param>
        /// <returns>Сущность.</returns>
        public Entity Add(ComponentBase componentBase) 
        { 
            if (_components.ContainsKey(componentBase.GetType())) return this;
            _components[componentBase.GetType()] = componentBase;
            componentBase.Owner = this;
            ComponentAdded?.Invoke(this, componentBase);
            return this;
        }

        /// <summary>
        /// Метод удаляет компонент из сущности.
        /// </summary>
        /// <param name="type">Тип удаляемого компонента <see cref="ComponentBase"/>.</param>
        /// <returns>Удаленный компонент <see cref="ComponentBase"/>.</returns>
        public ComponentBase Remove(Type type)
        {
            _components.TryGetValue(type, out var component);
            if (component == null) return component;
            _components.Remove(type);
            component.Owner = null;
            ComponentRemoved?.Invoke(this, component);
            return component;
        }

        public T Remove<T>() where T : ComponentBase => (T)Remove(typeof(T));

        /// <summary>
        /// Метод, возвращающий компонент <see cref="ComponentBase"/> по типу компонента.
        /// </summary>
        /// <param name="type">Тип компонента.</param>
        /// <returns>Компонент <see cref="ComponentBase"/>, если компонент найден, null в противном случае.</returns>
        public T Get<T>() where T : ComponentBase => (T)Get(typeof(T));
        public ComponentBase Get(Type type) => _components.TryGetValue(type, out var component) ? component : null;

        /// <summary>
        /// Метод проверяет, обладает ли сущность компонентом данного типа.
        /// </summary>
        /// <param name="type">Тип компонента.</param>
        /// <returns>true, если компонент найден, false в противном случае.</returns>
        public bool Contain<T>() where T : ComponentBase => _components.ContainsKey(typeof(T));

        /// <summary>
        /// Метод очищает сущность от компонентов.
        /// </summary>
        public void Clear() => _components.Clear();
        #endregion
    }
}
