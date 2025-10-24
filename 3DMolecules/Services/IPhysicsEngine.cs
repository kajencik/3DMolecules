using System.Collections.Generic;
using System.Windows.Media.Media3D;
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
    /// Provides the current boundary transform so physics can enforce collisions
    /// against a tilted/rotated container. If null, identity is assumed.
    /// </summary>
    /// <param name="transform">World transform of the container geometry.</param>
    void SetBoundaryTransform(Transform3D? transform);

    /// <summary>
    /// Gets diagnostic information about the physics engine.
    /// </summary>
    string GetDiagnostics();
}
