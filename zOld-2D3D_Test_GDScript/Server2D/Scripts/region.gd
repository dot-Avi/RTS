extends Polygon2D
class_name RegionGDScript

var lines: Dictionary = {}
var tiles: Array[Vector2i]
var map: MapServer

func _ready() -> void:
	map = get_parent() as MapServer
	color = Color(0.341, 0.153, 0.729, 0.4)
