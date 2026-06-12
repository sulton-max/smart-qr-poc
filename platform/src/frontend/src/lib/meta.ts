import { useEffect } from "react";

/**
 * Per-page document metadata for the client-rendered marketing pages. Sets `<title>` plus the
 * `description` / Open-Graph tags so each route is shareable and gives crawlers something real.
 *
 * This is client-side only — good enough for a POC. Swap to a prerender/SSG step (e.g. vite-ssg)
 * when SEO depth matters; the per-page strings defined via this hook are exactly what that step
 * would bake into static HTML.
 */
export function usePageMeta(title: string, description?: string): void {
  useEffect(() => {
    if (title) {
      document.title = title;
      setTag("property", "og:title", title);
    }
    if (description) {
      setTag("name", "description", description);
      setTag("property", "og:description", description);
    }
  }, [title, description]);
}

/** Upserts a `<meta>` tag keyed by an attribute (`name` or `property`). */
function setTag(attr: "name" | "property", key: string, content: string): void {
  let el = document.head.querySelector<HTMLMetaElement>(`meta[${attr}="${key}"]`);
  if (!el) {
    el = document.createElement("meta");
    el.setAttribute(attr, key);
    document.head.appendChild(el);
  }
  el.setAttribute("content", content);
}
