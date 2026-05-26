# AR Prosthetic Visualization System

An AR-based prosthetic visualization and customization application developed using **Unity**, **AR Foundation**, and **ARKit Body Tracking**.
The system allows users to preview customizable prosthetic arms in real-time AR before fabrication.

---

# Features

## AR Prosthetic Try-On

* Real-time prosthetic overlay using AR body tracking
* Prosthetic follows the user's arm movement
* Residual limb alignment for amputee visualization
* Overlay mode for able-bodied users

## Prosthetic Customization

* Multiple prosthetic model options
* Material customization:

  * Carbon Fiber
  * Matte
  * Metallic
* Color customization
* Real-time 3D model preview
* Rotate and zoom interaction similar to Sketchfab

## Cost & Time Estimation

* Estimated fabrication cost
* Estimated production time
* Dynamic updates based on selected customization

## IMU Integration

* ESP32 + MPU6050 based IMU support
* Calibration workflow
* BLE integration planned for native iOS deployment

---

# Tech Stack

* Unity 2022.3 LTS
* AR Foundation 5.2.2
* ARKit Body Tracking
* C#
* ESP32
* MPU6050
* iOS (Target Platform)

---

# Current Scope

## Implemented

* Scene flow architecture
* AR body tracking setup
* Prosthetic attachment system
* Prosthetic model preview system
* Customization workflow
* IMU calibration workflow

## Planned / In Progress

* Native BLE bridge for iOS
* Final AR alignment tuning
* Advanced material customization polish

---

# Application Flow

```text
TitleScene
   ↓
MainMenuScene
   ↓
ModesScene
   ↓
LevelSelectionScene
   ↓
CustomizationScene
   ↓
IMUConnectionScene
   ↓
CalibrationScene
   ↓
ARTryOnScene
```

---

# Modes

## Amputee Mode

Designed for amputee visualization where the prosthetic appears attached to the residual limb.

## Able-Bodied Mode

Designed for overlay visualization on an intact arm for demonstration and testing.

---

# AR Functionality

## Below Elbow

* Prosthetic forearm attachment
* Real-time follow mode
* Residual limb connection simulation

## Above Elbow

* Full prosthetic arm visualization
* Shoulder-to-hand prosthetic attachment
* Real-time body tracking

---

# Hardware Used

| Component      | Purpose           |
| -------------- | ----------------- |
| ESP32          | BLE communication |
| MPU6050        | Motion sensing    |
| TP4056         | Battery charging  |
| Li-ion Battery | Portable power    |

---

# Project Structure

```text
Assets/
 ├ _Project/
 │   ├ Animations/
 │   ├ Materials/
 │   ├ Prefabs/
 │   ├ Scenes/
 │   ├ Scripts/
 │   └ ScriptableObjects/
```

---

# Setup Instructions

## Unity Packages

Install:

* AR Foundation 5.2.2
* ARKit XR Plugin
* XR Plugin Management

## Build Settings

* Platform: iOS
* Architecture: ARM64
* Camera usage description enabled

## AR Requirements

* iPhone with ARKit support
* iOS 13 or later

---

# Future Improvements

* Native BLE communication
* Gesture-controlled prosthetic animations
* Additional prosthetic models
* Cloud-based prosthetic library
* AI-based fitting recommendations

---

# IEEE Research Paper

The research paper associated with this project can be accessed here:

[IEEE Paper Link](https://ieeexplore.ieee.org/document/11308017)

---

# Credits

## Prosthetic Models

Some prosthetic models used in this project are sourced from:

* [Sketchfab](https://sketchfab.com?utm_source=chatgpt.com)

Please refer to the respective model authors for original assets and licensing information.

## AR Foundation Samples

AR body tracking implementation references:

* [Unity AR Foundation Samples](https://github.com/Unity-Technologies/arfoundation-samples?utm_source=chatgpt.com)

---

# License

This project is intended for educational and research purposes.
