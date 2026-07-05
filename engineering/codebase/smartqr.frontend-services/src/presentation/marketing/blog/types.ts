import { type ReactNode } from "react";

export interface PostMeta {
  slug: string;
  title: string;
  description: string;
  /** ISO `YYYY-MM-DD`. */
  date: string;
  readingMinutes: number;
  /** Category label, shown as a pill. */
  tag: string;
}

export interface Post {
  meta: PostMeta;
  /** Body — rendered inside `<article class="prose">`. */
  Body: () => ReactNode;
}
