import { Link, useParams } from "react-router-dom";
import { ColorTone, SizePreset, SurfaceVariant } from "@wow-two-beta/ui/foundation/utils";
import { Button, ButtonVariant } from "@wow-two-beta/ui/presentation/actions";
import { Badge, BadgeVariant, Heading, HeadingSize, Text } from "@wow-two-beta/ui/presentation/display";
import { Grid, Surface } from "@wow-two-beta/ui/presentation/layout";
import { ArrowLeft } from "lucide-react";
import { usePageMeta } from "@/presentation/common";
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

  // Hooks run unconditionally — set meta before the early return.
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
        <Badge variant={BadgeVariant.Brand} size={SizePreset.Md} className="px-2.5 py-1 text-primary">
          {meta.tag}
        </Badge>
        <span>{formatDate(meta.date)}</span>
        <span aria-hidden>·</span>
        <span>{meta.readingMinutes} min read</span>
      </div>

      <Heading level={1} size={HeadingSize.Xxl} weight="bold" className="mt-4 leading-tight sm:text-4xl">
        {meta.title}
      </Heading>
      <Text size={SizePreset.Lg} color="muted" className="mt-4">
        {meta.description}
      </Text>

      <hr className="my-8 border-border" />

      <div className="prose">
        <Body />
      </div>

      <Surface
        variant={SurfaceVariant.Subtle}
        tone={ColorTone.Primary}
        radius="2xl"
        className="mt-12 bg-primary-soft/50 p-6 text-center"
      >
        <Text weight="semibold">Try it on a code that never expires.</Text>
        <Text size={SizePreset.Sm} color="muted" className="mt-1">
          Create a programmable QR code in under a minute — free, no account needed.
        </Text>
        <div className="mt-4 flex flex-wrap justify-center gap-3">
          <Button asChild tone={ColorTone.Primary} size={SizePreset.Sm}>
            <Link to="/app/new">Create a code</Link>
          </Button>
          <Button asChild tone={ColorTone.Neutral} variant={ButtonVariant.Outline} size={SizePreset.Sm}>
            <Link to="/pricing">See pricing</Link>
          </Button>
        </div>
      </Surface>

      {related.length > 0 && (
        <div className="mt-12">
          <Heading level={2} size={HeadingSize.Md} className="tracking-normal">
            Keep reading
          </Heading>
          <Grid columns="1" gap="6" className="mt-4 sm:grid-cols-2">
            {related.map((p) => (
              <BlogCard key={p.slug} post={p} />
            ))}
          </Grid>
        </div>
      )}
    </article>
  );
}
