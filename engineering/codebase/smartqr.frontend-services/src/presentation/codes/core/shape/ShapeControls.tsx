import { OptionTile, OptionTileGroup } from "@wow-two-beta/ui/presentation/actions";
import { ControlGroup, Divider, Stack } from "@wow-two-beta/ui/presentation/layout";
import { Orientation } from "@wow-two-beta/ui/foundation/utils";

import { FinderShape, ModuleShape } from "@/domain/codes/core";

import { FinderShapeDisplays, ModuleShapeDisplays } from "./ShapeDisplays";

const ModuleShapes = Object.keys(ModuleShapeDisplays) as ModuleShape[];
const FinderShapes = Object.keys(FinderShapeDisplays) as FinderShape[];

/** Defines props for the shape controls. */
export interface ShapeControlsProps {
  /** The body module shape. */
  readonly moduleShape: ModuleShape;

  /** The external (frame) eye shape. */
  readonly finderShape: FinderShape;

  /** The internal (pupil) eye shape. */
  readonly finderDotShape: FinderShape;

  /** Emits the next body module shape. */
  readonly onModuleShapeChange: (shape: ModuleShape) => void;

  /** Emits the next external eye shape. */
  readonly onFinderShapeChange: (shape: FinderShape) => void;

  /** Emits the next internal eye shape. */
  readonly onFinderDotShapeChange: (shape: FinderShape) => void;
}

/** Renders the code-styling shape controls — a body-module grid above the paired external / internal eyes. */
export function ShapeControls({
  moduleShape,
  finderShape,
  finderDotShape,
  onModuleShapeChange,
  onFinderShapeChange,
  onFinderDotShapeChange,
}: ShapeControlsProps) {
  return (
    <Stack gap="2">
      {/* Body — the module (data cell) shape. */}
      <ControlGroup label="Body" orientation={Orientation.Vertical} divided={false}>
        <OptionTileGroup label="Body shape" wrap>
          {ModuleShapes.map((shape) => (
            <OptionTile
              key={shape}
              selected={moduleShape === shape}
              label={`Body shape: ${ModuleShapeDisplays[shape].label}`}
              onSelect={() => onModuleShapeChange(shape)}
            >
              {ModuleShapeDisplays[shape].icon}
            </OptionTile>
          ))}
        </OptionTileGroup>
      </ControlGroup>

      <Divider orientation={Orientation.Horizontal} />

      {/* Eyes — the finder frame (external) + pupil (internal). */}
      <ControlGroup label="Eyes" orientation={Orientation.Vertical} divided={false}>
        <div className="flex items-start gap-3">
          <div className="min-w-0 flex-1">
            <div className="mb-1.5 text-xs text-muted-foreground">External</div>
            <OptionTileGroup label="External eye" wrap>
              {FinderShapes.map((shape) => (
                <OptionTile
                  key={shape}
                  selected={finderShape === shape}
                  label={`External eye: ${FinderShapeDisplays[shape].label}`}
                  onSelect={() => onFinderShapeChange(shape)}
                >
                  {FinderShapeDisplays[shape].icon}
                </OptionTile>
              ))}
            </OptionTileGroup>
          </div>
          <Divider orientation={Orientation.Vertical} />
          <div className="min-w-0 flex-1">
            <div className="mb-1.5 text-xs text-muted-foreground">Internal</div>
            <OptionTileGroup label="Internal eye" wrap>
              {FinderShapes.map((shape) => (
                <OptionTile
                  key={shape}
                  selected={finderDotShape === shape}
                  label={`Internal eye: ${FinderShapeDisplays[shape].label}`}
                  onSelect={() => onFinderDotShapeChange(shape)}
                >
                  {FinderShapeDisplays[shape].dotIcon}
                </OptionTile>
              ))}
            </OptionTileGroup>
          </div>
        </div>
      </ControlGroup>
    </Stack>
  );
}
