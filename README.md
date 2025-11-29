# Energy Sorter

[![License: MIT](https://img.shields.io/badge/License-MIT-478CBF.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)
[![Conduct](https://img.shields.io/badge/Conduct-Covenat%202.0-478CBF.svg?style=for-the-badge)](https://www.contributor-covenant.org/version/2/0/code_of_conduct/)
[![Made with Godot](https://img.shields.io/badge/GODOT-4.5-478CBF?style=for-the-badge&logo=godot%20engine&logoColor=white)](https://godotengine.org)

- **Description**: A small Godot puzzle/demonstration where colored "energy" units are moved between battery nodes until each battery is either empty or contains a single color. Built with Godot 4.5.

**Getting Started**

- **Requirements**: Godot Engine 4.5 (or 4.5-compatible). Open the project using the Godot editor.
- **Run**: Open `project.godot` in Godot and press Play, or set the main scene to `res://scenes/main.tscn` and run.

**Project Structure**
- `scenes/`: Godot scenes and nodes
- `resources/`: textures, fonts, and imported assets used by scenes.

**How it works (brief)**

- The main scene instantiates multiple battery scenes. Each battery holds up to 4 "energy" color units.
- Players click a battery to pick up a contiguous run of top-most energy units of the same color, then click another battery to place them if allowed by the rules.

**License**

- This project is licensed under the MIT License — see `LICENSE` for details.

**Contributing**

- Feel free to open issues or pull requests. For code changes, follow the existing GDScript style and keep changes focused.

**Contact**

- Author: Juan Medina (copyright 2025)
