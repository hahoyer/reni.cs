using System;
using JetBrains.Annotations;

namespace hw.UnitTest;

[PublicAPI]
[Obsolete("Use DependenceProvider")]
// ReSharper disable once IdentifierTypo
public abstract class DependantAttribute : DependenceProvider { }