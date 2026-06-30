# Draft Polish Track

conventions

- need to establish new convention for git
- when developing with claude together, on iterations
- when an iteration is complete, claude can stage related files, give me a commit msg and tell to commit
- can continue the work on in parallel to, so only build, complete code gets pushed and also the work isn't blocked

sdk work

- need to analyze how we can define inputs with regex - need one for color input for example so that only regex is allowed


## Frontend 

### Create / Edit Code Layer

#### Component Naming convention

- need to analyze a component naming convention for grouped controls
- real example - ShapeControls
- should it be ShapeControlsGroup or just ShapeControls ? any better alternatives ? 
- this is so that all such control groups will have a consistent name
- need to analyze suffix for inputs / controls as well



##### `ShapeControls.tsx`

- need to extract MODULE_LABEL component into sdk as selectable tiles / blocks
- need to create separate input shape input
- need to move module label into common constants - need to analyze better name for it, MMODULE_LABEL - need a different name, imo, is dots called module ?
- also we have like 3 types scattered for modules ( modules const, labels, order ) - need to analyze a better way to do this
- same for finder - finder and module are main parts of the qr code - so yes they should exist as main constant / enums so that all dto models can use it - but order, label, display name - they are bound to the controls
- module swatch - should be extracted and need to analyze how to provide select option content
- need to apply comment convention
- is there a way to have standard gaps ? like instead of mb-2 in the first group, can we just use gap or does it require flex ?
- optimized padding on the first group - added gap to the stack instead
- why to use border between the groups ? need to add a separator component
- EyeColumn - column is off, need to analyze all component names within shape controls after analyzing naming standard naming conventions
- eye column - usage of the same input for 2 things is wrong, plus need to update after extracting a single component
- vertical border among 2 eye controls - same, need some kind of separator component ( that takes 2 types of value as the size - half | eighty | full - to divide full, and another value is percentage )

- we need a convention to use gaps, and separators instead of borders where possible - analyze wow-two-ws frontend conventions to add this


##### `CreateCodeScreen.tsx`

- instead of using a few dozen state objects, is there better way to save the object ? maybe the whole object ? what's better approach ?
- need to separate color & fill into a separate component, same as shape controls

##### After Color and Fill separated

- analyze a better to use the trigger of ColorPicker - maybe we can add event in the sdk ? is that the role of the inner button in TileColorPicker.tsx ? 
- 