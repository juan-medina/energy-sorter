#!/usr/bin/env godot
# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

extends Node2D

const ENERGY_COLORS: Array[Color] = [
	Color8(31, 119, 180), # #1F77B4 blue
	Color8(255, 127, 14), # #FF7F0E orange
	Color8(44, 160, 44), # #2CA02C green
	Color8(214, 39, 40), # #D62728 red
	Color8(148, 103, 189), # #9467BD purple
	Color8(140, 86, 75), # #8C564B brown
	Color8(227, 119, 194), # #E377C2 pink
	Color8(255, 215, 0), # #FFD700 gold
	Color8(0, 128, 128), # #008080 teal
	Color8(23, 190, 207) # #17BECF cyan
]
const MAX_BATTERIES: int = 8

var max_colors_used: int = MAX_BATTERIES - 2

var _batteries: Array[Battery] = []
@onready var _message_label: Label = $UI/LayoutControl/MessageLabel
@onready var _battery_nodes: Array[Battery] = [
    $Battery1, $Battery2, $Battery3, $Battery4, $Battery5, $Battery6, $Battery7, $Battery8
]

var _origin_battery: int = -1
var _selected_energy: Array[Color] = []
var _game_over: bool = false
var _rng: RandomNumberGenerator = RandomNumberGenerator.new()

var _saved_batteries_state: Array = []

func _ready() -> void:
	_batteries.resize(MAX_BATTERIES)
	for i: int in range(MAX_BATTERIES):
		var battery_node: Battery = _battery_nodes[i]
		_batteries[i] = battery_node
		battery_node.clicked.connect(_on_battery_clicked.bind(i))

	_rng.randomize()
	_new_game()

func _new_game() -> void:
	_message_label.text = ""
	_game_over = false
	for i: int in range(MAX_BATTERIES):
		_batteries[i].reset()

	var empty_index: int = _rng.randi_range(0, MAX_BATTERIES - 1)
	var colors_to_use: int = clamp(max_colors_used, 1, ENERGY_COLORS.size())

	var selected_indices: Array = []
	while selected_indices.size() < colors_to_use:
		var idx: int = _rng.randi_range(0, ENERGY_COLORS.size() - 1)
		if not idx in selected_indices:
			selected_indices.append(idx)

	for idx: int in selected_indices:
		var color: Color = ENERGY_COLORS[idx]
		_distribute_color(color, 4, empty_index)

	for battery: Battery in _batteries:
		_saved_batteries_state.append(battery._energies.duplicate())

func _reset_game() -> void:
	_message_label.text = ""
	_game_over = false
	for i: int in range(MAX_BATTERIES):
		var battery: Battery = _batteries[i]
		battery.reset()
		var saved_energies: Array[Color] = _saved_batteries_state[i]
		for color: Color in saved_energies:
			if color != Color.BLACK:
				battery.add_energy(color)

func _distribute_color(color: Color, units: int, empty_index: int) -> void:
	var distributed: int = 0

	while distributed < units:
		var target_index: int = _rng.randi_range(0, MAX_BATTERIES - 1)

		if target_index == empty_index:
			continue

		var target: Battery = _batteries[target_index]

		if target.is_full:
			continue

		target.add_energy(color)
		distributed += 1

func _on_battery_clicked(index: int) -> void:
	if _origin_battery == -1:
		if _batteries[index].is_empty:
			return

		_origin_battery = index
		_batteries[index].is_selected = true
		_selected_energy = _batteries[index].get_top_energies()
	else:
		if _batteries[index].can_get_energy(_selected_energy):
			_batteries[_origin_battery].remove_energy(_selected_energy.size())

			for color: Color in _selected_energy:
				_batteries[index].add_energy(color)

			_check_end_condition()

		_batteries[_origin_battery].is_selected = false
		_origin_battery = -1

func _check_end_condition() -> void:
	var all_sorted: bool = true
	for battery: Battery in _batteries:
		if not (battery.is_closed or battery.is_empty):
			all_sorted = false
			break

	if all_sorted:
		_message_label.text = &"All Energy Sorted : You win!"
		_game_over = true
		for b: Battery in _batteries:
			b.accepts_input = false
	else:
		var has_possible_move: bool = false
		for i: int in range(MAX_BATTERIES):
			if _batteries[i].is_empty or _batteries[i].is_closed:
				continue

			var top_energy: Array[Color] = _batteries[i].get_top_energies()
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
			_message_label.text = &"No more moves : Game Over"
			_game_over = true
			for b: Battery in _batteries:
				b.accepts_input = false

func _on_new_button_button_up() -> void:
	_new_game()


func _on_reset_button_button_up() -> void:
	_reset_game()
