using System.Collections;
using System.Diagnostics.CodeAnalysis;
using GameServer.Model.Entities;
using GameServer.Model.EventBus;
using GameServer.Model.IoC;

namespace GameServer.Model.Components;


public class ComponentSystem : BaseSystem
{
    [Dependency] private EventBusSystem _event = null!;

    public override void Initialize()
    {
        base.Initialize();

        _event.Subscribe<EntityRemoveEvent>(RemoveAllComponents);
    }

    private void RemoveAllComponents(Entity ent, EntityRemoveEvent ev)
    {
        var componentTypes = ent.Info.Components.Keys.ToList();
        foreach (var compType in componentTypes)
            RemoveComponent(ent, compType);
    }

    public T? GetComponentOrDefault<T>(Entity ent) where T : Component
    {
        if (ent.Info.Components.TryGetValue(typeof(T), out var comp))
            return (T)comp;
        return null;
    }

    public Component? GetComponentOrDefault(Entity ent, Type type)
    {
        return ent.Info.Components.GetValueOrDefault(type);
    }

    public bool TryGetComponent<T>(Entity ent, [NotNullWhen(true)] out T? comp) where T : Component
    {
        comp = GetComponentOrDefault<T>(ent);
        return comp != null;
    }

    public T EnsureComponent<T>(Entity ent) where T : Component
    {
        if (!TryGetComponent<T>(ent, out var comp))
            comp = AddComponent<T>(ent);
        return comp;
    }


    public Component AddComponent(Entity ent, Type compType, Dictionary<string, object>? data = null)
    {
        var comp = (Component)Activator.CreateInstance(compType)!;
        comp.Owner = ent;

        ApplyComponentData(comp, data);

        ent.Info.Components.Add(compType, comp);
        return comp;
    }

    public T AddComponent<T>(Entity ent, Dictionary<string, object>? data = null) where T : Component
    {
        var comp = Activator.CreateInstance<T>();
        comp.Owner = ent;

        ApplyComponentData(comp, data);

        ent.Info.Components.Add(typeof(T), comp);
        return comp;
    }

    public void RemoveComponent<T>(Entity ent) where T : Component
    {
        RemoveComponent(ent, typeof(T));
    }

    public void RemoveComponent(Entity ent, Type compType)
    {
        ent.Info.Components.Remove(compType);
    }

    public bool HasComponent<T>(Entity ent) where T : Component
    {
        return HasComponent(ent, typeof(T));
    }

    public bool HasComponent(Entity ent, Type compType)
    {
        return ent.Info.Components.ContainsKey(compType);
    }

    private void ApplyComponentData(Component component, Dictionary<string, object>? data)
    {
        if (data == null)
            return;

        var componentType = component.GetType();
        
        AttachData(component, componentType, data);
    }

    private object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return null;
        
        if (targetType.IsInstanceOfType(value))
            return value;

        if (value is IDictionary dictionary && !IsSimpleType(targetType))
        {
            var data = new Dictionary<string, object>();
            foreach (DictionaryEntry entry in dictionary)
                if (entry.Key is string key)
                    data[key] = entry.Value!;
            return CreateComplexObject(targetType, data);
        }
            
        
        if (value is IEnumerable<object> collection && !IsSimpleType(targetType))
            return CreateCollection(targetType, collection);
        
        if (targetType.IsEnum)
            return Enum.Parse(targetType, value.ToString()!);
        
        return Convert.ChangeType(value, targetType);
    }

    private object CreateComplexObject(Type targetType, Dictionary<string, object> data)
    {
        var instance = Activator.CreateInstance(targetType);

        if (instance == null)
            throw new InvalidOperationException($"Cannot create instance of {targetType.Name}");
        
        AttachData(instance, targetType, data);
        
        return instance;
    }

    private object CreateCollection(Type targetType, IEnumerable<object> collection)
    {
        // Check []
        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var items = collection.Select(item => ConvertValue(item, elementType)).ToArray();
    
            var array = Array.CreateInstance(elementType, items.Length);
            Array.Copy(items, array, items.Length);
            return array;
        }

        // Check Generic IEnumerable<T>
        if (targetType.IsGenericType)
        {
            var genericTypeDef = targetType.GetGenericTypeDefinition();

            if (genericTypeDef == typeof(List<>) ||
                genericTypeDef == typeof(IList<>) ||
                genericTypeDef == typeof(ICollection<>) ||
                genericTypeDef == typeof(IEnumerable<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                
                var concreteType = targetType.IsInterface 
                    ? typeof(List<>).MakeGenericType(elementType)
                    : targetType;
                
                var list = (System.Collections.IList)Activator.CreateInstance(concreteType)!;
            
                foreach (var item in collection)
                    list.Add(ConvertValue(item, elementType));
            
                return list;
            }
        }

        throw new InvalidOperationException($"Unsupported collection type: {targetType.Name}");
    }


    private void AttachData(object obj, Type targetType, Dictionary<string, object> data)
    {
        foreach (var kvp in data)
        {
            var property = targetType.GetProperty(kvp.Key);

            if (property != null && property.CanWrite)
            {
                var value = ConvertValue(kvp.Value, property.PropertyType);
                property.SetValue(obj, value);
                continue;
            }
            
            
            var field = targetType.GetField(kvp.Key);
        
            if (field != null)
            {
                var value = ConvertValue(kvp.Value, field.FieldType);
                field.SetValue(obj, value);
            }
        }
    }
    
    private bool IsSimpleType(Type type)
    {
        return type.IsPrimitive 
               || type.IsEnum 
               || type == typeof(string)
               || type == typeof(DateTime) 
               || type == typeof(Guid);
    }
}