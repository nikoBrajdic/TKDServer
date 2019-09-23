using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ExtensionsNamespace
{
    public static class Extensions
    {
        /// <summary>
        /// Maps each element of the collection to a new form, given the element satisfies the condition.
        /// </summary>
        /// <typeparam name="TSource">The type of the collection element.</typeparam>
        /// <typeparam name="TResult">The type of the output element.</typeparam>
        /// <param name="collection">The collection on which to execute the mapping.</param>
        /// <param name="predicate">The condition expression against which to test each element.</param>
        /// <param name="action">The transform expression which is applied to each element, given the condition is satisfied.</param>
        /// <returns>A new IEnumerable object containing transformed elements.</returns>
        public static IEnumerable<TResult> MapIf<TSource, TResult>(this IEnumerable<TSource> collection, Func<TSource, bool> predicate, Func<TSource, TResult> action)
        {
            var output = new List<TResult>();
            foreach (var e in collection)
                if (predicate(e))
                    output.Add(action(e));
            return output;
        }
        /// <summary>
        /// Maps each element of the UIElementCollection to a new form, given the element satisfies the condition.
        /// </summary>
        /// <typeparam name="TResult">The type of the output element.</typeparam>
        /// <param name="collection">The collection on which to execute the mapping.</param>
        /// <param name="predicate">The condition expression against which to test each element.</param>
        /// <param name="action">The transform expression which is applied to each element, given the condition is satisfied.</param>
        /// <returns>A new IEnumerable object containing transformed elements.</returns>
        public static List<TResult> MapIf<TResult>(this UIElementCollection collection, Func<UIElement, bool> predicate, Func<UIElement, TResult> action)
        {
            var output = new List<TResult>();
            foreach (UIElement e in collection)
                if (predicate(e))
                    output.Add(action(e));
            return output;
        }
        /// <summary>
        /// Maps each element of the UIElementCollection to a new form, given the element satisfies the condition. Also exposes the index of each element.
        /// </summary>
        /// <typeparam name="TResult">The type of the output element.</typeparam>
        /// <param name="collection">The collection on which to execute the mapping.</param>
        /// <param name="predicate">The condition expression against which to test each element.</param>
        /// <param name="action">The transform expression which is applied to each element, given the condition is satisfied.</param>
        /// <returns>A new List object containing transformed elements.</returns>
        public static List<TResult> MapIfIndexed<TResult>(this UIElementCollection collection, Func<UIElement, int, bool> predicate, Func<UIElement, TResult> action)
        {
            var output = new List<TResult>();
            var i = 0;
            foreach (UIElement e in collection)
                if (predicate(e, i++))
                    output.Add(action(e));
            return output;
        }
    }
}
