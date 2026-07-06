import type { ReactNode } from "react";
import { FinderShape, ModuleShape } from "@/domain/codes/core";

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

/** Defines the display for a module-shape option. */
interface ModuleShapeDisplay {
  label: string;
  icon: ReactNode;
}

/** Maps each module shape to its label + preview swatch. */
export const ModuleShapeDisplays: Record<ModuleShape, ModuleShapeDisplay> = {
  [ModuleShape.Square]: { label: "Square", icon: <ModuleSwatch shape={ModuleShape.Square} /> },
  [ModuleShape.Rounded]: { label: "Rounded", icon: <ModuleSwatch shape={ModuleShape.Rounded} /> },
  [ModuleShape.Dots]: { label: "Dots", icon: <ModuleSwatch shape={ModuleShape.Dots} /> },
  [ModuleShape.Classy]: { label: "Classy", icon: <ModuleSwatch shape={ModuleShape.Classy} /> },
  [ModuleShape.ClassyRounded]: {
    label: "Classy rounded",
    icon: <ModuleSwatch shape={ModuleShape.ClassyRounded} />,
  },
  [ModuleShape.VerticalBars]: {
    label: "Vertical bars",
    icon: <ModuleSwatch shape={ModuleShape.VerticalBars} />,
  },
  [ModuleShape.HorizontalBars]: {
    label: "Horizontal bars",
    icon: <ModuleSwatch shape={ModuleShape.HorizontalBars} />,
  },
};

/** Defines the display for a finder-eye shape option — outer-frame + inner-pupil (`dot`) swatches. */
interface FinderShapeDisplay {
  label: string;
  icon: ReactNode;
  dotIcon: ReactNode;
}

/** Maps each finder shape to its label + outer/inner preview swatches. */
export const FinderShapeDisplays: Record<FinderShape, FinderShapeDisplay> = {
  [FinderShape.Square]: {
    label: "Square",
    icon: <FinderSwatch shape={FinderShape.Square} />,
    dotIcon: <FinderSwatch shape={FinderShape.Square} dot />,
  },
  [FinderShape.Rounded]: {
    label: "Rounded",
    icon: <FinderSwatch shape={FinderShape.Rounded} />,
    dotIcon: <FinderSwatch shape={FinderShape.Rounded} dot />,
  },
  [FinderShape.Circle]: {
    label: "Circle",
    icon: <FinderSwatch shape={FinderShape.Circle} />,
    dotIcon: <FinderSwatch shape={FinderShape.Circle} dot />,
  },
};
