﻿using System;
using System.Collections.Generic;

namespace AnyConfig
{
    /// <summary>
    /// A node based object
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// The name of the node
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The path of the node
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// The path of the node
        /// </summary>
        string FullPathWithArrayHints { get; }

        /// <summary>
        /// The raw length of the string this node occupies
        /// </summary>
        int Length { get; }

        /// <summary>
        /// If a child of an array, indicates the array element position it belongs in.
        /// </summary>
        int? ArrayPosition { get; set; }

        /// <summary>
        /// All child nodes inherited by this node
        /// </summary>
        List<INode> ChildNodes { get; set; }

        /// <summary>
        /// The parent mode
        /// </summary>
        INode ParentNode { get; set; }

        /// <summary>
        /// The raw string that makes up the node
        /// </summary>
        string OuterText { get; set; }

        /// <summary>
        /// The value of a node (value types only)
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Get all values for an array
        /// </summary>
        List<INode> ArrayNodes { get; }

        /// <summary>
        /// Get INode as concrete type
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <returns></returns>
        TNode As<TNode>() where TNode : INode;

        /// <summary>
        /// Select a node by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        INode SelectNodeByName(string name);

        /// <summary>
        /// Select a node by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        INode SelectNodeByName(string name, StringComparison comparisonType);

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string SelectValueByName(string name);

        /// <summary>
        /// Select a child node's value by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string SelectValueByName(string name, StringComparison comparisonType);

        /// <summary>
        /// Select a child node by its path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        INode SelectNodeByPath(string path);

        /// <summary>
        /// Select a child node by its path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        INode SelectNodeByPath(string path, StringComparison comparisonType);

        /// <summary>
        /// Select a child node's value by its path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string SelectValueByPath(string path);

        /// <summary>
        /// Select a child node's value by its path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string SelectValueByPath(string path, StringComparison comparisonType);

        /// <summary>
        /// Query child nodes
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        IEnumerable<INode> QueryNodes(Func<INode, bool> condition);

    }
}
