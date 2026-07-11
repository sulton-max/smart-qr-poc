// The single vendor-touching line in the app — pins the forms engine once
// (wow-two-ws conventions/development/frontend/presentation/forms.md § Engine pin).
// Screens import `useAppForm` from `@/form`; swapping engines = editing this line.
export { useAppForm } from "@wow-two-beta/ui/forms-engine/tanstack";
