import { Route, Routes } from "react-router-dom";
import { ScrollToTop } from "@/presentation/common";
import { BlogIndexPage, BlogPostPage, LandingPage, NotFoundPage, PricingPage } from "@/presentation/marketing";
import { AppLayout } from "./AppLayout";
import { MarketingLayout } from "./MarketingLayout";
import { BillingRoute, CodesListRoute, CreateCodeRoute, EditCodeRoute } from "./routes";

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
