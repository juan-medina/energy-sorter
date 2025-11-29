#!/usr/bin/env godot
# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

extends Area2D
class_name Battery

signal click

const MAX_ENERGY: int = 4

var _energies: Array[Color] = []
var _energy_sprites: Array[Sprite2D] = []
var _current_energy: int = 0
var _body: Sprite2D

enum State {NORMAL, HOVER, SELECTED}

var state: int = State.NORMAL

var is_full: bool:
	get: return _current_energy == MAX_ENERGY

var is_empty: bool:
	get: return _current_energy == 0

var is_closed: bool:
	get:
		if not is_full: return false
		var first_color: Color = _energies[0]
		for color: Color in _energies:
			if color != first_color:
				return false
		return true

var is_selected: bool:
	get: return state == State.SELECTED
	set(value):
		state = State.SELECTED if value else State.NORMAL
		_update_scale()

func _ready() -> void:
	_energies.resize(MAX_ENERGY)
	_energy_sprites.resize(MAX_ENERGY)

	for i: int in range(MAX_ENERGY):
		_energy_sprites[i] = get_node("Body/Energy%d" % (i + 1)) as Sprite2D

	_body = get_node("Body") as Sprite2D

	if not input_event.is_connected(_on_input_event):
		input_event.connect(_on_input_event)

	# Ensure scale matches initial state
	_update_scale()

func add_energy(color: Color) -> void:
	if _current_energy == MAX_ENERGY:
		return

	_energies[_current_energy] = color
	_energy_sprites[_current_energy].modulate = color
	_energy_sprites[_current_energy].visible = true
	_current_energy += 1

	if is_closed:
		_body.modulate = _energies[0]

func remove_energy(amount: int) -> void:
	if _current_energy - amount < 0:
		return

	for i: int in range(_current_energy - amount, _current_energy):
		_energy_sprites[i].modulate = Color.WHITE
		_energy_sprites[i].visible = false

	_current_energy -= amount

func get_top_energy() -> Array[Color]:
	var list: Array[Color] = []
	if _current_energy == 0:
		return list

	if _current_energy == 1:
		list.append(_energies[0])
		return list

	var top_color: Color = _energies[_current_energy - 1]
	var i: int = _current_energy - 1

	while i >= 0:
		if _energies[i] == top_color:
			list.append(top_color)
		else:
			break
		i -= 1

	return list

func can_get_energy(energy: Array[Color]) -> bool:
	if is_closed:
		return false

	if is_empty:
		return true

	if energy.is_empty():
		return false

	if energy.size() + _current_energy > MAX_ENERGY:
		return false

	return _energies[_current_energy - 1] == energy[0]

func reset() -> void:
	_current_energy = 0
	for sprite: Sprite2D in _energy_sprites:
		sprite.modulate = Color.WHITE
		sprite.visible = false
	_body.modulate = Color.WHITE
	state = State.NORMAL
	_update_scale()

func _on_input_event(_viewport: Viewport, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton:
		if (event as InputEventMouseButton).pressed == true:
			if is_closed:
				return
			click.emit()

func _on_mouse_entered() -> void:
	if state != State.SELECTED:
		state = State.HOVER
		_update_scale()

func _on_mouse_exited() -> void:
	if state != State.SELECTED:
		state = State.NORMAL
		_update_scale()

func _update_scale() -> void:
	if state == State.SELECTED:
		scale = Vector2.ONE * 1.5
	elif state == State.HOVER:
		scale = Vector2.ONE * 1.2
	else:
		scale = Vector2.ONE