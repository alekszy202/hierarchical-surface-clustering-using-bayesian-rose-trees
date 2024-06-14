import numpy as np
import trimesh
from scipy.spatial import KDTree

def calculate_loss(model1_path, model2_path):
    """
    Calculate the loss between two 3D models based on their detail level.

    Parameters:
    model1_path (str): Path to the first 3D model file.
    model2_path (str): Path to the second 3D model file.

    Returns:
    float: Loss value representing the difference in detail level.
    """
    # Load the models
    model1 = trimesh.load(model1_path)
    model2 = trimesh.load(model2_path)
    
    # Check if the models are loaded correctly
    if model1.is_empty or model2.is_empty:
        raise ValueError("One or both of the models could not be loaded.")
    
    # Calculate the number of faces and vertices
    faces1 = len(model1.faces)
    faces2 = len(model2.faces)
    vertices1 = len(model1.vertices)
    vertices2 = len(model2.vertices)
    
    # Calculate the loss based on the difference in the number of faces and vertices
    face_loss = abs(faces1 - faces2) / max(faces1, faces2)
    vertex_loss = abs(vertices1 - vertices2) / max(vertices1, vertices2)
    
    # Calculate geometric difference using KDTree for nearest neighbor distances
    tree1 = KDTree(model1.vertices)
    tree2 = KDTree(model2.vertices)
    
    distances1, _ = tree1.query(model2.vertices)
    distances2, _ = tree2.query(model1.vertices)
    
    geometric_loss = (np.mean(distances1) + np.mean(distances2)) / 2
    
    # Combine the losses
    total_loss = face_loss + vertex_loss + geometric_loss
    
    return total_loss