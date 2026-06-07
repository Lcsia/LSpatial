==================================================
LCSIA SDK
==================================================

LCSIA is a Unity SDK for creating educational,
research, simulation, and virtual reality
applications without requiring advanced
programming knowledge.

The SDK provides ready-to-use components for:

- Character movement
- Avatar animation
- Climbing mechanics
- Interactive objects
- Teleportation
- Notifications
- Data collection
- Player utilities

==================================================
PLAYER SYSTEM
==================================================

LCSIA provides two player types:

1. Simple Player
2. Avatar Player

Simple Player

A capsule-based controller with:

- WASD movement
- Run (Shift)
- Jump (Space)
- First-person camera
- Third-person camera
- Climbing support

Avatar Player

Includes everything from Simple Player plus:

- Animator integration
- Walking animations
- Running animations
- Jump animations
- Climbing animations

==================================================
LCSIAPlayer
==================================================

LCSIAPlayer is a static helper class that allows
access to the player from any script.

Get Player Position

Vector3 position =
    LCSIAPlayer.GetPosition();

Teleport Player

LCSIAPlayer.Teleport(
    0,
    1,
    0);

Rotate Player

LCSIAPlayer.SetRotation(
    new Vector3(
        0,
        180,
        0));

Get Distance

float distance =
    LCSIAPlayer.GetDistance(
        target.position);

Check Grounded State

bool grounded =
    LCSIAPlayer.IsGrounded();

Get Looked Object

GameObject obj =
    LCSIAPlayer.GetLookObject();

==================================================
LCSIANotification
==================================================

Displays notifications on screen.

Show Message

LCSIANotification.Show(
    "Welcome");

Show Message For 5 Seconds

LCSIANotification.Show(
    "Experiment Started",
    5f);

Clear All Messages

LCSIANotification.Clear();

==================================================
LCSIATrigger
==================================================

Creates trigger zones that execute UnityEvents.

Events:

- On Enter
- On Exit
- On Key Press

Example:

Player enters area
→ Show notification

Player presses E
→ Open door

==================================================
LCSIATeleporter
==================================================

Teleports the player to a destination.

Example:

Portal A
→ Portal B

Room 1
→ Room 2

Checkpoint
→ Spawn Area

Events:

- On Enter

==================================================
LCSIAPointOfInterest
==================================================

Interactive information point.

Features:

- Title
- Description
- Interaction Key
- Unity Events

Example:

Museum exhibit

Player approaches
→ Information appears

Player presses E
→ Play audio

==================================================
LCSIAClimbable
==================================================

Makes any object climbable.

Example:

Wall
Ladder
Rock Face
Tree

Properties:

- Climb Speed
- Minimum Climb Angle

==================================================
LCSIABillboard
==================================================

Makes an object always face the camera.

Useful for:

- Labels
- Name Tags
- Floating UI
- Markers

==================================================
LCSIASaveData
==================================================

Sends data from Unity to a remote server.

Configuration:

URL
Server endpoint

ID
Experiment identifier

Example:

ID = TEST

Automatically generates:

TEST_300526_458320.csv

Send Data

saveData.Send(
    "Hello World");

Send CSV Row

saveData.Send(
    "P001,25,120");

Typical Experiment

saveData.Send(
    "Participant,Age,Score");

saveData.Send(
    "P001,25,120");

saveData.Send(
    "P002,24,115");

==================================================
AVATAR ANIMATION SYSTEM
==================================================

The Avatar Player automatically updates the
Animator.

Supported Parameters:

Speed
Running
Grounded
Climbing
ClimbDirection
Jump

Supported States:

Idle
Walk
Run
Jump
JumpHold
Climb

No scripting required.

==================================================
CURRENT COMPONENTS
==================================================

LCSIAPlayerController

Basic character controller.

--------------------------------------------------

LCSIAAvatarPlayerController

Character controller with avatar support.

--------------------------------------------------

LCSIAPlayerAvatar

Animator controller.

--------------------------------------------------

LCSIAPlayer

Static player utility API.

--------------------------------------------------

LCSIANotification

Notification system.

--------------------------------------------------

LCSIATrigger

Trigger events.

--------------------------------------------------

LCSIATeleporter

Teleportation system.

--------------------------------------------------

LCSIAPointOfInterest

Interactive information points.

--------------------------------------------------

LCSIAClimbable

Climbable surfaces.

--------------------------------------------------

LCSIABillboard

Camera-facing objects.

--------------------------------------------------

LCSIASaveData

Remote data collection.

==================================================
DESIGN PHILOSOPHY
==================================================

LCSIA is designed around simplicity.

Most functionality can be used directly from
the Unity Inspector without writing code.

When scripting is required, the API aims to be
minimal and easy to learn.

Example:

LCSIANotification.Show(
    "Hello");

LCSIAPlayer.Teleport(
    0,
    1,
    0);

saveData.Send(
    "Participant,Score");

These commands represent the core philosophy
of LCSIA:

Simple.
Readable.
Reusable.