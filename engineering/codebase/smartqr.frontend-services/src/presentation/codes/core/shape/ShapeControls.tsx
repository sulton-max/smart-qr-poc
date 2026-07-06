import { OptionTile, OptionTileGroup } from "@wow-two-beta/ui/presentation/actions";
import { ControlGroup, Divider, Stack } from "@wow-two-beta/ui/presentation/layout";

import { FinderShape, ModuleShape } from "@/domain/codes/core";

import { FinderShapeDisplays, ModuleShapeDisplays } from "./ShapeDisplays";

const MODULE_SHAPES = Object.keys(ModuleShapeDisplays) as ModuleShape[];
const FINDER_SHAPES = Object.keys(FinderShapeDisplays) as FinderShape[];

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
      <ControlGroup label="Body" orientation="vertical" divided={false}>
        <OptionTileGroup label="Body shape" wrap>
          {MODULE_SHAPES.map((shape) => (
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

      {/* Eyes — the finder frame (external) + pupil (internal). */}
      <ControlGroup label="Eyes" orientation="vertical" divided={false}>
        <div className="flex items-start gap-3">
          <div className="min-w-0 flex-1">
            <div className="mb-1.5 text-xs text-muted-foreground">External</div>
            <OptionTileGroup label="External eye" wrap>
              {FINDER_SHAPES.map((shape) => (
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
          <Divider orientation="vertical" />
          <div className="min-w-0 flex-1">
            <div className="mb-1.5 text-xs text-muted-foreground">Internal</div>
            <OptionTileGroup label="Internal eye" wrap>
              {FINDER_SHAPES.map((shape) => (
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
