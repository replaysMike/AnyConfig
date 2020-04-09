using System;
using System.Collections.Generic;

namespace AnyConfig
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Recursively select children
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IEnumerable<INode> SelectChildren(this IEnumerable<INode> source, Func<INode, IEnumerable<INode>> selector)
        {
            foreach (var parent in source)
            {
                yield return parent;

                var children = selector(parent);
                foreach (var child in SelectChildren(children, x => x.ChildNodes))
                    yield return child;
            }
        }
    }
}
