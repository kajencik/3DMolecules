using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ThreeDMolecules.ViewModels;

public class SimulationViewModel : BaseViewModel
{
    private readonly DispatcherTimer _timer;
    private DateTime _lastFpsSample = DateTime.Now;
    private int _frameCounter;
    private double _fps;
    private bool _isRunning = true;

    private readonly Model3DGroup _moleculesRoot = new();
    private readonly CylindricalBoundary _boundary = new();

    // Orientation angles (degrees)
    private double _yaw;   // rotate around Y axis (left/right)
    private double _pitch; // rotate around X axis (up/down)
    private double _roll;  // rotate around Z axis (twist)

    public ObservableCollection<Molecule> Molecules { get; } = new();

    public Model3DGroup MoleculesRoot => _moleculesRoot; // bound in XAML
    public Model3DGroup BoundaryRoot => _boundary.Model; // bound in XAML

    public double Yaw
    {
        get => _yaw;
        set { if (SetProperty(ref _yaw, value)) OnPropertyChanged(nameof(SceneOrientationTransform)); }
    }
    public double Pitch
    {
        get => _pitch;
        set { if (SetProperty(ref _pitch, value)) OnPropertyChanged(nameof(SceneOrientationTransform)); }
    }
    public double Roll
    {
        get => _roll;
        set { if (SetProperty(ref _roll, value)) OnPropertyChanged(nameof(SceneOrientationTransform)); }
    }

    public Transform3D SceneOrientationTransform
    {
        get
        {
            var tg = new Transform3DGroup();
            // Order: yaw (Y), pitch (X), roll (Z)
            tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), Yaw)));
            tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), Pitch)));
            tg.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), Roll)));
            return tg;
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
                if (_isRunning) _timer.Start(); else _timer.Stop();
                StartCommand.RaiseCanExecuteChanged();
                PauseCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand ResetCommand { get; }

    private readonly Random _random = new();

    public SimulationViewModel()
    {
        StartCommand = new RelayCommand(() => IsRunning = true, () => !IsRunning);
        PauseCommand = new RelayCommand(() => IsRunning = false, () => IsRunning);
        ResetCommand = new RelayCommand(Reset);

        CreateInitialMolecules(120);

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
        _timer.Tick += (_, _) => Step();
        _timer.Start();
    }

    private void CreateInitialMolecules(int count)
    {
        Molecules.Clear();
        _moleculesRoot.Children.Clear();

        for (int i = 0; i < count; i++)
        {
            var position = new Point3D(
                _random.NextDouble() * 16 - 8,
                _random.NextDouble() * 16 - 8,
                _random.NextDouble() * 16 - 8
            );

            var velocity = new Vector3D(
                _random.NextDouble() * 0.04 - 0.02,
                _random.NextDouble() * 0.04 - 0.02,
                _random.NextDouble() * 0.04 - 0.02
            );

            var rotationSpeed = _random.NextDouble() * 4 + 1;
            var rotationAxis = new Vector3D(
                _random.NextDouble() * 2 - 1,
                _random.NextDouble() * 2 - 1,
                _random.NextDouble() * 2 - 1
            );

            var molecule = new Molecule(position, velocity, rotationSpeed, rotationAxis);
            Molecules.Add(molecule);
            _moleculesRoot.Children.Add(molecule.Model);
        }
    }

    private void Reset()
    {
        // Reset orientation
        Yaw = 0; Pitch = 0; Roll = 0; // property setters trigger transform update
        CreateInitialMolecules(120);
    }

    private void Step()
    {
        if (!IsRunning) return;

        foreach (var molecule in Molecules)
            molecule.Update();

        for (int i = 0; i < Molecules.Count; i++)
        {
            for (int j = i + 1; j < Molecules.Count; j++)
            {
                var a = Molecules[i];
                var b = Molecules[j];
                if (a.IsCollidingWith(b))
                    a.HandleCollision(b);
            }
        }

        _frameCounter++;
        var now = DateTime.Now;
        var elapsed = (now - _lastFpsSample).TotalSeconds;
        if (elapsed >= 1)
        {
            Fps = _frameCounter / elapsed;
            _frameCounter = 0;
            _lastFpsSample = now;
            Debug.WriteLine($"FPS: {Fps:F2}");
        }
    }
}
