using System.Collections.Generic;
using ThreeDMolecules.Models;

namespace ThreeDMolecules.Services;

/// <summary>
/// Interface for physics simulation engines.
/// Allows for different implementations (CPU, GPU, etc.)
/// </summary>
public interface IPhysicsEngine
{
    /// <summary>
  /// Updates all molecules for one simulation step.
    /// </summary>
    /// <param name="molecules">The molecules to update.</param>
    /// <param name="deltaTime">Time step in seconds (for frame-independent physics).</param>
    void Update(IList<MoleculeModel> molecules, double deltaTime);

    /// <summary>
    /// Gets diagnostic information about the physics engine.
    /// </summary>
    string GetDiagnostics();
}
