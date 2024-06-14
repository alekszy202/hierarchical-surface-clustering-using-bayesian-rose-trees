import numpy as np
import trimesh
from scipy.spatial import KDTree

def calculate_stats(original_path, new_path):
    # Load the models
    original_model = trimesh.load(original_path)
    new_model = trimesh.load(new_path)
    
    # Check if the models are loaded correctly
    if original_model.is_empty or new_model.is_empty:
        raise ValueError("One or both of the models could not be loaded.")
    
    # Calculate the number of faces and vertices
    faces_original = len(original_model.faces)
    faces_new = len(new_model.faces)
    vertices_orignal = len(original_model.vertices)
    vertices_new = len(new_model.vertices)
    
    # Calculate the loss based on the difference in the number of faces and vertices
    face_loss = abs(faces_original - faces_new) / max(faces_original, faces_new)
    vertex_loss = abs(vertices_orignal - vertices_new) / max(vertices_orignal, vertices_new)
    
    return (faces_new, vertices_new, face_loss, vertex_loss)