import { useCallback, useEffect, useState } from "react";
import { Button } from "@wow-two-beta/ui/actions";
import { Badge, Card, Heading } from "@wow-two-beta/ui/display";
import { Banner, MeterBar, Spinner } from "@wow-two-beta/ui/feedback";
import { ArrowUpRight, Check, CreditCard, Infinity as InfinityIcon } from "lucide-react";
import { createCheckout, createPortal, getBilling } from "../api";
import { PAID_PLANS, Plan } from "../types";
import type { BillingStatus } from "../types";

/** Outcome of the Stripe return redirect (`/app/billing?status=success|cancelled`). */
type ReturnStatus = "success" | "cancelled";

/** Static, plan-keyed display metadata. Caps mirror the backend `PlanLimits` (Free=3, Solo=25, Pro=200, Agency=∞). */
interface PlanMeta {
  name: string;
  price: string;
  cadence: string;
  tagline: string;
  highlights: string[];
}

const PLAN_META: Record<Plan, PlanMeta> = {
  [Plan.Free]: {
    name: "Free",
    price: "$0",
    cadence: "forever",
    tagline: "For tinkerers and first codes.",
    highlights: ["3 dynamic codes", "Unlimited scans", "Codes never expire"],
  },
  [Plan.Solo]: {
    name: "Solo",
    price: "$5",
    cadence: "/mo",
    tagline: "For a small business with physical surfaces.",
    highlights: ["25 codes", "Smart routing rules", "Custom domain", "Full analytics"],
  },
  [Plan.Pro]: {
    name: "Pro",
    price: "$15",
    cadence: "/mo",
    tagline: "For marketers and power users.",
    highlights: ["200 codes", "A/B + advanced rules", "Bulk generation", "All code & content types"],
  },
  [Plan.Agency]: {
    name: "Dev / Agency",
    price: "$39",
    cadence: "/mo",
    tagline: "For developers and agencies.",
    highlights: ["Unlimited codes", "REST API + keys", "White-label", "Client workspaces"],
  },
};

/** Rank in the upgrade ladder — drives which plans show an "Upgrade" CTA. */
const PLAN_RANK: Record<Plan, number> = {
  [Plan.Free]: 0,
  [Plan.Solo]: 1,
  [Plan.Pro]: 2,
  [Plan.Agency]: 3,
};

/** `-1` is the Agency unlimited sentinel from the backend. */
const UNLIMITED = -1;

interface BillingScreenProps {
  /** The `?status=` flag parsed from the Stripe return redirect, if any. */
  returnStatus?: ReturnStatus;
  /** Clears the `?status=` flag from the URL once its banner is dismissed. */
  onClearReturnStatus?: () => void;
}

/**
 * Billing dashboard: shows the caller's current plan, usage against their code cap, an upgrade
 * action for each higher paid plan (→ Stripe Checkout), and a "Manage billing" action (→ Stripe
 * Customer Portal). Both actions redirect the whole window to the hosted Stripe URL. A success /
 * cancelled banner is shown when returning from Checkout.
 */
