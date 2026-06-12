import { type Post } from "./types";

export const smartRoutingOneCodeManyDestinations: Post = {
  meta: {
    slug: "smart-routing-one-code-many-destinations",
    title: "One QR code, many destinations: how smart routing works",
    description:
      "Send iPhone users to the App Store, German visitors to your German store, and lunch-time scanners to the lunch menu — all from a single printed code. Here's how rule-based QR routing actually works.",
    date: "2026-06-11",
    readingMinutes: 6,
    tag: "How-to",
  },
  Body: () => (
    <>
      <p>
        The most useful thing a QR code can do isn't tracking scans or wearing your logo — it's sending
        different people to different places from the <strong>same printed code</strong>. That's
        "smart routing," and once you've used it you won't go back to one-code-one-link.
      </p>
      <p>Here's how it works, what you can route on, and a handful of patterns worth stealing.</p>

      <h2>The mental model: a rulebook in front of a redirect</h2>
      <p>
        A dynamic QR code points at a short link you control. Smart routing puts an ordered rulebook in
        front of that link. Every scan, the code asks a series of questions about the visitor and sends
        them to the first matching answer:
      </p>
      <blockquote>
        If the device is an iPhone → App Store. Else if the country is Germany → German store. Else if
        it's before 4&nbsp;pm → lunch menu. Otherwise → fall back to the homepage.
      </blockquote>
      <p>
        Rules are evaluated <strong>top to bottom, first match wins.</strong> The last line is always a{" "}
        <strong>fallback</strong> — the safety net that guarantees every scan resolves somewhere, even
        when no rule matches. Order matters: put your most specific rules first and your catch-alls
        last.
      </p>

      <h2>What you can route on</h2>
      <p>
        The useful part is that almost all of this is read from the scan request itself — no app, no GPS
        prompt, no friction for the person scanning:
      </p>
      <ul>
        <li>
          <strong>Device / OS</strong> — from the browser's user-agent. The classic "Download" code:
          iOS → App Store, Android → Google Play, desktop → a web landing page.
        </li>
        <li>
          <strong>Country or region</strong> — from the scanner's IP address. Route to the nearest
          store, the right-language site, or a region-specific offer.
        </li>
        <li>
          <strong>Language</strong> — from the browser's <code>Accept-Language</code> header. Serve an
          English, Russian, or Uzbek landing page automatically.
        </li>
        <li>
          <strong>Time of day / day of week</strong> — a restaurant code that shows the lunch menu
          before 4&nbsp;pm and the dinner menu after, or a weekend-only promotion.
        </li>
        <li>
          <strong>Scan count</strong> — the first 100 scans get an early-bird coupon; everyone after
          gets the standard page.
        </li>
        <li>
          <strong>A/B split</strong> — send 50% of scans to one landing page and 50% to another, sticky
          per visitor, to test which converts better.
        </li>
      </ul>

      <h2>Patterns worth stealing</h2>
      <h3>The app-store router</h3>
      <p>
        One "Get the app" code on your packaging. iPhones land in the App Store, Android phones in Play,
        and anyone on a laptop gets a page explaining the app. You print one code instead of three, and
        nobody hits the wrong store.
      </p>
      <h3>Menu by time of day</h3>
      <p>
        A café prints one code on the table tent. Before 4&nbsp;pm it opens the lunch menu; after, the
        dinner menu; and on Sundays, brunch. The staff never swaps a single piece of paper.
      </p>
      <h3>The geo store-finder</h3>
      <p>
        A retail chain puts the same code on a national flyer. Each scanner is routed to their nearest
        location's page based on the country or region they're scanning from.
      </p>
      <h3>Localized landing pages</h3>
      <p>
        A tourist-facing sign routes by browser language: English speakers get the English page, Russian
        speakers the Russian one, everyone else the default. No "choose your language" screen.
      </p>

      <h2>Two ways to build the rules</h2>
      <p>You generally want both a simple and an advanced path:</p>
      <ul>
        <li>
          <strong>Presets</strong> for the common cases — pick "App Store Router" or "Menu by Time,"
          fill in a couple of URLs, done.
        </li>
        <li>
          <strong>A visual rule builder</strong> for everything else — stack conditions in the order you
          want them evaluated, set a destination for each, and define the fallback.
        </li>
      </ul>
      <p>
        Under the hood it's just a list of <code>when → then</code> rules plus a fallback. Developers can
        treat that same rulebook as JSON and set it via an API — handy for generating thousands of
        routed codes programmatically.
      </p>

      <h2>A few rules of thumb</h2>
      <ul>
        <li>
          <strong>Always set a fallback.</strong> It's the one destination that catches every scan a
          rule didn't.
        </li>
        <li>
          <strong>Most specific first.</strong> A broad rule near the top will shadow the precise ones
          below it.
        </li>
        <li>
          <strong>Keep it legible.</strong> Three or four clear rules beat a tangle of fifteen. You'll
          thank yourself when you revisit it in six months.
        </li>
      </ul>
      <p>
        The payoff: one code on one printed surface that quietly does the right thing for whoever scans
        it — and that you can rewire any time without reprinting.
      </p>
    </>
  ),
};
