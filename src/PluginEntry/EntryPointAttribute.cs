using System;
using JetBrains.Annotations;

namespace GreyAnnouncer;

/// <summary>
/// Marks a type and method as an entry point for a submodule of the plugin.
/// The method MUST be static, and the attribute should be applied to the method and the declaring type
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class EntryPointAttribute : Attribute;