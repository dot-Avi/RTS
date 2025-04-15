extends Area2D
class_name Tri


var pathable: bool = true

var triangle_visualizer: Polygon2D
var triangle: CollisionPolygon2D
var triangle_array: Array[Vector2]
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	triangle_array.append(Vector2(0, 0))
	triangle_array.append(Vector2(20, 20))
	triangle_array.append(Vector2(0, 0))
	
	triangle = CollisionPolygon2D.new()
	add_child(triangle)
	
	triangle_visualizer = Polygon2D.new()
	add_child(triangle_visualizer)
	
	triangle.set_polygon(triangle_array)
	triangle_visualizer.set_polygon(triangle_array)
	triangle_visualizer.set_color(Color(0.341, 0.153, 0.729, 0.4))



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	var _moni: float = 0 * delta
	pass



func reposition_vertex(vertex: int, new_position: Vector2i) -> void:
	var vertex_new_pos: Vector2 = Vector2(new_position)
	triangle.polygon[vertex] = vertex_new_pos
	triangle_visualizer.polygon[vertex] = vertex_new_pos
