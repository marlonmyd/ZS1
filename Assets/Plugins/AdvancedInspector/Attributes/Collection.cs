using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;

namespace AdvancedInspector
{
    /// <summary>
    /// When affixes to a collection, prevent this collection's size to be modified by the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CollectionAttribute : Attribute, IListAttribute, IRuntimeAttribute<object>
    {
        public delegate object CollectionDelegate();

        private int size = -1;

        /// <summary>
        /// Size of this collection.
        /// Default -1; size is not controlled by code.
        /// 0 means the collection's size will be handled internally.
        /// > 0 indicate the same of the collection.
        /// </summary>
        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        private bool sortable = true;

        /// <summary>
        /// If true, the list can be sorted by hand.
        /// </summary>
        public bool Sortable
        {
            get { return sortable; }
            set { sortable = value; }
        }

        private CollectionDisplay display = CollectionDisplay.List;

        /// <summary>
        /// If not default, removes the collection list and only display one item at a time.
        /// </summary>
        public CollectionDisplay Display
        {
            get { return display; }
            set { display = value; }
        }

        private int maxDisplayedItems = 25;

        /// <summary>
        /// When a collection is very large, it get up/down arrows to scrolls in items instead of displaying them all.
        /// This property controls how many items are displayed before the scrolls appears. 
        /// </summary>
        public int MaxDisplayedItems
        {
            get { return maxDisplayedItems; }
            set { maxDisplayedItems = value; }
        }

        private int maxItemsPerRow = 6;

        /// <summary>
        /// When display is using Button, this is the maximum number of button displayed per rows before creating a new one.
        /// </summary>
        public int MaxItemsPerRow
        {
            get { return maxItemsPerRow; }
            set { maxItemsPerRow = value; }
        }

        private Type enumType = null;

        /// <summary>
        /// Bind the size of a collection to the values of an enum.
        /// The name of the indexed are displayed using the enum values' names. 
        /// </summary>
        public Type EnumType
        {
            get { return enumType; }
            set 
            {
                if (!value.IsEnum)
                    return;

                int index = 0;
                foreach (object i in Enum.GetValues(value))
                {
                    if ((int)i != index)
                        return;

                    index++;
                }

                enumType = value; 
            }
        }

        #region IRuntime Implementation
        private string methodName = "";

        public string MethodName
        {
            get { return methodName; }
        }

        public Type Template
        {
            get { return typeof(CollectionDelegate); }
        }

        private List<Delegate> delegates = new List<Delegate>();

        public List<Delegate> Delegates
        {
            get { return delegates; }
            set { delegates = value; }
        }

        public object Invoke(int index)
        {
            if (delegates.Count == 0 || index >= delegates.Count)
                return null;

            try
            {
                return delegates[index].DynamicInvoke();
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException)
                    e = ((TargetInvocationException)e).InnerException;

                Debug.LogError(string.Format("Invoking a method from a collection constructor failed. The exception was \"{0}\".", e.Message));
                return null;
            }
        }
        #endregion

        public CollectionAttribute() { }

        public CollectionAttribute(int size)
            : this(size, true, "") { }

        public CollectionAttribute(Type enumType)
            : this(enumType, true, "") { }

        public CollectionAttribute(bool sortable)
            : this(-1, sortable, "") { }

        public CollectionAttribute(string methodName)
            : this(-1, true, methodName) { }

        public CollectionAttribute(int size, bool sortable)
            : this(size, sortable, "") { }

        public CollectionAttribute(int size, string methodName)
            : this(size, true, methodName) { }

        public CollectionAttribute(Type enumType, bool sortable)
            : this(enumType, sortable, "") { }

        public CollectionAttribute(Type enumType, string methodName)
            : this(enumType, true, methodName) { }

        public CollectionAttribute(bool sortable, string methodName)
            : this(-1, sortable, methodName) { }

        public CollectionAttribute(int size, bool sortable, string methodName)
        {
            this.size = size;
            this.sortable = sortable;
            this.methodName = methodName;
        }

        public CollectionAttribute(Type enumType, bool sortable, string methodName)
        {
            this.EnumType = enumType;
            this.sortable = sortable;
            this.methodName = methodName;
        }

        public CollectionAttribute(Delegate method)
        {
            this.delegates.Add(method);
        }
    }

    /// <summary>
    /// None default display should only be used on collection that contain expandable objects.
    /// </summary>
    public enum CollectionDisplay
    { 
        List,
        DropDown,
        Button
    }
}