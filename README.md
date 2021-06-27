# DissUnityProjs

## Repository contents

This repository consists of three Unity 2019 projects.

NewGlassesClient - An Android project which runs on the glasses. Receives object recognition information.

ZTrackNet - A project to be run on a PC with the Zed 2 camera connected. Sends the results of the object recognition to other projects.

ZMimic - A project to be run on PC. Mimics the output of a Zed 2 camera, but is less intensive on the PC. Sends the information to other projects.

## Required equipment

* NReal AR Glasses
* Laptop with compatible graphics card (https://www.stereolabs.com/docs/installation/specifications/) and USB 3 port
* Device to create a local network (such as a smartphone hotspot)
* Zed 2 camera


## Initial Setup

### Adding the project to the NReal glasses

Connect the NReal computing unit to the PC using a USB cable. 

Open the NewGlassesClient project in Unity 2019 and select build and run, with the device set to the computing unit.

### Setting up the network

Start the local network (such as a phone hotspot).

With the NReal computing unit still connected to the laptop, connect them to the network. (Guide : https://developer.nreal.ai/develop/unity/android-quickstart)

Connect the laptop to the network as well. The computing unit can now be disconnected from the laptop.

Disable the laptops firewall. The glasses will not be able to connect if it is left on.

**Currently the IPs are hardcoded so you will need to edit the client file to set it to whatever the IP of the laptop is.**

### Running the object recognition

#### With Zed 2 camera

Connect the Zed 2 camera to the laptop using the USB cable. It requires a USB 3 port on the laptop.

Open the ZTrackNet project in Unity. Run the project, it may take a few seconds to start.

#### Without Zed 2 camera

Open the ZMimic project in Unity.

Ensure that the Host object is enabled and that the Client object is disabled.

Run the project.

### Using the application

Connect the NReal glasses to the computing unit.

Use the controller to select and start the project.