export function BillingScreen({ returnStatus, onClearReturnStatus }: BillingScreenProps) {
  const [billing, setBilling] = useState<BillingStatus | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  /** Which paid plan's Checkout is being created (disables its button + shows the spinner). */
  const [upgradingTo, setUpgradingTo] = useState<Plan | null>(null);
  const [openingPortal, setOpeningPortal] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setBilling(await getBilling());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function upgrade(plan: Plan) {
    setUpgradingTo(plan);
    setError(null);
    try {
      // Redirect the whole window to Stripe's hosted Checkout — never an in-app card form.
      window.location.href = await createCheckout(plan);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not start checkout");
      setUpgradingTo(null);
    }
  }

  async function manageBilling() {
    setOpeningPortal(true);
    setError(null);
    try {
      window.location.href = await createPortal();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Could not open the billing portal");
      setOpeningPortal(false);
    }
  }

  const currentPlan = billing?.plan ?? Plan.Free;
  const currentRank = PLAN_RANK[currentPlan];
  const maxCodes = billing?.limits.maxCodes ?? 0;
  const codeCount = billing?.usage.codeCount ?? 0;
  const unlimited = maxCodes === UNLIMITED;
  // Higher paid plans the caller can move up to.
  const upgradePlans = PAID_PLANS.filter((p) => PLAN_RANK[p] > currentRank);
  const busy = upgradingTo !== null || openingPortal;

  return (
    <div className="flex flex-col gap-6">
      <div>
        <Heading level={1} className="text-2xl font-bold">
          Billing
        </Heading>
        <p className="text-muted-foreground">Manage your plan, usage, and payment method.</p>
      </div>

      {returnStatus === "success" && (
        <Banner
          severity="success"
          title="You're all set."
          description="Your subscription is active. Your new plan and limits are reflected below."
          onClose={onClearReturnStatus}
        />
      )}
      {returnStatus === "cancelled" && (
        <Banner
          severity="warning"
          title="Checkout cancelled."
          description="No changes were made — you can pick a plan again whenever you're ready."
          onClose={onClearReturnStatus}
        />
      )}

      {error && (
        <p className="rounded-md border border-destructive/40 bg-destructive/5 p-3 text-sm text-destructive">
          {error}
        </p>
      )}

      {loading ? (
        <div className="flex min-h-[40vh] items-center justify-center">
          <Spinner size="lg" label="Loading billing" />
        </div>
      ) : (
        <>
          {/* ── Current plan + usage ── */}
          <Card className="flex flex-col gap-5 p-6">
            <div className="flex flex-wrap items-start justify-between gap-4">
              <div>
                <div className="flex items-center gap-2">
                  <span className="text-lg font-semibold">{PLAN_META[currentPlan].name}</span>
                  <Badge variant={currentPlan === Plan.Free ? "neutral" : "success"} size="sm">
                    {currentPlan === Plan.Free ? "Free plan" : "Active"}
                  </Badge>
                </div>
                <p className="mt-1 text-sm text-muted-foreground">{PLAN_META[currentPlan].tagline}</p>
              </div>
              <Button
                variant="outline"
                tone="neutral"
                leadingSlot={<CreditCard size={16} />}
                isLoading={openingPortal}
                loadingText="Opening…"
                isDisabled={busy}
                onClick={manageBilling}
              >
                Manage billing
              </Button>
            </div>

            <div>
              <div className="mb-1.5 flex items-center justify-between text-sm">
                <span className="font-medium">Codes used</span>
                <span className="tabular-nums text-muted-foreground">
                  {codeCount.toLocaleString()}
                  {" / "}
                  {unlimited ? (
                    <InfinityIcon size={15} className="-mt-0.5 inline align-middle" aria-label="unlimited" />
                  ) : (
                    maxCodes.toLocaleString()
                  )}
                </span>
              </div>
              {unlimited ? (
                // No cap to meter against on Agency — show a full, calm bar.
                <MeterBar value={1} max={1} thresholds={[1, 1]} aria-label="Unlimited codes" />
              ) : (
                <MeterBar
                  value={codeCount}
                  max={Math.max(maxCodes, 1)}
                  label={`${codeCount} of ${maxCodes} codes used`}
                />
              )}
              {!unlimited && codeCount >= maxCodes && (
                <p className="mt-2 text-sm text-warning">
                  You've reached your plan's code limit. Upgrade to create more — your existing codes keep
                  working regardless.
                </p>
              )}
            </div>
          </Card>

          {/* ── Upgrade options ── */}
          {upgradePlans.length > 0 ? (
            <div className="flex flex-col gap-3">
              <Heading level={2} className="text-lg font-semibold">
                {currentPlan === Plan.Free ? "Choose a plan" : "Upgrade"}
              </Heading>
              <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {upgradePlans.map((plan) => {
                  const meta = PLAN_META[plan];
                  return (
                    <Card key={plan} className="flex flex-col gap-4 p-5">
                      <div>
                        <div className="flex items-baseline justify-between gap-2">
                          <span className="text-base font-semibold">{meta.name}</span>
                          <span className="text-sm text-muted-foreground">
                            <span className="text-xl font-bold text-foreground">{meta.price}</span>
                            {meta.cadence}
                          </span>
                        </div>
                        <p className="mt-1 text-sm text-muted-foreground">{meta.tagline}</p>
                      </div>
                      <ul className="flex flex-1 flex-col gap-2 text-sm">
                        {meta.highlights.map((h) => (
                          <li key={h} className="flex items-start gap-2">
                            <Check size={15} className="mt-0.5 shrink-0 text-primary" />
                            <span>{h}</span>
                          </li>
                        ))}
                      </ul>
                      <Button
                        tone="primary"
                        isFullWidth
                        trailingSlot={<ArrowUpRight size={16} />}
                        isLoading={upgradingTo === plan}
                        loadingText="Redirecting…"
                        isDisabled={busy}
                        onClick={() => upgrade(plan)}
                      >
                        Upgrade to {meta.name}
                      </Button>
                    </Card>
                  );
                })}
              </div>
            </div>
          ) : (
            <Card className="p-6">
              <p className="text-sm text-muted-foreground">
                You're on the top plan — nothing more to upgrade to. Use{" "}
                <span className="font-medium text-foreground">Manage billing</span> to update your payment
                method or cancel.
              </p>
            </Card>
          )}
        </>
      )}
    </div>
  );
}
