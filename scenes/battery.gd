#!/usr/bin/env godot
# SPDX-FileCopyrightText: 2025 Juan Medina
# SPDX-License-Identifier: MIT

extends Area2D
class_name Battery

signal clicked

const MAX_ENERGY: int = 4

@onready var _body: Sprite2D = $Body
var _energy_sprites: Array[Sprite2D] = []

var _state: BatteryState = BatteryState.new()
var _saved_state : BatteryState = BatteryState.new()

var is_full: bool:
    get: return _state.current_energy == MAX_ENERGY

var is_empty: bool:
    get: return _state.current_energy == 0

var is_closed: bool:
    get:
        if not is_full:
            return false
        var first_color: Color = _state.energies[0]
        for color: Color in _state.energies:
            if color != first_color:
                return false
        return true

var is_selected: bool:
    get: return _state.status == BatteryState.Status.SELECTED
    set(value):
        _state.status = BatteryState.Status.SELECTED if value else BatteryState.Status.NORMAL
        _update_scale()

func _ready() -> void:
    _state.energies.resize(MAX_ENERGY)
    _energy_sprites.resize(MAX_ENERGY)
    for i: int in range(MAX_ENERGY):
        _energy_sprites[i] = _body.get_node("Energy%d" % (i + 1)) as Sprite2D
    if not input_event.is_connected(_on_input_event):
        input_event.connect(_on_input_event)
    _update_scale()

func add_energy(color: Color) -> void:
    if _state.current_energy == MAX_ENERGY:
        return
    _state.energies[_state.current_energy] = color
    _energy_sprites[_state.current_energy].modulate = color
    _energy_sprites[_state.current_energy].visible = true
    _state.current_energy += 1
    if is_closed:
        _body.modulate = _state.energies[0]
    else:
        _body.modulate = Color.WHITE

func remove_energy(amount: int) -> void:
    if _state.current_energy - amount < 0:
        return
    for i: int in range(_state.current_energy - amount, _state.current_energy):
        _energy_sprites[i].modulate = Color.WHITE
        _energy_sprites[i].visible = false
    _state.current_energy -= amount

func get_top_energies() -> Array[Color]:
    var list: Array[Color] = []
    if _state.current_energy == 0:
        return list
    if _state.current_energy == 1:
        list.append(_state.energies[0])
        return list
    var top_color: Color = _state.energies[_state.current_energy - 1]
    var i: int = _state.current_energy - 1
    while i >= 0:
        if _state.energies[i] == top_color:
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
    if energy.size() + _state.current_energy > MAX_ENERGY:
        return false
    return _state.energies[_state.current_energy - 1] == energy[0]

func reset() -> void:
    _state.current_energy = 0
    for sprite: Sprite2D in _energy_sprites:
        sprite.modulate = Color.WHITE
        sprite.visible = false
    for idx: int in range(_state.energies.size()):
        _state.energies[idx] = Color.BLACK
    _body.modulate = Color.WHITE
    _state.status = BatteryState.Status.NORMAL
    _state.accepts_input = true
    _update_scale()

func _on_input_event(_viewport: Viewport, event: InputEvent, _shape_idx: int) -> void:
    if event is InputEventMouseButton:
        var mb: InputEventMouseButton = event as InputEventMouseButton
        if mb.pressed == true:
            if not _state.accepts_input:
                return
            if is_closed:
                return
            clicked.emit()

func _on_mouse_entered() -> void:
    if not _state.accepts_input:
        return
    if _state.status != BatteryState.Status.SELECTED:
        _state.status = BatteryState.Status.HOVER
        _update_scale()

func _on_mouse_exited() -> void:
    if not _state.accepts_input:
        return
    if _state.status != BatteryState.Status.SELECTED:
        _state.status = BatteryState.Status.NORMAL
        _update_scale()

var accepts_input: bool:
    get: return _state.accepts_input
    set(value):
        _state.accepts_input = value
        if not value:
            if _state.status == BatteryState.Status.HOVER:
                _state.status = BatteryState.Status.NORMAL
                _update_scale()

func _update_scale() -> void:
    if _state.status == BatteryState.Status.SELECTED:
        scale = Vector2.ONE * 1.5
    elif _state.status == BatteryState.Status.HOVER:
        scale = Vector2.ONE * 1.2
    else:
        scale = Vector2.ONE

func save_state() -> void:
    _saved_state = _state.duplicate()

func load_state() -> void:
    reset()
    _state = _saved_state.duplicate()
    _update_scale()
    for i: int in range(MAX_ENERGY):
        if i < _state.current_energy:
            _energy_sprites[i].modulate = _state.energies[i]
            _energy_sprites[i].visible = true
        else:
            _energy_sprites[i].modulate = Color.WHITE
            _energy_sprites[i].visible = false
    if is_closed:
        _body.modulate = _state.energies[0]
    else:
        _body.modulate = Color.WHITE
