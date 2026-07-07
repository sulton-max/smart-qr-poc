// Native textarea / datetime inputs styled to match the SDK TextInput (which has no multiline / datetime
// variant yet). Plain text-likes use the SDK TextInput; these classes give the native elements the same look.

/** The class string that styles a native `<textarea>` / `<input type="datetime-local">` to match the SDK TextInput. */
export const NativeInputStyles =
  "w-full rounded-md border border-border bg-background px-3 py-2 text-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";
