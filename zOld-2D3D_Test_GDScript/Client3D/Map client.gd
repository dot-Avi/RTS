extends GridMap
class_name MapClient

var camera_client: Node3D


@export var load_map: String

var map_server: MapServer

const directions: Array[int] = [
	22, # east
	10, # south
	16, # west
	0 # north
]

func _ready() -> void:
	var root: Window = get_node('/root')
	var server: Node2D
	camera_client = get_parent().get_node("CameraNode")
	server = (load(load_map) as PackedScene).instantiate()
	
	root.call_deferred('add_child', server)
	map_server = server.get_node('Map')
	
	if get_used_cells():
		clear()
	else:
		print('Already empty!')
	
	await map_server.finished_loading
	
	var all_cells: Array[Vector2i] = map_server.used_tiles() #May need to also have a Vector3i version
	
	for cell in all_cells:
		var cell_type: Vector2i = map_server.get_cell_type_dir(cell) 
		set_cell_item(map_server.get_cell_height(cell), cell_type.x, directions[cell_type.y % 4])
	
	for cell in range(all_cells.size(), 0, -1):
		fix_floating(map_server.get_cell_height(all_cells[cell - 1]))



func fix_floating(cell_check: Vector3i) -> void:
	for cell_y in range(cell_check.y, 0, -1):
		if cell_is_visibly_floating(Vector3i(cell_check.x, cell_y - 1, cell_check.z)):
			set_cell_item(Vector3i(cell_check.x, cell_y - 1, cell_check.z), 0)
		else:
			set_cell_item(Vector3i(cell_check.x, cell_y - 1, cell_check.z), -1)



func cell_is_visibly_floating(cell: Vector3i) -> bool:
	if get_cell_item(Vector3i(cell.x + 1, cell.y, cell.z)) == -1:
		return true
	if get_cell_item(Vector3i(cell.x, cell.y, cell.z + 1)) == -1:
		return true
	if get_cell_item(Vector3i(cell.x - 1, cell.y, cell.z)) == -1:
		return true
	return false



#func debug_toggle_cam() -> void:
	#if Input.is_action_just_pressed("debug_change_cam"):
		#if visible:
			#visible = false
			#debug_camera.visible = true
		#else:
			#visible = true
			#debug_camera.visible = false



func _unhandled_input(event: InputEvent) -> void:
	if event.is_action("debug_change_cam") and Input.is_action_just_pressed("debug_change_cam"):
		visible = !visible
		(map_server.get_parent() as Node2D).visible = !(map_server.get_parent() as Node2D).visible
