import numpy as np
import trimesh
from scipy.spatial.distance import directed_hausdorff

def calculate_hausdorff_loss(model1_path, model2_path):
    """
    Calculate the loss between two 3D models based on their detail level using Hausdorff distance.

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
    
    # Calculate the Hausdorff distance between the vertices of the two models
    hausdorff_distance = max(directed_hausdorff(model1.vertices, model2.vertices)[0],
                             directed_hausdorff(model2.vertices, model1.vertices)[0])
    
    # Combine the losses
    total_loss = face_loss + vertex_loss + hausdorff_distance
    
    return total_loss