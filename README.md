# 3DMolecules

## Overview
3DMolecules is a WPF (.NET 8) application that simulates and visualizes many water molecules (H?O) moving and colliding inside a transparent cylindrical container. It uses HelixToolkit.Wpf for 3D rendering (spheres, pipes, custom meshes) and now follows an MVVM architecture.

![Screenshot](3Dmolecules.png)

## Key Features
- Real?time 3D molecular motion (position, rotation) with elastic collision handling.
- Cylindrical boundary with transparent wall, opaque base, liquid surface and decorative rings.
- Adjustable simulation control (Start / Pause / Reset) via commands.
- FPS counter and live molecule count.
- Clean MVVM separation (minimal code?behind, testable logic in ViewModel/Models).

## Technology Stack
- .NET 8 / C# 12
- WPF / XAML
- HelixToolkit.Wpf (3D scene + camera interaction)
- MVVM pattern (custom lightweight implementation: `BaseViewModel`, `RelayCommand`)

## Architecture (MVVM)
### Model Layer
- `Molecule`: Holds geometry (`Model3DGroup`) + physics state (velocity, rotation axis/speed, transforms). Provides `Update`, collision detection (`IsCollidingWith`) and collision response (`HandleCollision`).
- `CylindricalBoundary`: Builds static 3D geometry for the container (wall, base, rings, liquid surface) with transparent materials.
- `SimulationSettings`: Central constants (radius, height).

### ViewModel Layer
- `SimulationViewModel`:
  - Owns an `ObservableCollection<Molecule>`.
  - Maintains two persistent `Model3DGroup`s: `MoleculesRoot` (all molecule models) and `BoundaryRoot` (container) exposed for binding.
  - Runs a `DispatcherTimer` (15 ms) driving `Step()` which:
    1. Updates each molecule.
    2. Performs pairwise collision checks.
    3. Updates FPS once per second.
  - Exposes state: `Fps`, `IsRunning`.
  - Exposes commands: `StartCommand`, `PauseCommand`, `ResetCommand` (via `RelayCommand`).

### View Layer
- `MainWindow.xaml`:
  - Declares camera, lights, and two `ModelVisual3D` bindings: molecules first, boundary last (ensures correct transparency blending).
  - Binds buttons to commands and text blocks to properties (no simulation logic in code?behind).
  - Code?behind only calls `ZoomExtents` on load.

### Simulation Startup Flow
1. WPF loads `MainWindow` (declared in `App.xaml`).
2. XAML instantiates `SimulationViewModel` as `DataContext`.
3. `SimulationViewModel` constructor creates initial molecules and starts the dispatcher timer.
4. Timer ticks invoke `Step()`, mutating transforms; HelixToolkit redraws automatically.

## Extending the Simulation
| Goal | Suggested Change |
|------|------------------|
| Change molecule count | Add a bound property (e.g. `InitialMoleculeCount`) + UI input, call Reset. |
| Adjustable speed | Scale velocities in `Step()` or expose a global speed factor property. |
| Add molecule types | Derive new model builders or parameterize radii/colors in `Molecule`. |
| Selection / inspection | Wrap each `Molecule` in a `MoleculeViewModel` exposing additional info. |
| Persistence | Serialize initial seeds & settings, reconstruct on load. |
| Unit tests | Extract physics into a plain `SimulationEngine` class and test collision outcomes. |

## Building & Running
```bash
git clone https://github.com/kajencik/3DMolecules.git
cd 3DMolecules
dotnet run
```
(or open the solution in Visual Studio 2022+ and press F5)

## Project Status / Attribution
This project (including the MVVM refactor and much of the current structure) was largely produced with assistance from an AI coding assistant (GitHub Copilot Chat) based on user prompts and iterative refinement. Further manual adjustments are welcome.

## Contributing
Contributions are welcome. Ideas: performance tuning (spatial partitioning for collisions), UI panels for live parameter editing, exporting frames or trajectories.

## License
MIT License – see `LICENSE`.

## Acknowledgments
- [HelixToolkit.Wpf](https://github.com/helix-toolkit/helix-toolkit)
- WPF / .NET team

---
Enjoy exploring molecules in 3D!