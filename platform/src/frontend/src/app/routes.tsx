import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { CodesListScreen } from "../screens/CodesListScreen";
import { CreateCodeScreen } from "../screens/CreateCodeScreen";
import { BillingScreen } from "../screens/BillingScreen";

/**
 * Thin route adapters that inject router navigation into the existing screen components — the screens
 * keep their callback-prop contract untouched, so they stay exactly as verified.
 */

export function CodesListRoute() {
  const navigate = useNavigate();
  return (
    <CodesListScreen
      onCreate={() => navigate("/app/new")}
      onEdit={(id) => navigate(`/app/${id}/edit`)}
    />
  );
}

export function CreateCodeRoute() {
  const navigate = useNavigate();
  return <CreateCodeScreen onBack={() => navigate("/app")} />;
}

export function EditCodeRoute() {
  const navigate = useNavigate();
  const { id = "" } = useParams();
  return <CreateCodeScreen codeId={id} onBack={() => navigate("/app")} />;
}

export function BillingRoute() {
  const [searchParams, setSearchParams] = useSearchParams();
  // Stripe returns to `/app/billing?status=success|cancelled` — surface it as a banner, then drop it.
  const raw = searchParams.get("status");
  const returnStatus = raw === "success" || raw === "cancelled" ? raw : undefined;
  return (
    <BillingScreen
      returnStatus={returnStatus}
      onClearReturnStatus={() => {
        searchParams.delete("status");
        setSearchParams(searchParams, { replace: true });
      }}
    />
  );
}
