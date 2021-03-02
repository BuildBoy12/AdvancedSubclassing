// -----------------------------------------------------------------------
// <copyright file="CommentsPropertyDescriptor.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace Subclass.Managers
{
    using System;
    using System.ComponentModel;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Source: https://dotnetfiddle.net/8M6iIE.
    /// </summary>
    internal sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor baseDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentsPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="baseDescriptor">The base descriptor instance.</param>
        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            this.baseDescriptor = baseDescriptor;
            this.Name = baseDescriptor.Name;
        }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public Type Type => this.baseDescriptor.Type;

        /// <inheritdoc/>
        public Type TypeOverride
        {
            get => this.baseDescriptor.TypeOverride;
            set => this.baseDescriptor.TypeOverride = value;
        }

        /// <inheritdoc/>
        public int Order { get; set; }

        /// <inheritdoc/>
        public ScalarStyle ScalarStyle
        {
            get => this.baseDescriptor.ScalarStyle;
            set => this.baseDescriptor.ScalarStyle = value;
        }

        /// <inheritdoc/>
        public bool CanWrite => this.baseDescriptor.CanWrite;

        /// <inheritdoc/>
        public void Write(object target, object value)
        {
            this.baseDescriptor.Write(target, value);
        }

        /// <inheritdoc/>
        public T GetCustomAttribute<T>()
            where T : Attribute
        {
            return this.baseDescriptor.GetCustomAttribute<T>();
        }

        /// <inheritdoc/>
        public IObjectDescriptor Read(object target)
        {
            var description = this.baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            return new CommentsObjectDescriptor(this.baseDescriptor.Read(target), description.Description);
        }
    }
}