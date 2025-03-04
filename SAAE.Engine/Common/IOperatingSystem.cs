﻿namespace SAAE.Engine;

/// <summary>
/// Base interface for all operating systems across
/// all architectures.
/// </summary>
public interface IOperatingSystem : IDisposable {
    
    /// <summary>
    /// The target architecture that this operating system
    /// accepts.
    /// </summary>
    public Architecture CompatibleArchitecture { get; }
    
    /// <summary>
    /// The user-friendly name of this operating system. 
    /// </summary>
    /// <remarks>
    /// This string should not need to be localized. It
    /// is a name after all.
    /// </remarks>
    public string FriendlyName { get; }
}