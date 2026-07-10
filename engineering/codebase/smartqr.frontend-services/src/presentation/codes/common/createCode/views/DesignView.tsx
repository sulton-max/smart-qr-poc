import { SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { ToggleButton, ToggleButtonGroup, ToggleButtonGroupVariant, ToggleMode } from "@wow-two-beta/ui/presentation/actions";
import { ColorPicker, Field, Select } from "@wow-two-beta/ui/presentation/forms";
import { Accordion, AccordionType } from "@wow-two-beta/ui/presentation/display";
import { Stack } from "@wow-two-beta/ui/presentation/layout";
import { BarcodeFormat, FinderShape, ModuleShape, type DesignEmojiOverlay, type Gradient } from "@/domain/codes";
import { BarcodeFormatDisplays } from "@/presentation/codes/design/components/BarcodeFormatDisplays";
import { EmojiControls } from "@/presentation/codes/design/components/EmojiControls";
import { FillControls } from "@/presentation/codes/design/components/FillControls";
import { ShapeControls } from "@/presentation/codes/design/components/ShapeControls";

/** Defines props for the Design tab — the code symbology plus the colors / shape / center accordion. */
export interface DesignViewProps {
  /** The barcode symbology (QR / 1D / 2D) — gates which style options apply. */
  readonly symbology: BarcodeFormat;

  /** Fires when the symbology changes. */
  readonly onSymbologyChange: (value: BarcodeFormat) => void;

  /** The solid foreground color (#RRGGBB). */
  readonly foreground: string;

  /** Fires when the foreground color changes. */
  readonly onForegroundChange: (value: string) => void;

  /** The foreground gradient, or null for a solid fill. */
  readonly gradient: Gradient | null;

  /** Fires when the gradient changes. */
  readonly onGradientChange: (value: Gradient | null) => void;

  /** The background color (#RRGGBB). */
  readonly background: string;

  /** Fires when the background color changes. */
  readonly onBackgroundChange: (value: string) => void;

  /** Whether the background is transparent (overrides the background color). */
  readonly transparentBackground: boolean;

  /** Fires when the transparent-background toggle changes. */
  readonly onTransparentBackgroundChange: (value: boolean) => void;

  /** The data-module shape. */
  readonly moduleShape: ModuleShape;

  /** Fires when the module shape changes. */
  readonly onModuleShapeChange: (value: ModuleShape) => void;

  /** The finder (eye) frame shape. */
  readonly finderShape: FinderShape;

  /** Fires when the finder frame shape changes. */
  readonly onFinderShapeChange: (value: FinderShape) => void;

  /** The finder (eye) pupil shape. */
  readonly finderDotShape: FinderShape;

  /** Fires when the finder pupil shape changes. */
  readonly onFinderDotShapeChange: (value: FinderShape) => void;

  /** The center emoji overlay, or null for none. */
  readonly emoji: DesignEmojiOverlay | null;

  /** Fires when the center emoji changes. */
  readonly onEmojiChange: (value: DesignEmojiOverlay | null) => void;
}

/** Renders the Design tab: the code-type select plus a single-open accordion (colors & fill · shape & eyes · center). */
export function DesignView({
  symbology,
  onSymbologyChange,
  foreground,
  onForegroundChange,
  gradient,
  onGradientChange,
  background,
  onBackgroundChange,
  transparentBackground,
  onTransparentBackgroundChange,
  moduleShape,
  onModuleShapeChange,
  finderShape,
  onFinderShapeChange,
  finderDotShape,
  onFinderDotShapeChange,
  emoji,
  onEmojiChange,
}: DesignViewProps) {
  return (
    <>
      {/* Code type stays outside the accordion — it gates which style options apply (QR vs 1D/2D). */}
      <Field label="Code type">
        <Select<BarcodeFormat>
          value={symbology}
          onValueChange={(opt) => opt && onSymbologyChange(opt.itemKey)}
        >
          <Select.Trigger>
            <Select.Value />
          </Select.Trigger>
          <Select.Content>
            {Object.values(BarcodeFormat).map((f) => (
              <Select.Item key={f} itemKey={f} label={BarcodeFormatDisplays[f].label} />
            ))}
          </Select.Content>
        </Select>
      </Field>

      {/* Layout D: one styling section open at a time — caps height, scales to any number of sections. */}
      <Accordion
        type={AccordionType.Single}
        defaultValue="colors"
        isCollapsible
        className="overflow-hidden rounded-lg border border-border"
      >
        <Accordion.Item value="colors">
          <Accordion.Trigger>Colors &amp; fill</Accordion.Trigger>
          <Accordion.Content>
            <div className="px-3 py-2">
              <Stack gap="0">
                <FillControls
                  foreground={foreground}
                  onForegroundChange={onForegroundChange}
                  gradient={gradient}
                  onGradientChange={onGradientChange}
                />

                {/* Background row — [Color (swatch + label) | Transparent]. The swatch lives
                    INSIDE the Color segment, now valid markup: ToggleButton `as="div"` renders a
                    role=button div, so nesting the ColorPicker's <button> isn't button-in-button.
                    Clicking the swatch opens the picker (and selects Color via bubble); it greys
                    under Transparent. */}
                <div className="flex min-h-10 items-center justify-between gap-4 border-t border-border pt-2">
                  <span className="text-sm text-muted-foreground">Background</span>
                  <ToggleButtonGroup variant={ToggleButtonGroupVariant.Segmented}
                    type={ToggleMode.Single}
                    value={transparentBackground ? "transparent" : "color"}
                    onValueChange={(v) => {
                      if (v === "color") onTransparentBackgroundChange(false);
                      else if (v === "transparent") onTransparentBackgroundChange(true);
                    }}
                    aria-label="Background fill"
                  >
                    <ToggleButton value="color" size={SizePreset.Sm} as="div" className="gap-2">
                      <span
                        className={`inline-flex items-center${transparentBackground ? " pointer-events-none opacity-40" : ""}`}
                      >
                        <ColorPicker
                          triggerVariant="swatch"
                          value={background}
                          onValueChange={onBackgroundChange}
                          aria-label="Background color"
                          triggerSize="sm"
                        />
                      </span>
                      Color
                    </ToggleButton>
                    <ToggleButton value="transparent" size={SizePreset.Sm}>Transparent</ToggleButton>
                  </ToggleButtonGroup>
                </div>
              </Stack>
            </div>
          </Accordion.Content>
        </Accordion.Item>

        <Accordion.Item value="shape">
          <Accordion.Trigger>Shape &amp; eyes</Accordion.Trigger>
          <Accordion.Content>
            <div className="px-3 py-2">
              <ShapeControls
                moduleShape={moduleShape}
                finderShape={finderShape}
                finderDotShape={finderDotShape}
                onModuleShapeChange={onModuleShapeChange}
                onFinderShapeChange={onFinderShapeChange}
                onFinderDotShapeChange={onFinderDotShapeChange}
              />
            </div>
          </Accordion.Content>
        </Accordion.Item>

        <Accordion.Item value="center">
          <Accordion.Trigger>Center</Accordion.Trigger>
          <Accordion.Content>
            <div className="px-3 py-2">
              <EmojiControls emoji={emoji} onChange={onEmojiChange} size={{ icon: 16 }} />
            </div>
          </Accordion.Content>
        </Accordion.Item>
      </Accordion>
    </>
  );
}
