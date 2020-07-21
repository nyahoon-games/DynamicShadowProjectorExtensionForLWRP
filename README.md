# DynamicShadowProjectorExtensionForLWRP

This extension enables [Dynamic Shadow Projector](https://nyahoon.com/products/dynamic-shadow-projector) to work under Universal RP.

## Required assets
1. [Dynamic Shadow Projector](https://nyahoon.com/products/dynamic-shadow-projector)
2. [ProjectorForLWRP](https://github.com/nyahoon-games/ProjectorForLWRP)

## Branches
| Branch name | Description |
|:---|:---|
| master | A branch for Lightweight Render Pipeline (Unity 2019.2 or below). |
| master-universalrp | A branch for Universal Render Pipeline (Unity 2019.3 or higher). |

## Install
Clone (or submodule add) `master-universalrp` branch into the Assets folder in your Unity Project.

### Clone:
	cd Pass-to-Your-Unity-Project/Assets
	git clone -b master-universalrp https://github.com/nyahoon-games/DynamicShadowProjectorExtensionForLWRP.git

### Submodule Add:
	cd Pass-to-Your-Unity-Project
	git submodule add -b master-universalrp https://github.com/nyahoon-games/DynamicShadowProjectorExtensionForLWRP.git Assets/DynamicShadowProjectorExtensionForLWRP
