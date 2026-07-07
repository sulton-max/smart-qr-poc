import {
  CategoryDisplays,
  CategoryNavVariant,
  CategoryOrder,
  type CategoryKey,
  type EmojiPickerSize,
  EmojiPickerSizes,
} from "./emojiPicker";

/** Defines props for the horizontal category nav. */
export interface CategoryNavProps {
  /** Whether to render icon tiles (`strip`) or labelled chips (`pills`). */
  readonly variant: typeof CategoryNavVariant.Strip | typeof CategoryNavVariant.Pills;

  /** The active category. */
  readonly active: CategoryKey;

  /** Selects a category. */
  readonly onSelect: (category: CategoryKey) => void;

  /** The nav scale. */
  readonly size: EmojiPickerSize;
}

/** Renders the category picker as a horizontal icon strip or a labelled pill row. */
export function CategoryNav({ variant, active, onSelect, size }: CategoryNavProps) {
  // TODO(sdk): adopt a ToggleButtonGroup once the SDK ships one; raw <button> until then.
  if (variant === CategoryNavVariant.Pills) {
    return (
      <div className="flex flex-wrap gap-1.5" role="tablist" aria-label="Emoji categories">
        {CategoryOrder.map((key) => {
          const isActive = key === active;

          return (
            <button
              key={key}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => onSelect(key)}
              className={`rounded-full border px-3 py-1 text-xs transition-colors ${
                isActive
                  ? "border-primary bg-primary/10 text-foreground"
                  : "border-border text-muted-foreground hover:bg-muted"
              }`}
            >
              {CategoryDisplays[key].label}
            </button>
          );
        })}
      </div>
    );
  }

  const { nav } = EmojiPickerSizes[size];

  return (
    <div className="flex gap-0.5 rounded-md border border-border p-1" role="tablist" aria-label="Emoji categories">
      {CategoryOrder.map((key) => {
        const isActive = key === active;
        const { label, Icon } = CategoryDisplays[key];

        return (
          <button
            key={key}
            type="button"
            role="tab"
            aria-selected={isActive}
            aria-label={label}
            title={label}
            onClick={() => onSelect(key)}
            className={`flex flex-1 items-center justify-center rounded transition-colors ${
              isActive ? "bg-primary/10 text-foreground" : "text-muted-foreground hover:bg-muted"
            }`}
            style={{ height: nav }}
          >
            <Icon size={Math.round(nav * 0.55)} aria-hidden />
          </button>
        );
      })}
    </div>
  );
}
