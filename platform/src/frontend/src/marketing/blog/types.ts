import { type ReactNode } from "react";

export interface PostMeta {
  slug: string;
  title: string;
  description: string;
  /** ISO date, `YYYY-MM-DD`. */
  date: string;
  readingMinutes: number;
  /** Short category label shown as a pill. */
  tag: string;
}

export interface Post {
  meta: PostMeta;
  /** Article body — plain HTML elements, rendered inside an `<article class="prose">`. */
  Body: () => ReactNode;
}
