import { useEffect } from "react";

// Client-side `<title>` + description/OG tags per route; swap to prerender/SSG when SEO depth matters.
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

// Upserts a `<meta>` tag keyed by `name` or `property`.
function setTag(attr: "name" | "property", key: string, content: string): void {
  let el = document.head.querySelector<HTMLMetaElement>(`meta[${attr}="${key}"]`);
  if (!el) {
    el = document.createElement("meta");
    el.setAttribute(attr, key);
    document.head.appendChild(el);
  }
  el.setAttribute("content", content);
}
