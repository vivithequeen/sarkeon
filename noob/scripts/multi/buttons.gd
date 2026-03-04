extends Button

@export var scene_to: PackedScene
@export var exit: bool
@export var main_menu: bool
var main_menu_node = preload("res://noob/scenes/main_menu.tscn")

func _ready() -> void:
	pressed.connect(button_presseds)

func button_presseds() -> void:
	if exit: get_tree().quit(); return;
	if main_menu: get_tree().change_scene_to_packed(main_menu_node)
	if scene_to: get_tree().change_scene_to_packed(scene_to)
