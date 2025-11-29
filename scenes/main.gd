#!/usr/bin/env godot
# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

extends Node2D

const ENERGY_COLORS: Array[Color] = [
	Color8(230, 159, 0),   # #E69F00 (orange)
	Color8(86, 180, 233),  # #56B4E9 (sky blue)
	Color8(0, 158, 115),   # #009E73 (green)
	Color8(240, 228, 66),  # #F0E442 (yellow)
	Color8(0, 114, 178),   # #0072B2 (blue)
	Color8(213, 94, 0)     # #D55E00 (vermillion)
]
const MAX_BATTERIES: int = 8

var _batteries: Array[Battery] = []
var _message_label: Label

var _origin_battery: int = -1
var _selected_energy: Array[Color] = []

func _ready() -> void:
	_message_label = get_node("UI/LayoutControl/MessageLabel")

	_batteries.resize(MAX_BATTERIES)
	for i: int in range(MAX_BATTERIES):
		var battery_node: Battery = get_node("Battery%d" % (i + 1)) as Battery
		_batteries[i] = battery_node

		battery_node.click.connect(_on_battery_click.bind(i))

	reset()

func reset() -> void:
	_message_label.text = ""
	for i: int in range(MAX_BATTERIES):
		_batteries[i].reset()

	var empty_index: int = randi() % MAX_BATTERIES

	for color: Color in ENERGY_COLORS:
		distribute_color(color, 4, empty_index)

func distribute_color(color: Color, units: int, empty_index: int) -> void:
	var distributed: int = 0

	while distributed < units:
		var target_index: int = randi() % MAX_BATTERIES

		if target_index == empty_index:
			continue

		var target: Battery = _batteries[target_index]

		if target.is_full:
			continue

		target.add_energy(color)
		distributed += 1

func _on_battery_click(index: int) -> void:
	if _origin_battery == -1:
		if _batteries[index].is_empty:
			return

		_origin_battery = index
		_batteries[index].is_selected = true
		_selected_energy = _batteries[index].get_top_energy()
	else:
		if _batteries[index].can_get_energy(_selected_energy):
			_batteries[_origin_battery].remove_energy(_selected_energy.size())

			for color: Color in _selected_energy:
				_batteries[index].add_energy(color)

			check_end_condition()

		_batteries[_origin_battery].is_selected = false
		_origin_battery = -1

func check_end_condition() -> void:
	var all_sorted: bool = true
	for battery: Battery in _batteries:
		if not (battery.is_closed or battery.is_empty):
			all_sorted = false
			break

	if all_sorted:
		_message_label.text = "All Energy Sorted : You win!"
	else:
		var has_possible_move: bool = false
		for i: int in range(MAX_BATTERIES):
			if _batteries[i].is_empty or _batteries[i].is_closed:
				continue

			var top_energy: Array[Color] = _batteries[i].get_top_energy()
			for j: int in range(MAX_BATTERIES):
				if i == j:
					continue

				if not _batteries[j].can_get_energy(top_energy):
					continue

				has_possible_move = true
				break

			if has_possible_move:
				break

		if not has_possible_move:
			_message_label.text = "No more moves : Game Over"
