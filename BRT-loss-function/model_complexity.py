import trimesh
import numpy as np

def calculate_vertex_curvature(mesh):
    vertices = np.array(mesh.vertices)
    faces = np.array(mesh.faces)
    
    # Obliczanie krzywizny wierzchołków na podstawie sąsiadujących wierzchołków
    vertex_normals = mesh.vertex_normals
    curvature = np.zeros(len(vertices))
    
    for i in range(len(vertices)):
        # Znajdź wszystkie trójkąty, które zawierają wierzchołek i
        adjacent_faces = np.any(faces == i, axis=1)
        adjacent_vertices = faces[adjacent_faces].flatten()
        
        # Usuń bieżący wierzchołek z listy sąsiadów
        adjacent_vertices = adjacent_vertices[adjacent_vertices != i]
        adjacent_vertices = np.unique(adjacent_vertices)
        
        # Oblicz średnią różnicę normalnych
        normal_diffs = vertex_normals[adjacent_vertices] - vertex_normals[i]
        curvature[i] = np.linalg.norm(normal_diffs, axis=1).mean()
    
    average_curvature = np.mean(curvature)
    return average_curvature

def calculate_model_complexity(mesh):
    vertex_complexity = len(mesh.vertices)
    triangle_complexity = len(mesh.faces)
    
    # Gęstość siatki
    detail_complexity = triangle_complexity / vertex_complexity if vertex_complexity > 0 else 0
    
    # Krzywizna siatki
    curvature_complexity = calculate_vertex_curvature(mesh)
    
    # Możesz dostosować wagę poszczególnych komponentów złożoności
    total_complexity = vertex_complexity + triangle_complexity + detail_complexity + curvature_complexity
    
    return {
        'vertex_complexity': vertex_complexity,
        'triangle_complexity': triangle_complexity,
        'detail_complexity': detail_complexity,
        'curvature_complexity': curvature_complexity,
        'total_complexity': total_complexity
    }