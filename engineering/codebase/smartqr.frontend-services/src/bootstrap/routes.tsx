import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { ContentMode } from "@/domain/codes";
import { BillingScreen, ReturnStatus } from "@/presentation/billing";
import { CodesListScreen, CreateCodeScreen } from "@/presentation/codes";

// Route adapters injecting router navigation; screens keep their callback-prop contract.

export function CodesListRoute() {
  const navigate = useNavigate();
  return (
    <CodesListScreen
      onCreate={() => navigate("/app/new")}
      onEdit={(id) => navigate(`/app/${id}/edit`)}
      onCopy={(id, mode) => navigate(`/app/new?copyOf=${id}&mode=${mode}`)}
    />
  );
}

export function CreateCodeRoute() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  // `?copyOf` prefills a fresh builder from another code (CM5); `?mode` names the side of the axis to copy into.
  const raw = searchParams.get("mode");
  const copyMode = raw === ContentMode.Static || raw === ContentMode.Dynamic ? raw : undefined;
  return (
    <CreateCodeScreen
      copyFromId={searchParams.get("copyOf") ?? undefined}
      copyMode={copyMode}
      onBack={() => navigate("/app")}
    />
  );
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
  const returnStatus = raw === ReturnStatus.Success || raw === ReturnStatus.Cancelled ? raw : undefined;
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
