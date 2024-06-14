import process_file as pf
import model_complexity as mc
import trimesh
import os

if __name__ == '__main__':
    original_model_path = "Assets\\bunny.obj"
    folder_path = "Assets\Bunny_2"
    
    #original_model = trimesh.load_mesh(original_model_path)
    
    #complexity = mc.calculate_model_complexity(original_model)
    #print(complexity)
    
    for filename in os.listdir(folder_path):
        filepath = os.path.join(folder_path, filename)
        if(os.path.isfile(filepath)):
            pf.process_file(filepath, original_model_path)
