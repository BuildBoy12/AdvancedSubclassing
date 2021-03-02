// -----------------------------------------------------------------------
// <copyright file="CommentGatheringTypeInspector.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
namespace Subclass.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.TypeInspectors;

    /// <summary>
    /// Source: https://dotnetfiddle.net/8M6iIE.
    /// </summary>
    internal sealed class CommentGatheringTypeInspector : TypeInspectorSkeleton
    {
        private readonly ITypeInspector innerTypeDescriptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentGatheringTypeInspector"/> class.
        /// </summary>
        /// <param name="innerTypeDescriptor">The inner type description instance.</param>
        public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
        {
            this.innerTypeDescriptor =
                innerTypeDescriptor ?? throw new ArgumentNullException(nameof(innerTypeDescriptor));
        }

        /// <inheritdoc/>
        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
        {
            return this.innerTypeDescriptor.GetProperties(type, container)
                .Select(descriptor => new CommentsPropertyDescriptor(descriptor));
        }
    }
}