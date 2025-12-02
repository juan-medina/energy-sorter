#!/usr/bin/env godot
# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

class_name BatteryState

enum Status {NORMAL, HOVER, SELECTED}
var status: int = Status.NORMAL
var energies: Array[Color] = []
var current_energy: int = 0
var accepts_input: bool = true

func duplicate() -> BatteryState:
	var new_state: BatteryState = BatteryState.new()
	new_state.status = status
	new_state.energies = energies.duplicate()
	new_state.current_energy = current_energy
	new_state.accepts_input = accepts_input
	return new_state