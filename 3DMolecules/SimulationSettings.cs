namespace ThreeDMolecules
{
    /// <summary>
    /// Centralized simulation settings to avoid duplicating constants across files.
    /// </summary>
    public static class SimulationSettings
    {
        // Cylinder dimensions used by both the boundary visualization and the molecule physics
        public const double CylinderRadius = 10.0;
        public const double CylinderHeight = 20.0;
    }
}
