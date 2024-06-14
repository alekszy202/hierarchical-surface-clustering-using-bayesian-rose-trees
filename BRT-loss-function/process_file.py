import trimesh_loss as tl
import hausdorff_loss as hl
import stats as stat
import re

def extract_level(filename):
    return filename

def process_file(filepath, original_model_path):
    print('=============================')
    level = extract_level(filepath)
    print(f'{level}')
    
    (faces_new, vertices_new, face_loss, vertex_loss) = stat.calculate_stats(original_model_path, filepath)
    face_loss = round(face_loss, 4)
    vertex_loss = round(vertex_loss, 4)
    print(f'Vertex: {vertices_new}')
    print(f'Face: {faces_new}')
    print(f'Vertex avg loss: {vertex_loss}')
    print(f'Face avg loss: {face_loss}')
    
    
    
    loss = tl.calculate_loss(original_model_path, filepath)
    loss = round(loss, 4)
    print(f'Loss: {loss}')
    
    loss = hl.calculate_hausdorff_loss(original_model_path, filepath)
    loss = round(loss, 4)
    print(f'Hausdorff Loss: {loss}')