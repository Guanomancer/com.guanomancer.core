# Guanomancer Core Plugin for Unity 6000.4 and later.

This project is developed and maintained under an **MIT licence** which means you are welcome to use it, but **no support** will be provided by default.
The code in this project represents **basic architectural logic** and **editor tools** used by Guanomancer in internal- as well as client projects.

**Note:** I am converting the old codebase to be fully 6.4 comliant, which means switching from InstanceId to EntityId so many places, that it no longer makes sense for me to maintain a pre-6.4 version. The code in this repo represents the subset of the full plugin that I have had time and reason to update.
**Note:** Be aware that this repo is undergoing active development. Some features might not be ready for production yet, and there are no gurantees that a later commit is backwards compatible with an earlier commit.
If you find all of this interesting, look for `release` branches some time in the future.

## Current Features

### Editor

#### Button Attribute
Adds a button to an object's inspector.
```
[Button("Click Me To Do Stuff")] public void StuffDoer() { Debug.Log("I am doing stuff right now.."); }
```

#### CategoryBehaviour
Colored and uniformly formatted Hierarchy window categories.
Hierarchy context menu -> Guanomancer -> Create Category Object

### Runtime

#### SpherelsGizmo
Renders a sphere gizmo on a GameObject.

#### LineTrace
Performs a line trace and renders the result as a gizmo.

#### DocIt
DocIt and DocItMd (yes, Md for Markdown) enables asset-stored documentation where and when they are needed. Like a quick usage tip, a to-do reminder, or the documentation for this entire project (eventually) for that matter.

#### LookAt
Simple LookAt behavior for 3D GameObjects.
