import { Link, useParams } from "react-router-dom";
import { Button } from "@wow-two-beta/ui/actions";
import { ArrowLeft } from "lucide-react";
import { usePageMeta } from "../lib/meta";
import { getPost, POST_METAS } from "./blog";
import { BlogCard } from "./components";
import { NotFoundPage } from "./NotFoundPage";

function formatDate(iso: string): string {
  return new Date(`${iso}T00:00:00`).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

export function BlogPostPage() {
  const { slug = "" } = useParams();
  const post = getPost(slug);

  // Hooks must run unconditionally — set meta from the post (or a fallback) before any early return.
  usePageMeta(
    post ? `${post.meta.title} · Smart QR` : "Post not found · Smart QR",
    post?.meta.description,
  );

  if (!post) return <NotFoundPage />;

  const { meta, Body } = post;
  const related = POST_METAS.filter((p) => p.slug !== slug).slice(0, 2);

  return (
    <article className="mx-auto max-w-2xl px-6 py-14">
      <Link
        to="/blog"
        className="inline-flex items-center gap-1 text-sm text-muted-foreground transition-colors hover:text-foreground"
      >
        <ArrowLeft size={15} /> All posts
      </Link>

      <div className="mt-6 flex flex-wrap items-center gap-2 text-xs text-muted-foreground">
        <span className="rounded-full bg-primary-soft px-2.5 py-1 font-medium text-primary">{meta.tag}</span>
        <span>{formatDate(meta.date)}</span>
        <span aria-hidden>·</span>
        <span>{meta.readingMinutes} min read</span>
      </div>

      <h1 className="mt-4 text-3xl font-bold leading-tight tracking-tight sm:text-4xl">{meta.title}</h1>
      <p className="mt-4 text-lg text-muted-foreground">{meta.description}</p>

      <hr className="my-8 border-border" />

      <div className="prose">
        <Body />
      </div>

      {/* Inline CTA */}
      <div className="mt-12 rounded-2xl border border-primary/20 bg-primary-soft/50 p-6 text-center">
        <p className="font-semibold">Try it on a code that never expires.</p>
        <p className="mt-1 text-sm text-muted-foreground">
          Create a programmable QR code in under a minute — free, no account needed.
        </p>
        <div className="mt-4 flex flex-wrap justify-center gap-3">
          <Button asChild tone="primary" size="sm">
            <Link to="/app/new">Create a code</Link>
          </Button>
          <Button asChild tone="neutral" variant="outline" size="sm">
            <Link to="/pricing">See pricing</Link>
          </Button>
        </div>
      </div>

      {related.length > 0 && (
        <div className="mt-12">
          <h2 className="text-lg font-semibold">Keep reading</h2>
          <div className="mt-4 grid gap-6 sm:grid-cols-2">
            {related.map((p) => (
              <BlogCard key={p.slug} post={p} />
            ))}
          </div>
        </div>
      )}
    </article>
  );
}
