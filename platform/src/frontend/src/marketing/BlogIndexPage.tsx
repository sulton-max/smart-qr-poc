import { usePageMeta } from "../lib/meta";
import { POST_METAS } from "./blog";
import { BlogCard, Section, SectionHeading } from "./components";

export function BlogIndexPage() {
  usePageMeta(
    "Blog — QR codes, smart routing & best practices · Smart QR",
    "Guides on why and how to use QR codes: dynamic vs. static, how smart routing works, best practices for codes that actually scan, and why your codes should never expire.",
  );

  return (
    <Section>
      <SectionHeading
        eyebrow="Blog"
        title="Why and how to use QR codes"
        description="Practical guides to getting QR codes right — from the squares that have to scan to the routing rules behind them."
      />
      <div className="mx-auto mt-12 grid max-w-4xl gap-6 sm:grid-cols-2">
        {POST_METAS.map((post) => (
          <BlogCard key={post.slug} post={post} />
        ))}
      </div>
    </Section>
  );
}
