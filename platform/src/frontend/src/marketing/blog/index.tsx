import { type Post, type PostMeta } from "./types";
import { whyQrCodesShouldNeverExpire } from "./why-qr-codes-should-never-expire";
import { smartRoutingOneCodeManyDestinations } from "./smart-routing-one-code-many-destinations";
import { qrBestPracticesThatScan } from "./qr-best-practices-that-scan";
import { dynamicVsStaticQrCodes } from "./dynamic-vs-static-qr-codes";

export type { Post, PostMeta };

/** All blog posts. Add a new post's module here and it appears in the index automatically. */
export const POSTS: Post[] = [
  whyQrCodesShouldNeverExpire,
  smartRoutingOneCodeManyDestinations,
  qrBestPracticesThatScan,
  dynamicVsStaticQrCodes,
];

/** Post metadata, newest first — what the blog index renders. */
export const POST_METAS: PostMeta[] = POSTS.map((p) => p.meta).sort((a, b) =>
  a.date < b.date ? 1 : -1,
);

export function getPost(slug: string): Post | undefined {
  return POSTS.find((p) => p.meta.slug === slug);
}
