import { SizePreset } from "@wow-two-beta/ui/foundation/utils";
import { ToggleButton, ToggleButtonGroup, ToggleButtonGroupVariant, ToggleMode } from "@wow-two-beta/ui/presentation/actions";
import { ColorPicker, Field, Select } from "@wow-two-beta/ui/presentation/forms";
import { Accordion, AccordionType } from "@wow-two-beta/ui/presentation/display";
import { Stack } from "@wow-two-beta/ui/presentation/layout";
import type { AppForm } from "@wow-two-beta/ui/forms-engine";
import { BarcodeFormat } from "@/domain/codes";
import type { CodeCreateUpdateApiRequest } from "@/integration/codes";
import { BarcodeFormatDisplays } from "@/presentation/codes/design/components/BarcodeFormatDisplays";
import { EmojiControls } from "@/presentation/codes/design/components/EmojiControls";
import { FillControls } from "@/presentation/codes/design/components/FillControls";
import { ShapeControls } from "@/presentation/codes/design/components/ShapeControls";

/** Defines props for the Design tab — the code symbology plus the colors / shape / center accordion. */
export interface DesignViewProps {
  /** The code-builder form. */
  readonly form: AppForm<CodeCreateUpdateApiRequest>;
}

/**
 * Renders the Design tab: the code-type select plus a single-open accordion (colors & fill · shape & eyes ·
 * center). The design settings are one `style` field — each accordion pane binds through `form.Field name="style"`
 * and merges its slice back with a spread (the same whole-object idiom the content controls use).
 */
export function DesignView({ form }: DesignViewProps) {
  return (
    <>
      {/* Code type stays outside the accordion — it gates which style options apply (QR vs 1D/2D). */}
      <form.Field name="barcodeFormat">
        {(f) => (
          <Field label="Code type">
            <Select<BarcodeFormat>
              value={f.value}
              onValueChange={(opt) => opt && f.setValue(opt.itemKey)}
              getOptionLabel={(format) => BarcodeFormatDisplays[format].label}
            >
              <Select.Trigger>
                <Select.Value />
              </Select.Trigger>
              <Select.Content>
                {Object.values(BarcodeFormat).map((format) => (
                  <Select.Item key={format} itemKey={format} label={BarcodeFormatDisplays[format].label} />
                ))}
              </Select.Content>
            </Select>
          </Field>
        )}
      </form.Field>

      {/* one styling section open at a time */}
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
              <form.Field name="style">
                {(f) => (
                  <Stack gap="0">
                    <FillControls
                      foreground={f.value.foregroundColor}
                      onForegroundChange={(v) => f.setValue({ ...f.value, foregroundColor: v })}
                      gradient={f.value.gradient ?? null}
                      onGradientChange={(v) => f.setValue({ ...f.value, gradient: v ?? undefined })}
                    />

                    {/* Background row — [Color (swatch + label) | Transparent]. The swatch lives INSIDE the
                        Color segment (ToggleButton `as="div"` → valid nesting); it greys under Transparent. */}
                    <div className="flex min-h-10 items-center justify-between gap-4 border-t border-border pt-2">
                      <span className="text-sm text-muted-foreground">Background</span>
                      <ToggleButtonGroup
                        variant={ToggleButtonGroupVariant.Segmented}
                        type={ToggleMode.Single}
                        value={f.value.transparentBackground ? "transparent" : "color"}
                        onValueChange={(v) => {
                          if (v === "color") f.setValue({ ...f.value, transparentBackground: false });
                          else if (v === "transparent") f.setValue({ ...f.value, transparentBackground: true });
                        }}
                        aria-label="Background fill"
                      >
                        <ToggleButton value="color" size={SizePreset.Sm} as="div" className="gap-2">
                          <span
                            className={`inline-flex items-center${f.value.transparentBackground ? " pointer-events-none opacity-40" : ""}`}
                          >
                            <ColorPicker
                              triggerVariant="swatch"
                              value={f.value.backgroundColor}
                              onValueChange={(v) => f.setValue({ ...f.value, backgroundColor: v })}
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
                )}
              </form.Field>
            </div>
          </Accordion.Content>
        </Accordion.Item>

        <Accordion.Item value="shape">
          <Accordion.Trigger>Shape &amp; eyes</Accordion.Trigger>
          <Accordion.Content>
            <div className="px-3 py-2">
              <form.Field name="style">
                {(f) => (
                  <ShapeControls
                    moduleShape={f.value.moduleShape}
                    finderShape={f.value.finderShape}
                    finderDotShape={f.value.finderDotShape}
                    onModuleShapeChange={(v) => f.setValue({ ...f.value, moduleShape: v })}
                    onFinderShapeChange={(v) => f.setValue({ ...f.value, finderShape: v })}
                    onFinderDotShapeChange={(v) => f.setValue({ ...f.value, finderDotShape: v })}
                  />
                )}
              </form.Field>
            </div>
          </Accordion.Content>
        </Accordion.Item>

        <Accordion.Item value="center">
          <Accordion.Trigger>Center</Accordion.Trigger>
          <Accordion.Content>
            <div className="px-3 py-2">
              <form.Field name="style">
                {(f) => (
                  <EmojiControls
                    emoji={f.value.emoji ?? null}
                    onChange={(v) => f.setValue({ ...f.value, emoji: v ?? undefined })}
                    size={{ icon: 16 }}
                  />
                )}
              </form.Field>
            </div>
          </Accordion.Content>
        </Accordion.Item>
      </Accordion>
    </>
  );
}
