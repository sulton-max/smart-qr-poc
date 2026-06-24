import { ToggleButton, SegmentedControl } from "@wow-two-beta/ui/actions";
import { FormField } from "@wow-two-beta/ui/forms";
import { Text } from "@wow-two-beta/ui/display";
import { Stack } from "@wow-two-beta/ui/layout";
import { FinderShape, ModuleShape } from "../types";

export interface ShapeControlsProps {
  moduleShape: ModuleShape;
  finderShape: FinderShape;
  finderDotShape: FinderShape;
  onModuleShapeChange: (shape: ModuleShape) => void;
  onFinderShapeChange: (shape: FinderShape) => void;
  onFinderDotShapeChange: (shape: FinderShape) => void;
}

const MODULE_LABEL: Record<ModuleShape, string> = {
  square: "Square",
  rounded: "Rounded",
  dots: "Dots",
  classy: "Classy",
  classyRounded: "Classy rounded",
  verticalBars: "Vertical bars",
  horizontalBars: "Horizontal bars",
};

const MODULE_ORDER: ModuleShape[] = [
  ModuleShape.Square,
  ModuleShape.Rounded,
  ModuleShape.Dots,
  ModuleShape.Classy,
  ModuleShape.ClassyRounded,
  ModuleShape.VerticalBars,
  ModuleShape.HorizontalBars,
];

const FINDER_LABEL: Record<FinderShape, string> = {
  square: "Square",
  rounded: "Rounded",
  circle: "Circle",
};

const FINDER_ORDER: FinderShape[] = [FinderShape.Square, FinderShape.Rounded, FinderShape.Circle];

/**
 * A tiny 24×24 swatch that previews a module shape as a 3-cell row of `currentColor`
 * cells — a representative slice of how that shape tiles the symbol.
 */
function ModuleSwatch({ shape }: { shape: ModuleShape }) {
  // Three cells at x = 3 / 9.5 / 16, each 5 wide on a 24-unit canvas.
  const xs = [3, 9.5, 16];
  const w = 5;

  if (shape === ModuleShape.Dots) {
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {xs.map((x) => (
          <circle key={x} cx={x + w / 2} cy={12} r={2.5} fill="currentColor" />
        ))}
      </svg>
    );
  }

  if (shape === ModuleShape.VerticalBars) {
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {xs.map((x) => (
          <rect key={x} x={x} y={4} width={w} height={16} rx={2.4} fill="currentColor" />
        ))}
      </svg>
    );
  }

  if (shape === ModuleShape.HorizontalBars) {
    const ys = [3, 9.5, 16];
    return (
      <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
        {ys.map((y) => (
          <rect key={y} x={4} y={y} width={16} height={w} rx={2.4} fill="currentColor" />
        ))}
      </svg>
    );
  }

  // square / rounded / classy / classyRounded → three cells differing only by corner radius.
  const rx =
    shape === ModuleShape.Rounded
      ? 1.6
      : shape === ModuleShape.Classy
        ? 0.8
        : shape === ModuleShape.ClassyRounded
          ? 2.2
          : 0; // square
  return (
    <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
      {xs.map((x) => (
        <rect key={x} x={x} y={9.5} width={w} height={w} rx={rx} fill="currentColor" />
      ))}
    </svg>
  );
}

/** A finder-eye swatch: an outer ring frame + an inner pupil, both in `currentColor`. */
function FinderSwatch({ shape, dot }: { shape: FinderShape; dot?: boolean }) {
  // dot = render only the inner pupil emphasis; otherwise the full eye (frame + pupil).
  const frameRx = shape === FinderShape.Square ? 0 : shape === FinderShape.Rounded ? 4 : 11;
  const dotRx = shape === FinderShape.Square ? 0 : shape === FinderShape.Rounded ? 1.6 : 4;
  return (
    <svg viewBox="0 0 24 24" width="20" height="20" aria-hidden="true">
      {!dot && (
        <rect
          x={2}
          y={2}
          width={20}
          height={20}
          rx={frameRx}
          fill="none"
          stroke="currentColor"
          strokeWidth={2.5}
        />
      )}
      <rect
        x={dot ? 6 : 8}
        y={dot ? 6 : 8}
        width={dot ? 12 : 8}
        height={dot ? 12 : 8}
        rx={dot ? frameRx * 0.6 : dotRx}
        fill="currentColor"
      />
    </svg>
  );
}

/**
 * Code-styling controls (v0.5) — pick the body-module shape and the finder-eye
 * shapes that the backend renderer applies. Drives the live preview `style`:
 * `moduleShape` / `finderShape` (outer eye) / `finderDotShape` (inner eye).
 */
export function ShapeControls({
  moduleShape,
  finderShape,
  finderDotShape,
  onModuleShapeChange,
  onFinderShapeChange,
  onFinderDotShapeChange,
}: ShapeControlsProps) {
  return (
    <Stack gap="4">
      <FormField label="Body shape" helper="How the data modules are drawn.">
        <SegmentedControl
          type="single"
          value={moduleShape}
          onValueChange={(v) => v && onModuleShapeChange(v as ModuleShape)}
          aria-label="Body module shape"
          className="w-full"
        >
          {MODULE_ORDER.map((shape) => (
            <ToggleButton
              key={shape}
              value={shape}
              aria-label={MODULE_LABEL[shape]}
              title={MODULE_LABEL[shape]}
              className="flex-1 px-0"
            >
              <ModuleSwatch shape={shape} />
            </ToggleButton>
          ))}
        </SegmentedControl>
      </FormField>

      <FormField label="External eye" helper="The outer frame of the three corner markers.">
        <SegmentedControl
          type="single"
          value={finderShape}
          onValueChange={(v) => v && onFinderShapeChange(v as FinderShape)}
          aria-label="External eye shape"
        >
          {FINDER_ORDER.map((shape) => (
            <ToggleButton
              key={shape}
              value={shape}
              aria-label={FINDER_LABEL[shape]}
              className="flex items-center gap-2"
            >
              <FinderSwatch shape={shape} />
              <Text as="span" size="sm">
                {FINDER_LABEL[shape]}
              </Text>
            </ToggleButton>
          ))}
        </SegmentedControl>
      </FormField>

      <FormField label="Internal eye" helper="The pupil inside each corner marker.">
        <SegmentedControl
          type="single"
          value={finderDotShape}
          onValueChange={(v) => v && onFinderDotShapeChange(v as FinderShape)}
          aria-label="Internal eye shape"
        >
          {FINDER_ORDER.map((shape) => (
            <ToggleButton
              key={shape}
              value={shape}
              aria-label={FINDER_LABEL[shape]}
              className="flex items-center gap-2"
            >
              <FinderSwatch shape={shape} dot />
              <Text as="span" size="sm">
                {FINDER_LABEL[shape]}
              </Text>
            </ToggleButton>
          ))}
        </SegmentedControl>
      </FormField>
    </Stack>
  );
}
