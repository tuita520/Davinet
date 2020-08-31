## Davinet development is currently on hold as we explore networking options.

Davinet is a (work in progress) proof of concept for a Unity multiplayer networking framework (*it is **not** remotely production ready*). Its primary goal is to allow players to interact with physics objects with the same level of responsiveness and precision as they'd have playing offline.
It is designed with the upcoming [asymmetrical VR vs PC game Davigo](https://www.youtube.com/watch?v=Eoxb8ALHkZA) in mind. This proof of concept is created to both survey if there is interest in a community driven networking package of this type and as a request for feedback on the general architecture.

[![Image demonstrating multiple players rolling around an level, pushing large numbers of rigidbodies.](https://i.imgur.com/Es8218L.png)](https://www.youtube.com/watch?v=rhJAC_x3KQE)
*(Click the image for YouTube video)*

It has architecture goals to be a minimalist package with loose coupling, extensibility and encapsulation of netcode. A thread detailing the reasoning behind the package [can be found on the Unity forums](https://forum.unity.com/threads/community-project-for-physics-based-games-and-networking-vr-use-case.886756/). It is based on the excellent [State Synchronization](https://gafferongames.com/post/state_synchronization/) and [Networked Physics](https://gafferongames.com/post/networked_physics_in_virtual_reality/) articles by Glenn Fiedler (Gaffer On Games).

Davinet currently uses [LiteNetLib](https://github.com/RevenantX/LiteNetLib) for its transport layer.

# Current status
**Working towards proof of concept. See below for feature goals in detail**. Works well in LAN. Distributed authority system needs to be completed to avoid conflicts that arise from environments with latency. This is noticeable when using the latency simulator.

# Getting started
This repository contains a sample project that can be found in `Sample/PhysicsSample`. To run this scene, you can make a build and launch multiple instances of the executable. Alternatively, Davinet can create a **Junction Unity Project** to allow you to open the project in a second Unity Editor instance for easier testing. Unity does not normally allow opening a project in multiple Editor instances. To workaround this, Davinet will create a dummy project and create [junctions](https://docs.microsoft.com/en-us/windows/win32/fileio/hard-links-and-junctions) to `Assets`, `ProjectSettings` and `Packages`. This tool can be found in Unity in the Davinet menu (Windows only).

When running the project
1. Left click on the screen to move your object towards the mouse.
2. Press `T` to transform between a cube and a sphere.
3. Press 1 or 2 to switch powers.
4. When using power 2, right click on an object to grab it, and right click again to throw it.

To get started understanding the sample project, begin with the `Menu.cs` script.

# Overview
Davinet uses a state synchronization client-server model with a distributed authority scheme. All data flows through the server, but authority over objects is distributed among the peers. This allows for maximum responsiveness on the clients, with the tradeoff that Davinet does **not** have an authoritative server. Davinet allows for the server and client to run in the same game instance (clients of this type are called **listenClients**).

## State synchronization
Davinet sends all data about each objects state over the network. For a rigidbody, this would be its position, rotation, velocity and angular velocity. This allows peers to continously extrapolate the state of the simulation, with errors being corrected by the server.

## Distributed authority
*Work in progress...*

## Stateful world abstraction layer
To avoid having to code directly against the networking layer, gameplay logic talks to an abstraction layer called the **Stateful World**. Every game object that is part of the gameplay logic **must** do the following.
1. Have the `StatefulObject` and `OwnableObject` components attached to it.
2. Added to world using `StatefulWorld.Add`.
2. Components with frequent state updates (position, rotation) should implement the `IStreamable` interface (see `StatefulRigidbody.cs`).
3. Components with infrequent state updates (is the character in the cube or sphere shape? See `CubeSphereTransformer.cs`) can contain properties that implement `IStateField`. A generic class, as well as some premade common types, can be found in the `StateField` directory. These properties are gathered on `Awake` by `StatefulObject` using Reflection.

The network will handle serializing spawns, `IStateField` and `IStreamable` state changes.

# Contributing

Contributions are most welcome, and can either take the form of pull requests or direct feedback (the Unity thread is the best place for discussion!). To find areas that need improvement, you can search for comments marked `// TODO:`. If you're interested in frequently contributing, feel free to get in touch with the repo owner ([me, Roystan/Iron-Warrior](https://github.com/IronWarrior)). 

**The following needs to be completed for a fully functional prototype.**

1. ***Essential*** Complete distributed authority scheme (functions correctly with client, server and listen client).
2. Ability to remove objects from the stateful world.
3. Ability for clients to spawn objects in the world, and have this spawn propagate to the server and other peers.
4. Peers can send arbitrary data messages (chat would be a good test for this feature, username, desired color).
5. Input serialization functions correctly (right now only movement input is serialized to avoid state updates from being applied twice).
6. `StatefulObjects` should be permitted to have as many `IStreamable` components as they wish (right now it is limited to one).
7. Options to deal with packets arriving late, in bunches or out of order.
    * ***Work in progress*** Jitter buffer (outlined in the State Synchronization article above).
    * Discard out of date state updates.
8. Simple bandwidth optimization strategy (priority accumulator, as described in the State Synchronization article above, would be a good place to start). Currently there is no optimization, with the demo project hitting about 20-30mb/s.
