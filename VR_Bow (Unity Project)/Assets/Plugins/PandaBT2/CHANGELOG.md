# Panda BT - Change log

## Beta 2022.11.28

### Core

- Built-in `[PandaTasks]` are now defined on a prefab located at `PandaBT2/Config/Resources/PandaBTSettings.prefab`. You can attached your own `[PandaTasks]` under this `GameObject` to make them available to all `PandaBehaviour` components in your project.

- Variable can now be passed by reference the `'&'` prefix:

  ````
  Set &Target @SomeObject
  ````

  The variable is of type `IVariable` in the C# method and its value can be set:

  ```csharp
  [PandaTask] bool Set(IVariable variable, GameObject value)
  {
      variable.value = value;
      return true;
  }
  ```

- Added `null ` literal in BT Script:

  ```
  Set &Target null
  ```

- Added support for nullable value type.

  

### Fixes

- Fix tasks with default parameters raising undefined tasks exceptions.



## Beta 2022.11.21

### Core

- Added support for methods with default parameters.
- Added support for methods with variable number of arguments (`params` keyword).
- GC Alloc optimizations - 0 B allocated after initialization excepted for methods with value parameters.
- Decoupled from Visual Scripting (no compile error if Visual Scripting package is not installed)

### Fixes

- Fix enum value not parsed
- Fix task parameters not refreshed when BT Script is re-imported
- Fix syntax highlighting for retry keyword
- Fix variables no refreshed when bt displayed in Inspector

### Examples 

- Fix twitchy rotation in Shooter example
- Minor BT Script updates
- Added platformer controller example

### Experimental

- Added an experimental GOAP extension

## Panda BT 2 - Beta 2022.10.25 release notes

- Add Link To parameter to PandaLinker.
  Now it is possible to declare tasks for all PandaBehaviours in the scene.
- Added an icon for the PandaLinker component.
- Updated Shooter example graphics.
- Added support for Visual Scripting Variables.
- Variable can be accessed from the PandaBehaviour with the variables property and the GetVariable method.
- More expressions error checking.
- Several bugs fixing

## Version 2.0.0

### Core

- Trees and tree references are identified using #
- Variables 
- Added support for async task

### Fixes

- Fixed hovered and selected node not visible in Dark Editor Theme (Pro)

## version 1.4.4 - 20 December 2021

### Core

- Added `ThisTask` as an alias for `Task.current`.

### Package

- Reorganized project to fit Unity's package system.
- Add Assembly Definitions files for faster re-compilation and better modularity.

### Examples

- Update/Change graphics

### Fixes

- Fixed float parsing error occuring when local culture using other character than "." as decimal separator.
- Fixed BT scripts mark as modified (*) when another script is applied or reverted.
- Fixed various in-Inspector code rendering issues.
- Fixed breakpoints cleared on play for prefabs.
- Fixed `PandaTree` (returned by `PandaBehaviour.GetTree(string)`) not reset.
- Fixed `PandaBehaviour` component errors when undo `count`  value .
- Fixed continuous assets refreshing while inspecting a prefab from the `Assets` folder.


## version 1.4.3 - 17 February 2019

### Core

- Renamed "Tree" to "PandaTree" to avoid name collission witn Unity built-in class.

### Fixes

- Fixed BT not initialized when PandaBehaviour component is attached at runtime.
- Fixed debugging breakpoints not pausing when bt is ticked on FixedUpdate or LateUpdate.
- Fixed PandaBehaviour.GetTree generating GC Allocs.
- Fixed float parser using local culture and causing errors.

## Version 1.4.2 - 16 Augustus 2017

### Core

- GC alloc optimization:  minimize usage of RegEx during BT script parsing.
- Added built-in task WaitRandom(min, max)

### Fixes

- Fixed BT script not being recompiled on Undo.
- Fixed 'multiple definitions' error message not displaying target class name.
- Highlight tasks with multiple definitions in Inspector.
- Fix same tree not being reset when used at different place in the BT script.

## Version 1.4.1 - 18 April 2017

### Core

- Add PandaBehaviour.GetTree(string) which retrieves a Tree by its name. Then the tree can be ticked from a [Task].
- This feature is useful when it is requirred to decide at runtime which tree to tick.
- Add a new class: Panda.Tree, which as the following properties and methods.
  - name: the tree name
  - status: the current tree status
  - Tick(): tick the tree
  - Reset(): reset the tree
  - An instance of this class is returned by PandaBehaviour.GetTree(string).

### Fixes

- Fixed compiled BT script from string (using 'PandaBehaviour.Compile(...)') not being displayed in Inspector.
- Fixed TextAsset assigned in code to 'PandaBehaviour.scripts' not being displayed in Inspector.
  (Note: a call to 'PandaBehaviour.Apply()' is required to apply the change.)

## Version 1.4.0 - 12 December 2016

### Core

- Added PandaBehaviour.snapshot: The BT execution state can be stored and restored via this serializable property  (Pro only).
- Added support for Enum type in BT scripts.
- Made enum value selectable from a combo-box in the in-Inspector Editor (Pro only).

### Fixes

- Fixed GC allocations in PandaBehaviour.Wait(...) due to boxing into Task.current.item.

## Version 1.3.1 - 15 Augustus 2016

