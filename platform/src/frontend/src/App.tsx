import { Route, Routes } from "react-router-dom";
import { ScrollToTop } from "./lib/ScrollToTop";
import { MarketingLayout } from "./marketing/MarketingLayout";
import { LandingPage } from "./marketing/LandingPage";
import { PricingPage } from "./marketing/PricingPage";
import { BlogIndexPage } from "./marketing/BlogIndexPage";
import { BlogPostPage } from "./marketing/BlogPostPage";
import { NotFoundPage } from "./marketing/NotFoundPage";
import { AppLayout } from "./app/AppLayout";
import { BillingRoute, CodesListRoute, CreateCodeRoute, EditCodeRoute } from "./app/routes";

// `/app/*` is the guest-gated product; everything else is the public marketing surface.
export function App() {
  return (
    <>
      <ScrollToTop />
      <Routes>
        <Route path="/app" element={<AppLayout />}>
          <Route index element={<CodesListRoute />} />
          <Route path="new" element={<CreateCodeRoute />} />
          <Route path="billing" element={<BillingRoute />} />
          <Route path=":id/edit" element={<EditCodeRoute />} />
        </Route>

        <Route element={<MarketingLayout />}>
          <Route path="/" element={<LandingPage />} />
          <Route path="/pricing" element={<PricingPage />} />
          <Route path="/blog" element={<BlogIndexPage />} />
          <Route path="/blog/:slug" element={<BlogPostPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </>
  );
}
