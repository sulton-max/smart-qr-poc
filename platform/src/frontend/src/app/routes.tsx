import { useNavigate, useParams } from "react-router-dom";
import { CodesListScreen } from "../screens/CodesListScreen";
import { CreateCodeScreen } from "../screens/CreateCodeScreen";

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
