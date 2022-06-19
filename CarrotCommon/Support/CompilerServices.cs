// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

using System.ComponentModel;

namespace System.Runtime.CompilerServices {
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit {
    }

    /// <summary>
    /// Used to indicate to the compiler that the <c>.locals init</c> flag should not be set in method headers.
    /// </summary>
    /// <remarks>Internal copy of the .NET 5 attribute.</remarks>
    [AttributeUsage(
        AttributeTargets.Module |
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Interface |
        AttributeTargets.Constructor |
        AttributeTargets.Method |
        AttributeTargets.Property |
        AttributeTargets.Event,
        Inherited = false)]
    public sealed class SkipLocalsInitAttribute : Attribute {
    }
}

#endif