### Core

- Optimisation: drastically decreased GC Alloc during BTs initialisation.

### Fixes

- Fixed BT script field disappearing in the Inspector when BT script is invalid.
- Fixed PandaBehaviour inspector not being updated when selecting BT script through the asset selector window.
- Fixed null exception raised when adding PandaBehaviour component on prefabs.
- Fixed Task.IsInspected being true while BT is not displayed in the Inspector.
- Highlight unimplemented task in Inspector.
- Fixed parsing error occuring when no EOL on last line.
- Fixed BT scripts not being refreshed when reverted (Pro).

### Minor Changes

New icon for PandaBehaviour component and gizmo.
Added PandaBehaviour.xml for autocompletion documentation in IDE.



## Version 1.3.0 - 2 June 2016

### Core

- Added in-Inspector drag&drop editor (Pro Only).
- Refactored tasks binding.

### Fixes

- Fixed null exception occurring when a task is completed outside of its method (expl: from a callback).
- Fixed mute node without child not raising exception.

### Minor changes

- BT script fullpath is displayed in Exception message instead of file name only.
- Modified break point UI.


## Version 1.2.3 - 5 April 2016

### Core

- Optimized for GC Allocations per frame (after initialization, 0B is allocated by the engine).
- Added Task.isInspected, which returns whether the current BT script is displayed in the Inspector. (Use to avoid GC allocation when formatting string for debugInfo)
- Added PandaBehaviour.Compile( string source ) and PandaBehaviour.Compile( string[] sources ), which are used to make BTs from strings instead of TextAssets.
- Added built-in task DebugBreak, which breaks (pauses the editor) and succeeds immediately.

### Fixes

- Fixed BT being reset when object is selected in the Hierarchy.
- Built-in task WaitAnyKeyDown had parameter, now removed.

### Minor changes

- Simplified management of BT update and recompilation when scripts has been modified.

## Version 1.2.2 - 13 March 2016

### Changes

- Releasing Panda BT Pro containing the sources.
- Break point is now a pro-only feature.
- Added status dependent break point. You can setup different type of break points that will trigger when:
  -  The node is starting (blue break point),
  -  The node succeeds (green break point)
  -  The node fails (red break point)

### Minor changes

-Only left click is used to navigate to task definition.

- Left click toggles and cycles through break point type, right click clears break point.



## Version 1.2.1 - 24 February 2016

### Core

- Made some caching optimizations resulting in faster tree traversal.
- RANDOM node is now based on UnityEngine.Random instead of System.Random.

### Minor Changes

- The built-in task 'Wait' now display a countdown as debugInfo instead of elapsed time.
- Improved comments of the examples.

### Fixes

- Fixed break point and line number highlighting not working properly when using single line parenting.
- Fixed checking for task definitions when component is added or removed from the GameObject.
- Fixed parsing space indentations. BT scripts using spaces instead of tabulations for indentations was not parsed properly.


## Version 1.2.0 - 10 February 2016

### Package

- Simplified examples to reduce package size.

### Debugging

- Improved error messages.
- Highlighting errors in inspector.
- Highlighting currently executing lines.
- Opening task definitions in external script editor when double click on tasks in inspector.
- Improved breakpoints debugging.

### Core

- Modification of the WHILE node. The WHILE not was a duplication of the repeat not. Now the WHILE not does not repeat its child anymore. It runs its child while as long as the condition succeeds. It succeeds when the child succeeds and fails when the condition or the child fails.
- Removed IF node because conditionnal branching can be construct with SEQUENCE and FALLBACK.
- Renamed [TaskMethod] to [Task].
- Method returning bool, boolean field and boolean property can now be used from BT script with the [Task] attribute.
- Random node now accepts integer as weight in the node parameters.
- Improved RANDOM node weights computation and probability distribution.
- Added more build-in tasks to PandaBehaviour
- Using Task.current.Succeed(), Task.current.Fail() and Task.current.isStarting in task methods.
- WHILE and IF nodes now succeed when their condition fails.
- Allowed defining task as static method.
- behaviour node is now named tree.
- Root tree is now named "Root" instead of "Main".
- Syntax changes: parenthesis and coma are now optionals for node parameters.
- Improved Code Viewer appearance for better integration within Unity.

### Fixes

- Fixed parsing comments.
- Fixed null pointer error occuring when no BT script is assigned to a panda Behaviour.
- Fixed Panda Behaviour not being initialized when disable on game start.
- Fixed parser error sometimes occures when several BT scripts is assigned to one Panda Behaviour.
- Fixed parsing negative float or negative int in node parameters.
- Fixed public method not being bound.
- Fixed parenting error with task nodes.


### Minor changes

- It's not more possible to set break points on line not containing nodes.	


## Version 1.0.1 - 12 Mai 2015

- Wrapped example scripts under Panda.Demo namespace to avoid name collision during package import.
- Fixed subtree not being reset when called more than once.
- Concurrent running of subtree is not allowed. An exception is now raised when that happen.This might occures when the subtree is a children of PARALLEL or RACE node.
- Wait Task: display Debug info string for wait when the node is completed
- Clear debugInfo string when the node is reset.


## Version 1.0.0 - 23 Mars 2015

First release

