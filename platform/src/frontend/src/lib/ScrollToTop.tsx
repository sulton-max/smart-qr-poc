import { useEffect } from "react";
import { useLocation } from "react-router-dom";

/**
 * Resets the scroll position to the top on every route change. Without this an SPA keeps the
 * previous page's scroll offset when navigating (e.g. footer link → new page mid-scroll).
 * Renders nothing.
 */
export function ScrollToTop(): null {
  const { pathname } = useLocation();
  useEffect(() => {
    window.scrollTo({ top: 0, left: 0, behavior: "instant" });
  }, [pathname]);
  return null;
}
