import { Field, Select, UrlInput } from "@wow-two-beta/ui/presentation/forms";
import { MobileAppStoreType, type MobileAppLinkContent } from "@/domain/codes/content";
import type { ContentControlsProps } from "./fields";

/** The store picker's labels and per-store placeholder. */
const StoreDisplays: Record<MobileAppStoreType, { label: string; placeholder: string }> = {
  [MobileAppStoreType.AppStore]: { label: "App Store (iOS)", placeholder: "https://apps.apple.com/app/…" },
  [MobileAppStoreType.PlayStore]: { label: "Google Play", placeholder: "https://play.google.com/store/apps/…" },
  [MobileAppStoreType.Other]: { label: "Other", placeholder: "https://yourapp.com or another store" },
};

/** Renders one app-store link — the store it points at plus its URL. The rule carrying it supplies the device condition. */
export function MobileAppControls({ value, onChange }: ContentControlsProps<MobileAppLinkContent>) {
  return (
    <>
      <Field label="Store">
        <Select<MobileAppStoreType>
          value={value.store}
          onValueChange={(opt) => opt && onChange({ ...value, store: opt.itemKey })}
        >
          <Select.Trigger>
            <Select.Value />
          </Select.Trigger>
          <Select.Content>
            {Object.values(MobileAppStoreType).map((store) => (
              <Select.Item key={store} itemKey={store} label={StoreDisplays[store].label} />
            ))}
          </Select.Content>
        </Select>
      </Field>
      <Field label="Store URL">
        <UrlInput
          ring="sm"
          value={value.url}
          placeholder={StoreDisplays[value.store].placeholder}
          onChange={(e) => onChange({ ...value, url: e.target.value })}
        />
      </Field>
    </>
  );
}
