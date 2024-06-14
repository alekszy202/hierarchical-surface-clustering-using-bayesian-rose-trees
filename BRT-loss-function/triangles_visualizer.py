import trimesh
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

# Wczytanie modelu z pliku .obj
file_path = 'Assets\Suzanne.obj'
mesh = trimesh.load(file_path)

# Pobranie wierzchołków i trójkątów z modelu
vertices = mesh.vertices
faces = mesh.faces

# Obliczenie centrum każdego trójkąta
face_centers = vertices[faces].mean(axis=1)

# Wizualizacja modelu z ID trójkątów
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Rysowanie trójkątów
for i, face in enumerate(faces):
    triangle = vertices[face]

    center = face_centers[i]
    ax.text(center[0], center[1], center[2], str(i), color='red', zorder=10)  # Zwiększenie zorder

    poly = Poly3DCollection([triangle], alpha=0.3, edgecolor='k')
    ax.add_collection3d(poly)


# Ustawienie granic osi na podstawie zakresu współrzędnych modelu
ax.set_xlim([vertices[:, 0].min(), vertices[:, 0].max()])
ax.set_ylim([vertices[:, 1].min(), vertices[:, 1].max()])
ax.set_zlim([vertices[:, 2].min(), vertices[:, 2].max()])

# Ustawienie początkowego widoku kamery (opcjonalnie można dostosować kąt widzenia)
ax.view_init(elev=20, azim=30)

# Ustawienie etykiet osi
ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')

plt.show()
