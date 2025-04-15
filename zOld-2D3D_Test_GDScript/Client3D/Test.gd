extends Node

var scene: SceneTree
func _ready() -> void:
	scene = get_tree()
	print_children_of(scene.root)


func print_children_of(node: Node, recursive: bool = true) -> void:
	print(node)
	var node_children: Array[Node] = node.get_children()
	for child in node_children:
		print(child)
		if recursive and child.get_children().size() > 0:
			print_children_of(child, true)
