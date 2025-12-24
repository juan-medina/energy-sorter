#!/usr/bin/env python3

# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

import sys
from pathlib import Path

if len(sys.argv) < 2:
	print("Usage: version.py <path-to-project.godot>")
	sys.exit(1)

project_path = Path(sys.argv[1])

if not project_path.is_file():
	print(f"[VERSION BUMP ERROR] File not found: {project_path}")
	sys.exit(1)

key_prefix = 'config/version='  # Adjust if your key differs

lines = project_path.read_text(encoding="utf-8").splitlines()
new_lines = []
found = False

for line in lines:
	stripped = line.strip()

	if stripped.startswith(key_prefix):
		before, quote, rest = line.partition('"')
		if quote != '"':
			new_lines.append(line)
			continue

		version_str, quote2, _ = rest.partition('"')
		if quote2 != '"':
			new_lines.append(line)
			continue

		parts = version_str.split('.')
		while len(parts) < 4:
			parts.append("0")

		# Increment build number
		build_str = parts[3]
		width = len(build_str)

		try:
			build_int = int(build_str)
		except ValueError:
			build_int = 0

		build_int += 1
		parts[3] = str(build_int).zfill(width)

		new_version = ".".join(parts)
		new_line = f'{before}"{new_version}"'

		print("==============================================")
		print("   GODOT VERSION BUMPED SUCCESSFULLY")
		print("==============================================")
		print(f"Old Version: {version_str}")
		print(f"New Version: {new_version}")
		print(f"File Updated: {project_path}")
		print("==============================================")

		new_lines.append(new_line)
		found = True
	else:
		new_lines.append(line)

if not found:
	print("==============================================")
	print("[VERSION BUMP WARNING]")
	print(f"No version key '{key_prefix}' found in:")
	print(f"  {project_path}")
	print("==============================================")
else:
	project_path.write_text("\n".join(new_lines) + "\n", encoding="utf-8")
