using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using ThreeDMolecules.Models;
using ThreeDMolecules.Services;

namespace ThreeDMolecules.ViewModels;

/// <summary>
/// ViewModel for the molecular simulation.
/// Manages simulation state, physics updates, and provides data binding for the view.
/// </summary>
public class SimulationViewModel : BaseViewModel, IDisposable
{
    private readonly DispatcherTimer _timer;
    private readonly IPhysicsEngine _physicsEngine;
    private readonly MoleculeFactory _moleculeFactory;
    
    private DateTime _lastFpsSample = DateTime.Now;
    private int _frameCounter;
    private double _fps;
    private bool _isRunning = true;
    private int _moleculeCount = SimulationSettings.DefaultMoleculeCount;
    private string _diagnostics = string.Empty;

  private readonly Model3DGroup _moleculesRoot = new();
    private readonly CylindricalBoundary _boundary = new();

  // Collection of molecule data models
    private readonly ObservableCollection<MoleculeModel> _moleculeModels = new();
    
    // Collection of 3D visual representations (kept in sync with models)
    public ObservableCollection<Molecule> Molecules { get; } = new();

    public Model3DGroup MoleculesRoot => _moleculesRoot;
    public Model3DGroup BoundaryRoot => _boundary.Model;

  public int MoleculeCount
    {
        get => _moleculeCount;
        set
        {
     if (value < SimulationSettings.MinMoleculeCount)
     value = SimulationSettings.MinMoleculeCount;
          if (value > SimulationSettings.MaxMoleculeCount)
      value = SimulationSettings.MaxMoleculeCount;

      if (SetProperty(ref _moleculeCount, value))
   {
    AdjustMoleculeCount(_moleculeCount);
       }
        }
    }

    public double Fps
    {
        get => _fps;
        private set => SetProperty(ref _fps, value);
    }

  public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (SetProperty(ref _isRunning, value))
      {
                if (_isRunning) 
  _timer.Start(); 
         else 
             _timer.Stop();
    
      StartCommand.RaiseCanExecuteChanged();
    PauseCommand.RaiseCanExecuteChanged();
 }
        }
    }

    public string Diagnostics
    {
        get => _diagnostics;
        private set => SetProperty(ref _diagnostics, value);
    }

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand ResetCommand { get; }

    public SimulationViewModel() : this(new CpuPhysicsEngine(), new MoleculeFactory())
    {
    }

 /// <summary>
    /// Constructor with dependency injection for testability.
    /// </summary>
    public SimulationViewModel(IPhysicsEngine physicsEngine, MoleculeFactory moleculeFactory)
    {
    _physicsEngine = physicsEngine ?? throw new ArgumentNullException(nameof(physicsEngine));
        _moleculeFactory = moleculeFactory ?? throw new ArgumentNullException(nameof(moleculeFactory));

        StartCommand = new RelayCommand(() => IsRunning = true, () => !IsRunning);
        PauseCommand = new RelayCommand(() => IsRunning = false, () => IsRunning);
        ResetCommand = new RelayCommand(Reset);

        CreateInitialMolecules(_moleculeCount);

      _timer = new DispatcherTimer 
        { 
            Interval = TimeSpan.FromMilliseconds(SimulationSettings.UpdateIntervalMs) 
    };
        _timer.Tick += OnTimerTick;
 _timer.Start();
    }

  private void OnTimerTick(object? sender, EventArgs e)
    {
  Step();
    }

    private void CreateInitialMolecules(int count)
    {
        _moleculeModels.Clear();
        Molecules.Clear();
  _moleculesRoot.Children.Clear();

        for (int i = 0; i < count; i++)
        {
      var model = _moleculeFactory.CreateRandom();
         _moleculeModels.Add(model);
            
  var visual = new Molecule(model.Position, model.Velocity, model.RotationSpeed, model.RotationAxis);
          Molecules.Add(visual);
          _moleculesRoot.Children.Add(visual.Model);
        }
    }

    private void AdjustMoleculeCount(int target)
  {
        // Remove excess molecules
        while (_moleculeModels.Count > target)
 {
  var lastIndex = _moleculeModels.Count - 1;
    _moleculeModels.RemoveAt(lastIndex);
            _moleculesRoot.Children.RemoveAt(lastIndex);
            Molecules.RemoveAt(lastIndex);
        }

        // Add new molecules
 while (_moleculeModels.Count < target)
        {
            var model = _moleculeFactory.CreateRandom();
          _moleculeModels.Add(model);
            
         var visual = new Molecule(model.Position, model.Velocity, model.RotationSpeed, model.RotationAxis);
     Molecules.Add(visual);
            _moleculesRoot.Children.Add(visual.Model);
        }
    }

    private void Reset()
    {
     CreateInitialMolecules(MoleculeCount);
  _frameCounter = 0;
      _lastFpsSample = DateTime.Now;
        Fps = 0;
    }

    private void Step()
    {
        if (!IsRunning) return;

        try
        {
  // Update physics using the physics engine
            _physicsEngine.Update(_moleculeModels, SimulationSettings.DefaultTimeStep);

    // Sync visual representations with model data
            for (int i = 0; i < _moleculeModels.Count && i < Molecules.Count; i++)
       {
      Molecules[i].UpdateFromModel(_moleculeModels[i]);
            }

  // Update FPS counter
        _frameCounter++;
         var now = DateTime.Now;
 var elapsed = (now - _lastFpsSample).TotalSeconds;

            if (elapsed >= SimulationSettings.FpsUpdateIntervalSeconds)
            {
    Fps = _frameCounter / elapsed;
          Diagnostics = _physicsEngine.GetDiagnostics();
      _frameCounter = 0;
     _lastFpsSample = now;
                
        Debug.WriteLine($"FPS: {Fps:F2} | {Diagnostics}");
   }
     }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in simulation step: {ex.Message}");
            IsRunning = false;
   }
    }

    public void Dispose()
    {
        _timer?.Stop();
        _timer.Tick -= OnTimerTick;
  
        if (_physicsEngine is IDisposable disposable)
        {
     disposable.Dispose();
  }
    }
}
