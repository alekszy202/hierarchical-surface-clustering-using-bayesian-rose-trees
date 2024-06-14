<h1 align="center">Hierarchical surface clustering using Bayesian Rose Trees</h1>

<p align="center">
  <a href="#overview">Overview</a> •
  <a href="#project-structure">Project structure</a> •
  <a href="#getting-started">Getting started</a> •
  <a href="#usage">Usage</a> •
  <a href="#license">License</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/License-MIT-yellow.svg" />
  <img src="https://img.shields.io/badge/Author-Aleksandra Szymczak-blue" />
</p>


## Overview
This repository contains the implementation for a master's thesis project titled "Hierarchical Surface Clustering using Bayesian Rose Trees." The objective of this project is to verify the hypothesis that the Bayesian Rose Trees (BRT) algorithm can effectively cluster surfaces within 3D models. The key research focus is on developing a solution that simplifies a 3D object based on the hierarchy obtained through the clustering process. This project aims to address questions raised in prior research conducted during the author's studies.


## Project Structure
The repository is divided into two main projects:

### C# Implementation
The first project is a comprehensive C# application that encompasses the entire clustering process. This project includes:

- **OBJ File Handling:** Reading from and writing to OBJ files.
- **Mesh Representation:** Efficient data structures for representing 3D meshes.
- **BRT Algorithm Implementation:** A full implementation of the Bayesian Rose Trees algorithm, enhanced for operating on 3D meshes.
- **Mesh Reconstruction Algorithm:** Reconstructing the mesh from the clustering results provided by the BRT algorithm using Ear Clipping algorithm.

### Python Testing Framework
The second project, written in Python, is dedicated to comparative quality testing of the mesh simplification results obtained from the C# project and other sources. This project includes:

- **Evaluation Metrics:** Implementation of the One-sided Hausdorff measure and a custom error measure for evaluating the quality of mesh simplification.
- **Benchmarking Scripts:** Scripts to automate the comparison of different mesh simplification techniques.


## Getting Started
### Prerequisites
- .NET 8.0 (For C# project)
- Python 3.10 (For python project)
  
### Installation
1. **Clone the Repository:**
   ```
   git clone https://github.com/yourusername/Bayesian-Rose-Trees-Clustering.git
   ```
   
2. **Setup C# Project:**
  - Open the C# project in Visual Studio.
  - Build the solution to restore dependencies and compile the project.

3. **Setup Python Testing Framework:**
- Navigate to the Python project directory.
- Install the required packages:
  
  ```
  python -m venv venv
  venv\Scripts\activate
  pip install -r requirements.txt
  ```


## Usage
### Running the C# Project
1. Enter file path and clustering parameters.
2. Execute the BRT clustering process.
3. Enter the clusterisation level in the console.
4. Enjoy the generated mesh.

### Running the Python Tests
1. Prepare the meshes to be evaluated.
2. Use the provided scripts to compare the simplification results using the One-sided Hausdorff measure and the custom error measure.
3. Analyze the output metrics to assess the quality of the mesh simplification.


## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
&copy; 2024 Aleksandra Szymczak. All rights reserved.

<p align="left">
  <a href="https://github.com/alekszy202">GitHub</a> •
  <a href="https://www.linkedin.com/in/alekszy202/">LinkedIn</a>
</p>
