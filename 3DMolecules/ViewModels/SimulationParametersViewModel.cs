using ThreeDMolecules.Services;

namespace ThreeDMolecules.ViewModels
{
 /// <summary>
 /// Bindable wrapper around simulation parameters with sane ranges.
 /// The physics engine will read from this via ISimulationParameters.
 /// </summary>
 public class SimulationParametersViewModel : BaseViewModel, ISimulationParameters
 {
 private double _cohesionDistance = SimulationSettings.CohesionDistance;
 private double _liquidStiffness = SimulationSettings.LiquidStiffness;
 private double _liquidNearStiffness = SimulationSettings.LiquidNearStiffness;
 private double _viscosity = SimulationSettings.Viscosity;
 private double _cohesionStrength = SimulationSettings.CohesionStrength;
 private double _preferredSpacing = SimulationSettings.PreferredSpacing;
 private double _linearDamping = SimulationSettings.LinearDamping;
 private double _gravity = SimulationSettings.Gravity;
 private double _collisionRestitution = SimulationSettings.CollisionRestitution;
 private double _separationForce = SimulationSettings.SeparationForce;
 private double _maxSpeed = SimulationSettings.MaxSpeed;
 private double _minRotationSpeed = SimulationSettings.MinRotationSpeed;
 private double _boundaryEpsilon = SimulationSettings.BoundaryEpsilon;
 private double _defaultTimeStep = SimulationSettings.DefaultTimeStep;

 public double CohesionDistance { get => _cohesionDistance; set => SetProperty(ref _cohesionDistance, value); }
 public double LiquidStiffness { get => _liquidStiffness; set => SetProperty(ref _liquidStiffness, value); }
 public double LiquidNearStiffness { get => _liquidNearStiffness; set => SetProperty(ref _liquidNearStiffness, value); }
 public double Viscosity { get => _viscosity; set => SetProperty(ref _viscosity, value); }
 public double CohesionStrength { get => _cohesionStrength; set => SetProperty(ref _cohesionStrength, value); }
 public double PreferredSpacing { get => _preferredSpacing; set => SetProperty(ref _preferredSpacing, value); }
 public double LinearDamping { get => _linearDamping; set => SetProperty(ref _linearDamping, value); }
 public double Gravity { get => _gravity; set => SetProperty(ref _gravity, value); }
 public double CollisionRestitution { get => _collisionRestitution; set => SetProperty(ref _collisionRestitution, value); }
 public double SeparationForce { get => _separationForce; set => SetProperty(ref _separationForce, value); }
 public double MaxSpeed { get => _maxSpeed; set => SetProperty(ref _maxSpeed, value); }
 public double MinRotationSpeed { get => _minRotationSpeed; set => SetProperty(ref _minRotationSpeed, value); }
 public double BoundaryEpsilon { get => _boundaryEpsilon; set => SetProperty(ref _boundaryEpsilon, value); }
 public double DefaultTimeStep { get => _defaultTimeStep; set => SetProperty(ref _defaultTimeStep, value); }

 public void ResetToDefaults()
 {
 CohesionDistance = SimulationSettings.CohesionDistance;
 LiquidStiffness = SimulationSettings.LiquidStiffness;
 LiquidNearStiffness = SimulationSettings.LiquidNearStiffness;
 Viscosity = SimulationSettings.Viscosity;
 CohesionStrength = SimulationSettings.CohesionStrength;
 PreferredSpacing = SimulationSettings.PreferredSpacing;
 LinearDamping = SimulationSettings.LinearDamping;
 Gravity = SimulationSettings.Gravity;
 CollisionRestitution = SimulationSettings.CollisionRestitution;
 SeparationForce = SimulationSettings.SeparationForce;
 MaxSpeed = SimulationSettings.MaxSpeed;
 MinRotationSpeed = SimulationSettings.MinRotationSpeed;
 BoundaryEpsilon = SimulationSettings.BoundaryEpsilon;
 DefaultTimeStep = SimulationSettings.DefaultTimeStep;
 }
 }
}
