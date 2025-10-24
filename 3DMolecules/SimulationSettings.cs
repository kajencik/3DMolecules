namespace ThreeDMolecules
{
    /// <summary>
    /// Centralized configuration for the simulation.
    /// All magic numbers and settings should be defined here.
    /// </summary>
    public static class SimulationSettings
    {
        // Cylinder dimensions
        public const double CylinderRadius = 10.0;
        public const double CylinderHeight = 20.0;

        // Physics settings
        public const double DefaultTimeStep = 1.0; // Relative time per update
        public const double CollisionRestitution = 0.8; // Elasticity (0=inelastic, 1=perfectly elastic)
        public const double SeparationForce = 0.1; // Push-apart distance on collision
        public const double MinRotationSpeed = 0.1; // Minimum rotation to prevent stopping
        public const double BoundaryEpsilon = 1e-3; // Small inset to avoid visual clipping

        // Initial molecule generation
        public const double MinVelocity = -0.02;
        public const double MaxVelocity = 0.02;
        public const double MinRotationSpeedInit = 1.0;
        public const double MaxRotationSpeedInit = 5.0;

        // Collision physics
        public const double RotationChangeScale = 0.1; // How much collisions affect rotation

        // Simulation timing
        public const int UpdateIntervalMs = 15; // ~67 FPS target
        public const double FpsUpdateIntervalSeconds = 1.0;

        // UI defaults
        public const int DefaultMoleculeCount = 120;
        public const int MinMoleculeCount = 0;
        public const int MaxMoleculeCount = 240;
    }
}
